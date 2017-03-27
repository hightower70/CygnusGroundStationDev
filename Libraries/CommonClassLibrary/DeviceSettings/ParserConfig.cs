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
// Configuration data for parser functions
///////////////////////////////////////////////////////////////////////////////
using System.IO;
using System.Text;

namespace CommonClassLibrary.DeviceSettings
{
	public class ParserConfig
	{
		public StringBuilder HeaderFile;
		public MemoryStream ValueInfoFile;
		public MemoryStream DefaultValueFile;
		public bool UseOffsets;

		public ParserConfig()
		{
			HeaderFile = new StringBuilder();
			ValueInfoFile = new MemoryStream();
			DefaultValueFile = new MemoryStream();
			UseOffsets = false;
		}

		public ParserConfig(StringBuilder in_header_file, MemoryStream in_value_info_file, MemoryStream in_default_value_file, bool in_use_offsets)
		{
			HeaderFile = in_header_file;
			ValueInfoFile = in_value_info_file;
			DefaultValueFile = in_default_value_file;
			UseOffsets = in_use_offsets;
		}
	}
}
