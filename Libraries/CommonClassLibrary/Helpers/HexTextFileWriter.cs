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
// Writes text files using comma separated hex values
///////////////////////////////////////////////////////////////////////////////
using System.IO;

namespace CommonClassLibrary.Helpers
{
	public class HexTextFileWriter : StreamWriter
	{
		private int m_byte_count;

		/// <summary>
		/// Creates HEX file, overrwrites if exists
		/// </summary>
		/// <param name="in_name"></param>
		public HexTextFileWriter(string in_name) :base(in_name)
		{
			m_byte_count = 0;
		}

		/// <summary>
		/// Writes one byte into the file
		/// </summary>
		/// <param name="in_byte"></param>
		public void WriteHexByte(byte in_byte)
		{
			if (m_byte_count > 0)
			{
				base.Write(", ");

				if ((m_byte_count % 16) == 0)
					base.WriteLine();
			}

			base.Write(string.Format("0x{0:X2}", in_byte));
			m_byte_count++;
		}
	}
}
