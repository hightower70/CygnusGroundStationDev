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
// Realtime object member description
///////////////////////////////////////////////////////////////////////////////

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class RealtimeObjectMember
	{
		#region · Types ·

		/// <summary>
		/// Types of the possible realtime object members
		/// </summary>
		public enum MemberType
		{
			Int,
			Float,
			String
		}
		#endregion

		#region · Data members ·
		private string m_member_name;
		private MemberType m_member_type;
		private RealtimeObject m_parent_object;
		private int m_member_index;
		#endregion

		#region · Constructor ·
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="in_name"></param>
		/// <param name="in_parent"></param>
		public RealtimeObjectMember(string in_name, MemberType in_type)
		{
			m_member_name = in_name;
			m_member_type = in_type;
			m_parent_object = null;
			m_member_index = -1;
		}
		#endregion


		#region · Properties ·

		/// <summary>
		/// Gets member name
		/// </summary>
		public string Name
		{
			get { return m_member_name; }
		}

		/// <summary>
		/// Gets member type
		/// </summary>
		public MemberType Type
		{
			get { return m_member_type; }
		}

		/// <summary>
		/// Gets index of the parent object
		/// </summary>
		public RealtimeObject ParentObject
		{
			get { return m_parent_object; }
		}

		public int MemberIndex
		{
			get { return m_member_index; }
		}
		#endregion

		#region · Member access ·

		/// <summary>
		/// Updates member value
		/// </summary>
		/// <param name="in_new_value"></param>
		public void Write(int in_new_value)
		{
			m_parent_object.MemberWrite(m_member_index, in_new_value);
		}

		public void Write(float in_new_value)
		{
			m_parent_object.MemberWrite(m_member_index, in_new_value);
		}

		public void Write(string in_new_value)
		{
			m_parent_object.MemberWrite(m_member_index, in_new_value);
		}
		#endregion


		#region · Member creation ·

		/// <summary>
		/// Sets index of the parent object
		/// </summary>
		/// <param name="in_parent_object_index"></param>
		internal void SetParentObject(RealtimeObject in_parent_object)
		{
			m_parent_object = in_parent_object;
		}

		/// <summary>
		/// Sets index of this member within parent object
		/// </summary>
		/// <param name="in_member_index"></param>
		internal void SetMemberIndex(int in_member_index)
		{
			m_member_index = in_member_index;
		}

		/// <summary>
		/// Creates default member value based on member type
		/// </summary>
		/// <param name="in_type"></param>
		/// <returns></returns>
		public static object CreateDefaultMemberObject(MemberType in_type)
		{
			object retval;

			switch (in_type)
			{
				case MemberType.Float:
					retval = new float();
					retval = 0.0;
					break;

				case MemberType.Int:
					retval = new int();
					retval = 0;
					break;

				case MemberType.String:
					retval = string.Empty;
					break;

				default:
					retval = null;
					break;
			}

			return retval;
		}
		#endregion
	}
}
