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
using CommonClassLibrary.DeviceCommunication;
using CommonClassLibrary.XMLParser;
using System;
using System.IO;
using System.Xml.XPath;

namespace CommonClassLibrary.RealtimeObjectExchange
{
	public class ParserRealtimeObjectExchange : XMLParserBase
	{
		#region · Types ·

		/// <summary>
		/// File names using for compiler file generation
		/// </summary>
		public class ParserParameters
		{
			public string ROXFileName;
			public string HeaderFileName;
			public string TypedefsFileName;
			public RealtimeObjectTypedefs Typedefs;
			public TextWriter HeaderFile;
			public string DefaultPacketEnableFileName;

			public ParserParameters()
			{
				ROXFileName = null;
				HeaderFileName = null;
				TypedefsFileName = null;
				Typedefs = null;
				HeaderFile = null;
				DefaultPacketEnableFileName = null;
			}
		}

		#endregion

		#region · Constants ·
		public const string XMLRootName = "RealtimeObjectExchangle";
		#endregion

		#region · Data members ·

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
		/// Parses realtime object description file elements
		/// </summary>
		/// <param name="in_element"></param>
		protected override void ParseElement(XPathNavigator in_element, TextReader in_xml_stream, object in_parent)
		{
			switch (in_element.Name)
			{
				case "Object":
					{
						ParserRealtimeObject realtime_object = new ParserRealtimeObject((byte)(((ParserRealtimeObjectCollection)in_parent).Objects.Count + 1));

						realtime_object.ParseXML(in_element);

						ParseXMLChildNodes(in_element, in_xml_stream, realtime_object);

						((ParserRealtimeObjectCollection)in_parent).AddObject(in_element, realtime_object);
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
					{
						ParserRealtimeObjectMember realtime_member = new ParserRealtimeObjectMember();

						realtime_member.ParseValueMember(in_element, in_xml_stream, in_parent);

						((ParserRealtimeObject)in_parent).AddMember(in_element, realtime_member);
					}
					break;
			}
		}

		#endregion

		#region · Header file creation ·

		/// <summary>
		/// Creates C type header declaration for this realtime object file
		/// </summary>
		/// <param name="in_parser_parameters"></param>
		public void CreateHeaderFiles(ParserRealtimeObjectExchange.ParserParameters in_parser_parameters)
		{
			string file_name = Path.GetFileNameWithoutExtension(in_parser_parameters.HeaderFileName);

			// create C style header file
			in_parser_parameters.HeaderFile = new StreamWriter(in_parser_parameters.HeaderFileName, false);

			// start of file
			string define_name = "__" + file_name.ToUpper() + "_H";
			in_parser_parameters.HeaderFile.WriteLine("///////////////////////////////////////////////////////////////////////////////");
			in_parser_parameters.HeaderFile.WriteLine("// This header file was generated by the ROXParser");
			in_parser_parameters.HeaderFile.WriteLine("// at " + DateTime.Now.ToString());
			in_parser_parameters.HeaderFile.WriteLine();
			in_parser_parameters.HeaderFile.WriteLine("#ifndef " + define_name);
			in_parser_parameters.HeaderFile.WriteLine("#define " + define_name);
			in_parser_parameters.HeaderFile.WriteLine();

			in_parser_parameters.HeaderFile.WriteLine("/*****************************************************************************/");
			in_parser_parameters.HeaderFile.WriteLine("/* Packet types                                                              */");
			in_parser_parameters.HeaderFile.WriteLine("/*****************************************************************************/");

			// process packet type defines
			for (int i = 0; i < ((ParserRealtimeObjectCollection)m_root_class).Objects.Count; i++)
			{
				in_parser_parameters.HeaderFile.WriteLine("#define roxPT_" + ((ParserRealtimeObjectCollection)m_root_class).Objects[i].Name + " " + ((ParserRealtimeObjectCollection)m_root_class).Objects[i].PacketID.ToString());
			}
			in_parser_parameters.HeaderFile.WriteLine();

			// packet declarations
			in_parser_parameters.HeaderFile.WriteLine("/*****************************************************************************/");
			in_parser_parameters.HeaderFile.WriteLine("/* Packets                                                                   */");
			in_parser_parameters.HeaderFile.WriteLine("/*****************************************************************************/");
			in_parser_parameters.HeaderFile.WriteLine();

			// process objects
			foreach (ParserRealtimeObject obj in ((ParserRealtimeObjectCollection)m_root_class).Objects)
			{
				obj.CreateObjectDeclaration(in_parser_parameters);
			}

			// end of file
			in_parser_parameters.HeaderFile.WriteLine();
			in_parser_parameters.HeaderFile.WriteLine("#endif");
			in_parser_parameters.HeaderFile.WriteLine();

			in_parser_parameters.HeaderFile.Close();
		}

		#endregion

		#region · Header file creation ·

		/// <summary>
		/// Creates C type header declaration for this realtime object file
		/// </summary>
		/// <param name="in_parser_parameters"></param>
		public void CreateDefaultPacketEnabledFile(ParserRealtimeObjectExchange.ParserParameters in_parser_parameters)
		{
			byte[] enabled_bitfield = new byte[PacketConstants.MaxRealtimePacketCount / 8];
			string attribute_value;
			int i;
			byte packet_id;

			// create enabled bitfield
			for (i = 0; i < PacketConstants.MaxRealtimePacketCount / 8; i++)
				enabled_bitfield[i] = 0;

			// process packet type defines
			for (i = 0; i < ((ParserRealtimeObjectCollection)m_root_class).Objects.Count; i++)
			{
				attribute_value = ((ParserRealtimeObjectCollection)m_root_class).Objects[i].GetAttribute("EnabledByDefault");
				if (attribute_value != null && attribute_value == "1")
				{
					packet_id = ((ParserRealtimeObjectCollection)m_root_class).Objects[i].PacketID;
					enabled_bitfield[packet_id / 8] |= (byte)(1 << (packet_id % 8));
				}
			}

			// store enabled bitfield
			TextWriter file = new StreamWriter(in_parser_parameters.DefaultPacketEnableFileName, false);

			for (i = 0; i < PacketConstants.MaxRealtimePacketCount / 8; i++)
			{
				if (i != 0)
					file.Write(",");

				file.Write(string.Format("0x{0:X2}", enabled_bitfield[i]));
			}

			file.Close();
		}

		#endregion
	}
}