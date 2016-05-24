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
// Parser routines for XML elements attributes
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.XPath;

namespace CommonClassLibrary.DeviceSettings
{
	public class ParserDeviceSettingsGroup : IParserDeviceSettingsBase
	{
		#region · Data members ·

		private ParserDeviceSettingsRoot m_root;

		private string m_id;
		private string m_name;
		private string m_display_name;
		private List<ParserDeviceSettingValue> m_values = new List<ParserDeviceSettingValue>();
		private Dictionary<string, int> m_name_lookup = new Dictionary<string, int>();
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets reference name of the enum value
		/// </summary>
		public string ID
		{
			get { return m_id; }
		}

		/// <summary>
		/// Name of the settings group
		/// </summary>
		public string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// Gets root (parent) of this group
		/// </summary>
		public ParserDeviceSettingsRoot Root
		{
			get { return m_root; }
		}

		/// <summary>
		/// Gets settings values
		/// </summary>
		public List<ParserDeviceSettingValue> Values
		{
			get { return m_values; }
		}

		/// <summary>
		/// Gets display name of the setting value (user readable name of the settings)
		/// </summary>
		public string DisplayName
		{
			get { return m_display_name; }
		}

		#endregion

		#region · Parser routines ·

		/// <summary>
		/// Parses group element
		/// </summary>
		/// <param name="in_element"></param>
		public void ParseXML(XPathNavigator in_element)
		{
			// get name
			m_id = XMLAttributeParser.ConvertAttributeToString(in_element, "ID", XMLAttributeParser.atObligatory);

			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);

			// get display name
			string display_name = XMLAttributeParser.ConvertAttributeToString(in_element, "DisplayName", 0);
			if (!string.IsNullOrEmpty(display_name))
				m_display_name = display_name;
			else
				m_display_name = m_name;
		}

		/// <summary>
		/// Adds value to the group
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_value"></param>
		public void AddValue(XPathNavigator in_element, ParserDeviceSettingValue in_value)
		{
			// store value
			m_values.Add(in_value);
			in_value.SetParentGroup(this);
		}

		/// <summary>
		/// Sets root device settings for this group
		/// </summary>
		/// <param name="in_root"></param>
		internal void SetRoot(ParserDeviceSettingsRoot in_root)
		{
			m_root = in_root;
		}

		/// <summary>
		/// Updates current settings values from raw binary file
		/// </summary>
		/// <param name="in_binary_value_file"></param>
		public void UpdateValuesFromBinaryFile(byte[] in_binary_value_file)
		{
			// process all groups
			for (int i = 0; i < m_values.Count; i++)
			{
				m_values[i].UpdateValuesFromBinaryFile(in_binary_value_file);
			}
		}
		#endregion

		#region · IParserDeviceSettingsType interface ·

		/// <summary>
		/// Gets type of this class
		/// </summary>
		/// <returns></returns>
		public ParserDeviceSettings.ClassType GetClassType()
		{
			return ParserDeviceSettings.ClassType.Group;
		}

		/// <summary>
		/// Generates value offsets
		/// </summary>
		/// <param name="inout_current_value_offset"></param>
		public void GenerateOffsets(ref int inout_current_offset)
		{
			// process all groups
			for (int i = 0; i < m_values.Count; i++)
			{
				m_values[i].GenerateOffsets(ref inout_current_offset);
			}
		}

		/// <summary>
		/// Generates declaration and data files
		/// </summary>
		/// <param name="in_header_file"></param>
		/// <param name="in_default_value_file"></param>
		/// <param name="in_value_info_file"></param>
		public void GenerateFiles(StringBuilder in_header_file, MemoryStream in_value_info_file, MemoryStream in_default_value_file)
		{
			// process all groups
			for (int i = 0; i < m_values.Count; i++)
			{
				m_values[i].GenerateFiles(in_header_file, in_value_info_file, in_default_value_file);
			}
		}

		#endregion
	}
}
