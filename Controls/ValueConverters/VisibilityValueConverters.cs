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
// Collection of converters to Control.Visibility value
///////////////////////////////////////////////////////////////////////////////
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CygnusControls
{
	/// <summary>
	/// Converts Boolean Values to Control.Visibility values
	/// </summary>
	[TypeConverter(typeof(BooleanToVisibilityConverter))]
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BooleanToVisibilityConverter : IValueConverter
	{
		#region · Properties ·

		//Set to true if you want to show control when boolean value is true
		//Set to false if you want to hide/collapse control when value is true
		private bool triggerValue = false;
		public bool TriggerValue
		{
			get { return triggerValue; }
			set { triggerValue = value; }
		}
		//Set to true if you just want to hide the control
		//else set to false if you want to collapse the control
		private bool isHidden;
		public bool IsHidden
		{
			get { return isHidden; }
			set { isHidden = value; }
		}
		#endregion

		#region · Singleton instance handling ·
		private static BooleanToVisibilityConverter m_instance = null;

		public static BooleanToVisibilityConverter Instance
		{
			get
			{
				if (m_instance == null)
					m_instance = new BooleanToVisibilityConverter();

				return m_instance;
			}
		}
		#endregion

		#region · Converter functions ·

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool))
				return DependencyProperty.UnsetValue;

			bool objValue = (bool)value;
			if ((objValue && TriggerValue && IsHidden) || (!objValue && !TriggerValue && IsHidden))
			{
				return Visibility.Hidden;
			}

			if ((objValue && TriggerValue && !IsHidden) || (!objValue && !TriggerValue && !IsHidden))
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Visibility))
				return DependencyProperty.UnsetValue;

			if (((Visibility)value) == Visibility.Visible)
				return true;
			else
				return false;
		}
		#endregion
	}

	/// <summary>
	/// Converts Boolean Values to Control.Visibility values
	/// </summary>
	[TypeConverter(typeof(BooleanToVisibilityConverter))]
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class IsEqualToVisibilityConverter : IValueConverter
	{
		#region · Properties ·

		//Set to true if you just want to hide the control
		//else set to false if you want to collapse the control
		private bool m_is_hidden = false;
		public bool IsHidden
		{
			get { return m_is_hidden; }
			set { m_is_hidden = value; }
		}
		#endregion

		#region · Singleton instance handling ·
		private static IsEqualToVisibilityConverter m_instance = null;

		public static IsEqualToVisibilityConverter Instance
		{
			get
			{
				if (m_instance == null)
					m_instance = new IsEqualToVisibilityConverter();

				return m_instance;
			}
		}
		#endregion

		#region · Converter functions ·

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool condition = false;

			// determine condition result
			if (value == null)
			{
				condition = (parameter == null);
			}
			else
			{
				if (parameter == null)
				{
					condition = (value == null);
				}
				else
				{
					if (value is String)
					{
						condition = value.ToString().Equals(parameter.ToString());
					}
					else
					{
						object param = TypeDescriptor.GetConverter(value).ConvertFrom(parameter);

						condition = value.Equals(param);
					}
				}
			}

			// generate result value
			if (!condition)
			{
				if (m_is_hidden)
				{
					return Visibility.Hidden;
				}
				else
				{
					return Visibility.Collapsed;
				}
			}
			return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
