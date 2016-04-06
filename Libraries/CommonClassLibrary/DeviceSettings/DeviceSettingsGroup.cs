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
using System.Collections.Generic;
using System.Xml.XPath;

namespace CommonClassLibrary.DeviceSettings
{
	public class DeviceSettingsGroup
	{
		#region · Data members ·
		private string m_name;
		private string m_display_name;
		private List<DeviceSettingsGroup> m_subgroups = new List<DeviceSettingsGroup>();
		private List<DeviceSettingValue> m_values = new List<DeviceSettingValue>();
		private Dictionary<string, int> m_name_lookup = new Dictionary<string, int>();
		#endregion

		#region · Properties ·
		/// <summary>
		/// Name of the settings group
		/// </summary>
		public string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// Gets settings values
		/// </summary>
		public List<DeviceSettingValue> Values
		{
			get { return m_values; }
		}

		/// <summary>
		/// Gets child groups
		/// </summary>
		public List<DeviceSettingsGroup> Groups
		{
			get { return m_subgroups; }
		}

		/// <summary>
		/// Gets display name of the setting value (user readable name of the settings)
		/// </summary>
		public string DisplayName
		{
			get { return m_display_name; }
		}

		#endregion

		public void AddGroup(XPathNavigator in_element, DeviceSettingsGroup in_group)
		{
			// only group or values can be defined but not both
			if (m_values.Count > 0)
			{
				DeviceSettingsParserException exception = new DeviceSettingsParserException(in_element);
				exception.SetInvalidParentGroupError(m_name);
				throw exception;
			}

			// store group
			m_subgroups.Add(in_group);
		}

		public void ParseXML(XPathNavigator in_element)
		{
			// get name
			m_name = XMLAttributeParser.ConvertAttributeToString(in_element, "Name", XMLAttributeParser.atObligatory);

			// get display name
			string display_name = XMLAttributeParser.ConvertAttributeToString(in_element, "DisplayName", 0);
			if (!string.IsNullOrEmpty(display_name))
				m_display_name = display_name;
			else
				m_display_name = m_name;
		}


		public void AddValue(XPathNavigator in_element, DeviceSettingValue in_value)
		{
			// only group or values can be defined but not both
			if (m_subgroups.Count > 0)
			{
				DeviceSettingsParserException exception = new DeviceSettingsParserException(in_element);
				exception.SetInvalidParentGroupError(m_name);
				throw exception;
			} 

			// store value
			m_values.Add(in_value);
		}
	}
}
