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
// X-Plane simulator interface
///////////////////////////////////////////////////////////////////////////////
using CommonClassLibrary.RealtimeObjectExchange;
using CygnusGroundStation;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace XPlaneInterface
{
	class XPlaneCommunicator
	{
		#region · Private data ·
		private XPlaneInterfaceSettings m_settings;
		private UdpClient m_udp_client;
		private IPEndPoint m_remote_endpoint;
		private ParserRealtimeObjectCollection m_realtime_objects;
		private PacketConversionTable m_packet_conversion_table = new PacketConversionTable();
		#endregion

		#region · Public members ·

		/// <summary>
		/// Initializes X-Plane communication class
		/// </summary>
		/// <param name="in_settings"></param>
		public void Initialize(SettingsFileBase in_settings)
		{
			ParserRealtimeObject parser = new ParserRealtimeObject();

			// update settings
			m_settings = in_settings.GetSettings<XPlaneInterfaceSettings>(); ;

			// parse realtime object description
			//using (TextReader reader = new StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("XPlaneInterface.RealtimeDataConfig.xml")))
			{
				//TODO parser.ParseXMLStream("/" + ParserRealtimeObject.XMLRootName + "/*", reader);
			}

			// TODO m_realtime_objects = parser.Collection;
    }

		/// <summary>
		/// Starts module operation
		/// </summary>
		public void Start()
		{

			return;

			// build packet conversion table
			m_packet_conversion_table.CreateConversionTable(m_realtime_objects);

			// initialize realtime operation
			m_realtime_objects.RealtimeInitialization();

			// starts X-Plane
			StartXPlane();

			// create endpoint
			IPEndPoint endpoint = new IPEndPoint(m_settings.IPAddress, m_settings.RemotePortNumber);

			// Open UDP connection
			m_udp_client = new UdpClient(endpoint);

			// Start reception
			IAsyncResult res = m_udp_client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
		}

		/// <summary>
		/// Stops module operation
		/// </summary>
		public void Stop()
		{
			if (m_udp_client != null)
				m_udp_client.Close();

			m_udp_client = null;
		}

		#endregion

		#region · Private members ·

		private void ReceiveCallback(IAsyncResult res)
		{
			byte[] data = m_udp_client.EndReceive(res, ref m_remote_endpoint);
			int pos;
			int block_index;
			int block_id;
			float value;

			// check packet header
			if (data.Length > 5)
			{
				if ((data[0] == 'D') && (data[1] == 'A') && (data[2] == 'T') && (data[3] == 'A'))
				{
					// process packet data
					pos = 5;
					while (pos < data.Length)
					{
						block_id = BitConverter.ToInt32(data, pos);
						pos += sizeof(Int32);

						if (block_id <= PacketConversionTable.XPlaneMaxBlockID)
						{
							for (block_index = 0; block_index < PacketConversionTable.XPlaneDataBlockElementCount; block_index++)
							{
								// get value
								value = BitConverter.ToSingle(data, pos);
								pos += sizeof(float);

								// store value
								PacketConversionTable.ConversionTableEntry class_info = m_packet_conversion_table[block_id, block_index];

								if(class_info.ClassIndex >= 0 && class_info.MemberIndex >= 0)
								{
									m_realtime_objects.Objects[class_info.ClassIndex].SetValue(class_info.MemberIndex, value);
								}
							}
						}
						else
						{
							pos += PacketConversionTable.XPlaneDataBlockElementCount * sizeof(float);
						}
					}
				}
			}

			// get next packet
			m_udp_client.BeginReceive(ReceiveCallback, null);
		}

		/// <summary>
		/// Starts X-Plane program
		/// </summary>
		/// <param name="in_settings">Settings class for parameters</param>
		private void StartXPlane()
		{
			// check if start is required
			if (!m_settings.Autostart)
				return;

			// Check whether X-Plane is already running
			Process[] processes = Process.GetProcessesByName("X-Plane");

			if (processes == null || processes.Length <= 0)
			{
				string xplane_bin = Path.Combine(m_settings.XPlanePath, "X-Plane.exe");
				string xplane_dir = m_settings.XPlanePath;

				// Create process start information
				ProcessStartInfo startInfo = new ProcessStartInfo(xplane_bin, m_settings.Options);
				startInfo.WorkingDirectory = xplane_dir;

				// Start X-Plane
				Process.Start(startInfo);
			}
		}
		#endregion
	}
}
