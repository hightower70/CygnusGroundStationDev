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
// Data provider class for SetupCommunication dialog
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.DeviceCommunication;
using CygnusControls;
using CygnusGroundStation.Dialogs;
using System.Collections.Generic;

namespace CygnusGroundStation
{
	class SetupCommunicationDataProvider
	{
		private SetupCommunicationSettings m_settings;

		public List<UARTEnumerator.DeviceInfo> AvailablePorts
		{
			get { return UARTEnumerator.GetAllCOMPorts(); }
		}

		public SetupCommunicationSettings Settings
		{
			get { return m_settings; }
		}

		public SetupCommunicationDataProvider()
		{
			Load();
		}

		public void Load()
		{
			m_settings = SetupDialog.CurrentSettings.GetSettings<SetupCommunicationSettings>();
		}

		public void Save()
		{
			SetupDialog.CurrentSettings.SetSettings(m_settings);
		}
	}
}
