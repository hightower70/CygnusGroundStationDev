using System;
using System.Net;
using CygnusGroundStation;

namespace FlightGearInterface
{
	class FlightGearSettings : SettingsBase
	{
		// FlightGear Interface Settings
		public IPAddress InterfaceIPAddress;
		public UInt16 InterfacePort {set; get; }
		public bool AutostartFlightGear;

		public FlightGearSettings()
			: base("FlightGearInterface", "FlightGear")
		{
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			InterfacePort = 5000;
			InterfaceIPAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
			AutostartFlightGear = false;
		}
	}
}


