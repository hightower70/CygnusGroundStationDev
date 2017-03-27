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
		public const int PacketClassMask = 0xf0;
		public const int MaxRealtimePacketCount = 128;
	}

	/// <summary>
	/// Mode codes for file operation finished packet
	/// </summary>
	public enum FileOperationFinishMode : byte
	{
		Success = 1,
		Cancel = 2
	};

	/// <summary>
	/// Packet types
	/// </summary>
	public enum PacketType : byte
	{
		// packet flags
		FlagSystem = (1 << 7),
		FlagRequest = (1 << 3),

		// packet classes
		ClassComm = (1 << 4) | FlagSystem,
		ClassFile = (2 << 4) | FlagSystem,

		// communication class packets
		CommDeviceHeartbeat = (ClassComm + 1),
		CommHostHeartbeat = (ClassComm + 2),
		CommDeviceAnnounce = (ClassComm + 3),
		CommHostAnnounce = (ClassComm + 4),
		CommDeviceNameRequest = (ClassComm + FlagRequest + 5),
		CommDeviceNameResponse = (ClassComm + 5),

		// file class packets
		FileInfo = (ClassFile + 1),
		FileInfoRequest = (FileInfo + FlagRequest),
		FileInfoResponse = (FileInfo),

		FileDataRead = (ClassFile + 2),
		FileDataReadRequest = (FileDataRead + FlagRequest),
		FileDataReadResponse = (FileDataRead),

		FileDataWrite = (ClassFile + 3),
		FileDataWriteRequest = (FileDataWrite + FlagRequest),
		FileDataWriteResponse = (FileDataWrite),

		FileOperationFinished = (ClassFile + 4),
		FileOperationFinishedRequest = (FileOperationFinished + FlagRequest),
		FileOperationFinishedResponse = (FileOperationFinished)
	}

	/// <summary>
	/// Base type of all packets
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketBase
	{
		protected byte m_packet_length;
		private PacketType m_packet_type;
		private byte m_packet_counter;

		public PacketBase()
		{
		}

		public PacketBase(PacketType in_type)
		{
			m_packet_type = in_type;
			m_packet_length = (byte)(Marshal.SizeOf(this.GetType()) + PacketConstants.PacketCRCLength);
			m_packet_counter = 0;
		}

		public void SetPacketCounter(byte in_id)
		{
			m_packet_counter = in_id;
		}

		public PacketType PacketType
		{
			get { return m_packet_type; }
		}

		public PacketType PacketClass
		{
			get { return (PacketType)((int)m_packet_type &  PacketConstants.PacketClassMask); }
		}

		public byte PacketLength
		{
			get { return m_packet_length; }
		}

		public byte PacketCounter
		{
			get { return m_packet_counter; }
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
			return "Len:" + m_packet_length.ToString() + " " + m_packet_type.ToString() + " Cnt:" + m_packet_counter.ToString();
		}
	}

	#endregion

	#region · Communication packets ·

	/// <summary>
	/// Device announce packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketDeviceAnnounce : PacketBase
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = PacketConstants.DeviceNameLength)]
		private byte[] m_device_name;
		private UInt32 m_unique_id;

		private UInt32 m_address;

		/// <summary>
		/// Default constructor
		/// </summary>
		public PacketDeviceAnnounce()
			: base(PacketType.CommDeviceAnnounce)
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
	public class PacketHostAnnounce : PacketBase
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = PacketConstants.DeviceNameLength)]
		private byte[] m_host_name;

		private UInt32 m_address;
		/// <summary>
		/// Default constructor
		/// </summary>
		public PacketHostAnnounce()
			: base(PacketType.CommHostAnnounce)
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
	/// Base class for file operation packets
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileOperationBase : PacketBase
	{
		private byte m_id;

		/// <summary>
		/// Deafult constructor
		/// </summary>
		public PacketFileOperationBase()
		{
		}

		/// <summary>
		/// Default constuctor
		/// </summary>
		/// <param name="in_type"></param>
		public PacketFileOperationBase(PacketType in_type)
			: base(in_type)
		{
		}

		/// <summary>
		/// Gets/sets unique file ID
		/// </summary>
		public byte ID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// Generates file opoeration ID from file operation packet (either file name or file ID in string)
		/// </summary>
		/// <param name="in_packet">File operation paket to use to generate ID</param>
		/// <returns>File operation ID</returns>
		public string FileOperationID
		{
			get
			{
				switch (PacketType)
				{
					case PacketType.FileInfoRequest:
						return ((PacketFileInfoRequest)this).FileName;

					case PacketType.FileInfoResponse:
						return ((PacketFileInfoResponse)this).FileName;

					default:
						return m_id.ToString();
				}
			}
		}
	};

	/// <summary>
	/// File information request packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileInfoRequest : PacketFileOperationBase
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = PacketConstants.FilenameLength)]
		private byte[] m_filename; // file name

		/// <summary>
		/// Deafult constructor
		/// </summary>
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
	public class PacketFileInfoResponse : PacketFileOperationBase
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = PacketConstants.FilenameLength)]
		private byte[] m_filename; // file name

		private UInt32 m_length; // Length of the file
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = MD5Hash.MD5HashLength)]
		private byte[] m_hash; // MD5 hash of the file

		/// <summary>
		/// Deafult constructor
		/// </summary>
		public PacketFileInfoResponse()
		: base(PacketType.FileInfoResponse)
		{
			m_hash = new byte[MD5Hash.MD5HashLength];
		}


		/// <summary>
		/// Gets internal file name
		/// </summary>
		public string FileName
		{
			get
			{
				return PacketBase.ConvertFromZeroTerminatedString(m_filename);
			}
		}

		/// <summary>
		/// Gets length of the file in bytes
		/// </summary>
		public UInt32 Length
		{
			get { return m_length; }
		}

		/// <summary>
		/// Gets MD5 hash of the file
		/// </summary>
		public MD5Hash Hash
		{
			get { return new MD5Hash(m_hash); }
		}

		public override string ToString()
		{
			return "Length:" + m_length.ToString() + " ID:" + ID.ToString();
		}
	}

	/// <summary>
	/// File Data Read Request Packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileDataReadRequest : PacketFileOperationBase
	{
		private UInt32 m_pos; // Read start position
		private UInt16 m_length; // Bytes to read

		/// <summary>
		/// Default constructor
		/// </summary>
		public PacketFileDataReadRequest()
		: base(PacketType.FileDataReadRequest)
		{
		}

		/// <summary>
		/// Gets/sets file position
		/// </summary>
		public UInt32 Pos
		{
			get { return m_pos; }
			set { m_pos = value; }
		}

		/// <summary>
		/// Gets/sets data length of the packet
		/// </summary>
		public UInt16 Length
		{
			get { return m_length; }
			set { m_length = value; }
		}

		public override string ToString()
		{
			return base.ToString() + " ID:" + ID + " Pos:" + m_pos;
		}
	}

	/// <summary>
	/// File Data Read Response Packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileDataReadResponseHeader : PacketFileOperationBase
	{
		private UInt32 m_pos; // read position
		private UInt16 m_length; // number of bytes returned

		/// <summary>
		/// Gets/sets file position
		/// </summary>
		public UInt32 Pos
		{
			get { return m_pos; }
			set { m_pos = value; }
		}

		/// <summary>
		/// Gets length of the data in the response
		/// </summary>
		public UInt16 Length
		{
			get { return m_length; }
		}

		public override string ToString()
		{
			return base.ToString() + " ID:" + ID + " Pos:" + m_pos + " Length:" + Length;
		}
	}

	public class PacketFileDataReadResponse : PacketFileOperationBase
	{
		private PacketFileDataReadResponseHeader m_header;
		private byte[] m_data;

		public PacketFileDataReadResponse(PacketFileDataReadResponseHeader in_response_header) : base(PacketType.FileDataReadResponse)
		{
			m_header = in_response_header;
		}

		public void SetData(byte[] in_source_data, int in_offset, int in_length)
		{
			m_data = new byte[in_length];
			for (int index = 0; index < in_length; index++)
			{
				m_data[index] = in_source_data[index + in_offset];
			}
		}

		public byte[] Data
		{
			get { return m_data; }
		}


		public override string ToString()
		{
			return base.ToString() + " ID:" + ID + " Pos:" + m_header.Pos + " Length: " + m_header.Length;
		}
	}

	/// <summary>
	/// File Data Write Request Packet (paket header only)
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileDataWriteRequestHeader : PacketFileOperationBase
	{
		private UInt32 m_pos; // write operation start position
		private UInt16 m_length; // number of bytes to write

		public PacketFileDataWriteRequestHeader(byte in_data_length)
		: base(PacketType.FileDataWriteRequest)
		{
			m_packet_length = (byte)(Marshal.SizeOf(this.GetType()) + in_data_length + PacketConstants.PacketCRCLength);
			m_length = in_data_length;
		}

		/// <summary>
		/// Gets/sets file position
		/// </summary>
		public UInt32 Pos
		{
			get { return m_pos; }
			set { m_pos = value; }
		}

		/// <summary>
		/// Gets/sets data length of the packet
		/// </summary>
		public UInt16 Length
		{
			get { return m_length; }
		}

		public override string ToString()
		{
			return base.ToString() + " ID:" + ID + " Pos:" + m_pos;
		}
	}

	/// <summary>
	/// File Data Write Response Packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileDataWriteResponse : PacketFileOperationBase
	{
		private byte m_error;

		/// <summary>
		/// Gets/sets file position
		/// </summary>
		public byte Error
		{
			get { return m_error; }
			set { m_error = value; }
		}

		public override string ToString()
		{
			return base.ToString() + " ID:" + ID + " Error:" + m_error;
		}
	}


	/// <summary>
	/// File Operation Finished Request Packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileOperationFinishedRequest : PacketFileOperationBase
	{
		private byte m_finish_mode;

		public PacketFileOperationFinishedRequest()
		: base(PacketType.FileOperationFinishedRequest)
		{
		}

		/// <summary>
		/// Gets/sets unique file ID
		/// </summary>
		public FileOperationFinishMode FinishMode
		{
			get { return (FileOperationFinishMode)m_finish_mode; }
			set { m_finish_mode = (byte)value; }
		}

		public override string ToString()
		{
			return base.ToString() + " ID:" + ID + " Mode:" + m_finish_mode;
		}
	}

	/// <summary>
	/// File Operation Finished Response Packet
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
	public class PacketFileOperationFinishedResponse : PacketFileOperationBase
	{
		public const byte NoError = 0;

		private byte m_finish_mode;
		private byte m_error;

		public PacketFileOperationFinishedResponse()
		: base(PacketType.FileOperationFinishedResponse)
		{
		}

		/// <summary>
		/// Gets unique file ID
		/// </summary>
		public FileOperationFinishMode FinishMode
		{
			get { return (FileOperationFinishMode)m_finish_mode; }
		}

		/// <summary>
		/// Gets error code
		/// </summary>
		public byte Error
		{
			get { return m_error; }
		}

		public override string ToString()
		{
			return base.ToString() + " ID:" + ID + " Mode:" + m_finish_mode + " Error:" + m_error;
		}
	}

	#endregion
}
