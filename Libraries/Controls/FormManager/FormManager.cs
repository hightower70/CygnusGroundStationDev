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
using CommonClassLibrary.RealtimeObjectExchange;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Threading;

namespace CygnusControls
{
	public class FormManager
	{
		#region · Data members ·

		private Grid m_parent;
		private FrameworkElement m_current_form;
		private static FormManager m_default = null;
		private List<FormInfo> m_available_forms = new List<FormInfo>();

		private DispatcherTimer m_dispatcher_timer;
		private List<IRealtimeObjectRefresher> m_object_refreshers;

		#endregion

		#region · Constructor ·

		/// <summary>
		/// Defult constuctor
		/// </summary>
		public FormManager()
		{
			m_object_refreshers = new List<IRealtimeObjectRefresher>();

			//  DispatcherTimer setup
			m_dispatcher_timer = new DispatcherTimer();
			m_dispatcher_timer.Tick += new EventHandler(dispatcherTimer_Tick);
			m_dispatcher_timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
			m_dispatcher_timer.Start();
		}
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

		#region · Realtime object refresher ·

		public void RealtimeObjectRefresherAdd(IRealtimeObjectRefresher in_provider )
		{
			lock(m_object_refreshers)
			{
				m_object_refreshers.Add(in_provider);
			}
		}

		public void RealtimeObjectRefresherRemove(IRealtimeObjectRefresher in_provider)
		{
			lock(m_object_refreshers)
			{
				m_object_refreshers.Remove(in_provider);
			}
		}

		/// <summary>
		/// Stops realtime object provider refresh service
		/// </summary>
		public void ObjectRefreshStop()
		{
			m_dispatcher_timer.Stop();
		}

		/// <summary>
		/// Starts realtime object refresh servive
		/// </summary>
		public void ObjectRefreshStart()
		{
			m_dispatcher_timer.Start();
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			lock(m_object_refreshers)
			{
				for(int i=0; i<m_object_refreshers.Count;i++)
				{
					m_object_refreshers[i].RefreshObject();
				}
			}
		}

		static public void RealtimeObjectProvidersRegister(FrameworkElement in_parent)
		{
			// set resource parents and add sideband data
			foreach (DictionaryEntry entry in in_parent.Resources)
			{
				if (entry.Value is IRealtimeObjectProvider)
					((IRealtimeObjectProvider)entry.Value).Register(in_parent, entry.Key.ToString());
			}
		}

		static public void RealtimeObjectProvidersDeregister(FrameworkElement in_parent)
		{
			foreach (DictionaryEntry entry in in_parent.Resources)
			{
				if (entry.Value is IRealtimeObjectProvider)
					((IRealtimeObjectProvider)entry.Value).Deregister(in_parent);
			}
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
		/// <param name="in_form_file_name">Form (XAML) file name to load</param>
		public void LoadForm(string in_form_file_name, string in_modules_path, string in_forms_path)
		{
			// create parser context for XAML load
			var pc = new ParserContext();
			pc.BaseUri = new Uri(in_modules_path, UriKind.Absolute);
			string form_full_path = in_form_file_name;

			// try to rebuild form full path using current settings
			if(!File.Exists(form_full_path))
			{
				string form_filename = Path.GetFileName(form_full_path);

				form_full_path = Path.Combine(in_forms_path, form_filename);
			}

			// deregister form
			if (m_current_form != null)
				RealtimeObjectProvidersDeregister(m_current_form);

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

			// register form
			if (m_current_form != null)
				RealtimeObjectProvidersRegister(m_current_form);
		}

		/// <summary>
		/// Closes current from
		/// </summary>
		public void CloseCurrentForm()
		{
			if (m_current_form != null)
				RealtimeObjectProvidersDeregister(m_current_form);
		}

		/// <summary>
		/// Refreshes form info collection
		/// </summary>
		public void RefreshFormInfo(string in_path)
		{
			string[] form_files;
			FormInfo form_info;
			
			// init
			m_available_forms.Clear();

			try
			{
				form_files = Directory.GetFiles(in_path, "*.xaml");

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
