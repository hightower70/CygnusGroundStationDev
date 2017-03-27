using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonClassLibrary.Console;
using CommonClassLibrary.RealtimeObjectExchange;
using System.IO;

namespace ROXParser
{
	class Program
	{
		static ParserRealtimeObjectExchange.ParserParameters m_parser_parameters = new ParserRealtimeObjectExchange.ParserParameters();

		static bool ProcessCommandLineSwitches(CommandLineParser in_command_line)
		{
			for (int i = 0; i < in_command_line.Parameters.Length; i++)
			{
				switch (in_command_line.Parameters[i].Command.ToLower())
				{
					case "rox":
						m_parser_parameters.ROXFileName = in_command_line.Parameters[i].Parameter;
						in_command_line.Parameters[i].Used = true;
						break;

					case "cdecl":
						m_parser_parameters.HeaderFileName = in_command_line.Parameters[i].Parameter;
						in_command_line.Parameters[i].Used = true;
						break;

					case "typedefs":
						m_parser_parameters.TypedefsFileName = in_command_line.Parameters[i].Parameter;
						in_command_line.Parameters[i].Used = true;
						break;

					case "defaultpacketenable":
						m_parser_parameters.DefaultPacketEnableFileName = in_command_line.Parameters[i].Parameter;
						in_command_line.Parameters[i].Used = true;
						break;
				}
			}
			return true;
		}


		static void Main(string[] args)
		{
			// display title
			Console.Write(StringConstants.ProgramTitle);

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
				Console.WriteLine(StringConstants.Usage);

				return;
			}

			// use command line switches
			if (!ProcessCommandLineSwitches(command_line))
			{
				return;
			}

			m_parser_parameters.Typedefs = new RealtimeObjectTypedefs();
			m_parser_parameters.Typedefs.ParseXMLFile("/Types/*", m_parser_parameters.TypedefsFileName);

			ParserRealtimeObjectExchange parser = new ParserRealtimeObjectExchange();
			parser.ParseXMLFile("/RealtimeObjectExchangle/*", m_parser_parameters.ROXFileName);

			if (string.IsNullOrEmpty(parser.ErrorMessage))
			{
				// generate file names
				string config_file_name_without_extension;
				config_file_name_without_extension = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(m_parser_parameters.ROXFileName)), Path.GetFileNameWithoutExtension(m_parser_parameters.ROXFileName));

				if (m_parser_parameters.HeaderFileName == null)
					m_parser_parameters.HeaderFileName = config_file_name_without_extension + ".h";
			}


			if (string.IsNullOrEmpty(parser.ErrorMessage))
			{
				parser.CreateHeaderFiles(m_parser_parameters);
			}

			if (string.IsNullOrEmpty(parser.ErrorMessage))
			{
				parser.CreateDefaultPacketEnabledFile(m_parser_parameters);
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
