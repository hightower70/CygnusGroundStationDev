///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013 Laszlo Arvai. All rights reserved.
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
// FlightGear main data conversion thread
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FlightGearInterface
{
	class FlightGearThread
	{
		#region · Private data ·
		private FlightGearSettings m_settings;
		private UdpClient m_udp_client;
		private IPEndPoint m_endpoint;
		#endregion

		#region · Public members ·

		/// <summary>
		/// Sets up this interface module
		/// </summary>
		public void Configure(FlightGearSettings in_settings)
		{
			m_settings = in_settings;
		}

		/// <summary>
		/// Starts module operation
		/// </summary>
		public void Open()
		{
			// starts flight gear
			StartFlightGear();

			// create endpoint
			m_endpoint = new IPEndPoint(m_settings.IPAddress, m_settings.Port);
			
			// Open UDP connection
			m_udp_client = new UdpClient(m_endpoint);

			// start data reception
			IAsyncResult res = m_udp_client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
		}

		/// <summary>
		/// Stops module operation
		/// </summary>
		public void Close()
		{
			if (m_udp_client != null)
				m_udp_client.Close();

			m_udp_client = null;
		}

		#endregion

		#region · Private members ·

		private void ReceiveCallback(IAsyncResult res)
		{
			try
			{
				if (m_udp_client != null)
				{
					byte[] data = m_udp_client.EndReceive(res, ref m_endpoint);

					string receiveString = Encoding.ASCII.GetString(data);
					ProcessReceivedLine(receiveString);

					// get next packet
					m_udp_client.BeginReceive(ReceiveCallback, null);
				}
			}
			catch
			{
				// do nothing
			}
		}

		private void ProcessReceivedLine(string in_line)
		{
			double value;
			string[] chunks = in_line.Substring(0, in_line.Length - 1).Split(',');

			//////////////////////
			// Raw "sensor" values

			// Gyro

			// /orientation/roll-rate-degps
			value = Convert.ToSingle(chunks[0], CultureInfo.InvariantCulture);

			// /orientation/pitch-rate-degps
			value = Convert.ToSingle(chunks[1], CultureInfo.InvariantCulture);

			// /orientation/yaw-rate-degps
			value = Convert.ToSingle(chunks[2], CultureInfo.InvariantCulture);

			// Acceleration

			// /accelerations/pilot/x-accel-fps_sec
			value = Convert.ToSingle(chunks[3], CultureInfo.InvariantCulture);

			// /accelerations/pilot/y-accel-fps_sec
			value = Convert.ToSingle(chunks[4], CultureInfo.InvariantCulture);

			// /accelerations/pilot/z-accel-fps_sec
			value = Convert.ToSingle(chunks[5], CultureInfo.InvariantCulture);

			///////////
			// Attitude

			// /orientation/roll-deg
			value = Convert.ToSingle(chunks[6], CultureInfo.InvariantCulture);

			// /orientation/pitch-deg</node>
			value = Convert.ToSingle(chunks[7], CultureInfo.InvariantCulture);

			// /orientation/heading-deg</node>
			value = Convert.ToSingle(chunks[8], CultureInfo.InvariantCulture);

			///////////////
			// GPS Position

			// /position/latitude-deg
			value = Convert.ToSingle(chunks[9], CultureInfo.InvariantCulture);

			// /position/longitude-deg
			value = Convert.ToSingle(chunks[10], CultureInfo.InvariantCulture);

			// /position/altitude-ft
			value = Convert.ToSingle(chunks[11], CultureInfo.InvariantCulture);

			///////////
			// Airspeed
			// /velocities/airspeed-kt
			value = Convert.ToSingle(chunks[12], CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Starts FlightGear program
		/// </summary>
		/// <param name="in_settings">Settings class for parameters</param>
		private void StartFlightGear()
		{
			// check if start is required
			if (!m_settings.Autostart)
				return;

			// Check whether FlightGear is already running
			Process[] processes = Process.GetProcessesByName("fgfs");

			if (processes == null || processes.Length <= 0)
			{
				string flight_gear_bin = Path.Combine(m_settings.Path, "bin\\fgfs.exe");
				string flight_gear_dir = Path.Combine(m_settings.Path, "bin");


				// Create process start information
				ProcessStartInfo startInfo = new ProcessStartInfo(flight_gear_bin, m_settings.Options);
				startInfo.WorkingDirectory = flight_gear_dir;

				// Start FlightGear
				try
				{
					Process.Start(startInfo);
				}
				catch
				{
				}
			}
		}
		#endregion
	}
}

