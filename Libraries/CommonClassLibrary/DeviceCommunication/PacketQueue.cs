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
// Packet queue for device communication. 
// The push operatin is thread safe the pop operation has sigle thread only implementation. 
// The queue has serializing and deserializing (to/from c# class) function built in
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.Helpers;
using System;
using System.Runtime.InteropServices;

namespace CommonClassLibrary.DeviceCommunication
{
	public class PacketQueue
	{

		#region · Types ·

		const int InvalidPacketIndex = -1;

		enum PacketBufferState
		{
			Empty,
			Reserved,
			Valid,
		}

		class PacketBuffer
		{
			public byte[] Buffer;
			public byte Length;
			public PacketBufferState State;
			public byte Interface;
			public DateTime Timestamp;

			public PacketBuffer()
			{
				Buffer = new byte[PacketConstants.PacketMaxLength];
				Length = 0;
				State = PacketBufferState.Empty;
				Interface = CommunicationManager.InvalidInterface;
				Timestamp = DateTime.MinValue;
			}
		}
		#endregion

		#region · Data members ·
		private PacketBuffer[] m_packets;
		private int m_push_index;
		private int m_pop_index;
		#endregion

		#region · Constructor ·
		public PacketQueue(int in_max_packet_count)
		{
			m_packets = new PacketBuffer[in_max_packet_count];

			for(int i =0;i<m_packets.Length;i++)
			{
				m_packets[i] = new PacketBuffer();
			}

			m_push_index = 0;
			m_pop_index = 0;
		}
		#endregion

		#region · Push/Pop routines ·

		/// <summary>
		/// Pushes raw (byte array) packet without calculating CRC
		/// </summary>
		/// <param name="in_packet"></param>
		/// <param name="in_packet_length"></param>
		/// <returns></returns>
		public bool Push(byte in_interface, byte[] in_packet, byte in_packet_length)
		{
			int packet_index;

			// reserve storage for packet
			packet_index = ReservePushBuffer();
			if (packet_index == InvalidPacketIndex)
				return false;

			// copy packet content to the buffer
			Array.Copy(in_packet, m_packets[packet_index].Buffer, in_packet_length);

			// change buffer state
			m_packets[packet_index].Interface = in_interface;
			m_packets[packet_index].Length = in_packet_length;
			m_packets[packet_index].Timestamp = DateTime.Now;
			m_packets[packet_index].State = PacketBufferState.Valid;

			return true;
		}

		/// <summary>
		/// Serializes and pushes object to the queue with CRC calculation
		/// </summary>
		/// <param name="in_object">Object to push (must be Serializable)</param>
		/// <returns>True if success, false if there is no space left in the queue</returns>
		public bool Push(byte in_interface, object in_object)
		{
			int packet_index;
			UInt16 crc;

			// reserve storage for packet
			packet_index = ReservePushBuffer();
			if (packet_index == InvalidPacketIndex)
				return false;

			// serialize packet
			byte[] raw_packet_buffer = m_packets[packet_index].Buffer;
			GCHandle handle = GCHandle.Alloc(raw_packet_buffer, GCHandleType.Pinned);
			IntPtr buffer = handle.AddrOfPinnedObject();
			Marshal.StructureToPtr(in_object, buffer, false);
			handle.Free();

			// update CRC
			int raw_packet_size = Marshal.SizeOf(in_object);
			crc = CRC16.CalculateForBlock(CRC16.InitValue, raw_packet_buffer, raw_packet_size);
			raw_packet_buffer[raw_packet_size + 0] = ByteAccess.LowByte(crc);
			raw_packet_buffer[raw_packet_size + 1] = ByteAccess.HighByte(crc);

			// change buffer state
			m_packets[packet_index].Interface = in_interface;
			m_packets[packet_index].Length = (byte)(raw_packet_size + PacketConstants.PacketCRCLength);
			m_packets[packet_index].Timestamp = DateTime.Now;
			m_packets[packet_index].State = PacketBufferState.Valid;

			return true;
		}

		/// <summary>
		/// Serializes packet header and pushes header and packet data into the queue with CRC calculation
		/// </summary>
		/// <param name="in_interface"></param>
		/// <param name="in_packet_header"></param>
		/// <param name="in_packet_data"></param>
		/// <returns></returns>
		public bool Push(byte in_interface, object in_packet_header, byte[] in_packet_data)
		{
			int packet_index;
			UInt16 crc;

			// reserve storage for packet
			packet_index = ReservePushBuffer();
			if (packet_index == InvalidPacketIndex)
				return false;

			// calculate packet size
			int raw_packet_size = Marshal.SizeOf(in_packet_header) + in_packet_data.Length;

			// serialize packet header
			byte[] raw_packet_buffer = m_packets[packet_index].Buffer;
			GCHandle handle = GCHandle.Alloc(raw_packet_buffer, GCHandleType.Pinned);
			IntPtr buffer = handle.AddrOfPinnedObject();
			Marshal.StructureToPtr(in_packet_header, buffer, false);
			handle.Free();

			// copy packet data
			in_packet_data.CopyTo(raw_packet_buffer, Marshal.SizeOf(in_packet_header));

			// update CRC
			crc = CRC16.CalculateForBlock(CRC16.InitValue, raw_packet_buffer, raw_packet_size);
			raw_packet_buffer[raw_packet_size + 0] = ByteAccess.LowByte(crc);
			raw_packet_buffer[raw_packet_size + 1] = ByteAccess.HighByte(crc);

			// change buffer state
			m_packets[packet_index].Interface = in_interface;
			m_packets[packet_index].Length = (byte)(raw_packet_size + PacketConstants.PacketCRCLength);
			m_packets[packet_index].Timestamp = DateTime.Now;
			m_packets[packet_index].State = PacketBufferState.Valid;

			return true;

		}

		/// <summary>
		/// Returns true when queue is empty
		/// </summary>
		/// <returns>True when empty</returns>
		public bool IsEmpty()
		{
			return m_pop_index == m_push_index;
		}

		/// <summary>
		/// Gets the packet type of the next popping packet
		/// </summary>
		/// <returns></returns>
		public PacketType PeekPacketType()
		{
			return (PacketType)m_packets[m_pop_index].Buffer[PacketConstants.TypeOffset];
		}

		/// <summary>
		/// Returns true when next pop packet is valid
		/// </summary>
		/// <returns></returns>
		public bool PeekPacketValid()
		{
			return m_packets[m_pop_index].State == PacketBufferState.Valid;
		}

		/// <summary>
		/// Deleete packet from the queue
		/// </summary>
		/// <returns>True if packet was deleted, false if queue is empty</returns>
		public bool Pop()
		{
			int new_pop_index;

			// return if there is no packet to pop
			if (m_pop_index == m_push_index)
				return false;

			// increment pop index
			new_pop_index = m_pop_index + 1;
			if (new_pop_index >= m_packets.Length)
				new_pop_index = 0;

			// remove packet from the queue
			m_packets[m_pop_index].Length = 0;
			m_packets[m_pop_index].State = PacketBufferState.Empty;

			m_pop_index = new_pop_index;

			return true;
		}

		/// <summary>
		/// Pops packet into raw binary buffer
		/// </summary>
		/// <param name="out_buffer">Output buffer used to store popped packet (the length must be same sa the max packet length)</param>
		/// <param name="out_buffer_length"></param>
		/// <returns></returns>
		public bool Pop(byte[] out_buffer, ref int out_buffer_length)
		{
			int new_pop_index;

			// sanity check
			if (out_buffer.Length != PacketConstants.PacketMaxLength)
				return false;

			// return if there is no packet to pop
			if (m_pop_index == m_push_index)
				return false;

			// increment pop index
			new_pop_index = m_pop_index + 1;
			if (new_pop_index >= m_packets.Length)
				new_pop_index = 0;

			// check buffer state -> return if bufer is not valid
			if (m_packets[m_pop_index].State == PacketBufferState.Valid)
				return false;

			// copy packet to the buffer
			Array.Copy(m_packets[m_pop_index].Buffer, out_buffer, m_packets[m_pop_index].Length);

			// remove packet from the queue
			m_packets[m_pop_index].Length = 0;
			m_packets[m_pop_index].State = PacketBufferState.Empty;

			// update pop pointer
			m_pop_index = new_pop_index;

			return true;
		}

		/// <summary>
		/// Pops and deserializes packet
		/// </summary>
		/// <param name="in_type">Type of the packet to deserialize</param>
		/// <returns>Deserialized class or null if queue is empty</returns>
		public object Pop(Type in_type)
		{
			int new_pop_index;
			object result_object = null;

			// return if there is no packet to pop
			if (m_pop_index == m_push_index)
				return null;

			// increment pop index
			new_pop_index = m_pop_index + 1;
			if (new_pop_index >= m_packets.Length)
				new_pop_index = 0;

			// check buffer state -> return if bufer is not valid
			if (m_packets[m_pop_index].State != PacketBufferState.Valid)
				return null;

			// deserialize packet if size is correct
			int raw_packet_size = Marshal.SizeOf(in_type);
			if (raw_packet_size + PacketConstants.PacketCRCLength == m_packets[m_pop_index].Length)
			{
				byte[] raw_packet_buffer = m_packets[m_pop_index].Buffer;
				GCHandle handle = GCHandle.Alloc(raw_packet_buffer, GCHandleType.Pinned);
				IntPtr buffer = handle.AddrOfPinnedObject();
				result_object = Marshal.PtrToStructure(buffer, in_type);
				handle.Free();
			}

			// remove packet from the queue
			m_packets[m_pop_index].State = PacketBufferState.Empty;
			m_packets[m_pop_index].Length = 0;

			m_pop_index = new_pop_index;

			return result_object;
		}

		/// <summary>
		/// Gets packet content in the same buffer as used for the queue without removing from the queue
		/// The returned buffer will be valid until PopEnd is called
		/// </summary>
		/// <param name="out_packet"></param>
		/// <param name="out_packet_length"></param>
		/// <returns></returns>
		public bool PopBegin(out byte[] out_packet, out byte out_packet_length, out byte out_interface, out DateTime out_timestamp)
		{
			// return if there is no packet to pop
			if (m_pop_index == m_push_index || m_packets[m_pop_index].State != PacketBufferState.Valid)
			{
				out_packet = null;
				out_packet_length = 0;
				out_interface = 0;
				out_timestamp = DateTime.MinValue;

				return false;
			}

			// get packet without removing from the queue
			out_packet = m_packets[m_pop_index].Buffer;
			out_packet_length = m_packets[m_pop_index].Length;
			out_interface = m_packets[m_pop_index].Interface;
			out_timestamp = m_packets[m_pop_index].Timestamp;

			return true;
		}

		/// <summary>
		/// Finished PopBegin operation. The buffer returned by PopBegin will be invalid after calling this function.
		/// </summary>
		public void PopEnd()
		{
			int new_pop_index;

			// remove packet from the queue
			m_packets[m_pop_index].State = PacketBufferState.Empty;
			m_packets[m_pop_index].Length = 0;

			// increment pop index
			new_pop_index = m_pop_index + 1;
			if (new_pop_index >= m_packets.Length)
				new_pop_index = 0;

			m_pop_index = new_pop_index;
		}

		/// <summary>
		/// Reserves packet storage for packet pushing
		/// </summary>
		/// <returns></returns>
		private int ReservePushBuffer()
		{
			int new_push_index;
			int packet_index;

			lock (m_packets)
			{
				// check for empty space in the queue
				packet_index = m_push_index;
				if (m_packets[packet_index].State != PacketBufferState.Empty)
					return InvalidPacketIndex;

				// reserve packet buffer
				m_packets[packet_index].State = PacketBufferState.Reserved;

				// increment push pointer
				new_push_index = packet_index + 1;

				if (new_push_index >= m_packets.Length)
					new_push_index = 0;

				if (new_push_index != m_pop_index)
				{
					// update push pointer
					m_push_index = new_push_index;
				}
			}

			return packet_index;
		}
		#endregion
	}
}
