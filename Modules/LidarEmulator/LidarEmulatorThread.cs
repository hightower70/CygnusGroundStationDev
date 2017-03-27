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
// Lidar emulator main thread
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LidarEmulator
{
	class LidarEmulatorThread
	{
		#region · Private data ·
		//private LidarEmulatorSettings m_settings;
		#endregion

		#region · Public members ·

		/// <summary>
		/// Sets up this interface module
		/// </summary>
		/*public void Configure(LidarEmulatorSettings in_settings)
		{
			m_settings = in_settings;
		}*/

		/// <summary>
		/// Starts module operation
		/// </summary>
		public void Open()
		{
		}

		/// <summary>
		/// Stops module operation
		/// </summary>
		public void Close()
		{
		}

		#endregion

		#region · Private members ·

		#endregion
	}
}

