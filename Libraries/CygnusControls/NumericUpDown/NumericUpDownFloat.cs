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
// Implementation of Numeric Up/Down Control (float value)
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Linq;
using System.Windows;

namespace CygnusControls
{
	public class NumericUpDownFloat : NumericUpDownBase
	{
		#region · Constructor ·

		/// <summary>
		/// Default constuctor
		/// </summary>
		public NumericUpDownFloat() : base()
		{
			m_culture.NumberFormat.NumberDecimalDigits = DecimalPlaces;
		}
		#endregion

		#region · Properties ·

		#region · Value ·

		public static readonly DependencyProperty ValueProperty =
				DependencyProperty.Register("Value", typeof(float), typeof(NumericUpDownFloat),
																		new PropertyMetadata(0.0f, OnValueChanged, CoerceValue));

		public float Value
		{
			get { return (float)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, (float)value); }
		}

		private static void OnValueChanged(DependencyObject element,
																			 DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownFloat control = (NumericUpDownFloat)element;

			if (control.m_text_box != null)
			{
				control.m_text_box.UndoLimit = 0;
				control.m_text_box.UndoLimit = 1;
			}
		}

		private static readonly string m_all_float_digits = "0." + new string('#', 40);

		private static object CoerceValue(DependencyObject element, object baseValue)
		{
			NumericUpDownFloat control = (NumericUpDownFloat)element;
			float value = (float)baseValue;

			control.CoerceValueToBounds(ref value);

			// Get the text representation of Value
			string valueString = value.ToString(m_all_float_digits);

			// Count all decimal places
			int decimalPlaces = control.GetDecimalPlacesCount(valueString);

			if (decimalPlaces > control.DecimalPlaces)
			{
				if (control.IsDecimalPointDynamic)
				{
					// Assigning DecimalPlaces will coerce the number
					control.DecimalPlaces = decimalPlaces;

					// If the specified number of decimal places is still too much
					if (decimalPlaces > control.DecimalPlaces)
					{
						value = control.TruncateValue(valueString, control.DecimalPlaces);
					}
				}
				else
				{
					// Remove all overflowing decimal places
					value = control.TruncateValue(valueString, decimalPlaces);
				}
			}
			else if (control.IsDecimalPointDynamic)
			{
				control.DecimalPlaces = decimalPlaces;
			}

			string new_text;

			if (control.m_text_box != null)
			{
				if (control.IsThousandSeparatorVisible)
				{
					new_text = value.ToString("N", control.m_culture);
				}
				else
				{
					new_text = value.ToString("F", control.m_culture);
				}

				// update caret index
				int new_caret_index = new_text.Length - (control.m_text_box.Text.Length - control.m_text_box.CaretIndex);

				if (new_caret_index < 0)
					new_caret_index = 0;

				if (new_caret_index >= new_text.Length)
					new_caret_index = new_text.Length - 1;

				// update selection
				int original_decimal_places = control.GetDecimalPlacesCount(control.m_text_box.Text);
				int new_decimal_places = control.GetDecimalPlacesCount(new_text);
				int new_selection_start_index = control.m_text_box.SelectionStart;
				int original_selection_length = control.m_text_box.SelectionLength;
				if (control.m_text_box.SelectionLength == 1)
				{
					new_selection_start_index = control.m_text_box.SelectionStart - (control.m_text_box.Text.Length - original_decimal_places) + (new_text.Length - new_decimal_places);

					if (new_text.Length < new_selection_start_index + 1)
					{
						if (!new_text.Contains(control.m_culture.NumberFormat.NumberDecimalSeparator))
						{
							new_selection_start_index += 1;
							new_text += '.';
						}

						new_text += new string('0', new_selection_start_index - new_text.Length + 1);
					}
				}

				control.m_text_box.Text = new_text;
				control.m_text_box.CaretIndex = new_caret_index;
				control.m_text_box.Select(new_selection_start_index, original_selection_length);
			}

			return baseValue;
		}

		#endregion

		#region · MaxValue ·

		public static readonly DependencyProperty MaxValueProperty =
				DependencyProperty.Register("MaxValue", typeof(float), typeof(NumericUpDownFloat),
																		new PropertyMetadata(float.MaxValue, OnMaxValueChanged));

		public float MaxValue
		{
			get { return (float)GetValue(MaxValueProperty); }
			set { SetValue(MaxValueProperty, value); }
		}

		private static void OnMaxValueChanged(DependencyObject element,
																					DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownFloat control = (NumericUpDownFloat)element;
			float max_value = (float)e.NewValue;

			// If maxValue steps over MinValue, shift it
			if (max_value < (int)control.MinValue)
			{
				control.MinValue = max_value;
			}

			// max limit of the value
			if (max_value < (int)control.Value)
			{
				control.Value = max_value;
			}
		}

		#endregion

		#region · MinValue ·

		public static readonly DependencyProperty MinValueProperty =
				DependencyProperty.Register("MinValue", typeof(float), typeof(NumericUpDownFloat),
																		new PropertyMetadata(float.MinValue, OnMinValueChanged));

		public float MinValue
		{
			get { return (float)GetValue(MinValueProperty); }
			set { SetValue(MinValueProperty, value); }
		}

		private static void OnMinValueChanged(DependencyObject element,
																					DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownFloat control = (NumericUpDownFloat)element;
			float min_value = (float)e.NewValue;

			// If minValue steps over MaxValue, shift it
			if (min_value > control.MaxValue)
			{
				control.MaxValue = min_value;
			}

			if (min_value > control.Value)
			{
				control.Value = min_value;
			}
		}

		#endregion

		#region · MinorDelta ·

		public static readonly DependencyProperty MinorDeltaProperty =
				DependencyProperty.Register("MinorDelta", typeof(float), typeof(NumericUpDownFloat),
																		new PropertyMetadata(1f, OnMinorDeltaChanged));

		public float MinorDelta
		{
			get { return (float)GetValue(MinorDeltaProperty); }
			set { SetValue(MinorDeltaProperty, value); }
		}

		private static void OnMinorDeltaChanged(DependencyObject element,
																						DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownFloat control = (NumericUpDownFloat)element;
			float minorDelta = (float)e.NewValue;

			if (minorDelta > control.MajorDelta)
			{
				control.MajorDelta = minorDelta;
			}
		}

		#endregion

		#region · MajorDelta ·

		public static readonly DependencyProperty MajorDeltaProperty =
				DependencyProperty.Register("MajorDelta", typeof(float), typeof(NumericUpDownFloat),
																		new PropertyMetadata(10f, OnMajorDeltaChanged));

		public float MajorDelta
		{
			get { return (float)GetValue(MajorDeltaProperty); }
			set { SetValue(MajorDeltaProperty, value); }
		}

		private static void OnMajorDeltaChanged(DependencyObject element,
																						DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownFloat control = (NumericUpDownFloat)element;
			float majorDelta = (float)e.NewValue;

			if (majorDelta < control.MinorDelta)
			{
				control.MinorDelta = majorDelta;
			}
		}

		#endregion

		#region · DecimalPlaces ·

		public static readonly DependencyProperty DecimalPlacesProperty =
				DependencyProperty.Register("DecimalPlaces", typeof(Int32), typeof(NumericUpDownFloat),
																		new PropertyMetadata(0, OnDecimalPlacesChanged,
																												 CoerceDecimalPlaces));

		public Int32 DecimalPlaces
		{
			get { return (Int32)GetValue(DecimalPlacesProperty); }
			set { SetValue(DecimalPlacesProperty, value); }
		}

		private static void OnDecimalPlacesChanged(DependencyObject element,
																							 DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownFloat control = (NumericUpDownFloat)element;
			Int32 decimalPlaces = (Int32)e.NewValue;

			control.m_culture.NumberFormat.NumberDecimalDigits = decimalPlaces;

			if (control.IsDecimalPointDynamic)
			{
				control.IsDecimalPointDynamic = false;
				control.InvalidateProperty(ValueProperty);
				control.IsDecimalPointDynamic = true;
			}
			else
			{
				control.InvalidateProperty(ValueProperty);
			}
		}

		private static object CoerceDecimalPlaces(DependencyObject element, Object baseValue)
		{
			Int32 decimalPlaces = (Int32)baseValue;
			NumericUpDownFloat control = (NumericUpDownFloat)element;

			if (decimalPlaces < control.MinDecimalPlaces)
			{
				decimalPlaces = control.MinDecimalPlaces;
			}
			else if (decimalPlaces > control.MaxDecimalPlaces)
			{
				decimalPlaces = control.MaxDecimalPlaces;
			}

			return decimalPlaces;
		}

		#endregion

		#region · MaxDecimalPlaces ·

		public static readonly DependencyProperty MaxDecimalPlacesProperty =
				DependencyProperty.Register("MaxDecimalPlaces", typeof(Int32), typeof(NumericUpDownFloat),
																		new PropertyMetadata(28, OnMaxDecimalPlacesChanged,
																												 CoerceMaxDecimalPlaces));

		public Int32 MaxDecimalPlaces
		{
			get { return (Int32)GetValue(MaxDecimalPlacesProperty); }
			set { SetValue(MaxDecimalPlacesProperty, value); }
		}

		private static void OnMaxDecimalPlacesChanged(DependencyObject element,
																									DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownFloat control = (NumericUpDownFloat)element;

			control.InvalidateProperty(DecimalPlacesProperty);
		}

		private static object CoerceMaxDecimalPlaces(DependencyObject element, Object baseValue)
		{
			Int32 maxDecimalPlaces = (Int32)baseValue;
			NumericUpDownFloat control = (NumericUpDownFloat)element;

			if (maxDecimalPlaces > 28)
			{
				maxDecimalPlaces = 28;
			}
			else if (maxDecimalPlaces < 0)
			{
				maxDecimalPlaces = 0;
			}
			else if (maxDecimalPlaces < control.MinDecimalPlaces)
			{
				control.MinDecimalPlaces = maxDecimalPlaces;
			}

			return maxDecimalPlaces;
		}

		#endregion

		#region · MinDecimalPlaces ·

		public static readonly DependencyProperty MinDecimalPlacesProperty =
				DependencyProperty.Register("MinDecimalPlaces", typeof(Int32), typeof(NumericUpDownFloat),
																		new PropertyMetadata(0, OnMinDecimalPlacesChanged,
																												 CoerceMinDecimalPlaces));

		public Int32 MinDecimalPlaces
		{
			get { return (Int32)GetValue(MinDecimalPlacesProperty); }
			set { SetValue(MinDecimalPlacesProperty, value); }
		}

		private static void OnMinDecimalPlacesChanged(DependencyObject element,
																									DependencyPropertyChangedEventArgs e)
		{
			NumericUpDownFloat control = (NumericUpDownFloat)element;

			control.InvalidateProperty(DecimalPlacesProperty);
		}

		private static object CoerceMinDecimalPlaces(DependencyObject element, Object baseValue)
		{
			Int32 minDecimalPlaces = (Int32)baseValue;
			NumericUpDownFloat control = (NumericUpDownFloat)element;

			if (minDecimalPlaces < 0)
			{
				minDecimalPlaces = 0;
			}
			else if (minDecimalPlaces > 28)
			{
				minDecimalPlaces = 28;
			}
			else if (minDecimalPlaces > control.MaxDecimalPlaces)
			{
				control.MaxDecimalPlaces = minDecimalPlaces;
			}

			return minDecimalPlaces;
		}

		#endregion

		#region · IsDecimalPointDynamic ·

		public static readonly DependencyProperty IsDecimalPointDynamicProperty =
				DependencyProperty.Register("IsDecimalPointDynamic", typeof(Boolean), typeof(NumericUpDownBase),
																		new PropertyMetadata(true));

		public Boolean IsDecimalPointDynamic
		{
			get { return (Boolean)GetValue(IsDecimalPointDynamicProperty); }
			set { SetValue(IsDecimalPointDynamicProperty, value); }
		}

		#endregion

		#endregion

		#region · SubCoercion ·

		private void CoerceValueToBounds(ref float value)
		{
			if (value < MinValue)
			{
				value = MinValue;
			}
			else if (value > MaxValue)
			{
				value = MaxValue;
			}
		}


		#endregion

		#region · Methods ·

		protected override void UpdateValue()
		{
			float value;

			RetrieveValue(out value);

			Value = value;
		}

		private void ChangeValue(float changes)
		{
			// Get the value that's currently in the _textBox.Text
			float new_value;

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
			float changes;

			changes = DetermineChanges(minor);

			ChangeValue(changes);
		}

		protected override void DecreaseValue(Boolean minor)
		{
			float changes;

			changes = -DetermineChanges(minor);

			ChangeValue(changes);
		}

		private float DetermineChanges(Boolean minor)
		{
			float changes;

			// check if only one character is selected in the textbox
			if (m_text_box.SelectionLength == 1)
			{
				int decimal_places_count = GetDecimalPlacesCount(m_text_box.Text);
				int exponent;

				if (decimal_places_count > 0)
				{
					exponent = m_text_box.Text.Length - decimal_places_count - m_text_box.SelectionStart;

					if (exponent > 0)
						exponent -= 2;
					else
						exponent -= 1;
				}
				else
				{
					exponent = m_text_box.Text.Length - m_text_box.SelectionStart - 1;
				}
				changes = (float)Math.Pow(10,exponent);
			}
			else
			{
				if (minor)
					changes = MinorDelta;
				else
					changes = MajorDelta;
			}

			return changes;
		}

		#endregion

		#region · Data retrieval and deposit ·

		private void RetrieveValue(out float out_value)
		{
			float value;
			float.TryParse(m_text_box.Text, out value);

			out_value = value;
		}

		public Int32 GetDecimalPlacesCount(String valueString)
		{
			return valueString.SkipWhile(c => c.ToString(m_culture) != m_culture.NumberFormat.NumberDecimalSeparator).Skip(1).Count();
		}

		protected float TruncateValue(String valueString, Int32 decimalPlaces)
		{
			var endPoint = valueString.Length - (decimalPlaces - DecimalPlaces);
			endPoint++;

			var tempValueString = valueString.Substring(0, endPoint);

			return float.Parse(tempValueString, m_culture);
		}

		#endregion
	}
}
