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
// Collection of simple value converters
///////////////////////////////////////////////////////////////////////////////
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Xml;

namespace CygnusControls
{
  /// <summary>
  /// WPF/Silverlight ValueConverter : return true if Value differs from Parameter
  /// </summary>
	[TypeConverter(typeof(IsDifferentConverter))]
	[ValueConversion(typeof(object), typeof(bool))]
	public class IsDifferentConverter : IValueConverter
  {
    #region Converter function
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
			return !((bool)IsEqualConverter.Instance.Convert(value, targetType, parameter, culture));
    }
    #endregion

    #region Singleton Implementation
    /// <summary>
    /// Singleton storage
    /// </summary>
    private static IsDifferentConverter m_instance = new IsDifferentConverter();

    /// <summary> 
    /// The TriggerComparer instance 
    /// </summary> 
    public static IsDifferentConverter Instance
    {
      get
      {
        return m_instance;
      }
    }
    #endregion

    #region Convert back (non implemented)
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }
    #endregion
  }

  /// <summary>
  /// WPF ValueConverter : return true if Value equals Parameter 
  /// </summary>
	[TypeConverter(typeof(IsEqualConverter))]
	[ValueConversion(typeof(object), typeof(bool))]
	public class IsEqualConverter : IValueConverter
  {
    #region Converter function
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (targetType != typeof(bool) && targetType != typeof(object))
      {
        throw new ArgumentException("Target must be a boolean");
      }

      if (value == null)
      {
        return (parameter == null);
      }

      if (parameter == null)
      {
        return (value == null);
      }

      if (value is String)
      {
        return value.ToString().Equals(parameter.ToString());
      }

      object param = TypeDescriptor.GetConverter(value).ConvertFrom(parameter);

      return value.Equals(param);
    }
    #endregion

    #region Singleton Implementation

    /// <summary>
    /// Singleton storage
    /// </summary>
    private static IsEqualConverter m_instance = new IsEqualConverter();

    /// <summary> 
    /// The TriggerComparer instance 
    /// </summary> 
    public static IsEqualConverter Instance
    {
      get
      {
        return m_instance;
      }
    }
    #endregion

    #region Convert back (non implemented)
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }
    #endregion
  }

  /// <summary>
  /// WPF ValueConverter : Return inverted boolean (=Value)
  /// </summary>
	[ValueConversion(typeof(bool), typeof(bool))]
  public class InvertBoolConverter : IValueConverter
  {
    #region Converter function
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return !(bool)value;
    }
    #endregion

    #region Singleton Implementation

    /// <summary>
    /// Singleton storage
    /// </summary>
    private static InvertBoolConverter m_instance = new InvertBoolConverter();

    /// <summary> 
    /// The TriggerComparer instance 
    /// </summary> 
    public static InvertBoolConverter Instance
    {
      get
      {
        return m_instance;
      }
    }
    #endregion

    #region Convert back (non implemented)
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }
    #endregion
  }

  /// <summary>
  /// WPF ValueConverter : Return true if Value is less than Parameter
  /// </summary>
  public class IsLessThanConverter : IValueConverter
  {
    #region Converter function
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (targetType != typeof(bool) && targetType != typeof(object))
      {
        throw new ArgumentException("Target must be a boolean");
      }

      if (value == null || parameter == null)
      {
        return false;
      }

      double convertedValue;
      if (!double.TryParse(value.ToString(), out convertedValue))
      {
        throw new InvalidOperationException("The Value can not be converted to a Double");
      }

      double convertedParameter;
      if (!double.TryParse(parameter.ToString(), out convertedParameter))
      {
        throw new InvalidOperationException("The Parameter can not be converted to a Double");
      }

      return convertedValue < convertedParameter;
    }
    #endregion

    #region Singleton Implementation

    /// <summary>
    /// Singleton storage
    /// </summary>
    private static IsLessThanConverter m_instance = new IsLessThanConverter();

    /// <summary> 
    /// The TriggerComparer instance 
    /// </summary> 
    public static IsLessThanConverter Instance
    {
      get
      {
        return m_instance;
      }
    }
    #endregion

    #region Convert back (non implemented)
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }
    #endregion
  }

  /// <summary>
  /// WPF ValueConverter : Return true if Value is less than Parameter
  /// </summary>
  public class IsGreaterThanConverter : IValueConverter
  {
    #region Converter function
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (targetType != typeof(bool) && targetType != typeof(object))
      {
        throw new ArgumentException("Target must be a boolean");
      }

      if (value == null || parameter == null)
      {
        return false;
      }

      double convertedValue;
      if (!double.TryParse(value.ToString(), out convertedValue))
      {
        throw new InvalidOperationException("The Value can not be converted to a Double");
      }

      double convertedParameter;
      if (!double.TryParse(parameter.ToString(), out convertedParameter))
      {
        throw new InvalidOperationException("The Parameter can not be converted to a Double");
      }

      return convertedValue > convertedParameter;
    }
    #endregion

    #region Singleton Implementation

    /// <summary>
    /// Singleton storage
    /// </summary>
    private static IsGreaterThanConverter m_instance = new IsGreaterThanConverter();

    /// <summary> 
    /// The TriggerComparer instance 
    /// </summary> 
    public static IsGreaterThanConverter Instance
    {
      get
      {
        return m_instance;
      }
    }
    #endregion

    #region Convert back (non implemented)
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }

    #endregion
  }

  /// <summary>
  /// WPF ValueConverter : does Value match the regular expression (=Parameter) ?
  /// </summary>
  public class IsRegexMatchConverter : IValueConverter
  {
    #region Converter function
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if ((value == null) || (parameter == null))
      {
        return false;
      }

      Regex regex = new Regex((string)parameter);
      return regex.IsMatch((string)value);
    }
    #endregion

    #region Singleton Implementation

    /// <summary>
    /// Singleton storage
    /// </summary>
    private static IsRegexMatchConverter m_instance = new IsRegexMatchConverter();

    /// <summary> 
    /// The TriggerComparer instance 
    /// </summary> 
    public static IsRegexMatchConverter Instance
    {
      get
      {
        return m_instance;
      }
    }
    #endregion

    #region Convert back (non implemented)
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotSupportedException();
    }

    #endregion
  }

	/// <summary>
	/// Converts an XmlAttribute to its Value, as a string.
	/// </summary>
	[ValueConversion(typeof(XmlAttribute), typeof(string))]
	public class XmlAttributeToStringConverter : IValueConverter
	{
		#region Converter function
		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (targetType != typeof(string))
			{
				throw new ArgumentException("Target must be a string");
			}

			if (!(value is XmlAttribute))
			{
				throw new ArgumentException("Value must be an XmlAttribute");
			}

			XmlAttribute attr = value as XmlAttribute;
			return attr.Value;
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static XmlAttributeToStringConverter m_instance = new XmlAttributeToStringConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static XmlAttributeToStringConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back
		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("ConvertBack not supported.");
		}
		#endregion
	}

	/// <summary>
	/// DoubleToIntegerValueConverter provides a two-way conversion between
	/// a double value and an integer.
	/// </summary>
	[ValueConversion(typeof(double), typeof(int))]
	public class DoubleToIntegerValueConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				return GeneralConvert(value, targetType);
			}
			catch
			{
				return null;
			}
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static DoubleToIntegerValueConverter m_instance = new DoubleToIntegerValueConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static DoubleToIntegerValueConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return GeneralConvert(value, targetType);
		}
		#endregion

		#region General conversion function
		private object GeneralConvert(object in_value, Type in_target_type)
		{
			if (in_target_type == typeof(double))
			{
				if (in_value == null)
					return 0.0;
				else
					return System.Convert.ToDouble(in_value);
			}
			else
			{
				if (in_value == null)
					return 0;
				else
					return System.Convert.ToInt32(in_value);
			}
		}
		#endregion
	}


	/// <summary>
	/// DoubleToIntegerValueConverter provides a two-way conversion between
	/// a double value and an integer.
	/// </summary>
	[ValueConversion(typeof(double), typeof(int))]
	public class IntegerToDoubleValueConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				return GeneralConvert(value, targetType);
			}
			catch
			{
				return null;
			}
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static IntegerToDoubleValueConverter m_instance = new IntegerToDoubleValueConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static IntegerToDoubleValueConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return GeneralConvert(value, targetType);
		}
		#endregion

		#region General conversion function
		private object GeneralConvert(object in_value, Type in_target_type)
		{
			if (in_target_type == typeof(int))
			{
				if (in_value == null)
					return 0;
				else
					return System.Convert.ToInt32(in_value);
			}
			else
			{
				if (in_value == null)
					return 0.0;
				else
					return System.Convert.ToDouble(in_value);
			}
		}
		#endregion
	}

	/// <summary>
	/// DoubleToIntegerValueConverter provides a two-way conversion between
	/// a double value and an integer.
	/// </summary>
	[ValueConversion(typeof(int), typeof(byte))]
	public class IntegerToByteValueConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			try
			{
				return GeneralConvert(value, targetType);
			}
			catch
			{
				return null;
			}
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static IntegerToByteValueConverter m_instance = new IntegerToByteValueConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static IntegerToByteValueConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return GeneralConvert(value, targetType);
		}
		#endregion

		#region General conversion function
		private object GeneralConvert(object in_value, Type in_target_type)
		{
			if (in_target_type == typeof(byte))
			{
				if (in_value == null)
					return 0;
				else
					return System.Convert.ToInt32(in_value);
			}
			else
			{
				if (in_value == null)
					return 0;
				else
					return System.Convert.ToByte(in_value);
			}
		}
		#endregion
	}

	/// <summary>
	/// Converts string to boolean
	/// </summary>
	[ValueConversion(typeof(string), typeof(bool))]
	public class StringToBoolConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToBoolean(value);
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static StringToBoolConverter m_instance = new StringToBoolConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static StringToBoolConverter Instance
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
			return System.Convert.ToString(value);
		}
		#endregion
	}

	/// <summary>
	/// Converts string to boolean
	/// </summary>
	[ValueConversion(typeof(string), typeof(Int32))]
	public class StringToIntConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return System.Convert.ToInt32(value);
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static StringToIntConverter m_instance = new StringToIntConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static StringToIntConverter Instance
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
			return System.Convert.ToString(value);
		}
		#endregion
	}
	
	/// <summary>
	/// Converts object to string
	/// </summary>
	[ValueConversion(typeof(object), typeof(string))]
	public class ObjectToStringConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
				return value.ToString();
			else
				return null;
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static ObjectToStringConverter m_instance = new ObjectToStringConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static ObjectToStringConverter Instance
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
			return System.Convert.ChangeType(value, targetType);
		}
		#endregion
	}

	/// <summary>
	/// Converts text to single line
	/// </summary>
	public class SingleLineTextConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string s = (string)value;
			s = s.Replace(Environment.NewLine, " ");
			return s;
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static SingleLineTextConverter m_instance = new SingleLineTextConverter();

		/// <summary> 
		/// The SingleLineTextConverter instance 
		/// </summary> 
		public static SingleLineTextConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion
	}

	/// <summary>
	/// return true if Value differs from Parameter (Value2)
	/// </summary>
	public class IsEqualBindableConverter : IMultiValueConverter
	{
		#region Converter function
		public object Convert(object[] values, Type targetType,
													object parameter, CultureInfo culture)
		{
			if (values.Length != 2)
			{
				throw new ArgumentException("Two values must be supplied");
			}

			if (values[0] == DependencyProperty.UnsetValue)
			{
				return (values[1] == DependencyProperty.UnsetValue);
			}

			if (targetType != typeof(bool) && targetType != typeof(object))
			{
				throw new ArgumentException("Target must be a boolean");
			}

			if (values[0] == null)
			{
				return (values[1] == null);
			}

			if (values[0] is String && values[1] is String)
			{
				return values[0].ToString().Equals(values[1].ToString());
			}

			if (values[0].GetType() == values[1].GetType())
			{
				return values[0].Equals(values[1]);
			}

			object param = TypeDescriptor.GetConverter(values[0]).ConvertFrom(values[1]);

			return values[0].Equals(param);
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static IsEqualBindableConverter m_instance = new IsEqualBindableConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static IsEqualBindableConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back
		public object[] ConvertBack(object value, Type[] targetTypes,
																object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
		#endregion
	}

	/// <summary>
	/// return true if Value differs from Parameter (Value2)
	/// </summary>
	public class IsDifferentBindableConverter : IMultiValueConverter
	{
		#region Converter function
		public object Convert(object[] values, Type targetType,
													object parameter, CultureInfo culture)
		{
			return !((bool)IsEqualBindableConverter.Instance.Convert(values, targetType, parameter, culture));
		}
		#endregion

		#region Singleton Implementation

		/// <summary>
		/// Singleton storage
		/// </summary>
		private static IsDifferentBindableConverter m_instance = new IsDifferentBindableConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static IsDifferentBindableConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back
		public object[] ConvertBack(object value, Type[] targetTypes,
																object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
		#endregion
	}

	/// <summary>
	/// Returns 'parameter' value when 'value' is null
	/// </summary>
	[TypeConverter(typeof(DefaultValueConverter))]
	[ValueConversion(typeof(object), typeof(object))]
	public class DefaultValueConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return parameter;
			else
				return value;
		}
		#endregion

		#region Singleton Implementation
		/// <summary>
		/// Singleton storage
		/// </summary>
		private static DefaultValueConverter m_instance = new DefaultValueConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static DefaultValueConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value;
		}
		#endregion
	}

	/// <summary>
	/// Returns 'parameter' value when 'value' is null
	/// </summary>
	[TypeConverter(typeof(IsNullOrEmptyConverter))]
	[ValueConversion(typeof(string), typeof(bool))]
	public class IsNullOrEmptyConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return string.IsNullOrEmpty((string)value);
		}
		#endregion

		#region Singleton Implementation
		/// <summary>
		/// Singleton storage
		/// </summary>
		private static IsNullOrEmptyConverter m_instance = new IsNullOrEmptyConverter();

		/// <summary> 
		/// The TriggerComparer instance 
		/// </summary> 
		public static IsNullOrEmptyConverter Instance
		{
			get
			{
				return m_instance;
			}
		}
		#endregion

		#region Convert back
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}
		#endregion
	}


	/// <summary>
	/// Adds parameter value to the value to convert
	/// </summary>
	[TypeConverter(typeof(AdditionConverter))]
	[ValueConversion(typeof(double), typeof(double))]
	public class AdditionConverter : IValueConverter
	{
		#region Converter function

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double dParameter;
			if (targetType != typeof(double) ||
					!double.TryParse((string)parameter, NumberStyles.Any, CultureInfo.InvariantCulture, out dParameter))
			{
				throw new InvalidOperationException("Value and parameter passed must be of type double");
			}

			double dValue = (double)value;
			dValue += dParameter;

			return dValue;
		}

		#endregion

		#region Singleton Implementation
		/// <summary>
		/// Singleton storage
		/// </summary>
		private static AdditionConverter m_instance = new AdditionConverter();

		/// <summary> 
		/// The AdditionConverter instance 
		/// </summary> 
		public static AdditionConverter Instance
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

	[TypeConverter(typeof(GridLengthConverter))]
	[ValueConversion(typeof(double), typeof(GridLength))]
	public class GridLengthConverter : IValueConverter
	{
		#region Converter function
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double val = (double)value;
			GridLength gridLength = new GridLength(val);

			return gridLength;
		}
		#endregion

		#region Singleton Implementation
		/// <summary>
		/// Singleton storage
		/// </summary>
		private static GridLengthConverter m_instance = new GridLengthConverter();

		/// <summary> 
		/// The AdditionConverter instance 
		/// </summary> 
		public static GridLengthConverter Instance
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
			GridLength val = (GridLength)value;

			return val.Value;
		}
		#endregion
	}
}
