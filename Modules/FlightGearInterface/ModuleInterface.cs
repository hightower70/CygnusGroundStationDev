///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013 Laszlo Arvai. All rights reserved.
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
// FlightGear Module interface class
///////////////////////////////////////////////////////////////////////////////
using CygnusControls;
using CygnusGroundStation;

namespace FlightGearInterface
{
	public class ModuleInterface : ModuleBase
	{
		#region · Data members ·
		private ModuleSettingsInfo[] m_module_settings_info;
		private FlightGearThread m_thread;
		#endregion

		public ModuleInterface()
		{
			ModuleName = GetDisplayName();
			m_module_settings_info = new ModuleSettingsInfo[]
			{
				new ModuleSettingsInfo("Connection", new SetupConnection(), null),
				new ModuleSettingsInfo("Config file", new SetupConfigFile(), null),
				new ModuleSettingsInfo("About", new SetupAbout(), null)
			};
		}

		override public string GetDisplayName()
		{
			return "FlightGear Interface";
		}

		public override ModuleSettingsInfo[] GetSettingsInfo()
		{
			return m_module_settings_info;
		}

		public override string GetSettingsName()
		{
			return "FlightGear";
		}

		public override void Start()
		{
			FlightGearSettings settings;

			m_thread = new FlightGearThread();

			settings = ModuleSettings.GetSettings<FlightGearSettings>();
			m_thread.Configure(settings);
			m_thread.Open();
		}

		public override void Stop()
		{
			if (m_thread != null)
			{
				m_thread.Close();
				m_thread = null;
			}
		}
	}
}

