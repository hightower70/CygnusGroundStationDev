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
  public class DeviceSettingsStringConstants
  {
    // program title
    public const string ProgramTitle = "*** Settings parser v:0.1  Copyright by Laszlo Arvai 2012-2015 ***\n";

    // usage messages
    public const string Usage = "Usage: SettingsParser <-switches> <switchfile>\n The 'switchfile' is a text file and one line of the file must contain one\n command line switch.\n Supported switches:\n -help or -? - Displays help message\n -packetdecl:<packetdeclaration> - Specifies packet declaration file name\n -cdecl:<headerfilename> - Creates C style header file using the given name\n -types:<typefilename> - Specifies the file name of the type declaration file\n";

		// error messages
		public const string ErrorInvalidElementType = "Invalid element type. ({0})";

  }
}