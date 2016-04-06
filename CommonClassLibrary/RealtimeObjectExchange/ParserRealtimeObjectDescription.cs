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
// Parser for realtime object descriptor XML
///////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.Text;
using System.Xml.XPath;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class ParserRealtimeObjectDescription : XMLParserBase
	{
		#region · Constants ·
		public const string XMLRootName = "RealtimeObjectExchangle";
		#endregion

		#region · Data members ·
		private StringBuilder m_header_file;
		private byte[] m_binary_buffer;
		private int m_binary_pos;
		#endregion

		#region · Properties ·
		/// <summary>
		/// Gets realtime object collection
		/// </summary>
		public ParserRealtimeObjectCollection Collection
		{
			get { return (ParserRealtimeObjectCollection)m_root_class; }
		}
		#endregion

		#region · Overriden members ·

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
		/// Parses realtime object description file elements
		/// </summary>
		/// <param name="in_element"></param>
		protected override void ParseElement(XPathNavigator in_element, TextReader in_xml_stream, object in_parent)
		{
			switch (in_element.Name)
			{
				case "Object":
					{
						ParserRealtimeObject realtime_object = new ParserRealtimeObject();

						realtime_object.ParseXML(in_element);

						ParseXMLChildNodes(in_element, in_xml_stream, realtime_object);

						((ParserRealtimeObjectCollection)in_parent).AddObject(in_element, realtime_object);
					}
					break;

				case "Value":
					{
						ParserRealtimeObjectMember realtime_value = new ParserRealtimeObjectMember();

						realtime_value.ParseXML(in_element);

						((ParserRealtimeObject)in_parent).AddMember(in_element, realtime_value);
					}
					break;

				case "LinearConverter":
					{
						LinearConverter converter = new LinearConverter();

						converter.ParseXML(in_element);

						((ParserRealtimeObjectCollection)m_root_class).AddConverter(in_element, converter);
					}
					break;

				default:
					throw CreateXMLParseException(string.Format(ParserRealtimeObjectStringConstants.ErrorInvalidElementType, in_element.Name), in_element);
			}
		}
		#endregion
	}
}
