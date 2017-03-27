///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2017 Laszlo Arvai. All rights reserved.
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
// Type definition file handler for realtime object exchange parser
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.XMLParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class RealtimeObjectTypedefs : XMLParserBase
	{
		#region · Data members ·
		private string m_packet_header_declaration = null;
		private Dictionary<RealtimeObjectMember.MemberType, string> m_type_lookup = new Dictionary<RealtimeObjectMember.MemberType, string>();
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets header declaration
		/// </summary>
		public string PacketHeaderDeclaration
		{
			get { return m_packet_header_declaration; }
		}

		/// <summary>
		/// Returns type declaration string from the member type
		/// </summary>
		/// <param name="in_index"></param>
		/// <returns></returns>
		public string this[RealtimeObjectMember.MemberType in_index]
		{
			get { return m_type_lookup[in_index]; }
		}

		#endregion

		#region · Overridden members ·

		/// <summary>
		/// Clears all data content of the class
		/// </summary>
		public override void Clear()
		{
			m_root_class = new ParserRealtimeObjectCollection();
		}

		/// <summary>
		/// Parses base class
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_xml_stream"></param>
		protected override void ParseBase(XPathNavigator in_element, TextReader in_xml_stream)
		{
			((ParserRealtimeObjectCollection)m_root_class).ParseXML(in_element);
		}

		/// <summary>
		/// Parses real time object description file elements
		/// </summary>
		/// <param name="in_element"></param>
		protected override void ParseElement(XPathNavigator in_element, TextReader in_xml_stream, object in_parent)
		{
			string name;
			string cdecl;
			RealtimeObjectMember.MemberType type;

			switch (in_element.Name)
			{
				case "Type":
					{
						name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);
						cdecl = XMLAttributeParser.ConvertAttributeToString(in_element, "CDecl", XMLAttributeParser.atObligatory);

						if (name == "PacketHeader")
							m_packet_header_declaration = cdecl;
						else
						{
							type = ParserRealtimeObjectMember.TypeStringToMemberType(name);

							if (type == RealtimeObjectMember.MemberType.Unknown)
								throw XMLParserBase.CreateXMLParseException(string.Format(ParserRealtimeObjectStringConstants.ErrorInvalidElementType, in_element.Name), in_element);

							m_type_lookup.Add(type, cdecl);
						}
					}
					break;

				default:
					throw XMLParserBase.CreateXMLParseException(string.Format(ParserRealtimeObjectStringConstants.ErrorInvalidElementType, in_element.Name), in_element);
			}
		}

		#endregion

	}
}
