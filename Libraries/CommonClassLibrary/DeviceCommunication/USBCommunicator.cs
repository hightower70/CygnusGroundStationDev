///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2016 Laszlo Arvai. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2.1 of the License,
// or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
// MA 02110-1301  USA
///////////////////////////////////////////////////////////////////////////////
// File description
// ----------------
// Communication class over UART
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.RealtimeObjectExchange;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CommonClassLibrary.DeviceCommunication
{
	public class USBCommunicator : IDisposable, ICommunicator
	{
		#region · Constants ·
		private const int ReportHeaderLength = 1;
		private const int ReportTypeHeaderLength = 1;

		private const int ThreadWaitTimeout = 100;
		private const int DeviceOpenRetryPeriod = 1000;

		private const int HIDPacketHeaderSize = 1;
		private const int HIDReportHeaderSize = 1;
		private const int HIDReportPayloadSize = USBNativeMethods.HID_MAX_REPORT_SIZE - HIDReportHeaderSize - HIDPacketHeaderSize;
		private const int HIDTransmitBufferCount = (PacketConstants.PacketMaxLength + (HIDReportPayloadSize - 1)) / HIDReportPayloadSize; // (HIDReportPayloadSize - 1) beacuse the value must be rounded up

		private const int TransmitterLocked = 1;
		private const int TransmitterFree = 0;

		#endregion

		#region · Types ·
		/// <summary>
		/// Information about the HID device 
		/// </summary>
		public class DeviceInfo
		{
			public string DevicePath { set; get; }
			public string SerialNumber { get; set; }
		}
		#endregion

		#region · Data members ·
		private CommunicationManager m_communication_manager;
		private byte m_communication_channel_index;
		private bool m_is_disposed;

		private string m_device_path;
		private SafeFileHandle m_device_handle;
		private FileStream m_file_stream;

		private Thread m_thread;
		private volatile bool m_stop_requested; // external request to stop the thread
		private ManualResetEvent m_thread_stopped;  // Worker thread sets this event when it is stopped
		private AutoResetEvent m_thread_event;

		private AsyncCallback m_receiver_callback;

		private byte[] m_input_report_buffer;

		private byte[] m_receive_packet_buffer;
		private int m_received_packet_pos;
		private int m_received_packet_length;

		private int m_input_report_length = 0;
		private int m_output_report_length = 0;
		private int m_feature_report_length = 0;

		private byte[] m_transmitter_buffer;
		private int m_transmitter_locked;
		private int m_transmitter_index;
		private AsyncCallback m_transmitter_callback;

		private DateTime m_timestamp;

		private UInt32 m_client_unique_id;

		private volatile int m_upstream_bytes;
		private volatile int m_downstream_bytes;
		private RealtimeObjectMember m_upstream_member;
		private RealtimeObjectMember m_downstream_member;

		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		public USBCommunicator()
		{
			// create events
			m_thread_event = new AutoResetEvent(false); // thread event
			m_thread_stopped = new ManualResetEvent(false);
			m_stop_requested = false;
			m_thread = null;

			m_receive_packet_buffer = new byte[PacketConstants.PacketMaxLength];
			m_received_packet_length = 0;
			m_received_packet_pos = 0;
			m_receiver_callback = new AsyncCallback(ReceiverCallback);
			m_input_report_buffer = null;

			m_transmitter_buffer = new byte[HIDTransmitBufferCount * USBNativeMethods.HID_MAX_REPORT_SIZE];
			m_transmitter_callback = new AsyncCallback(TransmitterCallback);
			m_transmitter_locked = TransmitterFree;
		}
		#endregion

		#region · Properties ·
		public int VID { get; set; }
		public int PID { get; set; }
		#endregion

		#region · Public members ·

		/// <summary>
		/// Starts USB communication thread
		/// </summary>
		public void Start()
		{
			// reset events
			m_stop_requested = false;
			m_thread_stopped.Reset();

			// create worker thread instance
			m_thread = new Thread(new ThreadStart(Run));
			m_thread.Name = "USB Communication Thread";   // looks nice in Output window

			// start thread
			m_thread.Start();
		}

		/// <summary>
		/// Stops USB communciation thread
		/// </summary>
		public void Stop()
		{
			int retry = 0;

			if (m_thread != null && m_thread.IsAlive)  // thread is active
			{
				// set event "Stop"
				m_stop_requested = true;
				m_thread_event.Set();

				// wait when thread  will stop or finish
				while (m_thread.IsAlive)
				{
					if (retry < 3)
					{

						// We cannot use here infinite wait because our thread
						// makes syncronous calls to main form, this will cause deadlock.
						// Instead of this we wait for event some appropriate time
						// (and by the way give time to worker thread) and
						// process events. These events may contain Invoke calls.
						if (WaitHandle.WaitAll((new ManualResetEvent[] { m_thread_stopped }), 100, true))
						{
							break;
						}

						retry++;
					}
					else
					{
						m_thread.Abort();
						Close();
						break;
					}
				}
			}
		}

		/// <summary>
		/// Send packet. If sender is busy, than it waits 100ms before reporting send failure.
		/// </summary>
		/// <param name="in_packet">Packet to send</param>
		/// <param name="in_packet_length">Length of the packet in bytes</param>
		/// <returns>True if sending started, false if sending can't be started</returns>
		public bool SendPacket(byte[] in_packet, int in_packet_length)
		{
			int buffer_pos;
			int buffer_index;
			int bytes_to_copy;

			// if device is nos topened -> return
			if (m_device_handle == null || m_device_handle.IsClosed)
				return false;

			// try to lock sender
			if (Interlocked.CompareExchange(ref m_transmitter_locked, TransmitterLocked, TransmitterFree) != TransmitterFree)
				return false;

			// store packet in HID report chunks
			buffer_pos = 0;
			buffer_index = 0;
			while (buffer_pos < in_packet_length)
			{
				// store packet header (report type, remaining byte count)
				m_transmitter_buffer[buffer_index * USBNativeMethods.HID_MAX_REPORT_SIZE] = (byte)0; // report type = output report
				m_transmitter_buffer[buffer_index * USBNativeMethods.HID_MAX_REPORT_SIZE + HIDReportHeaderSize] = (byte)(in_packet_length - buffer_pos); // remaining byte cont

				// store packet
				bytes_to_copy = in_packet_length - buffer_pos;
				if (bytes_to_copy > HIDReportPayloadSize)
					bytes_to_copy = HIDReportPayloadSize;

				Buffer.BlockCopy(in_packet, buffer_pos, m_transmitter_buffer, buffer_index * USBNativeMethods.HID_MAX_REPORT_SIZE + HIDReportHeaderSize + HIDPacketHeaderSize, bytes_to_copy);

				buffer_pos += bytes_to_copy;
				buffer_index++;
			}

			// invalidate other buffers
			while (buffer_index < HIDTransmitBufferCount)
			{
				m_transmitter_buffer[buffer_index * USBNativeMethods.HID_MAX_REPORT_SIZE] = 0;
				buffer_index++;
			}

			// start sending
			m_transmitter_index = 0;
			try
			{
				m_file_stream.BeginWrite(m_transmitter_buffer, m_transmitter_index * USBNativeMethods.HID_MAX_REPORT_SIZE, USBNativeMethods.HID_MAX_REPORT_SIZE, m_transmitter_callback, this);
				m_file_stream.FlushAsync();
			}
			catch
			{
				m_transmitter_locked = TransmitterFree;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Gets currently connected client ID
		/// </summary>
		/// <returns></returns>
		public UInt32 GetClientID()
		{
			return m_client_unique_id;
		}

		#endregion

		#region · Device Enumeration ·

		/// <summary>
		/// Gets the list of the dvices
		/// </summary>
		/// <param name="in_vid">Vendor ID</param>
		/// <param name="in_pid">Product ID</param>
		/// <returns></returns>
		public List<DeviceInfo> EnumerateDevices()
		{
			bool result;
			int device_index = 0;
			List<DeviceInfo> device_info_collection = new List<DeviceInfo>();

			// init
			Guid hid_guid = Guid.Empty;

			//Obtain the device interface GUID for the HID class
			USBNativeMethods.HidD_GetHidGuid(ref hid_guid);

			// Requesting a pointer to a device information set
			IntPtr device_info_set = USBNativeMethods.SetupDiGetClassDevs(ref hid_guid, IntPtr.Zero, IntPtr.Zero, USBNativeMethods.DIGCF_PRESENT | USBNativeMethods.DIGCF_INTERFACEDEVICE);

			// The cbSize element of the MyDeviceInterfaceData structure must be set to
			// the structure's size in bytes. 
			USBNativeMethods.SP_DEVICE_INTERFACE_DATA device_interface_data = new USBNativeMethods.SP_DEVICE_INTERFACE_DATA();
			device_interface_data.Size = Marshal.SizeOf(device_interface_data);

			device_index = 0;
			while (true)
			{
				// Begin with 0 and increment through the device information set until
				// no more devices are available.
				result = USBNativeMethods.SetupDiEnumDeviceInterfaces(device_info_set, IntPtr.Zero, ref hid_guid, device_index, ref device_interface_data);
				if (!result)
				{
					// If it fails, that means we've reached the end of the list.
					break;
				}

				// A device is present.
				// Find out how big of a buffer is needed.
				Int32 buffer_size = 0;
				result = USBNativeMethods.SetupDiGetDeviceInterfaceDetail(device_info_set, ref device_interface_data, IntPtr.Zero, 0, ref buffer_size, IntPtr.Zero);
				if (result)
				{
					// This success is unexpected! We wanted to get an error, with the attendant
					// information of how big to make the buffer for a successful call.
					break;
				}

				// Allocate memory for the SP_DEVICE_INTERFACE_DETAIL_DATA structure, using the returned Length.
				IntPtr detail_data_buffer = Marshal.AllocHGlobal(buffer_size);

				// Store cbSize in the first bytes of the array. The number of bytes varies with 32-bit and 64-bit systems.
				Marshal.WriteInt32(detail_data_buffer, (IntPtr.Size == 4) ? (IntPtr.Size + Marshal.SystemDefaultCharSize) : 8);

				// Call SetupDiGetDeviceInterfaceDetail again.
				// This time, pass a pointer to DetailDataBuffer
				// and the returned required buffer size.
				result = USBNativeMethods.SetupDiGetDeviceInterfaceDetail(device_info_set, ref device_interface_data, detail_data_buffer, buffer_size, ref buffer_size, IntPtr.Zero);
				if (result)
				{

					// Skip over cbsize (4 bytes) to get the address of the devicePathName.
					IntPtr device_pathname_pointer = new IntPtr(detail_data_buffer.ToInt64() + sizeof(Int32));

					// Get the String containing the devicePathName.
					string device_pathname = Marshal.PtrToStringAuto(device_pathname_pointer);

					// Open a handle to the device.
					SafeFileHandle device_handle;
					device_handle = USBNativeMethods.CreateFile(device_pathname,
																			USBNativeMethods.GENERIC_READ | USBNativeMethods.GENERIC_WRITE,
																			USBNativeMethods.FILE_SHARE_READ | USBNativeMethods.FILE_SHARE_WRITE,
																			IntPtr.Zero,
																			USBNativeMethods.OPEN_EXISTING,
																			0,
																			0);

					if (!device_handle.IsInvalid)
					{
						USBNativeMethods.HIDD_ATTRIBUTES attributes = new USBNativeMethods.HIDD_ATTRIBUTES();

						// Set the Size to the number of bytes in the structure.
						attributes.Size = Marshal.SizeOf(attributes);

						// Requests information from the device.
						result = USBNativeMethods.HidD_GetAttributes(device_handle, ref attributes);

						// check vendor and product id
						if (attributes.VendorID == VID && attributes.ProductID == PID)
						{
							StringBuilder serial_number = new StringBuilder(1024);

							if (USBNativeMethods.HidD_GetSerialNumberString(device_handle, serial_number, serial_number.Capacity))
							{
								DeviceInfo device_info = new DeviceInfo();

								device_info.DevicePath = device_pathname;
								device_info.SerialNumber = serial_number.ToString();

								device_info_collection.Add(device_info);
							}
						}

						// close handle
						device_handle.Close();
					}
				}

				// Free the memory used by the detailData structure (no longer needed).
				Marshal.FreeHGlobal(detail_data_buffer);

				// next device
				device_index++;
			}

			// free device info set
			if (device_info_set != IntPtr.Zero)
			{
				USBNativeMethods.SetupDiDestroyDeviceInfoList(device_info_set);
			}

			return device_info_collection;
		}

		#endregion

		#region · Thread functions ·

		/// <summary>
		/// Main thread function
		/// </summary>
		public void Run()
		{
			bool event_occured;
			DateTime last_device_open_timestamp = DateTime.Now;

			// communication loop
			while (!m_stop_requested)
			{
				// wait for event
				event_occured = m_thread_event.WaitOne(ThreadWaitTimeout);

				// exit loop if thread must be stopped
				if (m_stop_requested)
					break;

				// try to open device if it is not opened 
				if (m_device_handle == null || m_device_handle.IsClosed)
				{
					// device is not opened -> try to open it
					if ((DateTime.Now - last_device_open_timestamp).TotalMilliseconds > DeviceOpenRetryPeriod)
					{
						// enumerate devices
						List<DeviceInfo> device_info = EnumerateDevices();

						// TODO: do device selection
						if (device_info.Count > 0)
						{
							if (Open(device_info[0].DevicePath))
							{
								// reserve report buffers
								m_input_report_buffer = new byte[m_input_report_length];

								// prepare for packet receiving
								m_received_packet_pos = 0;
								m_received_packet_length = 0;

								m_transmitter_locked = TransmitterFree;

								StartReading();
							}
						}
					}
				}
				else
				{
					// device is opened
					// process incoming packets
					if (m_received_packet_length > 0)
					{
						if (m_received_packet_length <= PacketConstants.PacketMaxLength)
							m_communication_manager.StoreReceivedPacket(m_communication_channel_index, m_receive_packet_buffer, (byte)m_received_packet_length);

						// restart packet reading
						m_received_packet_length = 0;
						m_received_packet_pos = 0;
						StartReading();
					}
				}
			}

			// close USB device
			Close();

			// thread is finished
			m_thread_stopped.Set();
		}
		#endregion

		#region · Open/Close functions ·

		/// <summary>
		/// Opens device in persistent mode (reopens after device removal-reconnection)
		/// </summary>
		/// <param name="in_path"></param>
		/// <returns></returns>
		public bool Open(string in_path)
		{
			bool success = true;

			// store devcie path
			m_device_path = in_path;

			// open device
			m_device_handle = USBNativeMethods.CreateFile(in_path, USBNativeMethods.GENERIC_READ | USBNativeMethods.GENERIC_WRITE, USBNativeMethods.FILE_SHARE_READ | USBNativeMethods.FILE_SHARE_WRITE, IntPtr.Zero, USBNativeMethods.OPEN_EXISTING, USBNativeMethods.FILE_FLAG_OVERLAPPED, 0);

			if (m_device_handle.IsInvalid)
				success = false;

			// get report length
			if (success)
			{
				IntPtr preparsed_data_pointer = new IntPtr();
				if (USBNativeMethods.HidD_GetPreparsedData(m_device_handle, ref preparsed_data_pointer))
				{
					USBNativeMethods.HIDP_CAPS capabilities = new USBNativeMethods.HIDP_CAPS();

					// Get report lengths
					USBNativeMethods.HidP_GetCaps(preparsed_data_pointer, ref capabilities);

					// Store report length
					m_input_report_length = capabilities.InputReportByteLength;
					m_output_report_length = capabilities.OutputReportByteLength;
					m_feature_report_length = capabilities.FeatureReportByteLength;

					// No need for PreparsedData any more, so free the memory it's using.
					USBNativeMethods.HidD_FreePreparsedData(ref preparsed_data_pointer);

					// check report size
					if (m_input_report_length != USBNativeMethods.HID_MAX_REPORT_SIZE || m_output_report_length != USBNativeMethods.HID_MAX_REPORT_SIZE)
						success = false;
				}
				else
					success = false;
			}

			if(success)
			{
				// determine buffer size (use the biggest report size, normally they must be same)
				int buffer_size = m_input_report_length;

				if (m_output_report_length > buffer_size)
					buffer_size = m_output_report_length;

				if (m_feature_report_length > buffer_size)
					buffer_size = m_feature_report_length;

				// get file stream
				m_file_stream = new FileStream(m_device_handle, FileAccess.ReadWrite, buffer_size, true);
			}

			return success;
		}

		public bool IsOpened()
		{
			return m_device_handle != null && !m_device_handle.IsClosed;
		}

		/// <summary>
		/// Closes device access and releases used resources
		/// </summary>
		public void Close()
		{
			// release device path
			if (m_device_path != null)
			{
				m_device_path = null;
			}

			// close file handle
			if (m_device_handle != null)
			{
				m_device_handle.Close();
				m_device_handle = null;
			}

			// release report buffers
			if (m_input_report_buffer != null)
				m_input_report_buffer = null;

		}

		#endregion

		#region · Receiver functions ·

		/// <summary>
		/// Starts/Restarts reading data from the USB device
		/// </summary>
		private void StartReading()
		{
			try
			{
				// start reading from the device
				m_file_stream.BeginRead(m_input_report_buffer, 0, m_input_report_buffer.Length, m_receiver_callback, this);
			}
			catch
			{
				Close();
			}
		}

		/// <summary>
		/// Processes received input reports
		/// </summary>
		private void InputReportProcess()
		{
			// handle only input report
			if (m_input_report_buffer[0] == 0)
			{
				// get real data length
				int report_remaining_byte_count = m_input_report_buffer[ReportTypeHeaderLength];
				int bytes_to_copy = report_remaining_byte_count;

				// determine number of bytes to copy
				if (bytes_to_copy > HIDReportPayloadSize)
					bytes_to_copy = HIDReportPayloadSize;

				// store in the buffer
				if (m_received_packet_pos + bytes_to_copy > m_receive_packet_buffer.Length)
				{
					m_received_packet_pos = 0;
				}

				Buffer.BlockCopy(m_input_report_buffer, ReportTypeHeaderLength + ReportHeaderLength, m_receive_packet_buffer, m_received_packet_pos, bytes_to_copy);
				m_received_packet_pos += bytes_to_copy;

				Interlocked.Add(ref m_upstream_bytes, bytes_to_copy);

				if (report_remaining_byte_count <= HIDReportPayloadSize)
				{
					// the complete packet received -> notify thread about the received packet
					m_received_packet_length = m_received_packet_pos;
					m_thread_event.Set();
				}
				else
				{
					// partial packet received -> start reading other packet parts
					StartReading(); // when all that is done, kick off another read for the next report
				}
			}
		}

		/// <summary>
		/// Callback function for received data handling
		/// </summary>
		/// <param name="ar"></param>
		protected void ReceiverCallback(IAsyncResult ar)
		{
			// Retrieve the communication object from the state object.
			USBCommunicator communicator = (USBCommunicator)ar.AsyncState;
			try
			{
				// finish read operation
				communicator.m_file_stream.EndRead(ar);

				communicator.InputReportProcess();
			}
			catch
			{
				communicator.Close();
			}
		}

		#endregion

		#region · Transmitter functinos ·

		/// <summary>
		/// Callback function when transmitter is empty
		/// </summary>
		/// <param name="ar"></param>
		protected void TransmitterCallback(IAsyncResult ar)
		{
			int sent_packet_length;

			try
			{
				// finish read operation
				m_file_stream.EndWrite(ar);

				// determine true number of bytes to sent
				sent_packet_length = m_transmitter_buffer[m_transmitter_index * USBNativeMethods.HID_MAX_REPORT_SIZE + HIDReportHeaderSize];
				if (sent_packet_length > HIDReportPayloadSize)
					sent_packet_length = HIDReportPayloadSize;

				// increment downstream data counter
				Interlocked.Add(ref m_downstream_bytes, sent_packet_length);

				// start sending new block
				m_transmitter_index++;
				if(m_transmitter_index < HIDTransmitBufferCount && m_transmitter_buffer[m_transmitter_index * USBNativeMethods.HID_MAX_REPORT_SIZE] > 0)
				{
					try
					{
						m_file_stream.BeginWrite(m_transmitter_buffer, m_transmitter_index * USBNativeMethods.HID_MAX_REPORT_SIZE, USBNativeMethods.HID_MAX_REPORT_SIZE, m_transmitter_callback, this);
					}
					catch
					{
						// connection lost -> release transmitter buffer
						m_transmitter_locked = TransmitterFree;
					}
				}
				else
				{
					// transmission finished
					m_transmitter_locked = TransmitterFree;
				}
			}
			catch
			{
				Close();
			}
		}

		#endregion

		#region · IDisposable Members ·

		/// <summary>
		/// Releases all resources
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Releases all resources
		/// </summary>
		/// <param name="disposing">Indicates wheter the managed resources should be released as well</param>
		private void Dispose(bool disposing)
		{
			if (m_is_disposed)
				return;

			Stop();

			m_is_disposed = true;
		}

		#endregion

		#region · ICommunicator members  ·

		/// <summary>
		/// Assigns communication interface (communicator) to the communication manager
		/// </summary>
		/// <param name="in_communication_manager"></param>
		/// <param name="in_communication_channel_index"></param>
		public void AddToManager(CommunicationManager in_communication_manager, byte in_communication_channel_index)
		{
			m_communication_channel_index = in_communication_channel_index;
			m_communication_manager = in_communication_manager;
		}

		/// <summary>
		/// Creates realtime object member used by this interface
		/// </summary>
		/// <param name="in_realtime_object"></param>
		public void CreateRealtimeObjects(RealtimeObject in_realtime_object)
		{
			m_upstream_member = in_realtime_object.MemberCreate("USBUpStream", RealtimeObjectMember.MemberType.Int);
			m_downstream_member = in_realtime_object.MemberCreate("USBDownStream", RealtimeObjectMember.MemberType.Int);
		}

		/// <summary>
		/// Updates communication statistics realtime object member
		/// </summary>
		/// <param name="in_ellapsed_millisec"></param>
		public void UpdateCommunicationStatistics(int in_ellapsed_millisec)
		{
			int upstream = m_upstream_bytes * 1000 / in_ellapsed_millisec;
			m_upstream_bytes = 0;
			int downstream = m_downstream_bytes * 1000 / in_ellapsed_millisec;
			m_downstream_bytes = 0;

			m_upstream_member.Write(upstream);
			m_downstream_member.Write(downstream);
		}

		public void ConnectionLost()
		{

		}
		#endregion
	}
}
