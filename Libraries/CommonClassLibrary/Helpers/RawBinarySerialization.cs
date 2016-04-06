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
// Serializer/deserializer class for converting between raw binary data and objects
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;

namespace CommonClassLibrary.DeviceCommunication
{
	public class RawBinarySerialization
	{
		#region · Packet-Object serialization/deserialization ·

		/// <summary>
		/// Deserialize object from raw binary data
		/// </summary>
		/// <param name="rawdatas">Raw bytes of the packet</param>
		/// <param name="anytype">Object type to deserialize</param>
		/// <returns>Deserialized object</returns>
		public static object DeserializeObject(byte[] rawdatas, Type anytype)
		{
			int rawsize = Marshal.SizeOf(anytype);
			if (rawsize > rawdatas.Length)
				return null;
			GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
			IntPtr buffer = handle.AddrOfPinnedObject();
			object retobj = Marshal.PtrToStructure(buffer, anytype);
			handle.Free();
			return retobj;
		}

		/// <summary>
		/// Serialize object to raw binary data
		/// </summary>
		/// <param name="anything">Object to serialize</param>
		/// <returns></returns>
		public static byte[] SerializeObject(object anything)
		{
			int rawsize = Marshal.SizeOf(anything);
			byte[] rawdatas = new byte[rawsize];
			GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
			IntPtr buffer = handle.AddrOfPinnedObject();
			Marshal.StructureToPtr(anything, buffer, false);
			handle.Free();
			return rawdatas;
		}

		#endregion


	}
}
