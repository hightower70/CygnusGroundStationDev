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
// Displayed form manager class
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CygnusGroundStation
{
	public class FormManager
	{
		#region · Data members ·

		private Grid m_parent;
		private FrameworkElement m_current_form;
		private static FormManager m_default = null;
		private List<FormInfo> m_available_forms = new List<FormInfo>();

		#endregion

		#region · Singleton members ·

		/// <summary>
		/// Singleton instance
		/// </summary>
		public static FormManager Default
		{
			get
			{
				if (m_default == null)
				{
					m_default = new FormManager();
				}

				return m_default;
			}
		}
		#endregion

		#region · Properties ·

		/// <summary>
		/// Form information collection. (RefreshFormInfo must be called before using this property)
		/// </summary>
		public List<FormInfo> AvailableForms
		{
			get { return m_available_forms; }
		}

		#endregion

		#region · Public Member functions ·

		/// <summary>
		/// Sets parent container of forms
		/// </summary>
		/// <param name="in_parent"></param>
		public void SetFormParent(Grid in_parent)
		{
			m_parent = in_parent;
		}


		/// <summary>
		/// Loads given form
		/// </summary>
		/// <param name="in_form_path">Form (XAML) file name to load</param>
		public void LoadForm(string in_form_path)
		{
			MainGeneralSettings main_settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			// create parser context for XAML load
			var pc = new ParserContext();
			pc.BaseUri = new Uri(main_settings.ModulesPath, UriKind.Absolute);
			string form_full_path = in_form_path;

			// try to rebuild form full path using current settings
			if(!File.Exists(form_full_path))
			{
				string form_filename = Path.GetFileName(form_full_path);

				form_full_path = Path.Combine(main_settings.FormsPath, form_filename);
			}

			// load form
			try
			{
				Stream sr = File.OpenRead(form_full_path);
				m_current_form = (FrameworkElement)XamlReader.Load(sr, pc);
			}
			catch
			{
				m_current_form = null;
			}

			// remove forms
			m_parent.Children.Clear();
			
			// add new form
			if(m_current_form != null)
				m_parent.Children.Add(m_current_form);
		}

		/// <summary>
		/// Refreshes form info collection
		/// </summary>
		public void RefreshFormInfo()
		{
			string[] form_files;
			FormInfo form_info;
			String path;
			MainGeneralSettings settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			// init
			m_available_forms.Clear();
			path = settings.FormsPath;

			try
			{
				form_files = Directory.GetFiles(path, "*.xaml");

				for (int i = 0; i < form_files.Length; i++)
				{
					form_info = new FormInfo();

					form_info.FormName = Path.GetFileNameWithoutExtension(form_files[i]);
					form_info.FormPath = form_files[i];

					m_available_forms.Add(form_info);
				}
			}
			catch
			{
			}
		}

		#endregion
	}
}
