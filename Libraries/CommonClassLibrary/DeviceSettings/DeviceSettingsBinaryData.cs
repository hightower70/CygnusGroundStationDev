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
// Device settings binary data handling
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;

namespace CommonClassLibrary.DeviceSettings
{
	public class DeviceSettingsBinaryDataFile
	{
		#region · Data members ·
		private byte[] m_setting_data;
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets raw binary settings data
		/// </summary>
		public byte[] BinaryDataFile
		{
			get { return m_setting_data; }
		}

		#endregion

		#region · File handling ·

		/// <summary>
		/// Loads settings binary data from a file
		/// </summary>
		/// <param name="in_path"></param>
		public void Load(string in_path)
		{
			FileStream binary_stream = File.OpenRead(in_path);

			m_setting_data = new byte[binary_stream.Length];

			binary_stream.Read(m_setting_data, 0, m_setting_data.Length);
		}

		/// <summary>
		/// Gets binary data
		/// </summary>
		/// <param name="in_offset"></param>
		/// <param name="in_length"></param>
		/// <returns></returns>
		public byte[] GetBinaryData(UInt16 in_offset, UInt16 in_length)
		{
			byte[] result = new byte[in_length];

			Array.Copy(m_setting_data, result, in_length);

			return result;
		}
		#endregion
	}
}
