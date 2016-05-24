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
// Parser routines for enumeration value definition
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.XPath;

namespace CommonClassLibrary.DeviceSettings
{
	public class ParserDeviceSettingsEnumDefs : IParserDeviceSettingsBase
	{
		#region · Data members ·
		private string m_id;
		private string m_name;
		private List<ParserDeviceSettingsEnumValue> m_values = new List<ParserDeviceSettingsEnumValue>();
		#endregion

		#region · Properties ·
		
		/// <summary>
		/// Reference name of the enum definition
		/// </summary>
		public string ID
		{
			get { return m_id; }
		}

		/// <summary>
		/// Display name of the enum definition
		/// </summary>
		public string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// Gets settings values
		/// </summary>
		public List<ParserDeviceSettingsEnumValue> Values
		{
			get { return m_values; }
		}

		#endregion

		#region · Parser routines ·

		/// <summary>
		/// Parses XML element containing enumeration definition
		/// </summary>
		/// <param name="in_element"></param>
		public void ParseXML(XPathNavigator in_element)
		{
			// get id
			m_id = XMLAttributeParser.ConvertAttributeToString(in_element, "ID", XMLAttributeParser.atObligatory);

			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);
		}

		/// <summary>
		/// Adds new enum value to the collection
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_value"></param>
		public void AddValue(XPathNavigator in_element, ParserDeviceSettingsEnumValue in_value)
		{
			// store value
			m_values.Add(in_value);
		}
		#endregion

		#region · IParserDeviceSettingsType interface ·

		/// <summary>
		/// Gets type of this class
		/// </summary>
		/// <returns></returns>
		public ParserDeviceSettings.ClassType GetClassType()
		{
			return ParserDeviceSettings.ClassType.EnumDefs;
		}

		public void GenerateOffsets(ref int inout_current_offset)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Generates declaration and data files
		/// </summary>
		/// <param name="in_header_file"></param>
		/// <param name="in_default_value_file"></param>
		/// <param name="in_value_info_file"></param>
		public void GenerateFiles(StringBuilder in_header_file, MemoryStream in_value_info_file, MemoryStream in_default_value_file)
		{
			for (int i = 0; i < m_values.Count; i++)
			{
				string declaration = "#define cfgENUM_" + m_id.ToUpper() + "_" +m_values[i].ID.ToUpper() + " " + i.ToString();

				in_header_file.AppendLine(declaration);
			}

			in_header_file.AppendLine();
		}

		#endregion
	}
}
