///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2016 Laszlo Arvai. All rights reserved.
// Code is based on: http://www.codeproject.com/Articles/509824/Creating-a-NumericUpDown-control-from-scratch
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
// Implementation of Numeric Up/Down Control (int value)
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Windows;

namespace CygnusControls
{
	public class NumericUpDownInt : NumericUpDownBase
	{
		#region · Properties ·

		#region · Value ·

		public static readonly DependencyProperty ValueProperty =
		DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDownInt), 
			new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

		public int Value
		{
			get { return (int)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, (int)value); }
		}

		private static void OnValueChanged(DependencyObject element,
																			 DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownInt control = (NumericUpDownInt)element;

			if (control.m_text_box != null)
			{
				control.m_text_box.UndoLimit = 0;
				control.m_text_box.UndoLimit = 1;
			}
		}


		private static object CoerceValue(DependencyObject element, object baseValue)
		{
			NumericUpDownInt control = (NumericUpDownInt)element;
			int value = (int)baseValue;

			control.CoerceValueToBounds(ref value);

			// Get the text representation of Value
			string valueString = value.ToString(control.m_culture);

			if (control.IsThousandSeparatorVisible)
			{
				if (control.m_text_box != null)
				{
					control.m_text_box.Text = value.ToString("N", control.m_culture);
				}
			}
			else
			{
				if (control.m_text_box != null)
				{
					control.m_text_box.Text = value.ToString();
				}
			}

			return baseValue;
		}

		#endregion

		#region · MaxValue ·

		public static readonly DependencyProperty MaxValueProperty =
				DependencyProperty.Register("MaxValue", typeof(int), typeof(NumericUpDownInt),
																		new PropertyMetadata(int.MaxValue, OnMaxValueChanged));

		public int MaxValue
		{
			get { return (int)GetValue(MaxValueProperty); }
			set { SetValue(MaxValueProperty, value); }
		}

		private static void OnMaxValueChanged(DependencyObject element,
																					DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownInt control = (NumericUpDownInt)element;
			int max_value = (int)e.NewValue;

			// If maxValue steps over MinValue, shift it
			if (max_value < (int)control.MinValue)
			{
				control.MinValue = max_value;
			}

			// max limit of the value
			if (max_value <= (int)control.Value)
			{
				control.Value = max_value;
			}
		}

		#endregion

		#region · MinValue ·

		public static readonly DependencyProperty MinValueProperty =
				DependencyProperty.Register("MinValue", typeof(int), typeof(NumericUpDownInt),
																		new PropertyMetadata(int.MinValue, OnMinValueChanged));

		public int MinValue
		{
			get { return (int)GetValue(MinValueProperty); }
			set { SetValue(MinValueProperty, value); }
		}

		private static void OnMinValueChanged(DependencyObject element,
																					DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownInt control = (NumericUpDownInt)element;
			int min_value = (int)e.NewValue;

			// If minValue steps over MaxValue, shift it
			if (min_value > (int)control.MaxValue)
			{
				control.MaxValue = min_value;
			}

			if (min_value > (int)control.Value)
			{
				control.Value = min_value;
			}
		}

		#endregion

		#region · MinorDelta ·

		public static readonly DependencyProperty MinorDeltaProperty =
				DependencyProperty.Register("MinorDelta", typeof(int), typeof(NumericUpDownInt),
																		new PropertyMetadata(1, OnMinorDeltaChanged));

		public int MinorDelta
		{
			get { return (int)GetValue(MinorDeltaProperty); }
			set { SetValue(MinorDeltaProperty, value); }
		}

		private static void OnMinorDeltaChanged(DependencyObject element,
																						DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownInt control = (NumericUpDownInt)element;
			int minorDelta = (int)e.NewValue;

			if (minorDelta > control.MajorDelta)
			{
				control.MajorDelta = minorDelta;
			}
		}

		#endregion

		#region · MajorDelta ·

		public static readonly DependencyProperty MajorDeltaProperty =
				DependencyProperty.Register("MajorDelta", typeof(int), typeof(NumericUpDownInt),
																		new PropertyMetadata(10, OnMajorDeltaChanged));

		public int MajorDelta
		{
			get { return (int)GetValue(MajorDeltaProperty); }
			set { SetValue(MajorDeltaProperty, value); }
		}

		private static void OnMajorDeltaChanged(DependencyObject element,
																						DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownInt control = (NumericUpDownInt)element;
			int majorDelta = (int)e.NewValue;

			if (majorDelta < control.MinorDelta)
			{
				control.MinorDelta = majorDelta;
			}
		}

		#endregion

		#endregion

		#region · SubCoercion ·

		private void CoerceValueToBounds(ref int value)
		{
			if (value < (int)MinValue)
			{
				value = (int)MinValue;
			}
			else if (value > (int)MaxValue)
			{
				value = (int)MaxValue;
			}
		}


		#endregion

		#region ·  Methods ·

		private void RetrieveValue(out int out_value)
		{
			int value;
			int.TryParse(m_text_box.Text, out value);

			out_value = value;

		}

		protected override void UpdateValue()
		{
			int value;

			RetrieveValue(out value);

			Value = value;
		}

		private void ChangeValue(int changes)
		{
			// Get the value that's currently in the _textBox.Text
			int new_value;

			RetrieveValue(out new_value);

			// Coerce the value to min/max
			CoerceValueToBounds(ref new_value);

			// change value
			new_value += changes;

			// check for min
			if (new_value < MinValue)
			{
				if (IsValueWrapAllowed)
				{
					new_value = MaxValue;
				}
				else
				{
					new_value = MinValue;
				}
			}

			// check for min
			if (new_value > MaxValue)
			{
				if (IsValueWrapAllowed)
				{
					new_value = MinValue;
				}
				else
				{
					new_value = MaxValue;
				}
			}

			Value = new_value;
		}

		protected override void InvalidateValueProperty()
		{
			InvalidateProperty(ValueProperty);
		}

		protected override void IncreaseValue(Boolean minor)
		{
			int changes;

			if (minor)
				changes = MinorDelta;
			else
				changes = MajorDelta;

			ChangeValue(changes);
		}

		protected override void DecreaseValue(Boolean minor)
		{
			int changes;

			if (minor)
				changes = -MinorDelta;
			else
				changes = -MajorDelta;

			ChangeValue(changes);
		}
		#endregion
	}
}
