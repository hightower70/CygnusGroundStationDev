using CommonClassLibrary.DeviceCommunication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CygnusGroundStation
{
	class SetupCommunicationSettings : SettingsBase
	{
		public int UDPLocalPort { set; get; }
		public int UDPRemotePort { set; get; }

		public string UARTPort { set; get; }
		public int UARTBaud { get; set; }

		public SetupCommunicationSettings()
				: base("Main", "SetupCommunication")
		{
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			UDPLocalPort = 9601;
			UDPRemotePort = 9602;

			UARTPort = "";
			UARTBaud = 115200;
		}
	}
}
