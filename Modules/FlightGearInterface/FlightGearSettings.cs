///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013 Laszlo Arvai. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
// MA 02110-1301  USA
///////////////////////////////////////////////////////////////////////////////
// File description
// ----------------
// FlightGear Module settings
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.Settings;
using System;
using System.Net;

namespace FlightGearInterface
{
	class FlightGearSettings : SettingsBase
	{
		// FlightGear Interface Settings
		public IPAddress IPAddress { set; get; }
		public UInt16 Port {set; get; }
		public bool Autostart { set; get; }
		public string Options { set; get; }
		public string Path { set; get; }

		public FlightGearSettings()
			: base("FlightGearInterface", "FlightGear")
		{
			SetDefaultValues();
		}

		override public void SetDefaultValues()
		{
			Port = 5000;
			IPAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
			Autostart = false;
			Options = "--in-air " +
									"--altitude=10000 " +
									"--vc=110 " +
									"--heading=300 " +
									"--timeofday=noon " +
									"--fg-root=\"..\\data\" " +
									"--disable-sound " +
									"--generic=socket,out,50,localhost,5000,udp,cygnusuav " +
									"--generic=socket,in,50,localhost,5000,udp,cygnusuav";
			Path = "c:\\Program Files\\FlightGear";
		}
	}
}


