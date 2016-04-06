using CygnusControls;
using System.Windows;

namespace CygnusGroundStation
{
	public class ModuleSettingsTreeInfo : TreeViewItemBase
	{
		public ModuleSettingsTreeInfo(ModuleSettingsInfo in_settings, int in_module_index, int in_form_index)
		{
			DisplayName = in_settings.PageName;
			Settings = in_settings;
			ModuleIndex = in_module_index;
			FormIndex = in_form_index;
		}

		public ModuleSettingsTreeInfo(string in_display_name, ModuleSettingsInfo in_settings, int in_module_index, int in_form_index)
		{
			DisplayName = in_display_name;
			Settings = in_settings;
			ModuleIndex = in_module_index;
			FormIndex = in_form_index;
		}

		public string DisplayName { get; private set; }
		public ModuleSettingsInfo Settings { get; set; }
		public int ModuleIndex { get; private set; }
		public int FormIndex { get; private set; }
	}
}
