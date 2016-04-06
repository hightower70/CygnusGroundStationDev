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
// Functions for accessing byte (high, low, etc.) of a multi byte integer type
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClassLibrary.Helpers
{
	public class ByteAccess
	{
		#region · Byte access functions  for UInt16 ·

		/// <summary>
		/// Gets lowest byte of an unsigned 16 bit integer
		/// </summary>
		/// <param name="in_value"></param>
		/// <returns></returns>
		public static byte LowByte(UInt16 in_value)
		{
			return (byte)(in_value & 0xff);
		}

		/// <summary>
		/// Gets highest byte of an unsigned 16 bit integer
		/// </summary>
		/// <param name="in_value"></param>
		/// <returns></returns>
		public static byte HighByte(UInt16 in_value)
		{
			return (byte)((in_value >> 8) & 0xff);
		}

		#endregion
	}
}
