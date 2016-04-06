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
// Converter for mapping numerical valus from one interval to another interval
// using linear interpolation
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace CygnusControls
{
	/// <summary>
	/// Point of linear interpolation
	/// </summary>
	public class LinearMappingPoint : IComparable
	{
		public LinearMappingPoint()
		{
		}

		public double Input { get; set; }
		public double Output { get; set; }

		public int CompareTo(object in_object)
		{
			return Input.CompareTo(((LinearMappingPoint)in_object).Input);
		}
	}	
	
	/// <summary>
	/// Adds parameter value to the value to convert
	/// </summary>
	[TypeConverter(typeof(LinearMappingConverter))]
	[ValueConversion(typeof(double), typeof(double))]
	public class LinearMappingConverter : IValueConverter
	{
		#region · Types ·
		/// <summary>
		/// Mode of the handling input value overflow
		/// </summary>
		public enum OverflowModeType
		{
			Limit,
			Wrap
		}

		#endregion

		#region · Data members ·
		private double m_input_min;
		private double m_input_max;
		private double m_output_min;
		private double m_output_max;
		private OverflowModeType m_overflow_mode = OverflowModeType.Limit;
		private List<LinearMappingPoint> m_items = new List<LinearMappingPoint>();
		private bool m_items_sorted = false;
		#endregion

		#region · Properties ·

		/// <summary>
		/// Templates collection
		/// </summary>
		public List<LinearMappingPoint> Items
		{
			get { return m_items; }
		}

		/// <summary>
		/// Input range lower value
		/// </summary>
		public double InputMin
		{
			get { return m_input_min; }
			set { m_input_min = value; }
		}

		/// <summary>
		/// Input range upper value
		/// </summary>
		public double InputMax
		{
			get { return m_input_max; }
			set { m_input_max = value; }
		}

		/// <summary>
		/// Output range lower value
		/// </summary>
		public double OutputMin
		{
			get { return m_output_min; }
			set { m_output_min = value; }
		}

		/// <summary>
		/// Output range upper value
		/// </summary>
		public double OutputMax
		{
			get { return m_output_max; }
			set { m_output_max = value; }
		}

		/// <summary>
		/// Overflow handling mode
		/// </summary>
		public OverflowModeType OverflowMode
		{
			get { return m_overflow_mode; }
			set { m_overflow_mode = value; }
		}
		

		#endregion

		#region · Converter function ·

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double val;
			double retval = 0;

			// check if items are sorted
			if (!m_items_sorted)
			{
				if(m_items != null)
					m_items.Sort();

				m_items_sorted = true;
			}

			// sanity check
			if (targetType != typeof(double))
				return null;

			val = (double)value;

			// handle overflow
			switch (m_overflow_mode)
			{
				// limit values
				case OverflowModeType.Limit:
				{
					if (val < m_input_min)
						val = m_input_min;
					if (val > m_input_max)
						val = m_input_max;
				}
				break;

				// wrap values
				case OverflowModeType.Wrap:
				{
					val = (val - m_input_min) % (m_input_max - m_input_min);

					if (val < m_input_min)
						val += (m_input_max - m_input_min);
				}
				break;
			}

			// convert value
			if ((m_input_max - m_input_min) == 0 || (m_output_max - m_output_min) == 0)
				return 0;

			if(m_items == null || m_items.Count == 0)
			{
				// simple linear interpolation
				retval = (val - m_input_min) / (m_input_max - m_input_min) * (m_output_max - m_output_min) + m_output_min;
			}
			else
			{
				// multi linear interpolation
				double input_min, input_max;
				double output_min, output_max;
				int index;

				input_min = m_input_min;
				output_min = m_output_min;

				input_max = m_items[0].Input;
				output_max = m_items[0].Output;

				index = 1;
				while (input_max < val)
				{
					input_min = input_max;
					output_min = output_max;

					if (index < m_items.Count)
					{
						input_max = m_items[index].Input;
						output_max = m_items[index].Output;
					}
					else
					{
						input_max = m_input_max;
						output_max = m_output_max;
					}

					index++;
				}

				retval = (val - input_min) / (input_max - input_min) * (output_max - output_min) + output_min;
			}

			return retval;
		}

		#endregion

		#region · Singleton Implementation ·
		/// <summary>
		/// Singleton storage
		/// </summary>
		private static LinearMappingConverter m_instance = new LinearMappingConverter();

		/// <summary> 
		/// The AdditionConverter instance 
		/// </summary> 
		public static LinearMappingConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region · Convert back ·
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
		#endregion
	}
}
