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
// Collection of geometry converters
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CygnusControls
{
	/// <summary>
	/// Converts value to a point on a circumference
	/// </summary>
	[ValueConversion(typeof(double), typeof(Point))]
	public class CircularConverter : IValueConverter
	{
		#region · Properties ·
		public double ValueMin
		{
			set;
			get;
		} = 0;

		public double ValueMax
		{
			set;
			get;
		} = 100;

		public double AngleMin
		{
			set;
			get;
		} = 0;

		public double AngleMax
		{
			set;
			get;
		} = 180;

		public double CenterX
		{
			set;
			get;
		} = 0;

		public double CenterY
		{
			set;
			get;
		} = 0;

		public double RadiusX
		{
			set;
			get;
		} = 1;

		public double RadiusY
		{
			set;
			get;
		} = 1;

		#endregion

		#region · Converter function ·
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double radius_x;
			double radius_y;
			double angle;

			// check types
			if ((targetType != typeof(Point)) || (value.GetType() != typeof(double)) )
			{
				throw new InvalidOperationException("Invalid type");
			}

			// check parameter
			if(double.TryParse((string)parameter, NumberStyles.Any, CultureInfo.InvariantCulture, out radius_x))
			{
				radius_y = radius_x;
			}
			else
			{
				radius_x = RadiusX;
				radius_y = RadiusY;
			}

			// calculate angle in rad
			angle = ((((double)value) - ValueMin) / (ValueMax - ValueMin) * (AngleMax - AngleMin) + AngleMin) / 180 * Math.PI;

			return new Point(Math.Cos(angle) * radius_x + CenterX, Math.Sin(angle) * radius_y + CenterY);
		}
		#endregion

		#region Singleton Implementation
		/// <summary>
		/// Singleton storage
		/// </summary>
		private static CircularConverter m_instance = new CircularConverter();

		/// <summary> 
		/// Instance  of the converter
		/// </summary> 
		public static CircularConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
		#endregion
	}
}
