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
using CommonClassLibrary.Helpers;
using CommonClassLibrary.RealtimeObjectExchange;
using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;

namespace CommonClassLibrary.DeviceCommunication
{
	public class UARTCommunicator : IDisposable, ICommunicator
	{
		#region · Constants ·
		private const int ReceiveBufferSize = 512;
		private const int TransmitBufferSize = 512;
		private const int ThreadWaitTimeout = 100;
		#endregion

		#region · Data members ·
		private CommunicationManager m_communication_manager;
		private byte m_communication_channel_index;
		private bool m_is_disposed;

		SerialPort m_serial_port;

		private Thread m_thread;
		private volatile bool m_stop_requested; // external request to stop the thread
		private ManualResetEvent m_thread_stopped;  // Worker thread sets this event when it is stopped
		private AutoResetEvent m_thread_event;
		private SemaphoreSlim m_sender_lock;

		private byte[] m_receive_data_buffer;
		private int m_received_data_length;
		private byte[] m_receive_packet_buffer;
		private int m_received_packet_length;
		private bool m_read_pending;

		private SLIP m_receiver_slip;
		private byte[] m_transmit_buffer;
		private int m_transmit_data_length;
		private DateTime m_timestamp;

		private UInt32 m_client_unique_id;

		private volatile int m_upstream_bytes;
		private volatile int m_downstream_bytes;
		private RealtimeObjectMember m_upstream_member;
		private RealtimeObjectMember m_downstream_member;

		private AsyncCallback m_receiver_callback;
		private AsyncCallback m_transmitter_callback;

		#endregion

		#region · Constructor ·
		/// <summary>
		/// Default constructor
		/// </summary>
		public UARTCommunicator()
		{
			m_thread_stopped = new ManualResetEvent(false);
			m_sender_lock = new SemaphoreSlim(1, 1);
			m_thread_event = new AutoResetEvent(false);
			m_stop_requested = false;
			m_thread = null;
			m_receive_data_buffer = new byte[ReceiveBufferSize];
			m_receive_packet_buffer = new byte[ReceiveBufferSize];
			m_received_data_length = 0;
			m_transmit_buffer = new byte[TransmitBufferSize];

			m_receiver_slip = new SLIP();

			m_receiver_callback = new AsyncCallback(ReceiveCallback);
			m_transmitter_callback = new AsyncCallback(SendCallback);
		}
		#endregion

		#region · Properties ·
		public string PortName { get; set; }
		public int BaudRate { get; set; }
		#endregion

		#region · Public members ·

		/// <summary>
		/// Starts TCPClient thread
		/// </summary>
		public void Start()
		{
			// reset events
			m_stop_requested = false;
			m_thread_stopped.Reset();
			m_thread_event.Reset();

			// create worker thread instance
			m_thread = new Thread(new ThreadStart(Run));
			m_thread.Name = "UART Communication Thread";   // looks nice in Output window

			// start thread
			m_thread.Start();
		}

		/// <summary>
		/// Stops FlightGear Thread
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
						m_serial_port = null;
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
			int source_pos;

			if (m_serial_port == null)
				return false;

			// accuire sender lock
			if (m_sender_lock.Wait(100))
			{
				// SLIP encode block
				m_transmit_data_length = 0;
				source_pos = 0;
				SLIP.EncodeBlock(in_packet, ref source_pos, in_packet_length, m_transmit_buffer, ref m_transmit_data_length, SLIP.EncoderFlags.StorePacketStart | SLIP.EncoderFlags.StorePacketEnd);

				// Begin sending the data to the remote device.
				try
				{
					m_serial_port.BaseStream.BeginWrite(m_transmit_buffer, 0, m_transmit_data_length, m_transmitter_callback, this);

					return true;
				}
				catch
				{
					// stop thread because of the unexpected error
					m_stop_requested = true;
					m_thread_event.Set();
				}
			}

			return false;
		}

		public void ConnectionLost()
		{
		}


		/// <summary>
		/// Gets currently connected client ID
		/// </summary>
		/// <returns></returns>
		public UInt32 GetClientID()
		{
			return m_client_unique_id;
		}

		/// <summary>
		/// Closes connection and resets communication
		/// </summary>
		private void CloseConnection()
		{
			m_serial_port.Close();
			m_serial_port = null;
			if (m_sender_lock.CurrentCount == 0)
			{
				try
				{
					m_sender_lock.Release();
				}
				catch
				{
				}
			}
		}
		#endregion

		#region · Thread functions ·

		/// <summary>
		/// Main thread function
		/// </summary>
		public void Run()
		{
			bool event_occured;
			UInt16 crc;
			int received_data_pos;
			int received_packet_pos;

			// init SLIPs
			m_receiver_slip.ResetDecoder();

			try
			{
				m_serial_port = new SerialPort(PortName, BaudRate, Parity.None, 8, StopBits.One);
				m_serial_port.ReadTimeout = 50;
				m_serial_port.WriteTimeout = 50;
				m_serial_port.ReadBufferSize = 4096;
				m_serial_port.Open();
			}
			catch
			{
				return;
			}

			// init
			m_upstream_bytes = 0;
			m_downstream_bytes = 0;
			received_data_pos = 0;
			received_packet_pos = 0;

			// start data reception
			m_read_pending = true;
			m_serial_port.BaseStream.BeginRead(m_receive_data_buffer, 0, m_receive_data_buffer.Length, m_receiver_callback, this);

			// communication loop
			while (!m_stop_requested)
			{
				// wait for event
				event_occured = m_thread_event.WaitOne(ThreadWaitTimeout);

				// exit loop if thread must be stopped
				if (m_stop_requested)
					break;

				// process received data
				if (m_received_data_length > 0)
				{
					received_data_pos = 0;
					while (received_data_pos < m_received_data_length)
					{
						// SLIP decode data
						if (m_receiver_slip.DecodeBlock(m_receive_data_buffer, ref received_data_pos, m_received_data_length, m_receive_packet_buffer, ref received_packet_pos))
						{
							// packet decoded -> check CRC
							if (received_packet_pos > (Marshal.SizeOf(typeof(PacketBase)) + PacketConstants.PacketCRCLength))
							{
								m_received_packet_length = received_packet_pos;
								crc = CRC16.CalculateForBlock(CRC16.InitValue, m_receive_packet_buffer, m_received_packet_length - PacketConstants.PacketCRCLength);

								if (ByteAccess.LowByte(crc) == m_receive_packet_buffer[m_received_packet_length - 2] && ByteAccess.HighByte(crc) == m_receive_packet_buffer[m_received_packet_length - 1])
								{
									// CRC OK -> continue processing received packet
									PacketType type = (PacketType)m_receive_data_buffer[PacketConstants.TypeOffset];

									// update statistics
									Interlocked.Add(ref m_upstream_bytes, m_received_data_length);

									// process received data
									if (m_received_packet_length <= PacketConstants.PacketMaxLength)
										m_communication_manager.StoreReceivedPacket(m_communication_channel_index, m_receive_packet_buffer, (byte)m_received_packet_length);
								}
							}
							// restart packet decoding
							received_packet_pos = 0;
						}

						m_read_pending = false;
					}
				}

				// restart reception
				if (!m_read_pending)
				{
					try
					{
						m_received_data_length = 0;
						m_read_pending = true;
						m_serial_port.BaseStream.BeginRead(m_receive_data_buffer, 0, m_receive_data_buffer.Length, m_receiver_callback, this);
					}
					catch
					{
						// stop thread because of the unexpected error
						m_stop_requested = true;
						m_thread_event.Set();
					}
				}
			}

			// close socket
			m_serial_port.Close();
			m_serial_port = null;

			// thread is finished
			m_thread_stopped.Set();
		}
		#endregion

		#region · Callback functions ·

		/// <summary>
		/// Callback function for data received event
		/// </summary>
		/// <param name="ar"></param>
		private static void ReceiveCallback(IAsyncResult ar)
		{
			// Retrieve the communication object from the state object.
			UARTCommunicator communicator = (UARTCommunicator)ar.AsyncState;

			if (communicator == null)
				return;

			try
			{
				// Retrieve serial port
				SerialPort serial_port = communicator.m_serial_port;
				if (serial_port != null)
				{
					// Read data from the remote device.
					int bytes_read = serial_port.BaseStream.EndRead(ar);

					if (bytes_read > 0)
					{
						communicator.m_received_data_length = bytes_read;
						communicator.m_thread_event.Set();
					}
					else
					{
						communicator.m_read_pending = false;
						communicator.m_thread_event.Set();
					}
				}
			}
			catch
			{
				// stop thread because of the unexpected error
				communicator.m_stop_requested = true;
				communicator.m_thread_event.Set();
			}
		}

		/// <summary>
		/// Callback function for sender ready event
		/// </summary>
		/// <param name="ar"></param>
		private static void SendCallback(IAsyncResult ar)
		{
			// Retrieve the communication object from the state object.
			UARTCommunicator communicator = (UARTCommunicator)ar.AsyncState;

			try
			{
				// Retrieve serial port
				SerialPort serial_port = communicator.m_serial_port;

				// Complete sending the data to the remote device.
				serial_port.BaseStream.EndWrite(ar);

				Interlocked.Add(ref communicator.m_downstream_bytes, communicator.m_transmit_data_length);

				// Signal that all bytes have been sent.
				communicator.m_sender_lock.Release();

				// signal communication manager about the freed buffer
				communicator.m_communication_manager.GenerateEvent();
			}
			catch
			{
				// stop thread because of the unexpected error
				communicator.m_stop_requested = true;
				communicator.m_thread_event.Set();
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
			m_upstream_member = in_realtime_object.MemberCreate("UARTUpStream", RealtimeObjectMember.MemberType.Int);
			m_downstream_member = in_realtime_object.MemberCreate("UARTDownStream", RealtimeObjectMember.MemberType.Int);
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

		#endregion

	}
}
