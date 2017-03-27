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

using System;

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
			Unknown,

			Int,
			Float,

			Int8,
			Int16,
			Int32,

			UInt8,
			UInt16,
			UInt32,

			Int8Fixed,
			UInt8Fixed,
			Int16Fixed,
			UInt16Fixed,
			Int32Fixed,
			UInt32Fixed,

			String
		}
		#endregion

		#region · Data members ·
		private string m_member_name;
		private MemberType m_member_type;
		private RealtimeObject m_parent_object;
		private int m_member_index;
		private int m_binary_length;

		private int m_multiplier; // multiplier for fixed values
		private int m_max_string_length; // length for fixed length strings

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
			m_max_string_length = 0;

			m_binary_length = RealtimeObjectMember.GetMemberSize(this);
		}

		/// <summary>
		/// Creates realtime object member from parser member data
		/// </summary>
		/// <param name="in_member"></param>
		public RealtimeObjectMember(ParserRealtimeObjectMember in_member) : this(in_member.Name, in_member.Type)
		{
			m_multiplier = in_member.FixedMultipler;
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

		/// <summary>
		/// Gets member index within object
		/// </summary>
		public int MemberIndex
		{
			get { return m_member_index; }
		}

		/// <summary>
		/// Gets binary length of the member
		/// </summary>
		public int BinaryLength
		{
			get { return m_binary_length; }
		}

		/// <summary>
		///  Gets length of fixed length strings
		/// </summary>
		public int MaxStringLength
		{
			get { return m_max_string_length; }
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

		#region · Member update ·
		public object GetMemberValueFromBinaryData(byte[] in_binary_buffer, int in_offset)
		{
			object retval;

			switch (m_member_type)
			{
				case MemberType.Unknown:
					throw new ArgumentException("Invalid member type");

				case MemberType.Int:
					retval = BitConverter.ToInt32(in_binary_buffer, in_offset);
					break;

				case MemberType.Float:
					retval = BitConverter.ToSingle(in_binary_buffer, in_offset);
					break;

				case MemberType.Int8:
					retval = (Int32)(sbyte)in_binary_buffer[in_offset];
					break;

				case MemberType.Int16:
					retval = (Int32)BitConverter.ToInt16(in_binary_buffer, in_offset);
					break;

				case MemberType.Int32:
					retval = BitConverter.ToInt32(in_binary_buffer, in_offset);
					break;

				case MemberType.UInt8:
					retval = (UInt32)(in_binary_buffer[in_offset]);
					break;

				case MemberType.UInt16:
					retval = (UInt32)BitConverter.ToUInt16(in_binary_buffer, in_offset);
					break;

				case MemberType.UInt32:
					retval = BitConverter.ToUInt32(in_binary_buffer, in_offset);
					break;

				case MemberType.Int8Fixed:
					retval = ((float)(sbyte)in_binary_buffer[in_offset]) / m_multiplier;
					break;

				case MemberType.UInt8Fixed:
					retval = ((float)in_binary_buffer[in_offset]) / m_multiplier;
					break;

				case MemberType.Int16Fixed:
					retval = ((float)BitConverter.ToInt16(in_binary_buffer, in_offset)) / m_multiplier;
					break;

				case MemberType.UInt16Fixed:
					retval = ((float)BitConverter.ToUInt16(in_binary_buffer, in_offset)) / m_multiplier;
					break;

				case MemberType.Int32Fixed:
					retval = ((float)BitConverter.ToInt32(in_binary_buffer, in_offset)) / m_multiplier;
					break;

				case MemberType.UInt32Fixed:
					retval = ((float)BitConverter.ToUInt32(in_binary_buffer, in_offset)) / m_multiplier;
					break;

				case MemberType.String:
					retval = string.Empty;
					break;

				default:
					throw new ArgumentException("Invalid member type");
			}

			return retval;
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
		/// Sets multiplier for fixed integer types
		/// </summary>
		/// <param name="in_multiplier"></param>
		public void SetFixedMultiplier(int in_multiplier)
		{
			if (!RealtimeObjectMember.IsFixedMember(m_member_type))
				throw new ArgumentException("Fixed member type expected");

			m_multiplier = in_multiplier;
		}

		/// <summary>
		/// Sets length of the fixed length string
		/// </summary>
		/// <param name="in_max_length"></param>
		public void SetMaxStringLength(int in_max_length)
		{
			m_max_string_length = in_max_length;
			m_binary_length = RealtimeObjectMember.GetMemberSize(this);
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
				case MemberType.Unknown:
					throw new ArgumentException("Invalid member type");

				case MemberType.Int:
					retval = new int();
					retval = 0;
					break;

				case MemberType.Float:
					retval = new float();
					retval = 0.0;
					break;

				case MemberType.Int8:
					retval = new int();
					retval = 0;
					break;

				case MemberType.Int16:
					retval = new int();
					retval = 0;
					break;

				case MemberType.Int32:
					retval = new int();
					retval = 0;
					break;

				case MemberType.UInt8:
					retval = new int();
					retval = 0;
					break;

				case MemberType.UInt16:
					retval = new int();
					retval = 0;
					break;

				case MemberType.UInt32:
					retval = new int();
					retval = 0;
					break;

				case MemberType.Int8Fixed:
					retval = new float();
					retval = 0.0;
					break;

				case MemberType.UInt8Fixed:
					retval = new float();
					retval = 0.0;
					break;

				case MemberType.Int16Fixed:
					retval = new float();
					retval = 0.0;
					break;

				case MemberType.UInt16Fixed:
					retval = new float();
					retval = 0.0;
					break;

				case MemberType.Int32Fixed:
					retval = new float();
					retval = 0.0;
					break;

				case MemberType.UInt32Fixed:
					retval = new float();
					retval = 0.0;
					break;

				case MemberType.String:
					retval = string.Empty;
					break;

				default:
					throw new ArgumentException("Invalid member type");
			}

			return retval;
		}

		/// <summary>
		/// Returns true if member is fixed integer type member
		/// </summary>
		/// <param name="in_type"></param>
		/// <returns></returns>
		static public bool IsFixedMember(MemberType in_type)
		{
			return in_type == MemberType.Int16Fixed || in_type == MemberType.Int32Fixed || in_type == MemberType.Int8Fixed ||
							in_type == MemberType.UInt16Fixed || in_type == MemberType.UInt32Fixed || in_type == MemberType.UInt8Fixed;
		}

		/// <summary>
		/// Gets member binary size based on the type of the member
		/// </summary>
		/// <param name="in_member"></param>
		/// <returns></returns>
		static public int GetMemberSize(RealtimeObjectMember in_member)
		{
			switch (in_member.Type)
			{
				case MemberType.Unknown:
					throw new ArgumentException("Invalid member type");

				case MemberType.Int:
					return 4;

				case MemberType.Float:
					return 4;

				case MemberType.Int8:
					return 1;

				case MemberType.Int16:
					return 2;

				case MemberType.Int32:
					return 4;

				case MemberType.UInt8:
					return 1;
				case MemberType.UInt16:
					return 2;

				case MemberType.UInt32:
					return 4;

				case MemberType.Int8Fixed:
					return 1;

				case MemberType.UInt8Fixed:
					return 1;

				case MemberType.Int16Fixed:
					return 2;

				case MemberType.UInt16Fixed:
					return 2;

				case MemberType.Int32Fixed:
					return 4;

				case MemberType.UInt32Fixed:
					return 4;

				case MemberType.String:
					return in_member.MaxStringLength;

				default:
					throw new ArgumentException("Invalid member type");
			}
		}
		#endregion
	}
}
