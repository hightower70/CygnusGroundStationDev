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
// System Setup Dialog
///////////////////////////////////////////////////////////////////////////////
using System.Windows;
using System;
using CygnusControls;

namespace CygnusGroundStation
{
	/// <summary>
	/// Interaction logic for SetupDialog.xaml
	/// </summary>
	public partial class SetupDialog : Window
	{
		static SettingsFileBase m_current_settings;

		public static SettingsFileBase CurrentSettings
		{
			get { return m_current_settings; }
		}

		public SetupDialog()
		{
			m_current_settings = new SettingsFileBase();
			m_current_settings.CopySettingsFrom(FrameworkSettingsFile.Default);

			InitializeComponent();
		}

		private void bAddModule_Click(object sender, RoutedEventArgs e)
		{
			ModuleManager.Default.RefreshModuleInfo();

			AddModuleDialog dialog = new AddModuleDialog();
			dialog.Owner = this;
			dialog.lbModules.DataContext = ModuleManager.Default;

			dialog.ShowDialog();
		}

		private void bOK_Click(object sender, RoutedEventArgs e)
		{
			ChangeSetupPage(null); 
			this.DialogResult = true;
		}

		private void tvSetupTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			int module_index;
			int page_index;

			if (e.NewValue is ModuleSettingsTreeInfo)
			{
				FrameworkElement new_page;

				// get module and page index
				module_index = (e.NewValue as ModuleSettingsTreeInfo).ModuleIndex;
				page_index = (e.NewValue as ModuleSettingsTreeInfo).FormIndex;

				// the module main node will display the same settings form as the first form
				if (page_index < 0)
					page_index = 0;

				// get new page
				new_page = ModuleManager.Default.Modules[module_index].GetSettingsInfo()[page_index].Form;

				ChangeSetupPage(new_page);
			}
		}

		private void ChangeSetupPage(FrameworkElement in_new_page)
		{
			FrameworkElement old_page;

			// get old page
			if (gSetupFormContainer.Children.Count > 0)
				old_page = (FrameworkElement)gSetupFormContainer.Children[0];
			else
				old_page = null;

			gSetupFormContainer.Children.Clear();

			// event arg
			SetupPageBase.SetupPageEventArgs event_args = new SetupPageBase.SetupPageEventArgs();
			event_args.NewPage = in_new_page;
			event_args.OldPage = old_page;

			// call changed event handler of the old page
			if (old_page is SetupPageBase)
				((SetupPageBase)old_page).OnSetupPageDeactivating(this, event_args);

			// add new page to the container
			if (in_new_page != null)
			{
				gSetupFormContainer.Children.Add(in_new_page);

				if (in_new_page is SetupPageBase)
					((SetupPageBase)in_new_page).OnSetupPageActivating(this, event_args);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			gSetupFormContainer.Children.Clear();

			SetupDialogSettings settings = m_current_settings.GetSettings<SetupDialogSettings>();

			settings.SetupDialogPos.SaveWindowPosition(this);

			m_current_settings.SetSettings(settings);
		}

		private void Window_Initialized(object sender, System.EventArgs e)
		{
			SetupDialogSettings settings = FrameworkSettingsFile.Default.GetSettings<SetupDialogSettings>();

			settings.SetupDialogPos.LoadWindowPosition(this);

			ModuleManager.Default.GenerateModuleSetupTreeInfo();

			tvSetupTree.DataContext = ModuleManager.Default;
		}
	}
}
