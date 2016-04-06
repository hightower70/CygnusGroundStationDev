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
using System.IO;
using System.Reflection;
using System.Windows;

namespace CygnusGroundStation
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
		protected int m_module_id;
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

		public int ModuleId
		{
			get { return m_module_id; }
			set { m_module_id = value; }
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

		public virtual ModuleSettingsInfo[] GetSettingsInfo()
		{
			return null;
		}

		

		#endregion

		#region Utility functions
		/*
    public string RemovePath(string in_filename)
    {
      
      if (in_filename != string.Empty && GetSafeDirectoryName(m_skin_settings.FileName) == GetSafeDirectoryName(in_filename))
      {
        return Path.GetFileName(in_filename);
      }
      else
        return in_filename;
    }
     */

		public string GetSafeDirectoryName(string in_file_name)
		{
			if (in_file_name.Length == 0)
				return "";

			return Path.GetDirectoryName(in_file_name);
		}
		#endregion
	}
	#endregion

}

