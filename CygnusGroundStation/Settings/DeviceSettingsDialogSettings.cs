using System.Windows;

namespace CygnusGroundStation
{
	public class DeviceSettinsgDialogSettings : SettingsBase
	{
		// Dialog position settings
		public WindowPosSettings DialogPos;

		public double GroupDisplayWidth { set; get; }
		public double DescriptionDisplayHeight { set; get; }

		public DeviceSettinsgDialogSettings() : base("Main", "DeviceSettingsDialog")
		{
			DialogPos = new WindowPosSettings();
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			DialogPos.SetDefault(400, 300);
			GroupDisplayWidth = 150.0;
			DescriptionDisplayHeight = 50.0;
		}
	}
}
