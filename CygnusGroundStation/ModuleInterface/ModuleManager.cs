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
// Module manager (Load, start, stop module, enumerate modules, etc.)
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;

namespace CygnusGroundStation
{
	/// <summary>
	/// Manages modules (enumerate, load, etc.)
	/// </summary>
	public class ModuleManager
	{
		#region · Data members ·

		private static ModuleManager m_default = null;
		private List<ModuleBase> m_modules = new List<ModuleBase>();
		private ObservableCollection<ModuleSettingsTreeInfo> m_module_setup_tree_info = new ObservableCollection<ModuleSettingsTreeInfo>();
		private List<ModuleInfo> m_available_modules = new List<ModuleInfo>();

		#endregion

		#region · Constructor ·
		public ModuleManager()
		{
			m_modules.Add(new ModuleInterface());
		}

		#endregion

		#region · Singleton members ·

		/// <summary>
		/// Singleton instance
		/// </summary>
		public static ModuleManager Default
		{
			get
			{
				if (m_default == null)
				{
					m_default = new ModuleManager();
				}

				return m_default;
			}
		}
		#endregion

		#region · Deep copy ·

		/// <summary>
		/// Copy modules information from another instance of this class
		/// </summary>
		/// <param name="in_module_manager"></param>
		public void CopyModulesFrom(ModuleManager in_module_manager, SettingsFileBase in_settings)
		{
			// copy module list
			m_modules.Clear();

			for (int i = 0; i < in_module_manager.m_modules.Count; i++)
			{
				ModuleBase module = (ModuleBase)Activator.CreateInstance(in_module_manager.m_modules[i].GetType());

				module.CopyFrom(in_module_manager.m_modules[i]);
				module.ModuleManager = this;
				module.ModuleSettings = in_settings;

				m_modules.Add(module);
			}

			// clear other members
			m_module_setup_tree_info.Clear();
			m_available_modules.Clear();
		}

		#endregion

		#region · Properties ·

		public List<ModuleBase> Modules
		{
			get { return m_modules; }
		}

		/// <summary>
		/// Module information collection. (RefreshModuleInfo must be called before using this property)
		/// </summary>
		public List<ModuleInfo> AvailableModules
		{
			get { return m_available_modules; }
		}

		/// <summary>
		/// Gets ModuleSetupTreeInfo collection for displaying module setup information in a tree
		/// </summary>
		public ObservableCollection<ModuleSettingsTreeInfo> ModuleSetupTreeInfo
		{
			get { return m_module_setup_tree_info; }
		}

		#endregion

		#region · Modules handler functions ·

		/// <summary>
		/// Loads all modules specified in the settings file
		/// </summary>
		/// <returns></returns>
		public bool ModulesLoad()
		{
			List<SettingsFileBase.ModuleInfo> module_list = FrameworkSettingsFile.Default.GetModuleList();

			m_modules.Clear();
			m_modules.Add(new ModuleInterface()); // add main module

			for (int i = 0; i < module_list.Count; i++)
			{
				ModuleBase module = new ModuleBase();

				LoadModule(module_list[i].ModuleName, out module);

				m_modules.Add(module);
			}

			return true;
		}

		/// <summary>
		/// Stops all modules
		/// </summary>
		public void ModulesStop()
		{
			for (int i = 0; i < m_modules.Count; i++)
			{
				m_modules[i].Stop();
			}
		}

		/// <summary>
		/// Initializes modules and starts them
		/// </summary>
		public void ModulesInitializeAndStart()
		{
			// initialize modules
			for (int i = 0; i < m_modules.Count; i++)
			{
				m_modules[i].Initialize();
			}

			// Starts modules
			for (int i = 0; i < m_modules.Count; i++)
			{
				m_modules[i].Start();
			}
		}

		#endregion

		#region · Setup (options settings) helper functions ·


		/// <summary>
		/// Refreshes module info collection
		/// </summary>
		public void SetupRefreshModuleInfo()
		{
			string[] module_files;
			String path = GetModulePath();
			ModuleBase main_class;

			module_files = Directory.GetFiles(path, "*.module");

			m_available_modules.Clear();
			for (int i = 0; i < module_files.Length; i++)
			{
				if (LoadModule(Path.GetFileNameWithoutExtension(module_files[i]), out main_class))
				{
					ModuleInfo module_info = new ModuleInfo();

					module_info.DLLName = Path.GetFileNameWithoutExtension(module_files[i]).ToLower();
					module_info.Description = main_class.GetDisplayName();
					module_info.VersionString = main_class.ModuleAssembly.GetName().Version.ToString();
					module_info.SectionName = Path.GetFileNameWithoutExtension(module_files[i]);

					m_available_modules.Add(module_info);
				}
			}
		}

		/// <summary>
		/// Add module in setup operation
		/// </summary>
		/// <param name="in_module_name"></param>
		public void SetupAddModule(string in_module_name)
		{
			// load module
			ModuleBase module = new ModuleBase();

			LoadModule(in_module_name, out module);

			m_modules.Add(module);

			// create tree information
			SetupCreateModuleTreeInfo(module, m_modules.Count - 1);
		}

		public void SetupRemoveModule(int in_module_index)
		{
			// do not remove main module
			if (in_module_index == 0)
				return;

			// remove module
			m_module_setup_tree_info.RemoveAt(in_module_index);
    }

		/// <summary>
		/// Generates ModuleSetupTreeInfo collection
		/// </summary>
		public void SetupBuildTreeInfo()
		{
			m_module_setup_tree_info.Clear();

			for(int module_index = 0; module_index < m_modules.Count;module_index++)
			{
				SetupCreateModuleTreeInfo(m_modules[module_index], module_index);
      }
		}

		/// <summary>
		/// Creates tree info of one module
		/// </summary>
		/// <param name="in_module"></param>
		/// <param name="in_index"></param>
		private void SetupCreateModuleTreeInfo(ModuleBase in_module, int in_index)
		{
			ModuleSettingsInfo[] settings_info = in_module.GetSettingsInfo();

			if (settings_info.Length > 0)
			{
				// create tree item
				ModuleSettingsTreeInfo info = new ModuleSettingsTreeInfo(in_module.GetDisplayName(), settings_info[0], in_index, -1);
				info.IsExpanded = true;

				for (int page_index = 0; page_index < settings_info.Length; page_index++)
				{
					info.AddChild(new ModuleSettingsTreeInfo(settings_info[page_index], in_index, page_index));
				}

				m_module_setup_tree_info.Add(info);
			}
		}

		#endregion

		#region · Non-public members ·

		/// <summary>
		/// Loads one module file
		/// </summary>
		/// <param name="in_filename"></param>
		/// <param name="out_interface_class"></param>
		/// <returns></returns>
		private bool LoadModule(String in_filename, out ModuleBase out_interface_class)
		{
			String filename;
			Assembly module_assembly;

			// generate filename
			filename = GetModulePath();
			filename = Path.Combine(filename, in_filename);
			filename += ".module";

			// load dll
			try
			{
				// load the assembly
				module_assembly = Assembly.LoadFrom(filename);

				// Walk through each type in the assembly looking for our class
				foreach (Type type in module_assembly.GetTypes())
				{
					if (type.IsClass == true)
					{
						if (type.FullName.EndsWith(".ModuleInterface"))
						{
							// create an instance of the object
							out_interface_class = (ModuleBase)Activator.CreateInstance(type);
							out_interface_class.ModuleName = in_filename.ToLower();
							out_interface_class.ModuleAssembly = module_assembly;
							out_interface_class.ModuleManager = this;
							out_interface_class.ModuleSettings = FrameworkSettingsFile.Default;

              return true;
						}
					}
				}
			}
			catch
			{
				out_interface_class = null;
				return false;
			}

			out_interface_class = null;
			return false;
		}

		/// <summary>
		/// Gets the path of the current executable
		/// </summary>
		/// <returns></returns>
		private string GetModulePath()
		{
			String path = Assembly.GetExecutingAssembly().Location;

			return Path.GetDirectoryName(path);
		}

		#endregion
	}
}

#if false

		/// <summary>
		/// Adds a new module to the list of active modules
		/// </summary>
		/// <param name="in_module"></param>
		public void AddModule(ModuleInfo in_module)
		{
			List<SettingsFileBase.ModuleInfo> module_list = FrameworkSettingsFile.Default.GetModuleList();
			SettingsFileBase.ModuleInfo search_entry = new SettingsFileBase.ModuleInfo();
			string name;
			int i;
			ModuleBase module_class;

			// check if this module already exists but not active
			name = in_module.DLLName;

			i = 0;
			while (i < module_list.Count)
			{
				if (module_list[i].ModuleName == name && !module_list[i].Active)
					break;

				i++;
			}

			// check if there was an inactive configuration data
			if (i < module_list.Count)
			{
				// activate inactive module
				//FrameworkSettingsFile.Default.ModuleActivate(in_module);
			}
			else
			{
				// create a new module
				SettingsFileBase.ModuleInfo module_info = new SettingsFileBase.ModuleInfo(in_module.SectionName, in_module.DLLName, true);

				FrameworkSettingsFile.Default.ModuleAdd(module_info);
			}

			LoadModule(in_module.DLLName, out module_class);

		
			// create module class
			m_modules.Add(module_class);

			// add module settings info to the tree
			int module_index = m_modules.Count - 1;

			ModuleSettingsInfo[] settings_info = m_modules[module_index].GetSettingsInfo();
			ModuleSettingsTreeInfo info = new ModuleSettingsTreeInfo(m_modules[module_index].GetDisplayName(), settings_info[0], module_index, -1);
			info.IsExpanded = true;

			for (int page_index = 0; page_index < settings_info.Length; page_index++)
			{
				info.AddChild(new ModuleSettingsTreeInfo(settings_info[page_index], module_index, page_index));
			}
			m_module_setup_tree_info.Add(info);
		}

#endif