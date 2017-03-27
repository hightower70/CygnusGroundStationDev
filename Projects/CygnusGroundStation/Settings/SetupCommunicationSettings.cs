using CommonClassLibrary.Settings;

namespace CygnusGroundStation
{
	class SetupCommunicationSettings : SettingsBase
	{
		public bool UDPEnabled { set; get; }
		public int UDPLocalPort { set; get; }
		public int UDPRemotePort { set; get; }

		public bool UARTEnabled { set; get; }
		public string UARTPort { set; get; }
		public int UARTBaud { get; set; }

		public bool USBEnabled { set; get; }
		public int USBVID { set; get; }
		public int USBPID { get; set; }

		public SetupCommunicationSettings()
				: base("Main", "SetupCommunication")
		{
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			UDPEnabled = true;
			UDPLocalPort = 9601;
			UDPRemotePort = 9602;

			UARTEnabled = false;
			UARTPort = "";
			UARTBaud = 115200;

			USBEnabled = true;
			USBVID = 0x1781;
			USBPID = 0x08fa;
		}
	}
}
