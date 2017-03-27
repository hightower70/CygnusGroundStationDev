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
using System.Diagnostics;
using System.IO;
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
		const uint InvalidDeviceUniqueID = 0xffffffff;

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
		private List<ICommunicator> m_communication_interfaces;

		// host status
		private DateTime m_host_heartbeat_timestamp;
		private int m_packet_counter;

		// device status
		private bool m_device_connected;
		private string m_device_name;
		private UInt32 m_device_unique_id;
		private DateTime m_last_device_heartbeat_timestamp;

		// realtime communication objects
		private DateTime m_communication_state_update_timestamp;
		private RealtimeObject m_communication_state_object;
		private RealtimeObjectMember m_device_connected_member;
		private RealtimeObjectMember m_device_name_member;

		private RealtimeObject m_device_heartbeat_object;
		private RealtimeObjectMember m_cpu_load_member;

		// singleton member
		private static CommunicationManager m_default;

		// file transfer
		private FileTransferManager m_file_transfer_manager;

		// message logging variables
		private StreamWriter m_packet_log_writter;
		private long m_packet_log_timestamp;

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

			if (m_packet_log_writter != null)
				m_packet_log_writter.Close();

			m_packet_log_writter = null;

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
			m_communication_interfaces = new List<ICommunicator>();
			m_packet_log_writter = null;
			m_file_transfer_manager = new FileTransferManager();
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

		/// <summary>
		///  Gets packet receiver queue for accessing/processing received packets
		/// </summary>
		public PacketQueue ReceiverQueue
		{
			get { return m_receiver_queue; }
		}

		/// <summary>
		/// Gets file transfer manager
		/// </summary>
		public FileTransferManager FileTransfer
		{
			get { return m_file_transfer_manager; }
		}

		/// <summary>
		/// Gets unique ID of the connected device
		/// </summary>
		public UInt32 ConnectedDeviceUniqueID
		{
			get { return m_device_unique_id; }
		}
		#endregion

		#region · Singleton members ·

		/// <summary>
		/// Singleton instance
		/// </summary>
		public static CommunicationManager Default
		{
			get
			{
				if (m_default == null)
				{
					m_default = new CommunicationManager();
				}

				return m_default;
			}
		}
		#endregion

		#region · Public members ·

		/// <summary>
		/// Creates realtime object used for communication state
		/// </summary>
		public void CreateRealtimeObjects()
		{
			// create file transfer state object
			m_file_transfer_manager.CreateRealtimeObjects();
			
			// create device heart beat object
			m_device_heartbeat_object = RealtimeObjectStorage.Default.ObjectCreate("DeviceHeartBeat");
			m_cpu_load_member = m_device_heartbeat_object.MemberCreate("CPULoad", RealtimeObjectMember.MemberType.Int);
			m_device_heartbeat_object.ObjectCreateEnd();

			// create communication state object
			m_communication_state_object = RealtimeObjectStorage.Default.ObjectCreate("DeviceCommunicationState");
			m_device_connected_member = m_communication_state_object.MemberCreate("DeviceConnected", RealtimeObjectMember.MemberType.Int);
			m_device_name_member = m_communication_state_object.MemberCreate("DeviceName", RealtimeObjectMember.MemberType.String);

			// create members from communicator plug-ins
			for (int i = 0; i < m_communication_interfaces.Count; i++)
			{
				m_communication_interfaces[i].CreateRealtimeObjects(m_communication_state_object);
			}

			m_communication_state_object.ObjectCreateEnd();
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
			m_device_name = string.Empty;
			m_device_unique_id = InvalidDeviceUniqueID;
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

			// stop communication
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
		public void AddCommunicator(ICommunicator in_communication_interface)
		{
			byte interface_index;

			lock (m_communication_interfaces)
			{
				m_communication_interfaces.Add(in_communication_interface);
				interface_index = (byte)(m_communication_interfaces.Count - 1);
			}

			in_communication_interface.AddToManager(this, interface_index);
		}

		/// <summary>
		/// Pushes packet into the transmitter queue to sending on all interfaces
		/// </summary>
		/// <param name="in_packet"></param>
		public void SendPacket(PacketBase in_packet)
		{
			byte counter = (byte)((UInt32)(Interlocked.Increment(ref m_packet_counter)) & 0xff);

			in_packet.SetPacketCounter(counter);

			m_transmitter_queue.Push(InvalidInterface, in_packet);

			// notify thread about the new packet to send
			m_thread_event.Set();
		}

		/// <summary>
		/// Pushes packet with additional data bytes into the transmitter queue to send on all interfaces
		/// </summary>
		/// <param name="in_packet_header"></param>
		/// <param name="in_packet_data"></param>
		public void SendPacket(PacketBase in_packet_header, byte[] in_packet_data)
		{
			byte counter = (byte)((UInt32)(Interlocked.Increment(ref m_packet_counter)) & 0xff);

			in_packet_header.SetPacketCounter(counter);

			m_transmitter_queue.Push(InvalidInterface, in_packet_header, in_packet_data);

			// notify thread about the new packet to send
			m_thread_event.Set();
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

		#region · Packet logging function ·

		/// <summary>
		/// Creates packet log file
		/// </summary>
		/// <param name="in_log_file_name"></param>
		public void PacketLogCreate(string in_log_file_name)
		{
			m_packet_log_writter = new StreamWriter(in_log_file_name);
			m_packet_log_timestamp = Stopwatch.GetTimestamp();
		}

		/// <summary>
		/// Writtes packet to log
		/// </summary>
		/// <param name="in_packet"></param>
		public void PacketLogWrite(string in_direction, PacketBase in_packet)
		{
			string log_message;

			if (m_packet_log_writter == null)
				return;

			long last_timestamp = m_packet_log_timestamp;

			m_packet_log_timestamp = Stopwatch.GetTimestamp();

			log_message = in_direction + " " + ((int)((m_packet_log_timestamp - last_timestamp) * (1000.0 / Stopwatch.Frequency))).ToString() + " " + in_packet.ToString();
			m_packet_log_writter.WriteLine(log_message);
			Debug.WriteLine(log_message);
		}

		/// <summary>
		/// Closes packet log file
		/// </summary>
		public void PacketLogClose()
		{
			if (m_packet_log_writter != null)
				m_packet_log_writter.Close();

			m_packet_log_writter = null;
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

			m_communication_state_update_timestamp = DateTime.Now;

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

				// Update Comm State realtime object
				UpdateCommunicationState();

				// handle file timeout
				m_file_transfer_manager.FileOperationTimeOutHandler();

				// handle communication lost event
				if (m_device_connected && (DateTime.Now - m_last_device_heartbeat_timestamp).TotalMilliseconds > DevicePacketTimeout)
				{
					m_device_connected = false;
					m_device_name = string.Empty;
					m_device_unique_id = InvalidDeviceUniqueID;
				}

				// send host heartbeat
				ellapsed_time = (DateTime.Now - m_host_heartbeat_timestamp).TotalMilliseconds;
				if (m_device_connected && ellapsed_time > HostHeartbeatPeriod)
				{
					m_host_heartbeat_timestamp = m_host_heartbeat_timestamp.AddMilliseconds(ellapsed_time);

					Debug.WriteLine("Host heartbeat");

					PacketHostHeartbeat packet = new PacketHostHeartbeat();

					SendPacket(packet);
				}
			}

			// thread is finished
			m_thread_stopped.Set();
		}

		/// <summary>
		/// Generates thread event
		/// </summary>
		/// <param name=""></param>
		public void GenerateEvent()
		{
			m_thread_event.Set();
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
					Debug.WriteLine("HandlePacketSending");

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
			bool last_device_connected;

			// return when there is no packet to process
			if (m_receiver_queue.IsEmpty())
				return;

			if (!m_receiver_queue.PeekPacketValid())
				return;

			// update connection state
			last_device_connected = m_device_connected;
			m_device_connected = true;
			m_last_device_heartbeat_timestamp = DateTime.Now;

			// get device name if it is just connected
			if (!last_device_connected)
			{
				PacketDeviceNameRequest packet = new PacketDeviceNameRequest();

				SendPacket(packet);
			}

			// get packet type
			PacketType packet_type = m_receiver_queue.PeekPacketType();

			if ((packet_type & PacketType.FlagSystem) != 0)
			{
				switch ((PacketType)((int)packet_type & PacketConstants.PacketClassMask))
				{
					case PacketType.ClassComm:
						// process sytem packets
						switch (packet_type)
						{
							// process device heartbeat packet
							case PacketType.CommDeviceHeartbeat:
								ProcessDeviceHeartbeatPacket();
								break;

							// process device name response
							case PacketType.CommDeviceNameResponse:
								ProcessDeviceNameResponsePacket();
								break;

						}
						break;

					case PacketType.ClassFile:
						m_file_transfer_manager.ProcessResponsePacket(packet_type);
						break;
				}
			}
			else
			{
				// process realtime object packet
				byte[] binary_packet;
				byte binary_packet_length;
				byte interface_index;
				DateTime timestamp;

				// pop packet
				m_receiver_queue.PopBegin(out binary_packet, out binary_packet_length, out interface_index, out timestamp);

				// process packet content
				RealtimeObject obj = RealtimeObjectStorage.Default.GetObjectByPacketID((byte)packet_type);
				if (obj != null)
					obj.Update(binary_packet, binary_packet_length, interface_index, timestamp);

				// finish popping
				m_receiver_queue.PopEnd();
			}
		}

		/// <summary>
		/// Updates communication state realtime object
		/// </summary>
		private void UpdateCommunicationState()
		{
			double ellapsed_time;

			ellapsed_time = (DateTime.Now - m_communication_state_update_timestamp).TotalMilliseconds;
			if (ellapsed_time >= 1000)
			{
				m_communication_state_update_timestamp = m_communication_state_update_timestamp.AddMilliseconds(ellapsed_time);

				// start object update
				m_communication_state_object.UpdateBegin();

				m_device_connected_member.Write(Convert.ToInt32(m_device_connected));
				m_device_name_member.Write(m_device_name);

				for (int i = 0; i < m_communication_interfaces.Count; i++)
				{
					m_communication_interfaces[i].UpdateCommunicationStatistics((int)ellapsed_time);
				}

				m_communication_state_object.UpdateEnd();
			}
		}

		#endregion

		#region · Communication packet processors ·

		/// <summary>
		/// Processes device name response packet
		/// </summary>
		private void ProcessDeviceNameResponsePacket()
		{
			PacketDeviceNameResponse packet = (PacketDeviceNameResponse)m_receiver_queue.Pop(typeof(PacketDeviceNameResponse));

			m_device_name = packet.DeviceName;
			m_device_unique_id = packet.UniqueID;
		}

		/// <summary>
		/// Processes heart beat packet
		/// </summary>
		private void ProcessDeviceHeartbeatPacket()
		{
			PacketDeviceHeartbeat packet = (PacketDeviceHeartbeat)m_receiver_queue.Pop(typeof(PacketDeviceHeartbeat));

			m_device_heartbeat_object.UpdateBegin();

			m_cpu_load_member.Write(packet.CPULoad);

			m_device_heartbeat_object.UpdateEnd();
		}

		#endregion

		}
}
