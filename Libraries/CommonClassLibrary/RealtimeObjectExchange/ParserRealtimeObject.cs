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
using CommonClassLibrary.XMLParser;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class ParserRealtimeObject
	{
		#region · Data members ·
		private string m_name;

		private List<ParserRealtimeObjectMember> m_members = new List<ParserRealtimeObjectMember>();
		private Dictionary<string, int> m_members_lookup = new Dictionary<string, int>();

		private Dictionary<string, string> m_attributes = new Dictionary<string, string>();

		private int m_default_index =0;

		private byte m_packet_id;


		#endregion

		#region · Constructor ·
		/// <summary>
		/// Default constructor
		/// </summary>
		public ParserRealtimeObject(byte in_packet_id)
		{
			m_packet_id = in_packet_id;
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

		/// <summary>
		/// Gets packet ID
		/// </summary>
		public byte PacketID
		{
			get { return m_packet_id; }
		}

		/// <summary>
		/// Gets object's attribute
		/// </summary>
		/// <param name="in_name"></param>
		/// <returns>Attribute value or null if attribute is not defined</returns>
		public string GetAttribute(string in_name)
		{
			if (m_attributes.ContainsKey(in_name))
				return m_attributes[in_name];
			else
				return null;
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

			// store attributes for further processing
			XPathNavigator element = in_element.Clone();
			if (element.MoveToFirstAttribute())
			{
				m_attributes.Add(element.Name, element.Value);
			}

			while (element.MoveToNextAttribute())
			{
				m_attributes.Add(element.Name, element.Value);
			}
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

		#region · Header file generation ·

		/// <summary>
		/// Creates C type declaration header for this object
		/// </summary>
		/// <param name="in_header_file"></param>
		public void CreateObjectDeclaration(ParserRealtimeObjectExchange.ParserParameters in_parameters)
		{
			// declaration header
			in_parameters.HeaderFile.WriteLine("///////////////////////////////////////////////////////////////////////////////");
			in_parameters.HeaderFile.WriteLine("// " + m_name);

			in_parameters.HeaderFile.WriteLine("typedef struct");
			in_parameters.HeaderFile.WriteLine("{");
			in_parameters.HeaderFile.WriteLine(in_parameters.Typedefs.PacketHeaderDeclaration);
			in_parameters.HeaderFile.WriteLine();

			// object members
			foreach (ParserRealtimeObjectMember member in m_members)
			{
				member.CreateMemberDeclaration(in_parameters);
			}

			// declaration end
			in_parameters.HeaderFile.WriteLine("} rox" + m_name + ";");
			in_parameters.HeaderFile.WriteLine();
		}

		#endregion
	}
}
