///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Laszlo Arvai. All rights reserved.
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
// Implementation of Numeric Up/Down Control
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CygnusControls
{
	[TemplatePart(Name = "PART_tbValue", Type = typeof(TextBox))]
	[TemplatePart(Name = "PART_btnUp", Type = typeof(RepeatButton))]
	[TemplatePart(Name = "PART_btnDown", Type = typeof(RepeatButton))]
	public class NumericUpDown : Control
	{
		#region · Properties ·

		#region · Value ·

		public static readonly DependencyProperty ValueProperty =
					DependencyProperty.Register("Value", typeof(Decimal), typeof(NumericUpDown),
																			new PropertyMetadata(0m, OnValueChanged, CoerceValue));

		public Decimal Value
		{
			get { return (Decimal)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		private static void OnValueChanged(DependencyObject element,
																			 DependencyPropertyChangedEventArgs e)
		{
			var control = (NumericUpDown)element;

			if (control.m_text_box != null)
			{
				control.m_text_box.UndoLimit = 0;
				control.m_text_box.UndoLimit = 1;
			}
		}

		public static readonly DependencyProperty IntValueProperty =
					DependencyProperty.Register("IntValue", typeof(int), typeof(NumericUpDown),
						new FrameworkPropertyMetadata { BindsTwoWayByDefault = true, DefaultValue=0, CoerceValueCallback= CoerceValueInt , PropertyChangedCallback= OnIntValueChanged });
	
		public int IntValue
		{
			get { return (int)GetValue(IntValueProperty); }
			set { SetValue(IntValueProperty, value); }
		}

		private static void OnIntValueChanged(DependencyObject element,
																			 DependencyPropertyChangedEventArgs e)
		{
			var control = (NumericUpDown)element;

			if (control.m_text_box != null)
			{
				control.m_text_box.UndoLimit = 0;
				control.m_text_box.UndoLimit = 1;
			}
		}

		private static object CoerceValue(DependencyObject in_element, object in_value)
		{
			var control = (NumericUpDown)in_element;
			var value = (Decimal)in_value;
   
			control.CoerceValueToBounds(ref value);
			
			// Get the text representation of Value
			var valueString = value.ToString(control.Culture);

			// Count all decimal places
			var decimalPlaces = control.GetDecimalPlacesCount(valueString);

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

			if (control.IsThousandSeparatorVisible)
			{
				if (control.m_text_box != null)
				{
					control.m_text_box.Text = value.ToString("N", control.Culture);
				}
			}
			else
			{
				if (control.m_text_box != null)
				{
					control.m_text_box.Text = value.ToString("F", control.Culture);
				}
			}

			return value;
		}


		private static object CoerceValueInt(DependencyObject in_element, object in_value)
		{
			var control = (NumericUpDown)in_element;
			var value = (int)in_value;

			control.CoerceValueToBoundsInt(ref value);

			// Get the text representation of Value
			var valueString = value.ToString(control.Culture);

			//// Count all decimal places
			//var decimalPlaces = control.GetDecimalPlacesCount(valueString);

			//if (decimalPlaces > control.DecimalPlaces)
			//{
			//	if (control.IsDecimalPointDynamic)
			//	{
			//		// Assigning DecimalPlaces will coerce the number
			//		control.DecimalPlaces = decimalPlaces;

			//		// If the specified number of decimal places is still too much
			//		if (decimalPlaces > control.DecimalPlaces)
			//		{
			//			value = control.TruncateValue(valueString, control.DecimalPlaces);
			//		}
			//	}
			//	else
			//	{
			//		// Remove all overflowing decimal places
			//		value = control.TruncateValue(valueString, decimalPlaces);
			//	}
			//}
			//else if (control.IsDecimalPointDynamic)
			//{
			//	control.DecimalPlaces = decimalPlaces;
			//}

			if (control.IsThousandSeparatorVisible)
			{
				if (control.m_text_box != null)
				{
					control.m_text_box.Text = value.ToString("N", control.Culture);
				}
			}
			else
			{
				if (control.m_text_box != null)
				{
					control.m_text_box.Text = value.ToString("F", control.Culture);
				}
			}

			return value;
		}
		#endregion

		#region · MaxValue ·

		public static readonly DependencyProperty MaxValueProperty =
				DependencyProperty.Register("MaxValue", typeof(Decimal), typeof(NumericUpDown),
																		new PropertyMetadata(100000000m, OnMaxValueChanged,
																												 CoerceMaxValue));

		public Decimal MaxValue
		{
			get { return (Decimal)GetValue(MaxValueProperty); }
			set { SetValue(MaxValueProperty, value); }
		}

		private static void OnMaxValueChanged(DependencyObject element,
																					DependencyPropertyChangedEventArgs e)
		{
			var control = (NumericUpDown)element;
			var maxValue = (Decimal)e.NewValue;

			// If maxValue steps over MinValue, shift it
			if (maxValue < control.MinValue)
			{
				control.MinValue = maxValue;
			}

			if (maxValue <= control.Value)
			{
				control.Value = maxValue;
			}
		}

		private static object CoerceMaxValue(DependencyObject element, Object baseValue)
		{
			var maxValue = (Decimal)baseValue;

			if (maxValue == Decimal.MaxValue)
			{
				return DependencyProperty.UnsetValue;
			}

			return maxValue;
		}

		#endregion

		#region · MinValue ·

		public static readonly DependencyProperty MinValueProperty =
				DependencyProperty.Register("MinValue", typeof(Decimal), typeof(NumericUpDown),
																		new PropertyMetadata(0m, OnMinValueChanged,
																												 CoerceMinValue));

		public Decimal MinValue
		{
			get { return (Decimal)GetValue(MinValueProperty); }
			set { SetValue(MinValueProperty, value); }
		}

		private static void OnMinValueChanged(DependencyObject element,
																					DependencyPropertyChangedEventArgs e)
		{
			var control = (NumericUpDown)element;
			var minValue = (Decimal)e.NewValue;

			// If minValue steps over MaxValue, shift it
			if (minValue > control.MaxValue)
			{
				control.MaxValue = minValue;
			}

			if (minValue >= control.Value)
			{
				control.Value = minValue;
			}
		}

		private static object CoerceMinValue(DependencyObject element, Object baseValue)
		{
			var minValue = (Decimal)baseValue;

			if (minValue == Decimal.MinValue)
			{
				return DependencyProperty.UnsetValue;
			}

			return minValue;
		}

		#endregion

		#region · DecimalPlaces ·

		public static readonly DependencyProperty DecimalPlacesProperty =
				DependencyProperty.Register("DecimalPlaces", typeof(Int32), typeof(NumericUpDown),
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
			var control = (NumericUpDown)element;
			var decimalPlaces = (Int32)e.NewValue;

			control.Culture.NumberFormat.NumberDecimalDigits = decimalPlaces;

			if (control.IsDecimalPointDynamic)
			{
				control.IsDecimalPointDynamic = false;
				control.InvalidateProperty(ValueProperty);
				control.InvalidateProperty(IntValueProperty);
				control.IsDecimalPointDynamic = true;
			}
			else
			{
				control.InvalidateProperty(ValueProperty);
				control.InvalidateProperty(IntValueProperty);
			}
		}

		private static object CoerceDecimalPlaces(DependencyObject element, Object baseValue)
		{
			var decimalPlaces = (Int32)baseValue;
			var control = (NumericUpDown)element;

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
				DependencyProperty.Register("MaxDecimalPlaces", typeof(Int32), typeof(NumericUpDown),
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
			var control = (NumericUpDown)element;

			control.InvalidateProperty(DecimalPlacesProperty);
		}

		private static object CoerceMaxDecimalPlaces(DependencyObject element, Object baseValue)
		{
			var maxDecimalPlaces = (Int32)baseValue;
			var control = (NumericUpDown)element;

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
				DependencyProperty.Register("MinDecimalPlaces", typeof(Int32), typeof(NumericUpDown),
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
			var control = (NumericUpDown)element;

			control.InvalidateProperty(DecimalPlacesProperty);
		}

		private static object CoerceMinDecimalPlaces(DependencyObject element, Object baseValue)
		{
			var minDecimalPlaces = (Int32)baseValue;
			var control = (NumericUpDown)element;

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
				DependencyProperty.Register("IsDecimalPointDynamic", typeof(Boolean), typeof(NumericUpDown),
																		new PropertyMetadata(false));

		public Boolean IsDecimalPointDynamic
		{
			get { return (Boolean)GetValue(IsDecimalPointDynamicProperty); }
			set { SetValue(IsDecimalPointDynamicProperty, value); }
		}

		#endregion

		#region · MinorStep ·

		public static readonly DependencyProperty MinorStepProperty =
				DependencyProperty.Register("MinorStep", typeof(Decimal), typeof(NumericUpDown),
																		new PropertyMetadata(1m, OnMinorStepChanged,
																												 CoerceMinorStep));

		public Decimal MinorStep
		{
			get { return (Decimal)GetValue(MinorStepProperty); }
			set { SetValue(MinorStepProperty, value); }
		}

		private static void OnMinorStepChanged(DependencyObject element,
																						DependencyPropertyChangedEventArgs e)
		{
			var MinorStep = (Decimal)e.NewValue;
			var control = (NumericUpDown)element;

			if (MinorStep > control.MajorStep)
			{
				control.MajorStep = MinorStep;
			}
		}

		private static object CoerceMinorStep(DependencyObject element, Object baseValue)
		{
			var MinorStep = (Decimal)baseValue;

			return MinorStep;
		}

		#endregion

		#region · MajorStep ·

		public static readonly DependencyProperty MajorStepProperty =
				DependencyProperty.Register("MajorStep", typeof(Decimal), typeof(NumericUpDown),
																		new PropertyMetadata(10m, OnMajorStepChanged,
																												 CoerceMajorStep));

		public Decimal MajorStep
		{
			get { return (Decimal)GetValue(MajorStepProperty); }
			set { SetValue(MajorStepProperty, value); }
		}

		private static void OnMajorStepChanged(DependencyObject element,
																						DependencyPropertyChangedEventArgs e)
		{
			var MajorStep = (Decimal)e.NewValue;
			var control = (NumericUpDown)element;

			if (MajorStep < control.MinorStep)
			{
				control.MinorStep = MajorStep;
			}
		}

		private static object CoerceMajorStep(DependencyObject element, Object baseValue)
		{
			var MajorStep = (Decimal)baseValue;

			return MajorStep;
		}

		#endregion

		#region · IsThousandSeparatorVisible ·

		public static readonly DependencyProperty IsThousandSeparatorVisibleProperty =
				DependencyProperty.Register("IsThousandSeparatorVisible", typeof(Boolean), typeof(NumericUpDown),
																		new PropertyMetadata(false, OnIsThousandSeparatorVisibleChanged));

		public Boolean IsThousandSeparatorVisible
		{
			get { return (Boolean)GetValue(IsThousandSeparatorVisibleProperty); }
			set { SetValue(IsThousandSeparatorVisibleProperty, value); }
		}

		private static void OnIsThousandSeparatorVisibleChanged(DependencyObject element,
																														DependencyPropertyChangedEventArgs e)
		{
			var control = (NumericUpDown)element;

			control.InvalidateProperty(ValueProperty);
			control.InvalidateProperty(IntValueProperty);
		}

		#endregion

		#region · IsAutoSelectionActive ·

		public static readonly DependencyProperty IsAutoSelectionActiveProperty =
				DependencyProperty.Register("IsAutoSelectionActive", typeof(Boolean), typeof(NumericUpDown),
																		new PropertyMetadata(false));

		public Boolean IsAutoSelectionActive
		{
			get { return (Boolean)GetValue(IsAutoSelectionActiveProperty); }
			set { SetValue(IsAutoSelectionActiveProperty, value); }
		}

		#endregion

		#region · IsValueWrapAllowed ·

		public static readonly DependencyProperty IsValueWrapAllowedProperty =
				DependencyProperty.Register("IsValueWrapAllowed", typeof(Boolean), typeof(NumericUpDown),
																		new PropertyMetadata(false));

		public Boolean IsValueWrapAllowed
		{
			get { return (Boolean)GetValue(IsValueWrapAllowedProperty); }
			set { SetValue(IsValueWrapAllowedProperty, value); }
		}

		#endregion

		#endregion

		#region · Data members ·

		protected readonly CultureInfo Culture;
		protected RepeatButton m_decrease_button;
		protected RepeatButton m_increase_button;
		protected TextBox m_text_box;

		#endregion

		#region · Commands ·

		private readonly RoutedUICommand _minorDecreaseValueCommand =
				new RoutedUICommand("MinorDecreaseValue", "MinorDecreaseValue", typeof(NumericUpDown));

		private readonly RoutedUICommand _minorIncreaseValueCommand =
				new RoutedUICommand("MinorIncreaseValue", "MinorIncreaseValue", typeof(NumericUpDown));

		private readonly RoutedUICommand _majorDecreaseValueCommand =
				new RoutedUICommand("MajorDecreaseValue", "MajorDecreaseValue", typeof(NumericUpDown));

		private readonly RoutedUICommand _majorIncreaseValueCommand =
				new RoutedUICommand("MajorIncreaseValue", "MajorIncreaseValue", typeof(NumericUpDown));

		private readonly RoutedUICommand _updateValueStringCommand =
				new RoutedUICommand("UpdateValueString", "UpdateValueString", typeof(NumericUpDown));

		private readonly RoutedUICommand _cancelChangesCommand =
				new RoutedUICommand("CancelChanges", "CancelChanges", typeof(NumericUpDown));

		#endregion

		#region · Constructors ·

		static NumericUpDown()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown),
																							 new FrameworkPropertyMetadata(
																									 typeof(NumericUpDown)));
		}

		public NumericUpDown()
		{
			Culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();

			Culture.NumberFormat.NumberDecimalDigits = DecimalPlaces;

			Loaded += OnLoaded;
		}

		#endregion

		#region · Event handlers ·

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			AttachToVisualTree();
			AttachCommands();
		}

		private void TextBoxOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
		{
			UpdateValue();
		}

		private void TextBoxOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			if (IsAutoSelectionActive)
			{
				m_text_box.SelectAll();
			}
		}

		private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			InvalidateProperty(ValueProperty);
			InvalidateProperty(IntValueProperty);
		}

		private void ButtonOnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
		{
			Value = 0;
		}

		#endregion

		#region · Utility Methods ·

		#region Attachment

		private void AttachToVisualTree()
		{
			AttachTextBox();
			AttachIncreaseButton();
			AttachDecreaseButton();
		}

		private void AttachTextBox()
		{
			var textBox = GetTemplateChild("PART_tbValue") as TextBox;

			// A null check is advised
			if (textBox != null)
			{
				m_text_box = textBox;
				m_text_box.LostFocus += TextBoxOnLostFocus;
				m_text_box.PreviewMouseLeftButtonUp += TextBoxOnPreviewMouseLeftButtonUp;

				m_text_box.UndoLimit = 1;
				m_text_box.IsUndoEnabled = true;
			}
		}

		private void AttachIncreaseButton()
		{
			var increaseButton = GetTemplateChild("PART_btnUp") as RepeatButton;
			if (increaseButton != null)
			{
				m_increase_button = increaseButton;
				m_increase_button.Focusable = false;
				m_increase_button.Command = _minorIncreaseValueCommand;
				m_increase_button.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
				m_increase_button.PreviewMouseRightButtonDown += ButtonOnPreviewMouseRightButtonDown;
			}
		}

		private void AttachDecreaseButton()
		{
			var decreaseButton = GetTemplateChild("PART_btnDown") as RepeatButton;
			if (decreaseButton != null)
			{
				m_decrease_button = decreaseButton;
				m_decrease_button.Focusable = false;
				m_decrease_button.Command = _minorDecreaseValueCommand;
				m_decrease_button.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
				m_decrease_button.PreviewMouseRightButtonDown += ButtonOnPreviewMouseRightButtonDown;
			}
		}

		private void AttachCommands()
		{
			CommandBindings.Add(new CommandBinding(_minorIncreaseValueCommand, (a, b) => IncreaseValue(true)));
			CommandBindings.Add(new CommandBinding(_minorDecreaseValueCommand, (a, b) => DecreaseValue(true)));
			CommandBindings.Add(new CommandBinding(_majorIncreaseValueCommand, (a, b) => IncreaseValue(false)));
			CommandBindings.Add(new CommandBinding(_majorDecreaseValueCommand, (a, b) => DecreaseValue(false)));
			CommandBindings.Add(new CommandBinding(_updateValueStringCommand, (a, b) => UpdateValue()));
			CommandBindings.Add(new CommandBinding(_cancelChangesCommand, (a, b) => CancelChanges()));

			CommandManager.RegisterClassInputBinding(typeof(TextBox),
																							 new KeyBinding(_minorIncreaseValueCommand, new KeyGesture(Key.Up)));
			CommandManager.RegisterClassInputBinding(typeof(TextBox),
																							 new KeyBinding(_minorDecreaseValueCommand, new KeyGesture(Key.Down)));
			CommandManager.RegisterClassInputBinding(typeof(TextBox),
																							 new KeyBinding(_majorIncreaseValueCommand,
																															new KeyGesture(Key.PageUp)));
			CommandManager.RegisterClassInputBinding(typeof(TextBox),
																							 new KeyBinding(_majorDecreaseValueCommand,
																															new KeyGesture(Key.PageDown)));
			CommandManager.RegisterClassInputBinding(typeof(TextBox),
																							 new KeyBinding(_updateValueStringCommand, new KeyGesture(Key.Enter)));
			CommandManager.RegisterClassInputBinding(typeof(TextBox),
																							 new KeyBinding(_cancelChangesCommand, new KeyGesture(Key.Escape)));
		}

		#endregion

		#region Data retrieval and deposit

		private Decimal ParseStringToDecimal(String source)
		{
			Decimal value;
			Decimal.TryParse(source, out value);

			return value;
		}

		private int ParseStringToInt(String source)
		{
			int value;
			int.TryParse(source, out value);

			return value;
		}
		
		public Int32 GetDecimalPlacesCount(String valueString)
		{
			return valueString.SkipWhile(c => c.ToString(Culture)
																				!= Culture.NumberFormat.NumberDecimalSeparator).Skip(1).Count();
		}

		private Decimal TruncateValue(String valueString, Int32 decimalPlaces)
		{
			var endPoint = valueString.Length - (decimalPlaces - DecimalPlaces);
			endPoint++;

			var tempValueString = valueString.Substring(0, endPoint);

			return Decimal.Parse(tempValueString, Culture);
		}

		#endregion

		#region SubCoercion

		private void CoerceValueToBounds(ref Decimal in_value)
		{
			if (in_value < MinValue)
			{
				in_value = MinValue;
			}
			else if (in_value > MaxValue)
			{
				in_value = (decimal)MaxValue;
			}
		}

		private void CoerceValueToBoundsInt(ref int in_value)
		{
			if (in_value < MinValue)
			{
				in_value = (int)MinValue;
			}
			else if (in_value > MaxValue)
			{
				in_value = (int)MaxValue;
			}
		}
		#endregion

		#endregion

		#region · Methods ·

		private void UpdateValue()
		{
			Value = ParseStringToDecimal(m_text_box.Text);
			UpdateIntValue();
		}

		private void UpdateIntValue()
		{
			if (Value <= int.MaxValue && Value >= int.MinValue)
				IntValue = (int)Value;
		}

		private void CancelChanges()
		{
			m_text_box.Undo();
		}

		private void RemoveFocus()
		{
			// Passes focus here and then just deletes it
			Focusable = true;
			Focus();
			Focusable = false;
		}

		private void IncreaseValue(Boolean minor)
		{
			// Get the value that's currently in the _textBox.Text
			var value = ParseStringToDecimal(m_text_box.Text);

			// Coerce the value to min/max
			CoerceValueToBounds(ref value);

			// Only change the value if it has any meaning
			if (value >= MinValue)
			{
				if (minor)
				{
					if (IsValueWrapAllowed && value + MinorStep > MaxValue)
					{
						value = MinValue;
					}
					else
					{
						value += MinorStep;
					}
				}
				else
				{
					if (IsValueWrapAllowed && value + MajorStep > MaxValue)
					{
						value = MinValue;
					}
					else
					{
						value += MajorStep;
					}
				}
			}

			Value = value;
			UpdateIntValue();
		}

		private void DecreaseValue(Boolean minor)
		{
			// Get the value that's currently in the _textBox.Text
			decimal value = ParseStringToDecimal(m_text_box.Text);

			// Coerce the value to min/max
			CoerceValueToBounds(ref value);

			// Only change the value if it has any meaning
			if (value <= MaxValue)
			{
				if (minor)
				{
					if (IsValueWrapAllowed && value - MinorStep < MinValue)
					{
						value = MaxValue;
					}
					else
					{
						value -= MinorStep;
					}
				}
				else
				{
					if (IsValueWrapAllowed && value - MajorStep < MinValue)
					{
						value = MaxValue;
					}
					else
					{
						value -= MajorStep;
					}
				}
			}

			Value = value;
			UpdateIntValue();
		}

		#endregion
	}
}