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
// MD5 hash storage and calculation class
///////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Security.Cryptography;

namespace CommonClassLibrary.Helpers
{
	public class MD5Hash
	{
		#region · Types ·
		public const int MD5HashLength = 16;
		#endregion

		#region · Data members ·
		private byte[] m_md5_hash;
		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		public MD5Hash()
		{
			m_md5_hash = new byte[MD5HashLength];
		}

		/// <summary>
		/// Construct form byte array
		/// </summary>
		/// <param name="in_value"></param>
		public MD5Hash(byte[] in_value)
		{
			if (in_value.Length != MD5HashLength)
				throw new ArgumentException("Invalid MD5 hash size.");

			m_md5_hash = in_value;
		}

		#endregion

		#region · Public functions ·

		/// <summary>
		/// 
		/// </summary>
		/// <param name="in_hash"></param>
		/// <returns></returns>
		public bool IsEqual(MD5Hash in_hash)
		{
			bool equal = true;

			for(int i = 0; i < MD5HashLength && equal; i++)
			{
				if (m_md5_hash[i] != in_hash.m_md5_hash[i])
					equal = false;
			}

			return equal;
		}

		/// <summary>
		/// Calculates hash for a file
		/// </summary>
		/// <param name="in_file_path">Name of the file</param>
		public void ComputeFileHash(string in_file_path)
		{
			using (MD5 md5 = MD5.Create())
			{
				using (FileStream fileStream = File.OpenRead(in_file_path))
				{
					m_md5_hash = md5.ComputeHash(fileStream);
				}
			}
		}
		#endregion
	}
}
