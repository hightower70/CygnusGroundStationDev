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
// Root class for parsing device settings XML
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace CommonClassLibrary.DeviceSettings
{
	public class ParserDeviceSettingsRoot : IParserDeviceSettingsBase
	{
		#region · Data members ·
		private ParserDeviceSettings m_parent;
		private List<ParserDeviceSettingsGroup> m_groups = new List<ParserDeviceSettingsGroup>();
		private List<ParserDeviceSettingsEnumDefs> m_enum_defs = new List<ParserDeviceSettingsEnumDefs>();
		private Dictionary<string, int> m_enumdefs_lookup = new Dictionary<string, int>();
		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="in_parent"></param>
		public ParserDeviceSettingsRoot(ParserDeviceSettings in_parent)
		{
			m_parent = in_parent;
		}

		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets parent class of this root
		/// </summary>
		public ParserDeviceSettings Parent
		{
			get { return m_parent; }
		}

		/// <summary>
		/// Gets child groups
		/// </summary>
		public List<ParserDeviceSettingsGroup> Groups
		{
			get { return m_groups; }
		}

		/// <summary>
		/// Gets enum definitions
		/// </summary>
		public List<ParserDeviceSettingsEnumDefs> EnumDefs
		{
			get { return m_enum_defs; }
		}

		#endregion

		#region · Parser routines ·

		/// <summary>
		/// Adds settings group to the root
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_group"></param>
		public void AddGroup(XPathNavigator in_element, ParserDeviceSettingsGroup in_group)
		{
			// store group
			m_groups.Add(in_group);
			in_group.SetRoot(this);
		}

		/// <summary>
		/// Adds enum definition to the root
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_enum_defs"></param>
		public void AddEnumDefs(XPathNavigator in_element, ParserDeviceSettingsEnumDefs in_enum_defs)
		{
			int index;

			// store group
			m_enum_defs.Add(in_enum_defs);
			index = m_enum_defs.Count - 1;
			m_enumdefs_lookup.Add(in_enum_defs.ID, index);
		}

		/// <summary>
		/// Gets index of the enumdefs
		/// </summary>
		/// <param name="in_name"></param>
		/// <returns></returns>
		internal int GetEnumDefIndex(string in_name)
		{
			return m_enumdefs_lookup[in_name];
		}

		/// <summary>
		/// Updates current settings values from raw binary file
		/// </summary>
		/// <param name="in_binary_value_file"></param>
		public void UpdateValuesFromBinaryFile(byte[] in_binary_value_file)
		{
			// process all groups
			for (int i = 0; i < m_groups.Count; i++)
			{
				m_groups[i].UpdateValuesFromBinaryFile(in_binary_value_file);
			}
		}

#endregion

#region · IParserDeviceSettingsBase interface ·

/// <summary>
/// Gets type of this class
/// </summary>
/// <returns></returns>
public ParserDeviceSettings.ClassType GetClassType()
		{
			return ParserDeviceSettings.ClassType.Root;
		}

		/// <summary>
		/// Generates value offsets
		/// </summary>
		/// <param name="inout_current_offset"></param>
		public void GenerateOffsets(ref int inout_current_offset)
		{
			// process all groups
			for (int i = 0; i < m_groups.Count; i++)
			{
				m_groups[i].GenerateOffsets(ref inout_current_offset);
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
			// process enums
			if (m_enum_defs.Count > 0)
			{
				in_header_file.AppendLine("// Enum definitions");

				for(int i=0; i< m_enum_defs.Count;i++)
				{
					m_enum_defs[i].GenerateFiles(in_header_file, in_value_info_file, in_default_value_file);
				}
			}

			if (m_groups.Count > 0)
			{
				in_header_file.AppendLine("// Value definitions");

				// process all groups
				for (int i = 0; i < m_groups.Count; i++)
				{
					m_groups[i].GenerateFiles(in_header_file, in_value_info_file, in_default_value_file);
				}
			}
		}
		#endregion

	}
}
