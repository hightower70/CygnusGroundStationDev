///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2016 Laszlo Arvai. All rights reserved.
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
// Provides various path used by the application. 
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace CygnusGroundStation
{
	public class SystemPaths
	{
		/// <summary>
		/// Gets application data path (usually: "users\user.name\Application Data\CygnusGroundStation")
		/// </summary>
		/// <returns></returns>
		static public string GetApplicationDataPath()
		{
			string user_directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string application_data_path = Path.Combine(user_directory, "CygnusGroundStation");

			if (!Directory.Exists(application_data_path))
			{
				Directory.CreateDirectory(application_data_path);
			}

			return application_data_path;
		}
	}
}
