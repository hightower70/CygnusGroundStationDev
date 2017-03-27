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
// Base class for module main class
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.Settings;
using System.Reflection;
using System.Windows;

namespace CygnusControls
{
	#region Module base class
	public class ModuleBase
	{
		#region · Types ·

		public struct SetupInfo
		{
			public string Name;
			public FrameworkElement DisplayPanel;
		}

		#endregion

		#region Data Members

		protected SettingsFileBase m_module_settings;
		protected string m_module_name;
		protected object m_module_manager;
		protected Assembly m_module_assembly;

		#endregion

		#region Properties

		public SettingsFileBase ModuleSettings
		{
			get { return m_module_settings; }
			set { m_module_settings = value; }
		}

		public string ModuleName
		{
			get { return m_module_name; }
			set { m_module_name = value; }
		}


		public object ModuleManager
		{
			get { return m_module_manager; }
			set { m_module_manager = value; }
		}

		public Assembly ModuleAssembly
		{
			get { return m_module_assembly; }
			set { m_module_assembly = value; }
		}

		#endregion

		#region Constructor
		public ModuleBase()
		{
		}

		public ModuleBase(ModuleBase in_module_base, SettingsFileBase in_settings, object in_module_manager)
		{
			m_module_name = in_module_base.ModuleName;
			m_module_assembly = in_module_base.ModuleAssembly;

			m_module_manager = in_module_manager;
			m_module_settings = in_settings;
		}

		#endregion

		#region · Virtual functions to override ·

		/// <summary>
		/// Initializes module
		/// </summary>
		/// <returns></returns>
		public virtual bool Initialize()
		{
			return true;
		}

		/// <summary>
		/// Returns display name of the module
		/// </summary>
		/// <returns></returns>
		public virtual string GetDisplayName()
		{
			return "unknown";
		}

		/// <summary>
		/// Returns section name of the module for the configuration file
		/// </summary>
		/// <returns></returns>
		public virtual string GetSettingsName()
		{
			return null;
		}

		/// <summary>
		/// Gets module settings info
		/// </summary>
		/// <returns>Settings info array</returns>
		public virtual ModuleSettingsInfo[] GetSettingsInfo()
		{
			return null;
		}

		/// <summary>
		/// Gets display panel with the given name. If panel can't be found it returns null.
		/// </summary>
		/// <param name="in_name">Name of the panel to display</param>
		/// <returns>Panel to display</returns>
		public virtual FrameworkElement GetDisplayPanel(string in_name)
		{
			return null;
		}

		/// <summary>
		/// Starts module operation
		/// </summary>
		public virtual void Start()
		{
		}

		/// <summary>
		/// Stops module operation
		/// </summary>
		public virtual void Stop()
		{
		}
		

		#endregion

		#region Utility functions

		public void CopyFrom(ModuleBase in_module)
		{
			m_module_name = in_module.ModuleName;
			m_module_assembly = in_module.ModuleAssembly;

			m_module_manager = in_module.ModuleManager;
			m_module_settings = in_module.ModuleSettings;
		}

		#endregion
	}
	#endregion

}

