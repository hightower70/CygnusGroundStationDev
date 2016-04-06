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
// Data provider class for SetupForms dialog
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusGroundStation
{
	class SetupFormsDataProvider
	{
		private List<FormInfo> m_available_forms;
		private SetupFormSettings m_settings;

		public List<FormInfo> AvailableForms
		{
			get { return m_available_forms; }
		}

		public SetupFormSettings Settings
		{ 
			get { return m_settings; } 
		}

		public SetupFormsDataProvider()
		{
			Load();
		}

		public void Load()
		{
			FormManager.Default.RefreshFormInfo();
			m_available_forms = FormManager.Default.AvailableForms;

			m_settings = SetupDialog.CurrentSettings.GetSettings<SetupFormSettings>();
		}

		public void Save()
		{
			SetupDialog.CurrentSettings.SetSettings(m_settings);
		}
	}
}
