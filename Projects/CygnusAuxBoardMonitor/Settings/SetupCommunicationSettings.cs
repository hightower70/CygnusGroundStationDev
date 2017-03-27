using CommonClassLibrary.Settings;

namespace CygnusAuxBoardMonitor.Settings
{
	class SetupCommunicationSettings : SettingsBase
	{
		public string UARTPort { set; get; }

		public SetupCommunicationSettings()
				: base("Main", "CommunicationSettings")
		{
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			UARTPort = "";
		}
	}
}
