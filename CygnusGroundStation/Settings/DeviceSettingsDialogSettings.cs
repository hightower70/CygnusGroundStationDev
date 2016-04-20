using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusGroundStation
{
	class DeviceSettinsgDialogSettings : SettingsBase
	{
		// Dialog position settings
		public WindowPosSettings DialogPos;

		public DeviceSettinsgDialogSettings() : base("Main", "DeviceSettingsDialog")
		{
			DialogPos = new WindowPosSettings();
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			DialogPos.SetDefault(400, 300);
		}
	}
}
