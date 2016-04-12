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
// Communication class over UDP
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.Helpers;
using CommonClassLibrary.RealtimeObjectExchange;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CommonClassLibrary.DeviceCommunication
{
	public class UDPCommunicator : IDisposable, ICommunicationInterface
	{
		#region · Constants ·
		public int ReceiveBufferSize = 512;
		public int TransmitBufferSize = 512;
		public int DeviceAnnounceTimeout = 3000;
		#endregion

		#region · Types ·
		class DeviceInfo
		{
			public string Name;
			public UInt32 UniqueID;
			public IPAddress Address;
			public bool HostInfoPending;

			public DateTime LastInfoTimestamp;


			public DeviceInfo(PacketDeviceInfo in_packet)
			{
				Name = in_packet.DeviceName;
				UniqueID = in_packet.UniqueID;
				try
				{
					Address = new IPAddress(IPAddress.HostToNetworkOrder((int)in_packet.Address));
				}
				catch
				{
					Address = IPAddress.None;
				}
				HostInfoPending = false;
				LastInfoTimestamp = DateTime.Now;
			}

			public override string ToString()
			{
				return Name + " (" + UniqueID.ToString() + ") [" + Address.ToString() + "]";
			}

			public override bool Equals(object obj)
			{
				if (obj == null) return false;

				DeviceInfo info = obj as DeviceInfo;

				if (info == null)
					return false;
				else
					return Equals(info);
			}
			public override int GetHashCode()
			{
				return (int)UniqueID;
			}
			public bool Equals(DeviceInfo in_other)
			{
				if (in_other == null)
					return false;

				return (this.UniqueID.Equals(in_other.UniqueID));
			}
		}
		#endregion

		#region · Data members ·
		private bool m_is_disposed;
		private volatile bool m_stop_requested; // external request to stop the thread
		private ManualResetEvent m_thread_stopped;  // Worker thread sets this event when it is stopped
		private AutoResetEvent m_event_occured;
		private Thread m_thread;
		private byte[] m_receive_buffer;
		private int m_received_data_length;
		private byte[] m_transmit_buffer;
		private CommunicationManager.ReceivedPacketProcessorCallback m_received_packet_callback;
		private DateTime m_timestamp;
		private SemaphoreSlim m_sender_lock;
		private byte m_communication_channel_index;

		private Socket m_client;
		private EndPoint m_receiver_endpoint;
		private IPAddress m_local_ip_address = null;
		private UInt32 m_client_unique_id;

		private List<DeviceInfo> m_detected_devices = new List<DeviceInfo>();

		private volatile int m_upstream_bytes;
		private volatile int m_downstream_bytes;
		private RealtimeObjectMember m_upstream_member;
		private RealtimeObjectMember m_downstream_member;

		#endregion

		#region · Properties ·
		public int UDPTransmiterPort { get; set; }
		public int UDPReceiverPort { get; set; }
		public int UDPDeviceReceiverPort { get; set; }
		#endregion

		#region · Constructor ·
		/// <summary>
		/// Default constructor
		/// </summary>
		public UDPCommunicator()
		{
			m_stop_requested = false;
			m_thread_stopped = new ManualResetEvent(false);
			m_sender_lock = new SemaphoreSlim(1, 1);
			m_event_occured = new AutoResetEvent(false);
			m_thread = null;
			m_receive_buffer = new byte[ReceiveBufferSize];
			m_received_packet_callback = null;
			m_received_data_length = 0;
			m_transmit_buffer = new byte[TransmitBufferSize];
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

		public void SetReceivedPacketProcessor(byte in_communication_channel_index, CommunicationManager.ReceivedPacketProcessorCallback in_received_data_processor)
		{
			m_communication_channel_index = in_communication_channel_index;
			m_received_packet_callback = in_received_data_processor;
		}

		public void CreateRealtimeObjects(RealtimeObject in_realtime_object)
		{
			m_upstream_member = in_realtime_object.MemberCreate("UDPUpStream", RealtimeObjectMember.MemberType.Float);
			m_downstream_member = in_realtime_object.MemberCreate("UDPDownStream", RealtimeObjectMember.MemberType.Float);
		}


		public void UpdateCommunicationStatistics(int in_ellapsed_millisec)
		{
			float upstream = m_upstream_bytes * 1000 / in_ellapsed_millisec;
			m_upstream_bytes = 0;
			float downstream = m_downstream_bytes * 1000 / in_ellapsed_millisec;
			m_downstream_bytes = 0;

			m_upstream_member.Write(upstream);
			m_downstream_member.Write(downstream);
		}


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
			m_event_occured.Reset();

			// create worker thread instance
			m_thread = new Thread(new ThreadStart(Run));
			m_thread.Name = "UDP Communication Thread";   // looks nice in Output window

			// start thread
			m_thread.Start();
		}

		/// <summary>
		/// Stops FlightGear Thread
		/// </summary>
		public void Stop()
		{
			if (m_thread != null && m_thread.IsAlive)  // thread is active
			{
				// set event "Stop"
				m_stop_requested = true;
				m_event_occured.Set();

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
		/// Send packet. If sender is busy, than it waits 100ms before reporting send failure.
		/// </summary>
		/// <param name="in_packet">Packet to send</param>
		/// <param name="in_packet_length">Length of the packet in bytes</param>
		/// <returns>True if sending started, false if sending can't be started</returns>
		public bool SendPacket(byte[] in_packet, int in_packet_length)
		{
			EndPoint transmitter_endpoint;

			if (m_client == null)
				return false;

			// get destination endpoint
			lock(m_detected_devices)
			{
				if (m_detected_devices.Count > 0)
				{
					transmitter_endpoint = new IPEndPoint(m_detected_devices[0].Address, UDPDeviceReceiverPort);
				}
				else
					return false;
			}

			// accuire sender lock
			if (m_sender_lock.Wait(100))
			{
				// Begin sending the data to the remote device.
				try
				{
					m_client.BeginSendTo(in_packet, 0, in_packet_length, 0, transmitter_endpoint, new AsyncCallback(SendCallback), this);

					return true;
				}
				catch
				{
					//TODO: error handling
				}
			}

			return false;
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
			m_client.Close();
			m_client = null;
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
			int i;
			UInt16 crc;

			// determine local IP address
			IPAddress[] resolved_ip_address = Dns.GetHostAddresses("localhost");

			// find first IPv4 address
			for (i = 0; i < resolved_ip_address.Length; i++)
			{
				if (resolved_ip_address[i].AddressFamily == AddressFamily.InterNetwork)
					m_local_ip_address = resolved_ip_address[i];
			}

			// create endpoints
			m_receiver_endpoint = new IPEndPoint(IPAddress.Any, UDPReceiverPort);

			// init socket
			m_timestamp = DateTime.MinValue;

			m_client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_client.Blocking = false;
			m_client.EnableBroadcast = true;
			m_client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			m_client.Bind(m_receiver_endpoint);

			// init
			m_upstream_bytes = 0;
			m_downstream_bytes = 0;

			// start data reception
			m_client.BeginReceive(m_receive_buffer, 0, m_receive_buffer.Length, 0, new AsyncCallback(ReceiveCallback), this);

			// communication loop
			while (!m_stop_requested)
			{
				// wait for event
				event_occured = m_event_occured.WaitOne(100);

				// exit loop if thread must be stopped
				if (m_stop_requested)
					break;

				// process received data
				if (m_received_data_length > 0)
				{
					if (m_received_data_length > 2)
					{
						// check CRC
						crc = CRC16.CalculateForBlock(CRC16.InitValue, m_receive_buffer, m_received_data_length - 2);

						if (ByteAccess.LowByte(crc) == m_receive_buffer[m_received_data_length - 2] && ByteAccess.HighByte(crc) == m_receive_buffer[m_received_data_length - 1])
						{
							// CRC OK -> continue processing received packet
							PacketType type = (PacketType)m_receive_buffer[PacketConstants.TypeOffset];

							// update statistics
							Interlocked.Add(ref m_upstream_bytes, m_received_data_length);

							switch (type)
							{
								case PacketType.CommDeviceInfo:
									{
										PacketDeviceInfo device_info;

										// process device info
										device_info = (PacketDeviceInfo)RawBinarySerialization.DeserializeObject(m_receive_buffer, typeof(PacketDeviceInfo));

										// add device info into the collection
										DeviceInfo current_device_info = m_detected_devices.Find(x => x.UniqueID == device_info.UniqueID);

										if (current_device_info != null)
										{
											current_device_info.HostInfoPending = true;
											current_device_info.LastInfoTimestamp = DateTime.Now;
										}
										else
										{
											DeviceInfo new_device = new DeviceInfo(device_info);
											new_device.HostInfoPending = true;

											if (new_device.Address != IPAddress.None)
											{
												m_detected_devices.Add(new_device);
												if(m_detected_devices.Count == 1)
												{
													m_client_unique_id = m_detected_devices[0].UniqueID;
												}
											}
										}
									}
									break;

								default:
									// process received data
									if(m_received_data_length <= PacketConstants.PacketMaxLength)
										m_received_packet_callback(m_communication_channel_index, m_receive_buffer, (byte)m_received_data_length);
									break;
							}
						}
					}

					// restart reception
					try
					{
						m_received_data_length = 0;
						m_client.BeginReceive(m_receive_buffer, 0, m_receive_buffer.Length, 0, new AsyncCallback(ReceiveCallback), this);
					}
					catch
					{
						// TODO: error handling
					}
				}

				// send host info for devices
				for (i = 0; i < m_detected_devices.Count; i++)
				{
					if (m_detected_devices[i].HostInfoPending)
					{
						if (m_sender_lock.Wait(0))
						{
							// create host info packet
							PacketHostInfo info = new PacketHostInfo();

							info.Address = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(m_local_ip_address.GetAddressBytes(), 0));

							byte[] packet = RawBinarySerialization.SerializeObject(info);

							// send host info packet
							EndPoint transmitter_endpoint = new IPEndPoint(m_detected_devices[i].Address, UDPDeviceReceiverPort);

							InternalPacketSend(transmitter_endpoint, packet);

							m_detected_devices[i].HostInfoPending = false;
						}
					}
				}

				// delete unreachable devices from the list
				DateTime current_timestamp = DateTime.Now;
				i = 0;
				while(i<m_detected_devices.Count)
				{
					// if devlice is silent for a while -> remove from the list
					if( (current_timestamp - m_detected_devices[i].LastInfoTimestamp).TotalMilliseconds > DeviceAnnounceTimeout)
					{
						m_detected_devices.RemoveAt(i);
					}
					else
					{
						i++;
					}
				}
			}

			// close socket
			m_client.Close();

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
			UDPCommunicator communicator = (UDPCommunicator)ar.AsyncState;
			try
			{
				// Retrieve socket
				Socket client = communicator.m_client;

				// Read data from the remote device.
				int bytes_read = client.EndReceive(ar);

				if (bytes_read > 0)
				{
					communicator.m_received_data_length = bytes_read;
					communicator.m_event_occured.Set();
				}
				else
				{
				}
			}
			catch
			{
				// TODO: Error handling
			}
		}

		/// <summary>
		/// Callback function for sender ready event
		/// </summary>
		/// <param name="ar"></param>
		private static void SendCallback(IAsyncResult ar)
		{
			// Retrieve the communication object from the state object.
			UDPCommunicator communicator = (UDPCommunicator)ar.AsyncState;

			try
			{
				// Retrieve socket
				Socket client = communicator.m_client;

				// Complete sending the data to the remote device.
				int bytes_sent = client.EndSend(ar);

				Interlocked.Add(ref communicator.m_downstream_bytes, bytes_sent);

				// Signal that all bytes have been sent.
				communicator.m_sender_lock.Release();
			}
			catch
			{
				// TODO: Error handling
			}
		}

		#endregion

		#region · Internal functions ·

		/// <summary>
		/// Send packets over the opened socket (internal function only, no error checking, locking, etc. mechanism is implemented)
		/// Only CRC is calculated before sending packet
		/// </summary>
		/// <param name="in_endpoint"></param>
		/// <param name="in_packet"></param>
		private void InternalPacketSend(EndPoint in_endpoint, byte[] in_packet)
		{
			// copy packet to the buffer
			in_packet.CopyTo(m_transmit_buffer, 0);

			// calculate and store CRC
			UInt16 crc;
			crc = CRC16.CalculateForBlock(CRC16.InitValue, in_packet, in_packet.Length);

			m_transmit_buffer[in_packet.Length] = ByteAccess.LowByte(crc);
			m_transmit_buffer[in_packet.Length + 1] = ByteAccess.HighByte(crc);

			// send packet
			m_client.BeginSendTo(m_transmit_buffer, 0, in_packet.Length + PacketConstants.PacketCRCLength, 0, in_endpoint, new AsyncCallback(SendCallback), this);
		}
		#endregion

	}
}
