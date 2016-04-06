using System;
using System.Globalization;
using System.Windows.Data;

namespace CygnusControls
{
	public sealed class NumericFormatConverter : IValueConverter, IMultiValueConverter
	{
		public NumericFormatConverter()
		{
			PaddingCharacter = ' ';
		}


		public char PaddingCharacter
		{
			get;
			set;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(new object[] { value }, targetType, parameter, culture);
		}

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			string format = parameter as string;
			string result;

			if (string.IsNullOrEmpty(format))
				throw new ArgumentException();

			if (values == null)
				return Binding.DoNothing;

			result = string.Format(culture, format, values);

			if (PaddingCharacter != ' ')
			{
				result = result.Replace(' ', PaddingCharacter);
			}

			return result;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
