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
// Enumerator class for UART selection
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CommonClassLibrary.DeviceCommunication
{
	public class UARTEnumerator
	{
		#region · DLL import ·

		/*************/
		/* Constants */
		/*************/
		private const UInt32 DIGCF_PRESENT = 0x00000002;
		private const UInt32 DIGCF_DEVICEINTERFACE = 0x00000010;
		private const UInt32 SPDRP_DEVICEDESC = 0x00000000;
		private const UInt32 DICS_FLAG_GLOBAL = 0x00000001;
		private const UInt32 DIREG_DEV = 0x00000001;
		private const UInt32 KEY_QUERY_VALUE = 0x0001;
		private const string GUID_DEVINTERFACE_COMPORT = "86E0D1E0-8089-11D0-9CE4-08003E301F73";

		/*********/
		/* Types */
		/*********/
		[StructLayout(LayoutKind.Sequential)]
		private struct SP_DEVINFO_DATA
		{
			public Int32 cbSize;
			public Guid ClassGuid;
			public Int32 DevInst;
			public UIntPtr Reserved;
		};

		/**************/
		/* Functions  */
		/**************/

		// setupapi.dll
		[DllImport("setupapi.dll")]
		private static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

		[DllImport("setupapi.dll")]
		private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, Int32 MemberIndex, ref SP_DEVINFO_DATA DeviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData,
				uint property, out UInt32 propertyRegDataType, StringBuilder propertyBuffer, uint propertyBufferSize, out UInt32 requiredSize);

		[DllImport("setupapi.dll", SetLastError = true)]
		private static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, UInt32 iEnumerator, IntPtr hParent, UInt32 nFlags);

		[DllImport("Setupapi", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetupDiOpenDevRegKey(IntPtr hDeviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, uint scope,
				uint hwProfile, uint parameterRegistryValueKind, uint samDesired);

		// advapi32.dll
		[DllImport("advapi32.dll", CharSet = CharSet.Unicode, EntryPoint = "RegQueryValueExW", SetLastError = true)]
		private static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, out uint lpType,
				StringBuilder lpData, ref uint lpcbData);

		[DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		private static extern int RegCloseKey(IntPtr hKey);

		// kernel32.dll
		[DllImport("kernel32.dll")]
		private static extern Int32 GetLastError();

		#endregion

		#region · Types ·

		/// <summary>
		/// Device information class
		/// </summary>
		public class DeviceInfo
		{
			public string Port { get; set; }
			public string Description { get; set; }

			public string DisplayName
			{
				get { return Port + " (" + Description + ")"; }
			}

			public override bool Equals(object in_object)
			{
				// If this and obj do not refer to the same type, then they are not equal.
				if (in_object.GetType() != this.GetType())
					return false;

				return Port == ((DeviceInfo)in_object).Port;
			}

			public override int GetHashCode()
			{
				return Port.GetHashCode();
			}

		}

		#endregion

		#region · Device Enumeration ·

		/// <summary>
		/// Enumerates all COM ports
		/// </summary>
		/// <returns>Information of the avaiable COM ports</returns>
		public static List<DeviceInfo> GetAllCOMPorts()
		{
			Guid guidComPorts = new Guid(GUID_DEVINTERFACE_COMPORT);
			IntPtr hDeviceInfoSet = SetupDiGetClassDevs(
					ref guidComPorts, 0, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
			if (hDeviceInfoSet == IntPtr.Zero)
			{
				throw new Exception("Failed to get device information set for the COM ports");
			}

			try
			{
				List<DeviceInfo> devices = new List<DeviceInfo>();
				Int32 iMemberIndex = 0;
				while (true)
				{
					SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA();
					deviceInfoData.cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA));
					bool success = SetupDiEnumDeviceInfo(hDeviceInfoSet, iMemberIndex, ref deviceInfoData);
					if (!success)
					{
						// No more devices in the device information set   
						break;
					}

					DeviceInfo deviceInfo = new DeviceInfo();
					deviceInfo.Port = GetDeviceName(hDeviceInfoSet, deviceInfoData);
					deviceInfo.Description = GetDeviceDescription(hDeviceInfoSet, deviceInfoData);
					devices.Add(deviceInfo);

					iMemberIndex++;
				}
				return devices;
			}
			finally
			{
				SetupDiDestroyDeviceInfoList(hDeviceInfoSet);
			}
		}

		/// <summary>
		/// Gets displayable device name of the given COM device
		/// </summary>
		/// <param name="pDevInfoSet"></param>
		/// <param name="deviceInfoData"></param>
		/// <returns></returns>
		private static string GetDeviceName(IntPtr pDevInfoSet, SP_DEVINFO_DATA deviceInfoData)
		{
			IntPtr hDeviceRegistryKey = SetupDiOpenDevRegKey(pDevInfoSet, ref deviceInfoData,
					DICS_FLAG_GLOBAL, 0, DIREG_DEV, KEY_QUERY_VALUE);
			if (hDeviceRegistryKey == IntPtr.Zero)
			{
				throw new Exception("Failed to open a registry key for device-specific configuration information");
			}

			StringBuilder deviceNameBuf = new StringBuilder(256);
			try
			{
				uint lpRegKeyType;
				uint length = (uint)deviceNameBuf.Capacity;
				int result = RegQueryValueEx(hDeviceRegistryKey, "PortName", 0, out lpRegKeyType, deviceNameBuf, ref length);
				if (result != 0)
				{
					throw new Exception("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
				}
			}
			finally
			{
				RegCloseKey(hDeviceRegistryKey);
			}

			string deviceName = deviceNameBuf.ToString();
			return deviceName;
		}

		/// <summary>
		/// Gets device description text of the given COM device
		/// </summary>
		/// <param name="hDeviceInfoSet"></param>
		/// <param name="deviceInfoData"></param>
		/// <returns></returns>
		private static string GetDeviceDescription(IntPtr hDeviceInfoSet, SP_DEVINFO_DATA deviceInfoData)
		{
			StringBuilder descriptionBuf = new StringBuilder(256);
			uint propRegDataType;
			uint length = (uint)descriptionBuf.Capacity;
			bool success = SetupDiGetDeviceRegistryProperty(hDeviceInfoSet, ref deviceInfoData, SPDRP_DEVICEDESC,
					out propRegDataType, descriptionBuf, length, out length);
			if (!success)
			{
				throw new Exception("Can not read registry value PortName for device " + deviceInfoData.ClassGuid);
			}
			string deviceDescription = descriptionBuf.ToString();
			return deviceDescription;
		}

		#endregion
	}
}
