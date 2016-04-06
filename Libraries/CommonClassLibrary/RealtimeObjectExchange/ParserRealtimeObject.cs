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
// Object for storing realtime values
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class ParserRealtimeObject
	{
		#region · Data members ·
		private string m_name;

		private List<ParserRealtimeObjectMember> m_members = new List<ParserRealtimeObjectMember>();
		private Dictionary<string, int> m_members_lookup = new Dictionary<string, int>();

		private int m_default_index;

		private float[] m_values = null;
		#endregion

		#region · Constructor ·
		/// <summary>
		/// Default constructor
		/// </summary>
		public ParserRealtimeObject()
		{

		}

		/// <summary>
		/// Creates named object
		/// </summary>
		/// <param name="in_name"></param>
		public ParserRealtimeObject(string in_name)
		{
			m_name = in_name;
		}

		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets index of clone of this class in the default realtime object collection
		/// </summary>
		public int DefaultIndex
		{
			get { return m_default_index; }
		}

		/// <summary>
		/// Gets Name of the realtime object 
		/// </summary>
		public string Name
		{
			get
			{
				return m_name;
			}
		}

		/// <summary>
		/// Gets list of members
		/// </summary>
		public List<ParserRealtimeObjectMember> Members
		{
			get { return m_members; }
		}
		#endregion

		#region · Parser function ·

		/// <summary>
		/// Parses Realtime object from the XML file
		/// </summary>
		/// <param name="in_element"></param>
		public void ParseXML(XPathNavigator in_element)
		{
			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);
		}

		/// <summary>
		/// Adds a new value description to the object
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_member"></param>
		public void AddMember(XPathNavigator in_element, ParserRealtimeObjectMember in_member)
		{
			m_members.Add(in_member);
			m_members_lookup.Add(in_member.Name, m_members.Count - 1);
		}
		#endregion

		#region · Object generation from code support ·

		/// <summary>
		/// Adds nem member to this object as a part of object generation from code functionality. The object generation must be initialized from RealtimeObjectColection class.
		/// This function is thread safe.
		/// </summary>
		/// <param name="in_name"></param>
		/// <param name="in_member_type"></param>
		public void AddMember(string in_name, ParserRealtimeObjectMember.MemberType in_member_type)
		{
			ParserRealtimeObjectMember new_member = new ParserRealtimeObjectMember();

			lock(m_members)
			{
				
			}
		}
		#endregion

		#region · Realtime access functions ·

		/// <summary>
		/// Initializes realtime access
		/// </summary>
		public void RealtimeInitialization()
		{
			// init array of values
			m_values = new float[m_members.Count];

			for (int i = 0; i < m_members.Count; i++)
			{
				m_values[i] = 0;
			}
		}

		/// <summary>
		/// Sets member value
		/// </summary>
		/// <param name="in_index"></param>
		/// <param name="in_value"></param>
		public void SetValue(int in_index, float in_value)
		{
			m_values[in_index] = in_value;
		}

		#endregion
	}
}
