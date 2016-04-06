using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;

namespace CygnusControls
{
	/// <summary>
	/// Network browser class. Provides functions for browsing network resources (computers, shared folders, etc.)
	/// </summary>
	public class NetworkBrowser
	{
		#region · Dll Imports ·

		//declare the Netapi32 : NetServerEnum method import
		[DllImport("Netapi32", CharSet = CharSet.Auto,SetLastError = true),SuppressUnmanagedCodeSecurityAttribute]

		/// <summary>
		/// Netapi32.dll : The NetServerEnum function lists all servers
		/// of the specified type that are
		/// visible in a domain. For example, an 
		/// application can call NetServerEnum
		/// to list all domain controllers only
		/// or all SQL servers only.
		/// You can combine bit masks to list
		/// several types. For example, a value 
		/// of 0x00000003  combines the bit
		/// masks for SV_TYPE_WORKSTATION 
		/// (0x00000001) and SV_TYPE_SERVER (0x00000002)
		/// </summary>
		public static extern int NetServerEnum(
				string ServerNane, // must be null
				int dwLevel,
				ref IntPtr pBuf,
				int dwPrefMaxLen,
				out int dwEntriesRead,
				out int dwTotalEntries,
				int dwServerType,
				string domain, // null for login domain
				out int dwResumeHandle
				);

		//declare the Netapi32 : NetApiBufferFree method import
		[DllImport("Netapi32", SetLastError = true),SuppressUnmanagedCodeSecurityAttribute]

		/// <summary>
		/// Netapi32.dll : The NetApiBufferFree function frees 
		/// the memory that the NetApiBufferAllocate function allocates. 
		/// Call NetApiBufferFree to free
		/// the memory that other network 
		/// management functions return.
		/// </summary>
		public static extern int NetApiBufferFree(
				IntPtr pBuf);


		[DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
		private static extern int NetShareEnum(
				 StringBuilder ServerName,
				 int level,
				 ref IntPtr bufPtr,
				 uint prefmaxlen,
				 ref int entriesread,
				 ref int totalentries,
				 ref int resume_handle
				 );


		//create a _SERVER_INFO_100 STRUCTURE
		[StructLayout(LayoutKind.Sequential)]
		public struct _SERVER_INFO_100
		{
			internal int sv100_platform_id;
			[MarshalAs(UnmanagedType.LPWStr)]
			internal string sv100_name;
		}

		// create a SHARE_INFO_1 structure
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct SHARE_INFO_1
		{
			public string shi1_netname;
			public uint shi1_type;
			public string shi1_remark;
			public SHARE_INFO_1(string sharename, uint sharetype, string remark)
			{
				this.shi1_netname = sharename;
				this.shi1_type = sharetype;
				this.shi1_remark = remark;
			}
			public override string ToString()
			{
				return shi1_netname;
			}
		}

		const uint MAX_PREFERRED_LENGTH = 0xFFFFFFFF;
		const int NERR_Success = 0;

		private enum NetError : uint
		{
			NERR_Success = 0,
			NERR_BASE = 2100,
			NERR_UnknownDevDir = (NERR_BASE + 16),
			NERR_DuplicateShare = (NERR_BASE + 18),
			NERR_BufTooSmall = (NERR_BASE + 23),
		}

		private enum SHARE_TYPE : uint
		{
			STYPE_DISKTREE = 0,
			STYPE_PRINTQ = 1,
			STYPE_DEVICE = 2,
			STYPE_IPC = 3,
			STYPE_SPECIAL = 0x80000000,
		}

		#endregion

		#region · Public Methods ·

		/// <summary>
		/// Uses the DllImport : NetServerEnum
		/// with all its required parameters
		/// (see http://msdn.microsoft.com/library/default.asp?
		///      url=/library/en-us/netmgmt/netmgmt/netserverenum.asp
		/// for full details or method signature) to
		/// retrieve a list of domain SV_TYPE_WORKSTATION
		/// and SV_TYPE_SERVER PC's
		/// </summary>
		/// <returns>Arraylist that represents
		/// all the SV_TYPE_WORKSTATION and SV_TYPE_SERVER
		/// PC's in the Domain</returns>
		public static List<string> GetNetworkComputers()
		{
			//local fields
			List<string> networkComputers = new List<string>();
			const int MAX_PREFERRED_LENGTH = -1;
			int SV_TYPE_WORKSTATION = 1;
			int SV_TYPE_SERVER = 2;
			IntPtr buffer = IntPtr.Zero;
			IntPtr tmpBuffer = IntPtr.Zero;
			int entriesRead = 0;
			int totalEntries = 0;
			int resHandle = 0;
			int sizeofINFO = Marshal.SizeOf(typeof(_SERVER_INFO_100));


			try
			{
				//call the DllImport : NetServerEnum 
				//with all its required parameters
				//see http://msdn.microsoft.com/library/
				//default.asp?url=/library/en-us/netmgmt/netmgmt/netserverenum.asp
				//for full details of method signature
				int ret = NetServerEnum(null, 100, ref buffer,
						MAX_PREFERRED_LENGTH,
						out entriesRead,
						out totalEntries, SV_TYPE_WORKSTATION |
						SV_TYPE_SERVER, null, out 
                    resHandle);
				//if the returned with a NERR_Success 
				//(C++ term), =0 for C#
				if (ret == 0)
				{
					//loop through all SV_TYPE_WORKSTATION 
					//and SV_TYPE_SERVER PC's
					for (int i = 0; i < totalEntries; i++)
					{
						//get pointer to, Pointer to the 
						//buffer that received the data from
						//the call to NetServerEnum. 
						//Must ensure to use correct size of 
						//STRUCTURE to ensure correct 
						//location in memory is pointed to
						tmpBuffer = new IntPtr((int)buffer +
											 (i * sizeofINFO));
						//Have now got a pointer to the list 
						//of SV_TYPE_WORKSTATION and 
						//SV_TYPE_SERVER PC's, which is unmanaged memory
						//Needs to Marshal data from an 
						//unmanaged block of memory to a 
						//managed object, again using 
						//STRUCTURE to ensure the correct data
						//is marshalled 
						_SERVER_INFO_100 svrInfo = (_SERVER_INFO_100)
								Marshal.PtrToStructure(tmpBuffer,
												typeof(_SERVER_INFO_100));

						//add the PC names to the ArrayList
						networkComputers.Add(svrInfo.sv100_name);
					}
				}
			}
			catch
			{
				return null;
			}
			finally
			{
				//The NetApiBufferFree function frees 
				//the memory that the 
				//NetApiBufferAllocate function allocates
				NetApiBufferFree(buffer);
			}

			//return entries found
			return networkComputers;

		}

		/// <summary>
		/// Gets list of the shared folders on a given network computer. Only
		/// non special folders are returned.
		/// </summary>
		/// <param name="in_network_computer">Network computer name</param>
		/// <returns>List of the shared folders</returns>
		public static List<string> GetSharedFolders(string in_network_computer)
		{
			List<SHARE_INFO_1> ShareInfos = new List<SHARE_INFO_1>();
			int entriesread = 0;
			int totalentries = 0;
			int resume_handle = 0;
			int nStructSize = Marshal.SizeOf(typeof(SHARE_INFO_1));
			IntPtr bufPtr = IntPtr.Zero;

			StringBuilder server = new StringBuilder(in_network_computer);

			int ret = NetShareEnum(server, 1, ref bufPtr, MAX_PREFERRED_LENGTH, ref entriesread, ref totalentries, ref resume_handle);
			if (ret == NERR_Success)
			{
				IntPtr currentPtr = bufPtr;
				for (int i = 0; i < entriesread; i++)
				{
					SHARE_INFO_1 shi1 = (SHARE_INFO_1)Marshal.PtrToStructure(currentPtr, typeof(SHARE_INFO_1));
					ShareInfos.Add(shi1);
					currentPtr = new IntPtr(currentPtr.ToInt32() + nStructSize);
				}
				NetApiBufferFree(bufPtr);

				// convert to string array (add only non special folders)
				List<string> retval = new List<string>();
				foreach (SHARE_INFO_1 info in ShareInfos)
				{
					if (info.shi1_type == (uint)SHARE_TYPE.STYPE_DISKTREE)
						retval.Add(info.shi1_netname);
				}

				return retval;
			}
			else
			{
				return null;
			}
		}
		#endregion
	}
}
