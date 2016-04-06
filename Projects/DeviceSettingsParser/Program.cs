///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2015 Laszlo Arvai. All rights reserved.
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
// Settings parser main file and class
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.Console;
using CommonClassLibrary.DeviceSettings;
using System;

namespace DeviceSettingsParser
{
	class Program
	{
		static bool ProcessCommandLineSwitches(CommandLineParser  in_command_line)
		{
			foreach (CommandLineParser.CommandLineParameters parameter in in_command_line.Parameters)
			{
				switch (parameter.Command.ToLower())
				{
					case "param":
						break;
				}
			}
			return true;

		}

		static void Main(string[] args)
		{
			string xml_file_name = @"d:\Projects\CygnusGroundStation\devel\SettingParser\setting.xml";

			// display title
			Console.Write(DeviceSettingsStringConstants.ProgramTitle);

			// process command line
			CommandLineParser command_line = new CommandLineParser();
			if (!command_line.ProcessCommandLine(args))
			{
				Console.Write(command_line.ErrorMessage);
				Console.ReadKey();
				return;
			}

			// display help text
			if (command_line.IsHelpRequested())
			{
				Console.WriteLine(DeviceSettingsStringConstants.Usage);

				return;
			}

			// use command line switches
			if (!ProcessCommandLineSwitches(command_line))
			{
				return;
			}

			DeviceSettings parser = new DeviceSettings();
			parser.ParseXMLFile("/Settings/*", xml_file_name);


			if (string.IsNullOrEmpty(parser.ErrorMessage))
			{
				parser.CreateCFiles(xml_file_name);
			}

			if (!string.IsNullOrEmpty(parser.ErrorMessage))
			{
				Console.WriteLine(parser.ErrorMessage + " at line:" + parser.ErrorLine);
				Console.ReadKey();
				return;
			}

			Console.ReadKey();
		}
	}
}
