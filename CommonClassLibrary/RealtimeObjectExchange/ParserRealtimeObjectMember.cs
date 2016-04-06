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
// Class for realtime value
///////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Xml.XPath;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class ParserRealtimeObjectMember
	{
		#region · Types ·

		public enum MemberType
		{
			Integer,
			Float,
			String
		};
		#endregion

		#region · Data members ·
		private MemberType m_member_type;
		private string m_name;
		private Dictionary<string, string> m_attributes = new Dictionary<string, string>();
		#endregion

		#region · Constructor ·
		internal ParserRealtimeObjectMember()
		{
			m_member_type = MemberType.Float;
		}

		public ParserRealtimeObjectMember(string in_name, MemberType in_type)
		{

		}

		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets name of the value
		/// </summary>
		public string Name
		{
			get
			{
				return m_name;
			}
		}

		#endregion

		#region · Parser function ·

		/// <summary>
		/// Parse value from XML file
		/// </summary>
		/// <param name="in_element"></param>
		internal void ParseXML(XPathNavigator in_element)
		{
			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);

			// default type
			m_member_type = MemberType.Float;

			// store other attributes for further processing
			XPathNavigator element = in_element.Clone();
			if(element.MoveToFirstAttribute())
			{
				m_attributes.Add(element.Name, element.Value);
			}

			while (element.MoveToNextAttribute())
			{
				m_attributes.Add(element.Name, element.Value);
			}
		}

		// gets attribute related to member
		public string GetAttribute(string in_attribute_name)
		{
			if (m_attributes.ContainsKey(in_attribute_name))
				return m_attributes[in_attribute_name];
			else
				return null;
		}

		#endregion
	}
}
