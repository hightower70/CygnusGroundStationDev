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
// Tracked (skid steering) vehicle kinematics simulation class
///////////////////////////////////////////////////////////////////////////////
using System;

namespace LidarEmulator
{
	public class TrackedVehicleSimulation
	{
		#region · Data members ·
		double m_max_speed; // maximum speed in m/s
		double m_track_base_distance; // track distance in meters

		double m_pos_x; // X world position in meters
		double m_pos_y; // Y world position in meters
		double m_heading_in_rad; // heading of the robot in rad
		#endregion

		#region · Properties ·

		/// <summary>
		/// Current heading in deg relative to X axis
		/// </summary>
		public double HeadingInDeg
		{
			get { return m_heading_in_rad / Math.PI * 180; }
			set { m_heading_in_rad = value / 180 * Math.PI; }
		}

		/// <summary>
		/// Current heading in rad relative to X axis
		/// </summary>
		public double HeadingInRad
		{
			get { return m_heading_in_rad; }
			set { m_heading_in_rad = value; }
		}

		/// <summary>
		/// X coordinate in meters
		/// </summary>
		public double PositionX
		{
			get { return m_pos_x; }
			set { m_pos_x = value; }
		}

		/// <summary>
		/// Y coordinate in meters
		/// </summary>
		public double PositionY
		{
			get { return m_pos_y; }
			set { m_pos_y = value; }
		}
		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		public TrackedVehicleSimulation()
		{
			m_pos_x = 0;
			m_pos_y = 0;
			m_max_speed = 0.3;
			m_track_base_distance = 0.27;
		}

		#endregion

		#region · Public members ·

		/// <summary>
		/// Updates robot position
		/// </summary>
		/// <param name="in_left_track_throttle">Throttle for left track [-100%..100%]</param>
		/// <param name="in_right_track_throttle">Throttle for rigth track [-100%..100%]</param>
		/// <param name="in_time_step">Ellapsed time since the last update [sec]</param>
		public void UpdatePosition(double in_left_track_throttle, double in_right_track_throttle, double in_time_step)
		{
			// check movement type
			if (Math.Abs(in_left_track_throttle - in_right_track_throttle) <= 1)
			{
				// linear movement (no rotation -> heading will not be changed)
				double speed = (in_left_track_throttle + in_right_track_throttle) / 2 / 100 * m_max_speed; // speed of the vehicle
				double distance = speed * in_time_step; // distance travelled in this dt intervall

				// update position
				m_pos_x += Math.Cos(m_heading_in_rad) * distance;
				m_pos_y += Math.Sin(m_heading_in_rad) * distance;
			}
			else
			{
				// circular movement
				double Vl = in_left_track_throttle / 100 * m_max_speed; // left track speed
				double Vr = in_right_track_throttle / 100 * m_max_speed; // right track speed

				double R = m_track_base_distance / 2 * (Vl + Vr) / (Vr - Vl); // center point distance from ICC (Instaneous Center of Curvature)
				double omega = (Vr - Vl) / m_track_base_distance; // rate of rotation around ICC

				double omega_dt = omega * in_time_step; // omega * dt

				// calculate ICC coordinates
				double ICCx = m_pos_x - R * Math.Sin(m_heading_in_rad);
				double ICCy = m_pos_y + R * Math.Cos(m_heading_in_rad);

				// update position
				m_pos_x = (m_pos_x - ICCx) * Math.Cos(omega_dt) + (m_pos_y - ICCy) * -Math.Sin(omega_dt) + ICCx;
				m_pos_y = (m_pos_x - ICCx) * Math.Sin(omega_dt) + (m_pos_y - ICCy) * Math.Cos(omega_dt) + ICCy;

				// update and normalize heading [0..2PI)
				double new_heading_in_rad = m_heading_in_rad + omega_dt;

				new_heading_in_rad = new_heading_in_rad % (2 * Math.PI);
				if (new_heading_in_rad < 0)
				{
					new_heading_in_rad += 2 * Math.PI;
				}

				m_heading_in_rad = new_heading_in_rad;
			}
		}

		#endregion
	}
}
