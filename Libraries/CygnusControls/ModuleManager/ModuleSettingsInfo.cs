using CommonClassLibrary.Settings;
using System.Windows;

namespace CygnusControls
{
	public class ModuleSettingsInfo
	{
		public ModuleSettingsInfo(string in_page_name, FrameworkElement in_form, ISettingsDataProvider in_data_provider)
		{
			PageName = in_page_name;
			Form = in_form;
			DataProvider = in_data_provider;
		}

		public string PageName;
		public FrameworkElement Form;
		public ISettingsDataProvider DataProvider;
	}
}
