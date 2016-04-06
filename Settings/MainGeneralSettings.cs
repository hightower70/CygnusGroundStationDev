using System.IO;
using System.Reflection;

namespace CygnusGroundStation
{
	public class MainGeneralSettings : SettingsBase
	{
		// Path settings
		public string ModulesPath { set; get; }
		public string FormsPath { set; get; }

		// Main window settings
		public WindowPosSettings MainWindowPos;
		
		public MainGeneralSettings()	: base("Main","General")
		{
			MainWindowPos = new WindowPosSettings();
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			MainWindowPos.SetDefault(800, 600);

			string application_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			ModulesPath = application_path;
			FormsPath = Path.GetFullPath(Path.Combine(application_path, "../../../Forms"));
		}
	}

}
