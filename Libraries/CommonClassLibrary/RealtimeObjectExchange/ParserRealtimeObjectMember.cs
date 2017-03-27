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
using CommonClassLibrary.XMLParser;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class ParserRealtimeObjectMember
	{
		#region · Data members ·
		private RealtimeObjectMember.MemberType m_member_type;
		private string m_name;
		private Dictionary<string, string> m_attributes = new Dictionary<string, string>();
		private int m_fixed_multiplier;
		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		internal ParserRealtimeObjectMember()
		{
			m_member_type = RealtimeObjectMember.MemberType.Float;
			m_fixed_multiplier = 1;
		}

		/// <summary>
		/// Constructor for defining name and type
		/// </summary>
		/// <param name="in_name"></param>
		/// <param name="in_type"></param>
		public ParserRealtimeObjectMember(string in_name, RealtimeObjectMember.MemberType in_type)
		{
			m_name = in_name;
			m_member_type = in_type;
			m_fixed_multiplier = 1;
		}

		/// <summary>
		/// Constructor for defining type
		/// </summary>
		/// <param name="in_type"></param>
		public ParserRealtimeObjectMember(RealtimeObjectMember.MemberType in_type)
		{
			m_member_type = in_type;
			m_fixed_multiplier = 1;
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

		/// <summary>
		/// Gets type of the member
		/// </summary>
		public RealtimeObjectMember.MemberType Type
		{
			get { return m_member_type; }
		}

		/// <summary>
		/// Gets multiplier for fixed values
		/// </summary>
		public int FixedMultipler
		{
			get { return m_fixed_multiplier; }
		}
		#endregion

		#region · Parser function ·

		/// <summary>
		/// Parses value member of the realtimeobject
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_xml_stream"></param>
		/// <param name="in_parent"></param>
		internal void ParseValueMember(XPathNavigator in_element, TextReader in_xml_stream, object in_parent)
		{
			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);

			// get type
			m_member_type = TypeStringToMemberType(in_element.Name);

			// check type
			if(m_member_type == RealtimeObjectMember.MemberType.Unknown)
					throw XMLParserBase.CreateXMLParseException(string.Format(ParserRealtimeObjectStringConstants.ErrorInvalidElementType, in_element.Name), in_element);

			// get multiplier for fixed types
			if (RealtimeObjectMember.IsFixedMember(m_member_type))
				m_fixed_multiplier = XMLAttributeParser.ConvertAttributeToInt(in_element, "Multiplier", XMLAttributeParser.atObligatory);

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
		/// Converts type string to realtime object member type
		/// </summary>
		/// <param name="in_type_string">Type string</param>
		/// <returns>Member type</returns>
		public static RealtimeObjectMember.MemberType TypeStringToMemberType(string in_type_string)
		{
			switch (in_type_string)
			{
				case "Int8Fixed":
					return RealtimeObjectMember.MemberType.Int8Fixed;

				case "UInt8Fixed":
					return RealtimeObjectMember.MemberType.UInt8Fixed;

				case "Int16Fixed":
					return RealtimeObjectMember.MemberType.Int16Fixed;

				case "UInt16Fixed":
					return RealtimeObjectMember.MemberType.UInt16Fixed;

				case "Int32Fixed":
					return RealtimeObjectMember.MemberType.Int32Fixed;

				case "UInt32Fixed":
					return RealtimeObjectMember.MemberType.UInt32Fixed;

				case "Int8":
					return RealtimeObjectMember.MemberType.Int8;

				case "Int16":
					return RealtimeObjectMember.MemberType.Int16;

				case "Int32":
					return RealtimeObjectMember.MemberType.Int32;

				case "UInt8":
					return RealtimeObjectMember.MemberType.UInt8;

				case "UInt16":
					return RealtimeObjectMember.MemberType.UInt16;

				case "UInt32":
					return RealtimeObjectMember.MemberType.UInt32;

				case "Value":
					return RealtimeObjectMember.MemberType.Float;
			}

			return RealtimeObjectMember.MemberType.Unknown;
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

		#region · Header file generation ·

		/// <summary>
		/// Creates C type header declaration of this member
		/// </summary>
		/// <param name="in_header_file"></param>
		public void CreateMemberDeclaration(ParserRealtimeObjectExchange.ParserParameters  in_parameters)
		{
			string typestring = in_parameters.Typedefs[m_member_type];

			in_parameters.HeaderFile.WriteLine("  " + typestring + " " + m_name + ";");
		}

		#endregion
	}
}
