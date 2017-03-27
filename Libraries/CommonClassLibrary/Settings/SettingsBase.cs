///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2014 Laszlo Arvai. All rights reserved.
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
// Interface for settings data class
///////////////////////////////////////////////////////////////////////////////

namespace CommonClassLibrary.Settings
{
	public abstract class SettingsBase
	{
		private string m_module_name;
		private string m_section_name;

		internal string ModuleName { get { return m_module_name; } }
		internal string SectionName { get { return m_section_name; } }

		public SettingsBase(string in_module_name, string in_section_name)
		{
			m_module_name = in_module_name;
			m_section_name = in_section_name;
		}

		abstract public void SetDefaultValues();
	}
}
