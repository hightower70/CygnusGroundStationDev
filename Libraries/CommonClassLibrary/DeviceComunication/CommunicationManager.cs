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
// Device communication manager (handles communication interfaces, packets, etc.) 
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.RealtimeObjectExchange;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace CommonClassLibrary.DeviceCommunication
{
	public class CommunicationManager : IDisposable
	{
		#region · Constants ·

		public const byte InvalidInterface = 0xff;
		const int TransmitterQueueLength = 16;
		const int ReceiverQueueLength = 64;
		const int DevicePacketTimeout = 3000;
		const int HostHeartbeatPeriod = 1000;
		const int PacketTransmitTimeout = 500;
		const byte InvalidSystemFileID = 0xff;

		#endregion

		#region · Types ·
		public delegate void ReceivedPacketProcessorCallback(byte in_interface_index, byte[] in_packet_buffer, byte in_packet_length);

		private enum FileOperationState
		{
			Idle,

			DownoadInfo,
			DownloadData
		};

		public enum FileOperationResult
		{
			Success,

			NotFound,
			NoConnection
		}

		#endregion

		#region · Data members ·
		private bool m_is_disposed;

		// thread variables
		private volatile bool m_stop_requested; // external request to stop the thread
		private ManualResetEvent m_thread_stopped;  // Worker thread sets this event when it is stopped
		private AutoResetEvent m_thread_event;
		private Thread m_thread;

		// communication buffers
		private PacketQueue m_receiver_queue;
		private PacketQueue m_transmitter_queue;
		private List<ICommunicationInterface> m_communication_interfaces;

		// host status
		private DateTime m_host_heartbeat_timestamp;
		private int m_packet_counter;

		// device status
		private bool m_device_connected;
		private UInt32 m_device_unique_id;
		private DateTime m_last_device_heartbeat_timestamp;

		// file operation
		private FileOperationState m_file_operation_state;
		private int m_file_operation_retry_count;
		private DateTime m_file_operation_timestamp;
		private string m_file_operation_current_name;
		private byte m_file_operation_current_id;
		private UInt32 m_file_operation_current_position;
		private UInt32 m_file_operation_current_size;
		private DeviceFileCache.CachedFile m_file_operation_current_file;

		// Realtime Object interface members
		private RealtimeObjectStorage m_realtime_object_creator;


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

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		public CommunicationManager()
		{
			// init members
			m_stop_requested = false;
			m_thread_stopped = new ManualResetEvent(false);
			m_thread_event = new AutoResetEvent(false);
			m_thread = null;
			m_receiver_queue = new PacketQueue(TransmitterQueueLength);
			m_transmitter_queue = new PacketQueue(TransmitterQueueLength);
			m_communication_interfaces = new List<ICommunicationInterface>();
			m_file_operation_state = FileOperationState.Idle;
			m_file_operation_current_file = null;
		}
		#endregion

		#region · Properties ·

		/// <summary>
		/// True if device is connected
		/// </summary>
		public bool Connected
		{
			get { return m_device_connected; }
		}
		#endregion

		#region · Public members ·

		public void CreateRealtimeObjects()
		{
			ParserRealtimeObject communication_state = m_realtime_object_creator.CreateObject("DeviceCommunicationState");
		}

		/// <summary>
		/// Starts TCPClient thread
		/// </summary>
		public void Start()
		{
			int i;

			// reset events
			m_stop_requested = false;
			m_thread_stopped.Reset();
			m_thread_event.Reset();

			// create worker thread instance
			m_thread = new Thread(new ThreadStart(Run));
			m_thread.Name = "Communication Thread";   // looks nice in Output window

			// initialize communication state
			m_device_connected = false;
			m_last_device_heartbeat_timestamp = DateTime.MinValue;
			m_host_heartbeat_timestamp = DateTime.MinValue;

			// start thread
			m_thread.Start();

			// start communicatios
			for (i = 0; i < m_communication_interfaces.Count; i++)
			{
				m_communication_interfaces[i].Start();
			}

		}

		/// <summary>
		/// Stops communciation thread
		/// </summary>
		public void Stop()
		{
			int i;

			// stop communicatios
			for (i = 0; i < m_communication_interfaces.Count; i++)
			{
				m_communication_interfaces[i].Stop();
			}

			//stop commuication thread
			if (m_thread != null && m_thread.IsAlive)  // thread is active
			{
				// set event "Stop"
				m_stop_requested = true;
				m_thread_event.Set();

				// wait when thread  will stop or finish
				while (m_thread.IsAlive)
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

					Thread.Sleep(100);
				}
			}
		}


		/// <summary>
		/// Adds new communication device to the list of the communicators
		/// </summary>
		/// <param name="in_communicator"></param>
		public void AddCommunicator(ICommunicationInterface in_communication_interface)
		{
			byte interface_index;

			lock (m_communication_interfaces)
			{
				m_communication_interfaces.Add(in_communication_interface);
				interface_index = (byte)(m_communication_interfaces.Count - 1);
			}

			in_communication_interface.SetReceivedPacketProcessor(interface_index, StoreReceivedPacket);
		}

		/// <summary>
		/// Pushes packet into the transmitter queue to sending on all interfaces
		/// </summary>
		/// <param name="in_packet"></param>
		public void SendPacket(PacketBase in_packet)
		{
			byte counter = (byte)((UInt32)(Interlocked.Increment(ref m_packet_counter)) & 0xff);

			in_packet.SetCounter(counter);

			m_transmitter_queue.Push(InvalidInterface, in_packet);
		}

		/// <summary>
		/// Called by interfaces to store received packets
		/// </summary>
		/// <param name="in_channel_info"></param>
		/// <param name="in_packet_buffer"></param>
		/// <param name="in_packet_length"></param>
		public void StoreReceivedPacket(byte in_channel_index, byte[] in_packet_buffer, byte in_packet_length)
		{
			m_receiver_queue.Push(in_channel_index, in_packet_buffer, in_packet_length);
			m_thread_event.Set();
		}

		#endregion

		#region · Thread functions ·

		/// <summary>
		/// Main thread function
		/// </summary>
		public void Run()
		{
			bool event_occured;
			double ellapsed_time;

			while (!m_stop_requested)
			{
				// wait for event
				event_occured = m_thread_event.WaitOne(100);

				// exit loop if thread must be stopped
				if (m_stop_requested)
					break;

				// handle packet sending
				HandlePacketSending();

				// handle packet receiving
				ProcessReceivedPacket();

				// handle file timeout
				if (m_file_operation_state != FileOperationState.Idle)
					FileOperationTimeOutHandler();

				// handle communication lost event
				if (m_device_connected && (DateTime.Now - m_last_device_heartbeat_timestamp).TotalMilliseconds > DevicePacketTimeout)
				{
					m_device_connected = false;
				}

				// send host heartbeat
				ellapsed_time = (DateTime.Now - m_host_heartbeat_timestamp).TotalMilliseconds;
				if (m_device_connected && ellapsed_time > HostHeartbeatPeriod)
				{
					m_host_heartbeat_timestamp = m_host_heartbeat_timestamp.AddMilliseconds(ellapsed_time);

					PacketHostHeartbeat packet = new PacketHostHeartbeat();

					SendPacket(packet);
				}
			}

			// thread is finished
			m_thread_stopped.Set();
		}

		/// <summary>
		/// Sends one packet from the transmitter packet queue
		/// </summary>
		private void HandlePacketSending()
		{
			byte[] packet;
			byte packet_length;
			byte target_interface;
			bool packet_sent = false;
			DateTime timestamp;

			if (m_transmitter_queue.PopBegin(out packet, out packet_length, out target_interface, out timestamp))
			{
				if (target_interface == InvalidInterface)
				{
					// send on all interfaces
					for (byte interface_index = 0; interface_index < m_communication_interfaces.Count; interface_index++)
					{
						if (m_communication_interfaces[interface_index].SendPacket(packet, packet_length))
							packet_sent = true; // packet was sent at least over one interface
					}
				}
				else
				{
					// send on the specified interface
					packet_sent = m_communication_interfaces[target_interface].SendPacket(packet, packet_length);
				}

				// remove packet from th queue if send was started or packet is older than transmit timeout
				if (packet_sent || (DateTime.Now - timestamp).TotalMilliseconds > PacketTransmitTimeout)
				{
					// packet was sent -> remove packet from the queue
					m_transmitter_queue.PopEnd();

					// signal event in order to process next pending packet (if exists)
					m_thread_event.Set();
				}
			}
		}

		/// <summary>
		/// Process one packet from the packet receiver queue
		/// </summary>
		private void ProcessReceivedPacket()
		{
			// return when there is no packet to process
			if (m_receiver_queue.IsEmpty())
				return;

			if (!m_receiver_queue.PeekPacketValid())
				return;

			// update connection state
			m_device_connected = true;
			m_last_device_heartbeat_timestamp = DateTime.Now;

			// get packet type
			PacketType packet_type = m_receiver_queue.PeekPacketType();

			if ((packet_type & PacketType.FlagSystem) != 0)
			{
				// process sytem packets
				switch (packet_type)
				{
					// process device heartbeat packet
					case PacketType.CommDeviceHeartbeat:
						ProcessDeviceHeartbeatPacket();
						break;

					// process file information response packet
					case PacketType.FileInfoResponse:
						ProcessFileInfoResponse();
						break;

					case PacketType.FileDataResponse:
						ProcessFileDataResponse();
						break;
				}
			}
			else
			{
				// process telemetry packet
			}
		}


		#endregion

		#region · Comm packet processors ·

		/// <summary>
		/// Processes heart beat packet
		/// </summary>
		private void ProcessDeviceHeartbeatPacket()
		{
			PacketDeviceHeartbeat packet = (PacketDeviceHeartbeat)m_receiver_queue.Pop(typeof(PacketDeviceHeartbeat));

		}

		#endregion

		#region · File operation ·

		/// <summary>
		/// Starts file download operation
		/// </summary>
		/// <param name="in_file_name"></param>
		public bool StartFileDownload(string in_file_name)
		{
			// sanity check
			if (m_file_operation_state != FileOperationState.Idle)
				return false;

			m_file_operation_state = FileOperationState.DownoadInfo;
			m_file_operation_retry_count = 0;
			m_file_operation_timestamp = DateTime.Now;
			m_file_operation_current_name = in_file_name;

			SendFileInfoPacketRequest(in_file_name);

			return true;
		}

		/// <summary>
		/// Send file information request packet
		/// </summary>
		/// <param name="in_file_name"></param>
		public void SendFileInfoPacketRequest(string in_file_name)
		{
			PacketFileInfoRequest request_packet = new PacketFileInfoRequest();

			request_packet.FileName = in_file_name;

			SendPacket(request_packet);
		}


		/// <summary>
		/// Processes file information response packet
		/// </summary>
		private void ProcessFileInfoResponse()
		{
			PacketFileInfoResponse packet = (PacketFileInfoResponse)m_receiver_queue.Pop(typeof(PacketFileInfoResponse));

			if(packet.FileID == InvalidSystemFileID)
			{
				// if file not found -> error
				m_file_operation_state = FileOperationState.Idle;

				CallFileOperationCallback(FileOperationResult.NotFound, null);
			}
			else
			{
				// file exists -> check file in the cache
				if(DeviceFileCache.IsFileExists(m_file_operation_current_name, packet.FileLength, packet.FileHash))
				{
					// file exists in the cache -> call callback with success code
					string file_path;
					file_path = Path.Combine(DeviceFileCache.GetFileCachePath(), m_file_operation_current_name);

					CallFileOperationCallback(FileOperationResult.Success, file_path);
				}
				else
				{
					// file doesn't exists in the cache create file in the cache and download it
					m_file_operation_current_file = DeviceFileCache.CreateFile(m_file_operation_current_name, 0);

					// start download
					m_file_operation_retry_count = 0;
					m_file_operation_current_position = 0;
					m_file_operation_current_size = packet.FileLength;
					m_file_operation_state = FileOperationState.DownloadData;

					SendFileDataPacketRequest();
				}
			}
		}

		private void SendFileDataPacketRequest()
		{
			UInt32 request_data_length;
			UInt32 max_data_length;
			PacketFileDataRequest request_packet;

			// determine data length
			max_data_length = (uint)(PacketConstants.PacketMaxLength - PacketConstants.PacketCRCLength - Marshal.SizeOf(typeof(PacketFileDataResponseHeader)));
			request_data_length = m_file_operation_current_size - m_file_operation_current_position;

			if (request_data_length > max_data_length)
				request_data_length = max_data_length;

			// create packet
			request_packet = new PacketFileDataRequest();
			request_packet.FileID = m_file_operation_current_id;
			request_packet.FilePos = m_file_operation_current_position;
			request_packet.DataLength = (byte)request_data_length;

			SendPacket(request_packet);
		}

		private void ProcessFileDataResponse()
		{
			byte[] packet;
			byte packet_length;
			byte interface_index;
			DateTime packet_timestamp;
			PacketFileDataResponseHeader packet_header;
			int data_offset;
			int data_length;

			if (m_receiver_queue.PopBegin(out packet, out packet_length, out interface_index, out packet_timestamp))
			{

				if (m_file_operation_current_file != null)
				{
					packet_header = (PacketFileDataResponseHeader)RawBinarySerialization.DeserializeObject(packet, typeof(PacketFileDataResponseHeader));

					data_offset = Marshal.SizeOf(typeof(PacketFileDataResponseHeader));
					data_length = packet_header.Length - PacketConstants.PacketCRCLength - Marshal.SizeOf(typeof(PacketFileDataResponseHeader));

					m_file_operation_current_file.Write(packet, data_offset, data_length);

					m_file_operation_current_position += (uint)data_length;

					if (m_file_operation_current_position < m_file_operation_current_size)
						SendFileDataPacketRequest();
					else
					{
						m_file_operation_current_file.Close();
						CallFileOperationCallback(FileOperationResult.Success, m_file_operation_current_file.FullFilePath);
						m_file_operation_current_file.Dispose();
						m_file_operation_current_file = null;
					}
				}

				m_receiver_queue.PopEnd();
			}
		}


		/// <summary>
		/// Handles timeout condition
		/// </summary>
		private void FileOperationTimeOutHandler()
		{

		}

		private void CallFileOperationCallback(FileOperationResult in_result, string in_file_path)
		{

		}
		#endregion
	}
}
