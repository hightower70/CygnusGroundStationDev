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
using System.IO;

namespace DeviceSettingsParser
{
	class Program
	{
		static ParserDeviceSettings.GeneratedFileNames m_file_names;

		static bool ProcessCommandLineSwitches(CommandLineParser  in_command_line)
		{
			for (int i = 0; i < in_command_line.Parameters.Length; i++)
			{
				switch (in_command_line.Parameters[i].Command.ToLower())
				{
					case "config":
						m_file_names.ConfigFileName = in_command_line.Parameters[i].Parameter;
						in_command_line.Parameters[i].Used = true;
						break;

					case "header":
						m_file_names.HeaderFileName = in_command_line.Parameters[i].Parameter;
						in_command_line.Parameters[i].Used = true;
						break;

					case "defaultdata":
						m_file_names.DefaultDataFileName = in_command_line.Parameters[i].Parameter;
						in_command_line.Parameters[i].Used = true;
						break;

					case "xmldata":
						m_file_names.XmlDataFileName = in_command_line.Parameters[i].Parameter;
						in_command_line.Parameters[i].Used = true;
						break;

					case "valueinfo":
						m_file_names.ValueInfoFileName = in_command_line.Parameters[i].Parameter;
						in_command_line.Parameters[i].Used = true;
						break;
				}
			}

			return true;
		}

		static void Main(string[] args)
		{
			m_file_names = new ParserDeviceSettings.GeneratedFileNames();

			// display title
			Console.Write(ParserDeviceSettingsStringConstants.ProgramTitle);

			// process command line
			CommandLineParser command_line = new CommandLineParser();
			if (!command_line.ProcessCommandLine(args))
			{
				Console.Write(command_line.ErrorMessage);
				Console.ReadKey();
				return;
			}

			// display help text
			if (command_line.IsHelpRequested() || command_line.Parameters.Length == 0)
			{
				Console.WriteLine(ParserDeviceSettingsStringConstants.Usage);

				return;
			}

			// use command line switches
			if (!ProcessCommandLineSwitches(command_line))
			{
				return;
			}

			// process command line file names
			ParserDeviceSettings parser = new ParserDeviceSettings();
			parser.ParseXMLFile("/Settings/*", m_file_names.ConfigFileName);

			if (string.IsNullOrEmpty(parser.ErrorMessage))
			{
				// generate file names
				string config_file_name_without_extension;
				config_file_name_without_extension = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(m_file_names.ConfigFileName)), Path.GetFileNameWithoutExtension(m_file_names.ConfigFileName));

				if (m_file_names.HeaderFileName == null)
					m_file_names.HeaderFileName = config_file_name_without_extension + ".h";

				if (m_file_names.DefaultDataFileName == null)
					m_file_names.DefaultDataFileName = config_file_name_without_extension + "_default.inl";

				if (m_file_names.XmlDataFileName == null)
					m_file_names.XmlDataFileName = config_file_name_without_extension + "_xml.inl";

				if (m_file_names.ValueInfoFileName == null)
					m_file_names.ValueInfoFileName = config_file_name_without_extension + "_info.inl";
			}

			// Generate output files
			if (string.IsNullOrEmpty(parser.ErrorMessage))
			{
				// update value offsets
				parser.GenerateBinaryValueOffset();

				// files
				parser.CreateFiles(m_file_names);
			}


			if (!string.IsNullOrEmpty(parser.ErrorMessage))
			{
				Console.WriteLine(parser.ErrorMessage + " at line:" + parser.ErrorLine);
				Console.ReadKey();
				return;
			}
		}
	}
}
