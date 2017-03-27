using CommonClassLibrary.Settings;
using System.IO;
using System.Reflection;

namespace CygnusAuxBoardMonitor.Settings
{
	public class MainGeneralSettings : SettingsBase
	{
		// Main window settings
		public WindowPosSettings MainWindowPos;

		public MainGeneralSettings() : base("Main", "General")
		{
			MainWindowPos = new WindowPosSettings();
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			MainWindowPos.SetDefault(800, 600);
		}
	}

}
