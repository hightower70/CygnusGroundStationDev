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
	public class UDPCommunicator : IDisposable, ICommunicator
	{
		#region · Constants ·
		private int ReceiveBufferSize = 512;
		private int TransmitBufferSize = 512;
		private int DeviceAnnounceTimeout = 3000;
		private const int ThreadWaitTimeout = 100;

		#endregion

		#region · Types ·
		class DeviceInfo
		{
			public string Name;
			public UInt32 UniqueID;
			public IPAddress Address;
			public bool HostAnnouncePending;

			public DateTime LastInfoTimestamp;


			public DeviceInfo(PacketDeviceAnnounce in_packet)
			{
				Name = in_packet.DeviceName;
				UniqueID = in_packet.UniqueID;
				try
				{
					Address = new IPAddress((UInt32)IPAddress.HostToNetworkOrder((int)in_packet.Address));
				}
				catch
				{
					Address = IPAddress.None;
				}
				HostAnnouncePending = false;
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
		private CommunicationManager m_communication_manager;
		private byte m_communication_channel_index;
		private bool m_is_disposed;

		private Thread m_thread;
		private volatile bool m_stop_requested; // external request to stop the thread
		private ManualResetEvent m_thread_stopped;  // Worker thread sets this event when it is stopped
		private AutoResetEvent m_thread_event;
		private SemaphoreSlim m_sender_lock;

		private byte[] m_receive_buffer;
		private int m_received_data_length;
		private byte[] m_transmit_buffer;
		private DateTime m_timestamp;

		private Socket m_client;
		private EndPoint m_receiver_endpoint;

		private List<DeviceInfo> m_detected_devices = new List<DeviceInfo>();
		private DeviceInfo m_current_device = null;

		private volatile int m_upstream_bytes;
		private volatile int m_downstream_bytes;
		private RealtimeObjectMember m_upstream_member;
		private RealtimeObjectMember m_downstream_member;

		#endregion

		#region · Properties ·
		public int UDPLocalPort { get; set; }
		public int UDPRemotePort { get; set; }
		#endregion

		#region · Constructor ·
		/// <summary>
		/// Default constructor
		/// </summary>
		public UDPCommunicator()
		{
			m_thread_stopped = new ManualResetEvent(false);
			m_sender_lock = new SemaphoreSlim(1, 1);
			m_thread_event = new AutoResetEvent(false);
			m_stop_requested = false;
			m_thread = null;
			m_receive_buffer = new byte[ReceiveBufferSize];
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
			m_upstream_member = in_realtime_object.MemberCreate("UDPUpStream", RealtimeObjectMember.MemberType.Int);
			m_downstream_member = in_realtime_object.MemberCreate("UDPDownStream", RealtimeObjectMember.MemberType.Int);
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
			lock (m_detected_devices)
			{
				m_current_device = null;
			}
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
			m_thread_event.Reset();

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
				if (m_current_device != null)
				{
					transmitter_endpoint = new IPEndPoint(m_current_device.Address, UDPRemotePort);
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
			UInt32 client_id = 0;

			lock (m_detected_devices)
			{
				if (m_current_device != null)
					client_id = m_current_device.UniqueID;
			}

			return client_id;
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
			AsyncCallback receiver_callback = new AsyncCallback(ReceiveCallback);

			// create endpoints
			m_receiver_endpoint = new IPEndPoint(IPAddress.Any, UDPLocalPort);

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
			m_client.BeginReceive(m_receive_buffer, 0, m_receive_buffer.Length, 0, receiver_callback, this);

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

							// process received packet
							switch (type)
							{
								// process device annnounce packet
								case PacketType.CommDeviceAnnounce:
									{
										PacketDeviceAnnounce device_info;

										// get announce packet
										device_info = (PacketDeviceAnnounce)RawBinarySerialization.DeserializeObject(m_receive_buffer, typeof(PacketDeviceAnnounce));

										// check if it is already on the list of active devices
										DeviceInfo current_device_info = m_detected_devices.Find(x => x.UniqueID == device_info.UniqueID);

										if (current_device_info != null)
										{
											// device is on the list
											current_device_info.HostAnnouncePending = true;
											current_device_info.LastInfoTimestamp = DateTime.Now;
										}
										else
										{
											DeviceInfo new_device = new DeviceInfo(device_info);
											new_device.HostAnnouncePending = true;

											if (new_device.Address != IPAddress.None)
											{
												m_detected_devices.Add(new_device);
												if(m_detected_devices.Count == 1)
												{
													lock (m_detected_devices)
													{
														m_current_device = m_detected_devices[0];
													}
												}
											}
										}
									}
									break;

								default:
									// process received data
									if(m_received_data_length <= PacketConstants.PacketMaxLength)
										m_communication_manager.StoreReceivedPacket(m_communication_channel_index, m_receive_buffer, (byte)m_received_data_length);
									break;
							}
						}
					}

					// restart reception
					try
					{
						m_received_data_length = 0;
						m_client.BeginReceive(m_receive_buffer, 0, m_receive_buffer.Length, 0, receiver_callback, this);
					}
					catch
					{
						// TODO: error handling
					}
				}

				// send host announce for devices
				for (i = 0; i < m_detected_devices.Count; i++)
				{
					IPAddress address_mask = new IPAddress(new byte[] {255,255,255,0});

					if (m_detected_devices[i].HostAnnouncePending)
					{
						IPAddress local_ip = GetLocalIPAddress(m_detected_devices[i].Address, address_mask);

						if (m_sender_lock.Wait(0))
						{
							// create host info packet
							PacketHostAnnounce info = new PacketHostAnnounce();

							info.Address = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(local_ip.GetAddressBytes(), 0));

							byte[] packet = RawBinarySerialization.SerializeObject(info);

							// send host info packet
							EndPoint transmitter_endpoint = new IPEndPoint(m_detected_devices[i].Address, UDPRemotePort);

							InternalPacketSend(transmitter_endpoint, packet);

							m_detected_devices[i].HostAnnouncePending = false;
						}
					}
				}

				// delete unreachable or connectecd devices from the list
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
			m_client.Shutdown(SocketShutdown.Both);
			Thread.Sleep(10);
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
					communicator.m_thread_event.Set();
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

				// signal communication manager about the freed buffer
				communicator.m_communication_manager.GenerateEvent();
			}
			catch
			{
				// TODO: Error handling
			}
		}

		#endregion

		#region · Internal functions ·

		/// <summary>
		/// Gets local ip address of the given subnet
		/// </summary>
		/// <param name="in_network_address">Network address</param>
		/// <param name="in_network_mask">Network mask</param>
		/// <returns></returns>
		private IPAddress GetLocalIPAddress(IPAddress in_network_address, IPAddress in_network_mask)
		{
			IPAddress local_address = null;
			byte[] network_address = in_network_address.GetAddressBytes();
			byte[] network_mask = in_network_mask.GetAddressBytes();
			byte[] ip_address;

			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					ip_address = ip.GetAddressBytes();

					if(((network_address[0] & network_mask[0]) == (ip_address[0] & network_mask[0])) &&
						((network_address[1] & network_mask[1]) == (ip_address[1] & network_mask[1])) &&
						((network_address[2] & network_mask[2]) == (ip_address[2] & network_mask[2])) &&
						((network_address[3] & network_mask[3]) == (ip_address[3] & network_mask[3])))
					{
						local_address = ip;
					}
				}
			}

			return local_address;
		}

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
