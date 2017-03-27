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
// Lidar emulator interface class
///////////////////////////////////////////////////////////////////////////////
using CygnusControls;
using System.Windows;

namespace OccupancyGrid
{
	public class ModuleInterface : ModuleBase
	{
		#region · Data members ·
		private ModuleSettingsInfo[] m_module_settings_info;
		private OccupancyGridThread m_thread;
		#endregion

		public ModuleInterface()
		{
			ModuleName = GetDisplayName();
			m_module_settings_info = new ModuleSettingsInfo[]
			{
				//new ModuleSettingsInfo("Connection", new SetupConnection(), null),
				//new ModuleSettingsInfo("Config file", new SetupConfigFile(), null),
				new ModuleSettingsInfo("About", new SetupAbout(), null)
			};
		}

		/// <summary>
		/// Gets module display name
		/// </summary>
		/// <returns></returns>
		override public string GetDisplayName()
		{
			return "Occupancy Grid";
		}

		public override ModuleSettingsInfo[] GetSettingsInfo()
		{
			return m_module_settings_info;
		}

		public override string GetSettingsName()
		{
			return "Occupancy Grid";
		}

		public override FrameworkElement GetDisplayPanel(string in_name)
		{
			if (in_name == "OccupancyGridDisplay")
			{
				return new OccupancyGridDisplayPanel();
			}
			else
			{
				return null;
			}
		}

		public override void Start()
		{
			//LidarEmulatorSettings settings;

			m_thread = new OccupancyGridThread();

			//settings = ModuleSettings.GetSettings<LidarEmulatorSettings>();
			//m_thread.Configure(settings);
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

