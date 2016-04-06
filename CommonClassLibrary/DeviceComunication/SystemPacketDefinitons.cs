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
				return Encoding.ASCII.GetString(m_device_name);
			}

			set
			{
				byte[] bytes = Encoding.ASCII.GetBytes(value);

				if (bytes.Length + 1 >= PacketConstants.DeviceNameLength)
					throw new ArgumentOutOfRangeException();

				bytes.CopyTo(m_device_name, 0);
				m_device_name[bytes.Length] = 0;
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
		private byte[] m_device_name;

		private UInt32 m_address;
		/// <summary>
		/// Default constructor
		/// </summary>
		public PacketHostInfo()
			: base(PacketType.CommHostInfo)
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
				return Encoding.ASCII.GetString(m_device_name);
			}

			set
			{
				byte[] bytes = Encoding.ASCII.GetBytes(value);

				if (bytes.Length + 1 >= PacketConstants.DeviceNameLength)
					throw new ArgumentOutOfRangeException();

				bytes.CopyTo(m_device_name, 0);
				m_device_name[bytes.Length] = 0;
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
		private byte m_cpu_load;

		public PacketDeviceHeartbeat()
		: base(PacketType.CommHostHeartbeat)
		{
		}

		/// <summary>
		/// Gets current CPU load
		/// </summary>
		public byte CPULoad
		{
			get { return m_cpu_load; }
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
				return Encoding.ASCII.GetString(m_filename);
			}

			set
			{
				byte[] bytes = Encoding.ASCII.GetBytes(value);

				if (bytes.Length + 1 >= PacketConstants.FilenameLength)
					throw new ArgumentOutOfRangeException();

				bytes.CopyTo(m_filename, 0);
				m_filename[bytes.Length] = 0;
			}
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
	}



	#endregion
}
