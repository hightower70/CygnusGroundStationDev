using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CygnusGroundStation
{
	class SetupFormSettings : SettingsBase
	{
		public string StartupForm { set; get; }

		public SetupFormSettings()
			: base("Main", "SetupForms")
		{
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			StartupForm = "";
		}
	}
}
