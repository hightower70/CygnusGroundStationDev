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
// String resource for Device Settings Parser
///////////////////////////////////////////////////////////////////////////////
namespace CommonClassLibrary.DeviceSettings
{
  public class ParserDeviceSettingsStringConstants
  {
    // program title
    public const string ProgramTitle = "*** Device Settings Parser v:0.1  Copyright by Laszlo Arvai 2012-2016 ***\n";

		// usage messages
		public const string Usage = "Usage: DeviceSettingsParser <-switches> <switchfile>\n The 'switchfile' is a text file and one line of the file must contain one\n command line switch.\n\n" +
			"Supported switches:\n -help or -? - Displays this help message\n" +
			" -config:<filename>      - Specifies device settings definition XML file name\n" +
			" -header:<filename>      - Name of C style header file for settings constants\n" +
			" -defaultdata:<filename> - File name for default settings storage inline file\n" +
			" -xmldata:<filename>     - File name for XML storage inline file\n";

		// error messages
		public const string ErrorInvalidElementType = "Invalid element type. ({0})";

  }
}