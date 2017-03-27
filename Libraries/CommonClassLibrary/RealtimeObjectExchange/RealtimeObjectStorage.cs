///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2015 Laszlo Arvai. All rights reserved.
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
// Storage class for all realtime objects
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.DeviceCommunication;
using System.Collections.Generic;
using System.Linq;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class RealtimeObjectStorage
	{
		#region · Data members · 

		// singleton member
		private static RealtimeObjectStorage m_default;

		// object storage
		private List<RealtimeObject> m_objects;
		private Dictionary<string, int> m_object_lookup;

		private List<RealtimeObject> m_packet_id_lookup;

		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constuctor
		/// </summary>
		public RealtimeObjectStorage()
		{
			m_objects = new List<RealtimeObject>();
			m_object_lookup = new Dictionary<string, int>();
			m_packet_id_lookup = new List<RealtimeObject>(PacketConstants.MaxRealtimePacketCount);
			for (int i = 0; i < PacketConstants.MaxRealtimePacketCount; i++)
				m_packet_id_lookup.Add(null);
		}
		#endregion

		#region · Singleton members ·

		/// <summary>
		/// Singleton instance
		/// </summary>
		public static RealtimeObjectStorage Default
		{
			get
			{
				if (m_default == null)
				{
					m_default = new RealtimeObjectStorage();
				}

				return m_default;
			}
		}
		#endregion

		#region · Object access functions ·

		/// <summary>
		/// Indexer to access objects by name
		/// </summary>
		/// <param name="in_name"></param>
		/// <returns></returns>
		public RealtimeObject this[string in_name]
		{
			get
			{
				if (m_object_lookup.ContainsKey(in_name))
				{
					return m_objects[m_object_lookup[in_name]];
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Indexer to access objects by index
		/// </summary>
		/// <param name="in_index"></param>
		/// <returns></returns>
		public RealtimeObject this[int in_index]
		{
			get
			{
				return m_objects[in_index];
			}
		}

		/// <summary>
		/// Gets object index
		/// </summary>
		/// <param name="in_name"></param>
		/// <returns></returns>
		public int ObjectGetIndex(string in_name)
		{
			if(m_object_lookup.ContainsKey(in_name))
			{
				return m_object_lookup[in_name];
			}
			else
			{
				return -1;
			}
		}

		/// <summary>
		/// Gets realtime object by packet ID
		/// </summary>
		/// <param name="in_packet_id">Packet ID</param>
		/// <returns>RealtimeObject related to the given packet ID</returns>
		public RealtimeObject GetObjectByPacketID(byte in_packet_id)
		{
			return m_packet_id_lookup[in_packet_id];
		}

		#endregion

		#region · Realtime object generation from code ·

		/// <summary>
		/// Clears all stored objects
		/// </summary>
		public void ObjectClear()
		{
			lock (m_objects)
			{
				m_objects.Clear();
				m_object_lookup.Clear();
			}
		}

		/// <summary>
		/// Creates an object for the realtime object collection
		/// </summary>
		/// <param name="in_name"></param>
		/// <returns></returns>
		public RealtimeObject ObjectCreate(string in_name)
		{
			RealtimeObject realtime_object = new RealtimeObject(in_name);

			ObjectAdd(realtime_object);

			return realtime_object;
		}

		/// <summary>
		/// Adds realtime object to the storage
		/// </summary>
		/// <param name="in_realtime_object"></param>
		public void ObjectAdd(RealtimeObject in_realtime_object)
		{
			int index;

			// add object to the collection in a thread safe way
			lock (m_objects)
			{
				m_objects.Add(in_realtime_object);
				index = m_objects.Count - 1;
				m_object_lookup.Add(in_realtime_object.Name, index);
			}

			in_realtime_object.SetObjectIndex(index);
		}

		#endregion

		#region · Realtime object generation from parser object ·

		/// <summary>
		/// Copes all obejcts from parsed realtime objects
		/// </summary>
		/// <param name="in_parsed_objects"></param>
		public void CopyParsedObjects(List<ParserRealtimeObject> in_parsed_objects)
		{
			foreach(ParserRealtimeObject parsed_object in in_parsed_objects)
			{
				RealtimeObject realtime_object = new RealtimeObject(parsed_object);

				ObjectAdd(realtime_object);

				realtime_object.CopyParsedMember(parsed_object);

				realtime_object.ObjectCreateEnd();

				m_packet_id_lookup[parsed_object.PacketID] = realtime_object;
			}
		}
		#endregion
	}
}
