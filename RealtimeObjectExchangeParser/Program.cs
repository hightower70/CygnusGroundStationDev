using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonClassLibrary.Console;
using CommonClassLibrary.RealtimeObjectExchange;

namespace ROXParser
{
    class Program
    {
		static bool ProcessCommandLineSwitches(CommandLineParser in_command_line)
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
			string xml_file_name = @"d:\Projects\CygnusGroundStation\Projects\ObjectVaultParser\QuadroSimObjectVault.xml";

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

			ParserRealtimeObjectDescription parser = new ParserRealtimeObjectDescription();
			parser.ParseXMLFile("/RealtimeObjectExchangle/*", xml_file_name);


			if (string.IsNullOrEmpty(parser.ErrorMessage))
			{
				//parser.CreateCFiles(xml_file_name);
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
