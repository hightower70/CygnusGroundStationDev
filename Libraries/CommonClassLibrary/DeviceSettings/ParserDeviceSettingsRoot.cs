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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace CommonClassLibrary.DeviceSettings
{
	public class ParserDeviceSettingsRoot : IParserDeviceSettingsType
	{
		#region · Data members ·
		private List<ParserDeviceSettingsGroup> m_subgroups = new List<ParserDeviceSettingsGroup>();
		private List<ParserDeviceSettingsEnumDefs> m_enum_defs = new List<ParserDeviceSettingsEnumDefs>();
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets child groups
		/// </summary>
		public List<ParserDeviceSettingsGroup> Groups
		{
			get { return m_subgroups; }
		}

		/// <summary>
		/// Gets enum definitions
		/// </summary>
		public List<ParserDeviceSettingsEnumDefs> EnumDefs
		{
			get { return m_enum_defs; }
		}
		#endregion

		#region · Parser rouitnes ·

		/// <summary>
		/// Adds settings group to the root
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_group"></param>
		public void AddGroup(XPathNavigator in_element, ParserDeviceSettingsGroup in_group)
		{
			// store group
			m_subgroups.Add(in_group);
		}

		/// <summary>
		/// Adds enum definition to the root
		/// </summary>
		/// <param name="in_element"></param>
		/// <param name="in_group"></param>
		public void AddEnumDefs(XPathNavigator in_element, ParserDeviceSettingsEnumDefs in_group)
		{
			// store group
			m_enum_defs.Add(in_group);
		}
		#endregion

		#region · IParserDeviceSettingsType interface ·

		/// <summary>
		/// Gets type of this class
		/// </summary>
		/// <returns></returns>
		public ParserDeviceSettings.ClassType GetClassType()
		{
			return ParserDeviceSettings.ClassType.Root;
		}
		#endregion

	}
}
