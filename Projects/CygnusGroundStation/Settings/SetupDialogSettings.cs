using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusGroundStation
{
	class SetupDialogSettings	: SettingsBase
	{
		// Dialog position settings
		public WindowPosSettings SetupDialogPos;

		public SetupDialogSettings() : base("Main","SetupDialog")
		{
			SetupDialogPos = new WindowPosSettings();
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			SetupDialogPos.SetDefault(800, 600);
		}
	}
}
