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
// Emulates Lidar data using bitmap floorplan
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LidarEmulator
{
	public class LidarEmulator
	{
		#region · Data Members ·
		private byte[,] m_map;
		private int m_circular_resolution;
		private double m_pixels_per_meter;
		private double m_lidar_range;
		#endregion

		#region · Constructor ·

		/// <summary>
		/// Default constructor
		/// </summary>
		public LidarEmulator()
		{
			m_circular_resolution = 360; // 1 deg per scan
			m_pixels_per_meter = 40;
			m_lidar_range = 6;
		}
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets/Sets the number of scans per revolution
		/// </summary>
		public int CircularResolution
		{
			get { return m_circular_resolution; }
			set { m_circular_resolution = value; }
		}

		/// <summary>
		/// Gets/sets resolution of the map (in pixels per meter)
		/// </summary>
		public double PixelsPerMeter
		{
			get { return m_pixels_per_meter; }
			set { m_pixels_per_meter = value; }
		}

		/// <summary>
		/// Range of the Lidar in meters
		/// </summary>
		public double LidarRange
		{
			get { return m_lidar_range; }
			set { m_lidar_range = value; }
		}

		#endregion

		#region · Map loading functions ·

		/// <summary>
		/// Loads maps from bitmap file 
		/// </summary>
		/// <param name="in_file_name">Bitmap file name to load. All pixels has higher garyscaled value than 127 will be considered as empty grid. All pixel calues lower will be considered as wall.</param>
		public void LoadMapFromFile(string in_file_name)
		{
			BitmapSource map = new BitmapImage(new Uri(in_file_name));

			LoadMap(map);
		}

		/// <summary>
		/// Internal map loading function. It converts the loaded bitmap into the internal map representation
		/// </summary>
		/// <param name="in_map">Bitmap to covert</param>
		private void LoadMap(BitmapSource in_map)
		{
			int x, y;
			int index;
			byte red, green, blue;
			byte gray_pixel;
			BitmapSource input_map;

			// ensure bitmap is in RGB24 format
			if (in_map.Format == PixelFormats.Rgb24)
			{
				input_map = in_map;
			}
			else
			{
				// convert bitmap into RGB24
				FormatConvertedBitmap converted_bitmap = new FormatConvertedBitmap();
				converted_bitmap.BeginInit();
				converted_bitmap.Source = in_map;
				converted_bitmap.DestinationFormat = PixelFormats.Rgb24;
				converted_bitmap.EndInit();

				input_map = converted_bitmap;
			}

			// get bitmap binary pixel data
			int stride = input_map.PixelWidth * 3;
			byte[] pixels = new byte[stride * input_map.PixelHeight];
			input_map.CopyPixels(pixels, stride, 0);

			// convert it to binary map
			m_map = new byte[input_map.PixelWidth, input_map.PixelHeight];

			for (y = 0; y < input_map.PixelHeight; y++)
			{
				index = y * stride;
				for (x = 0; x < input_map.PixelWidth; x++)
				{
					// red pixel color
					red = pixels[index++];
					green = pixels[index++];
					blue = pixels[index++];

					// convert it to gray scale
					gray_pixel = (byte)((red * 0.3) + (green * 0.59) + (blue * 0.11));

					// store on the map
					m_map[x, y] = (byte)((gray_pixel > 127) ? 0 : 1);
				}
			}
		}

		/// <summary>
		/// Cheks maps for wall at the given position
		/// </summary>
		/// <param name="in_x">X position to be checked</param>
		/// <param name="in_y">Y position to be checked</param>
		/// <returns>True when wall detected at the given position</returns>
		private bool CheckForWall(int in_x, int in_y)
		{
			if (in_x < 0 || in_x >= m_map.GetLength(0) || in_y < 0 || in_y >= m_map.GetLength(1))
				return false;

			return m_map[in_x, in_y] != 0;
		}

		/// <summary>
		/// Generates Lidar scan
		/// </summary>
		/// <param name="in_x_in_meter">Lidar X position on the map (in meters)</param>
		/// <param name="in_y_in_meter">Lidar Y position on the map (in meters)</param>
		/// <param name="in_heading_in_deg">Heading of the lidar (relative to the X axis) in deg</param>
		/// <returns></returns>
		public UInt16[] GetLidarScan(double in_x_in_meter, double in_y_in_meter, double in_heading_in_deg)
		{
			UInt16[] scan = new UInt16[m_circular_resolution];
			int center_x, center_y;
			int max_x, max_y;
			int lidar_range;
			double angle;

			lidar_range = (int)Math.Round(m_lidar_range * m_pixels_per_meter, 0);

			// calculate current position on the map
			center_x = (int)Math.Round(in_x_in_meter * m_pixels_per_meter, 0);
			center_y = (int)Math.Round(in_y_in_meter * m_pixels_per_meter, 0);
			//center_x = (int)(in_x_in_meter * m_pixels_per_meter);
			//center_y = (int)(in_y_in_meter * m_pixels_per_meter);

			if (center_x < 0)
				center_x = 0;
			if (center_x > m_map.GetLength(0))
				center_x = m_map.GetLength(0) - 1;

			if (center_y < 0)
				center_y = 0;
			if (center_y > m_map.GetLength(1))
				center_y = m_map.GetLength(1) - 1;

			for (int scan_index = 0; scan_index < m_circular_resolution; scan_index++)
			{
				angle = in_heading_in_deg / 180 * Math.PI + Math.PI * 2 * scan_index / m_circular_resolution;

				max_x = (int)Math.Round(center_x + Math.Cos(angle) * lidar_range, 0);
				max_y = (int)Math.Round(center_y + Math.Sin(angle) * lidar_range, 0);

				//max_x = (int)(center_x + Math.Cos(angle) * lidar_range);
				//max_y = (int)(center_y + Math.Sin(angle) * lidar_range);

				scan[scan_index] = (UInt16)Math.Round(CalculateLidarDistance(center_x, center_y, max_x, max_y) / m_pixels_per_meter * 1000, 0);
				//scan[scan_index] = (UInt16)(CalculateLidarDistance(center_x, center_y, max_x, max_y) / m_pixels_per_meter * 1000);
			}

			return scan;
		}

		/// <summary>
		/// Calcualtes distance for one Lidar measurement
		/// </summary>
		/// <param name="in_center_x">Position of the Lidar in meters</param>
		/// <param name="in_center_y">Position of the Lidar in meters</param>
		/// <param name="in_max_x">End Xposition of the Lidar ray</param>
		/// <param name="in_max_y">End Y position of the lidar ray</param>
		/// <returns>Distance in millimeters</returns>
		private int CalculateLidarDistance(int in_center_x, int in_center_y, int in_max_x, int in_max_y)
		{
			int x;
			bool steep = Math.Abs(in_max_y - in_center_y) > Math.Abs(in_max_x - in_center_x);

			if (steep)
			{
				int t;
				t = in_center_x; // swap in_center_x and in_center_y
				in_center_x = in_center_y;
				in_center_y = t;
				t = in_max_x; // swap in_max_x and in_max_y
				in_max_x = in_max_y;
				in_max_y = t;
			}

			int dx = Math.Abs(in_max_x - in_center_x);
			int dy = Math.Abs(in_max_y - in_center_y);
			int error = dx / 2;
			int ystep = (in_center_y < in_max_y) ? 1 : -1;
			int xstep = (in_center_x < in_max_x) ? 1 : -1;
			int y = in_center_y;

			if (steep)
			{
				for (x = in_center_x; x != in_max_x; x+=xstep)
				{
					if (CheckForWall(y, x))
						break;

					error = error - dy;
					if (error < 0)
					{
						y += ystep;
						error += dx;
					}
				}
			}
			else
			{
				for (x = in_center_x; x != in_max_x; x+=xstep)
				{
					if (CheckForWall(x, y))
						break;

					error = error - dy;
					if (error < 0)
					{
						y += ystep;
						error += dx;
					}
				}
			}

			// Calculate distance
			return (int)(Math.Round(Math.Sqrt((x - in_center_x) * (x - in_center_x) + (y - in_center_y) * (y - in_center_y)), 0));
			//return (int)((Math.Sqrt((x - in_center_x) * (x - in_center_x) + (y - in_center_y) * (y - in_center_y))));
		}

		#endregion
	}
}
