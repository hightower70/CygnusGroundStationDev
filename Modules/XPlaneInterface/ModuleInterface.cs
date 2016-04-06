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
// X-Plane simulator interface class
///////////////////////////////////////////////////////////////////////////////
using CygnusGroundStation;
using CommonClassLibrary.RealtimeObjectExchange;
using System.IO;

namespace XPlaneInterface
{
	public class ModuleInterface : ModuleBase
	{
		#region · Data members ·
		private ModuleSettingsInfo[] m_module_settings_info;
		private XPlaneCommunicator m_communicator = new XPlaneCommunicator();
		#endregion

		public ModuleInterface()
		{
			ModuleName = GetDisplayName();
			m_module_settings_info = new ModuleSettingsInfo[]
			{
				new ModuleSettingsInfo("Interface", new SetupInterfaceSettings(), null),
				new ModuleSettingsInfo("About", new SetupAbout(), null)
			};
		}

		override public bool Initialize()
		{
			m_communicator.Initialize(base.ModuleSettings);
			return true;
		}

		public override void Start()
		{
			m_communicator.Start();
		}

		public override void Stop()
		{
			m_communicator.Stop();
		}

		override public string GetDisplayName()
		{
			return "X-Plane Interface";
		}

		public override ModuleSettingsInfo[] GetSettingsInfo()
		{
			return m_module_settings_info;
		}

		public override string GetSettingsName()
		{
			return "XPlane";
		}

	}
}
