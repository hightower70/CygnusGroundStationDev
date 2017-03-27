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
// USB Native methods, types and constans
///////////////////////////////////////////////////////////////////////////////
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CommonClassLibrary.DeviceCommunication
{
	class USBNativeMethods
	{
		#region · Constants ·

		public const short DIGCF_PRESENT = 0x00000002;
		public const short DIGCF_INTERFACEDEVICE = 0x00000010;

		public const uint GENERIC_READ = 0x80000000;
		public const uint GENERIC_WRITE = 0x40000000;
		public const uint FILE_SHARE_READ = 0x00000001;
		public const uint FILE_SHARE_WRITE = 0x00000002;
		public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
		public const int INVALID_HANDLE_VALUE = -1;
		public const short OPEN_EXISTING = 3;
		public const int WAIT_TIMEOUT = 0x102;
		public const int WAIT_OBJECT_0 = 0;

		public const UInt32 ERROR_INVALID_HANDLE = 6;
		public const UInt32 ERROR_HANDLE_EOF = 38;
		public const UInt32 ERROR_IO_PENDING = 997;

		public const int HID_MAX_REPORT_SIZE = 65;

		#endregion

		#region · Types ·

		// SP_DEVICE_INTERFACE_DATA
		[StructLayout(LayoutKind.Sequential)]
		public struct SP_DEVICE_INTERFACE_DATA
		{
			public Int32 Size;
			public Guid InterfaceClassGuid;
			public UInt32 Flags;
			public UIntPtr Reserved;
		}

		// Device interface detail data
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct SP_DEVICE_INTERFACE_DETAIL_DATA
		{
			public Int32 Size;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string DevicePath;
		}

		// HIDD_ATTRIBUTES
		[StructLayout(LayoutKind.Sequential)]
		public struct HIDD_ATTRIBUTES
		{
			public Int32 Size;
			public UInt16 VendorID;
			public UInt16 ProductID;
			public UInt16 VersionNumber;
		}

		// HIDP_CAPS
		[StructLayout(LayoutKind.Sequential)]
		public struct HIDP_CAPS
		{
			public Int16 Usage;
			public Int16 UsagePage;
			public Int16 InputReportByteLength;
			public Int16 OutputReportByteLength;
			public Int16 FeatureReportByteLength;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
			public Int16[] Reserved;
			public Int16 NumberLinkCollectionNodes;
			public Int16 NumberInputButtonCaps;
			public Int16 NumberInputValueCaps;
			public Int16 NumberInputDataIndices;
			public Int16 NumberOutputButtonCaps;
			public Int16 NumberOutputValueCaps;
			public Int16 NumberOutputDataIndices;
			public Int16 NumberFeatureButtonCaps;
			public Int16 NumberFeatureValueCaps;
			public Int16 NumberFeatureDataIndices;
		}

		#endregion

		#region · Functions ·

		// hid.dll
		[DllImport("hid.dll", SetLastError = true)]
		public static extern Boolean HidD_GetAttributes(SafeFileHandle HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern void HidD_GetHidGuid(ref Guid HidGuid);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern Boolean HidD_GetSerialNumberString(SafeFileHandle HidDeviceObject, [MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 2)] StringBuilder Buffer, int BufferLength);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern Boolean HidD_GetPreparsedData(SafeFileHandle HidDeviceObject, ref IntPtr PreparsedData);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern Boolean HidD_FreePreparsedData(ref IntPtr PreparsedData);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern Int32 HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);

		// setupapi.dll
		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern IntPtr SetupDiGetClassDevs(ref Guid InterfaceClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, UInt32 dwFlagsAndAttributes, Int32 hTemplateFile);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

		// kernel32.dll
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern bool ResetEvent(IntPtr hEvent);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadFile(SafeFileHandle hFile, ref byte lpBuffer, int NumberOfBytesToRead, ref UInt32 pNumberOfBytesRead, ref NativeOverlapped lpOverlapped);

		[DllImport("kernel32.dll")]
		public static extern UInt32 GetLastError();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetOverlappedResult(SafeFileHandle hFile, ref NativeOverlapped lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);

		[DllImport("kernel32.dll")]
		public static extern bool CancelIo(SafeFileHandle hFile);

		#endregion
	}
}
