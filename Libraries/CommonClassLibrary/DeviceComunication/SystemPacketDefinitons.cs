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
// Host-device communication packet definitions
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.Helpers;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonClassLibrary.DeviceCommunication
{

	#region · Types ·

	/// <summary>
	/// Colection of a packet related constants
	/// </summary>
	public static class PacketConstants
	{
		public const int FilenameLength = 32;
		public const int DeviceNameLength = 16;
		public const int TypeOffset = 1;
		public const int PacketMaxLength = 255;
		public const int PacketCRCLength = 2;
	}

	/// <summary>
	/// Packet types
	/// </summary>
	public enum PacketType : byte
	{
		// packet flags
		FlagSystem = (1 << 7),
		FlagRequest = (1 << 3),

		// packet classes
		ClassComm = (1 << 4),
		ClassFile = (2 << 4),
		ClassConfig = (3 << 4),

		// communication class packets
		CommDeviceHeartbeat = (FlagSystem + ClassComm + 1),
		CommHostHeartbeat = (FlagSystem + ClassComm + 2),
		CommDeviceInfo = (FlagSystem + ClassComm + 3),
		CommHostInfo = (FlagSystem + ClassComm + 4),
		CommDeviceNameRequest = (FlagSystem + ClassComm + FlagRequest + 5),
		CommDeviceNameResponse = (FlagSystem + ClassComm + 5),

		// file class packets
		FileInfo = (FlagSystem + ClassFile + 1),
		FileInfoRequest = (FileInfo + FlagRequest),
		FileInfoResponse = (FileInfo),

		FileData = (FlagSystem + ClassFile + 2),
		FileDataRequest = (FileData + FlagRequest),
		FileDataResponse = (FileData)
	}


	/// <summary>
	/// Base type of all packets
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketBase
	{
		private byte m_length;
		private PacketType m_type;
		private byte m_counter;

		public PacketBase()
		{
		}

		public PacketBase(PacketType in_type)
		{
			m_type = in_type;
			m_length = (byte)(Marshal.SizeOf(this.GetType()) + PacketConstants.PacketCRCLength);
			m_counter = 0;
		}

		public void SetCounter(byte in_id)
		{
			m_counter = in_id;
		}

		public PacketType Type
		{
			get { return m_type; }
		}

		public byte Length
		{
			get { return m_length; }
		}

		public byte Counter
		{
			get { return m_counter; }
		}

		protected static string ConvertFromZeroTerminatedString(byte[] in_buffer)
		{
			int count = Array.IndexOf<byte>(in_buffer, 0, 0, in_buffer.Length);
			if (count < 0)
				count = in_buffer.Length;

			return Encoding.ASCII.GetString(in_buffer, 0, count);
		}

		protected static void ConvertToZeroTerminatedString(ref byte[] out_buffer, string in_string)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(in_string);

			if (bytes.Length + 1 >= out_buffer.Length)
				throw new ArgumentOutOfRangeException();

			bytes.CopyTo(out_buffer, 0);
			out_buffer[bytes.Length] = 0;
		}

		public override string ToString()
		{
			return "Len:" + m_length.ToString() + " " + m_type.ToString() + " Cnt:" + m_counter.ToString();
		}
	}

	#endregion

	#region · Communication packets ·

	/// <summary>
	/// Device information packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketDeviceInfo : PacketBase
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = PacketConstants.DeviceNameLength)]
		private byte[] m_device_name;
		private UInt32 m_unique_id;

		private UInt32 m_address;

		/// <summary>
		/// Default constructor
		/// </summary>
		public PacketDeviceInfo()
			: base(PacketType.CommDeviceInfo)
		{
			m_device_name = new byte[PacketConstants.DeviceNameLength];
			m_device_name[0] = 0;
		}

		/// <summary>
		/// Gets/sets device name string
		/// </summary>
		public string DeviceName
		{
			get
			{
				return PacketBase.ConvertFromZeroTerminatedString(m_device_name);
			}

			set
			{
				ConvertToZeroTerminatedString(ref m_device_name, value);
			}
		}

		/// <summary>
		/// Gets unique ID
		/// </summary>
		public UInt32 UniqueID
		{
			get
			{
				return m_unique_id;
			}
		}

		/// <summary>
		/// Return address of the device (meaning of th ID is depending on the underlying communication layer)
		/// </summary>
		public UInt32 Address
		{
			get
			{
				return m_address;
			}
		}
	}

	/// <summary>
	/// Host information packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketHostInfo : PacketBase
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = PacketConstants.DeviceNameLength)]
		private byte[] m_host_name;

		private UInt32 m_address;
		/// <summary>
		/// Default constructor
		/// </summary>
		public PacketHostInfo()
			: base(PacketType.CommHostInfo)
		{
			m_host_name = new byte[PacketConstants.DeviceNameLength];
			m_host_name[0] = 0;
		}

		/// <summary>
		/// Gets/sets device name string
		/// </summary>
		public string HostName
		{
			get
			{
				return PacketBase.ConvertFromZeroTerminatedString(m_host_name);
			}

			set
			{
				ConvertToZeroTerminatedString(ref m_host_name, value);
			}
		}

		/// <summary>
		/// Gets/sets device address
		/// </summary>
		public UInt32 Address
		{
			set
			{
				m_address = value;
			}
			get
			{
				return m_address;
			}
		}
	}

	/// <summary>
	/// Heart beat packet send by the host
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketHostHeartbeat : PacketBase
	{
		private UInt16 m_year;
		private byte m_month;
		private byte m_day;

		private byte m_hour;
		private byte m_minute;
		private byte m_seconds;

		public PacketHostHeartbeat()
			: base(PacketType.CommHostHeartbeat)
		{
			DateTime timestamp = DateTime.Now;

			m_year = (UInt16)timestamp.Year;
			m_month = (byte)timestamp.Month;
			m_day = (byte)timestamp.Day;

			m_hour = (byte)timestamp.Hour;
			m_minute = (byte)timestamp.Minute;
			m_seconds = (byte)timestamp.Second;
		}
	}

	/// <summary>
	/// Heart beat packet sent by the device
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketDeviceHeartbeat : PacketBase
	{
		private UInt32 m_unique_id;
		private byte m_cpu_load;

		public PacketDeviceHeartbeat()
		: base(PacketType.CommHostHeartbeat)
		{
		}

		/// <summary>
		/// Gets unique ID
		/// </summary>
		public UInt32 UniqueID
		{
			get
			{
				return m_unique_id;
			}
		}

		/// <summary>
		/// Gets current CPU load
		/// </summary>
		public byte CPULoad
		{
			get { return m_cpu_load; }
		}
	}

	/// <summary>
	/// Device name request packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketDeviceNameRequest : PacketBase
	{
		public PacketDeviceNameRequest()
		: base(PacketType.CommDeviceNameRequest)
		{
		}
	}

	/// <summary>
	/// Device name response packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketDeviceNameResponse : PacketBase
	{
		private UInt32 m_unique_id;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = PacketConstants.DeviceNameLength)]
		private byte[] m_device_name;

		/// <summary>
		/// Gets/sets device name string
		/// </summary>
		public string DeviceName
		{
			get
			{
				return PacketBase.ConvertFromZeroTerminatedString(m_device_name);
			}
		}

		/// <summary>
		/// Gets unique ID
		/// </summary>
		public UInt32 UniqueID
		{
			get
			{
				return m_unique_id;
			}
		}

	}

	#endregion

	#region · File management packets ·

	/// <summary>
	/// File information request packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileInfoRequest : PacketBase
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = PacketConstants.FilenameLength)]
		private byte[] m_filename;

		public PacketFileInfoRequest()
			: base(PacketType.FileInfoRequest)
		{
			m_filename = new byte[PacketConstants.FilenameLength];
			m_filename[0] = 0;
		}

		/// <summary>
		/// Gets/sets internal file name
		/// </summary>
		public string FileName
		{
			get
			{
				return PacketBase.ConvertFromZeroTerminatedString(m_filename);
			}

			set
			{
				ConvertToZeroTerminatedString(ref m_filename, value);
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + FileName;
		}
	}
	/// <summary>
	/// File information response packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileInfoResponse : PacketBase
	{
		private byte m_file_id;
		private UInt32 m_file_length;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MD5Hash.MD5HashLength)]
		private byte[] m_file_hash;


		public PacketFileInfoResponse()
		: base(PacketType.FileInfoResponse)
		{
			m_file_hash = new byte[MD5Hash.MD5HashLength];
		}

		/// <summary>
		/// Gets unique file ID
		/// </summary>
		public byte FileID
		{
			get { return m_file_id; }
		}

		/// <summary>
		/// Gets length of the file in bytes
		/// </summary>
		public UInt32 FileLength
		{
			get { return m_file_length; }
		}

		/// <summary>
		/// Gets MD5 hash of the file
		/// </summary>
		public MD5Hash FileHash
		{
			get { return new MD5Hash(m_file_hash); }
		}

		public override string ToString()
		{
			return "Length:" + m_file_length.ToString() + " FileID:" + m_file_id.ToString();
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileDataRequest : PacketBase
	{
		private byte m_file_id;
		private UInt32 m_file_pos;
		private byte m_length;


		public PacketFileDataRequest()
		: base(PacketType.FileDataRequest)
		{
		}

		/// <summary>
		/// Gets/sets unique file ID
		/// </summary>
		public byte FileID
		{
			get { return m_file_id; }
			set { m_file_id = value; }
		}

		/// <summary>
		/// Gets/sets file position
		/// </summary>
		public UInt32 FilePos
		{
			get { return m_file_pos; }
			set { m_file_pos = value; }
		}

		/// <summary>
		/// Gets/sets data length of the packet
		/// </summary>
		public byte DataLength
		{
			get { return m_length; }
			set { m_length = value; }
		}

		public override string ToString()
		{
			return base.ToString() + " ID:" + m_file_id + " Pos:" + m_file_pos;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileDataResponseHeader : PacketBase
	{
		private byte m_file_id;
		private UInt32 m_file_pos;

		/// <summary>
		/// Gets/sets unique file ID
		/// </summary>
		public byte FileID
		{
			get { return m_file_id; }
			set { m_file_id = value; }
		}

		/// <summary>
		/// Gets/sets file position
		/// </summary>
		public UInt32 FilePos
		{
			get { return m_file_pos; }
			set { m_file_pos = value; }
		}

		public override string ToString()
		{
			return base.ToString() + " ID:" + m_file_id + " Pos:" + m_file_pos;
		}
	}



	#endregion
}
