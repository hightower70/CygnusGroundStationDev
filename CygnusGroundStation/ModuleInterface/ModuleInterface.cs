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
// Main module interface
///////////////////////////////////////////////////////////////////////////////
using CygnusGroundStation.Dialogs;
using System.Windows;

namespace CygnusGroundStation
{
	public class ModuleInterface : ModuleBase
	{
		ModuleSettingsInfo[]	m_module_settings_info;

		public ModuleInterface()
		{
			ModuleName = GetDisplayName();
			m_module_settings_info = new ModuleSettingsInfo[]
			{
				new ModuleSettingsInfo("General", new SetupGeneral(), null),
				new ModuleSettingsInfo("Forms", new SetupForms(), null),
				new ModuleSettingsInfo("Communication", new SetupCommunication(), null),
				new ModuleSettingsInfo("About", new AboutControl(), null)
			};
		}


		override public string GetDisplayName()
		{
			return "Gygnus Ground Station";
		}

		public override ModuleSettingsInfo[] GetSettingsInfo()
		{
			return m_module_settings_info;
		}
	}
}
