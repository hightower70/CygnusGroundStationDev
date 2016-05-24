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
// Realtime object description
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class RealtimeObject
	{
		#region · Data members ·
		private string m_object_name;
		private int m_object_storage_index;

		private volatile int m_read_storage_index;
		private volatile int m_write_storage_index;

		private List<RealtimeObjectMember> m_members;
		private Dictionary<string, int> m_member_lookup;
		private object[,] m_member_values;

		private List<IRealtimeObjectUpdateNotifier> m_update_notifiers;

		#endregion

		#region · Constructor ·

		/// <summary>
		/// Creates object
		/// </summary>
		/// <param name="in_name"></param>
		public RealtimeObject(string in_name)
		{
			m_object_name = in_name;
			m_members = new List<RealtimeObjectMember>();
			m_member_lookup = new Dictionary<string, int>();
			m_read_storage_index = 0;
			m_write_storage_index = -1;
			m_member_values = null;
			m_update_notifiers = new List<IRealtimeObjectUpdateNotifier>();
		}
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets name of the object
		/// </summary>
		public string Name
		{
			get { return m_object_name; }
		}

		#endregion

		#region · Update notifiers ·

		/// <summary>
		/// Adds object update notifier class
		/// </summary>
		/// <param name="in_notifier"></param>
		public void UpdateNotifierAdd(IRealtimeObjectUpdateNotifier in_notifier)
		{
			lock(m_update_notifiers)
			{
				m_update_notifiers.Add(in_notifier);
			}
		}

		/// <summary>
		/// Removes object update notifier class
		/// </summary>
		/// <param name="in_notifier"></param>
		public void UpdateNotifierRemove(IRealtimeObjectUpdateNotifier in_notifier)
		{
			lock(m_update_notifiers)
			{
				m_update_notifiers.Remove(in_notifier);
			}
		}

		/// <summary>
		/// Executes object update notification
		/// </summary>
		private void UpdateNotifierExecute()
		{
			lock(m_update_notifiers)
			{
				for (int i = 0; i < m_update_notifiers.Count; i++)
					m_update_notifiers[i].ObjectUpdated();
			}
		}
		#endregion

		#region · Object creation ·

		/// <summary>
		/// Initializes member values to their default value
		/// </summary>
		internal void ObjectCreateEnd()
		{
			// convert object structure to the 
			m_member_values = new object[2, m_members.Count];

			// create default values
			for (int i = 0; i < m_members.Count; i++)
			{
				m_member_values[0, i] = RealtimeObjectMember.CreateDefaultMemberObject(m_members[i].Type);
				m_member_values[1, i] = RealtimeObjectMember.CreateDefaultMemberObject(m_members[i].Type);
			}
		}

		/// <summary>
		/// Sets index of this class in the realtime object collection
		/// </summary>
		/// <param name="in_index"></param>
		internal void SetObjectIndex(int in_index)
		{
			m_object_storage_index = in_index;
		}

		/// <summary>
		/// Creates and adds new member to the object
		/// </summary>
		/// <param name="in_name"></param>
		/// <param name="in_member_type"></param>
		/// <returns></returns>
		public RealtimeObjectMember MemberCreate(string in_name, RealtimeObjectMember.MemberType in_member_type)
		{
			RealtimeObjectMember member = new RealtimeObjectMember(in_name, in_member_type);

			MemberAdd(member);

			return member;
		}

		/// <summary>
		/// Adds a new member to the object
		/// </summary>
		/// <param name="in_member"></param>
		public void MemberAdd(RealtimeObjectMember in_member)
		{
			int index;

			in_member.SetParentObject(this);

			m_members.Add(in_member);
			index = m_members.Count - 1;
			m_member_lookup.Add(in_member.Name, index);

			in_member.SetMemberIndex(index);

		}

		/// <summary>
		/// Gets member index
		/// </summary>
		/// <param name="in_name"></param>
		/// <returns></returns>
		public int MemberGetIndex(string in_name)
		{
			if (m_member_lookup.ContainsKey(in_name))
				return m_member_lookup[in_name];
			else
				return -1;
		}

		/// <summary>
		/// Gets name of the members
		/// </summary>
		/// <returns></returns>
		public List<string> MemberGetList()
		{
			List<string> member_list = new List<string>();

			for (int i = 0; i < m_members.Count; i++)
				member_list.Add(m_members[i].Name);

			return member_list;
		}
		#endregion

		#region · Object/member update ·

		/// <summary>
		/// Starts object update operation
		/// </summary>
		internal void UpdateBegin()
		{
			if (m_write_storage_index != -1)
				throw new InvalidOperationException("Object already opened for write");

			m_write_storage_index = 1 - m_read_storage_index;
		}

		/// <summary>
		/// Ends object write operation
		/// </summary>
		internal void UpdateEnd()
		{
			if (m_write_storage_index == -1)
				throw new InvalidOperationException("Object is not opened for write");

			m_read_storage_index = m_write_storage_index;
			m_write_storage_index = -1;

			UpdateNotifierExecute();
		}

		/// <summary>
		/// Updates member value
		/// </summary>
		/// <param name="in_member_index"></param>
		/// <param name="in_new_value"></param>
		public void MemberWrite(int in_member_index, object in_new_value)
		{
			m_member_values[m_write_storage_index, in_member_index] = in_new_value;
		}

		/// <summary>
		/// Reads member value based on the name of the member
		/// </summary>
		/// <param name="in_member_name"></param>
		/// <returns></returns>
		public object MemberRead(string in_member_name)
		{
			return m_member_values[m_read_storage_index, m_member_lookup[in_member_name]];
		}

		/// <summary>
		/// Reads member value based on member index
		/// </summary>
		/// <param name="in_member_index"></param>
		/// <returns></returns>
		public object MemberRead(int in_member_index)
		{
			if (in_member_index >= 0)
				return m_member_values[m_read_storage_index, in_member_index];
			else
				return null;
		}

		#endregion
	}
}
