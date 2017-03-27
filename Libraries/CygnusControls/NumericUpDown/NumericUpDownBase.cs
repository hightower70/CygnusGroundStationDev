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
// Implementation of Numeric Up/Down Control (Abstract base class)
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CygnusControls
{
	[TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
	[TemplatePart(Name = "PART_IncreaseButton", Type = typeof(RepeatButton))]
	[TemplatePart(Name = "PART_DecreaseButton", Type = typeof(RepeatButton))]
	public abstract class NumericUpDownBase : Control
	{
		#region · Constructors ·

		static NumericUpDownBase()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDownBase),
																							 new FrameworkPropertyMetadata(
																									 typeof(NumericUpDownBase)));
		}

		public NumericUpDownBase()
		{
			m_culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();

			Loaded += OnLoaded;
		}

		#endregion

		#region · Abstract members ·
		protected abstract void InvalidateValueProperty();
		protected abstract void UpdateValue();
		protected abstract void IncreaseValue(Boolean minor);
		protected abstract void DecreaseValue(Boolean minor);
		#endregion

		#region · Properties ·

		#region · IsThousandSeparatorVisible ·

		public static readonly DependencyProperty IsThousandSeparatorVisibleProperty =
				DependencyProperty.Register("IsThousandSeparatorVisible", typeof(Boolean), typeof(NumericUpDownBase),
																		new PropertyMetadata(false, OnIsThousandSeparatorVisibleChanged));

		public Boolean IsThousandSeparatorVisible
		{
			get { return (Boolean)GetValue(IsThousandSeparatorVisibleProperty); }
			set { SetValue(IsThousandSeparatorVisibleProperty, value); }
		}

		private static void OnIsThousandSeparatorVisibleChanged(DependencyObject element,
																														DependencyPropertyChangedEventArgs e)
		{

			var control = (NumericUpDownBase)element;

			control.InvalidateValueProperty();
		}

		#endregion

		#region · IsAutoSelectionActive ·

		public static readonly DependencyProperty IsAutoSelectionActiveProperty =
				DependencyProperty.Register("IsAutoSelectionActive", typeof(Boolean), typeof(NumericUpDownBase),
																		new PropertyMetadata(false));

		public Boolean IsAutoSelectionActive
		{
			get { return (Boolean)GetValue(IsAutoSelectionActiveProperty); }
			set { SetValue(IsAutoSelectionActiveProperty, value); }
		}

		#endregion

		#region · IsValueWrapAllowed ·

		public static readonly DependencyProperty IsValueWrapAllowedProperty =
				DependencyProperty.Register("IsValueWrapAllowed", typeof(Boolean), typeof(NumericUpDownBase),
																		new PropertyMetadata(false));

		public Boolean IsValueWrapAllowed
		{
			get { return (Boolean)GetValue(IsValueWrapAllowedProperty); }
			set { SetValue(IsValueWrapAllowedProperty, value); }
		}

		#endregion

		#endregion

		#region · Fields ·

		protected readonly CultureInfo m_culture;
		protected RepeatButton m_decrease_button;
		protected RepeatButton m_increase_button;
		protected TextBox m_text_box;

		#endregion

		#region · Commands ·

		private readonly RoutedUICommand _minorDecreaseValueCommand =
				new RoutedUICommand("MinorDecreaseValue", "MinorDecreaseValue", typeof(NumericUpDownBase));

		private readonly RoutedUICommand _minorIncreaseValueCommand =
				new RoutedUICommand("MinorIncreaseValue", "MinorIncreaseValue", typeof(NumericUpDownBase));

		private readonly RoutedUICommand _majorDecreaseValueCommand =
				new RoutedUICommand("MajorDecreaseValue", "MajorDecreaseValue", typeof(NumericUpDownBase));

		private readonly RoutedUICommand _majorIncreaseValueCommand =
				new RoutedUICommand("MajorIncreaseValue", "MajorIncreaseValue", typeof(NumericUpDownBase));

		private readonly RoutedUICommand _updateValueStringCommand =
				new RoutedUICommand("UpdateValueString", "UpdateValueString", typeof(NumericUpDownBase));

		private readonly RoutedUICommand _cancelChangesCommand =
				new RoutedUICommand("CancelChanges", "CancelChanges", typeof(NumericUpDownBase));

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
			InvalidateValueProperty();
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
			InvalidateValueProperty();
		}

		#endregion

		#region · Attachment ·

		private void AttachToVisualTree()
		{
			AttachTextBox();
			AttachIncreaseButton();
			AttachDecreaseButton();
		}

		private void AttachTextBox()
		{
			var textBox = GetTemplateChild("PART_TextBox") as TextBox;

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
			var increaseButton = GetTemplateChild("PART_IncreaseButton") as RepeatButton;
			if (increaseButton != null)
			{
				m_increase_button = increaseButton;
				m_increase_button.Focusable = false;
				m_increase_button.Command = _minorIncreaseValueCommand;
				m_increase_button.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
			}
		}

		private void AttachDecreaseButton()
		{
			var decreaseButton = GetTemplateChild("PART_DecreaseButton") as RepeatButton;
			if (decreaseButton != null)
			{
				m_decrease_button = decreaseButton;
				m_decrease_button.Focusable = false;
				m_decrease_button.Command = _minorDecreaseValueCommand;
				m_decrease_button.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
			}
		}

		private void RemoveFocus()
		{
			// Passes focus here and then just deletes it
			Focusable = true;
			Focus();
			Focusable = false;
		}

	private void AttachCommands()
		{
			CommandBindings.Add(new CommandBinding(_minorIncreaseValueCommand, (a, b) => IncreaseValue(true)));
			CommandBindings.Add(new CommandBinding(_minorDecreaseValueCommand, (a, b) => DecreaseValue(true)));
			CommandBindings.Add(new CommandBinding(_majorIncreaseValueCommand, (a, b) => IncreaseValue(false)));
			CommandBindings.Add(new CommandBinding(_majorDecreaseValueCommand, (a, b) => DecreaseValue(false)));
			CommandBindings.Add(new CommandBinding(_updateValueStringCommand, (a, b) => UpdateValue()));
			CommandBindings.Add(new CommandBinding(_cancelChangesCommand, (a, b) => CancelChanges()));
			/*
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
																							 */
			m_text_box.InputBindings.Add(new KeyBinding(_minorIncreaseValueCommand, new KeyGesture(Key.Up)));
			m_text_box.InputBindings.Add(new KeyBinding(_minorIncreaseValueCommand, new KeyGesture(Key.Up)));
			m_text_box.InputBindings.Add(new KeyBinding(_minorDecreaseValueCommand, new KeyGesture(Key.Down)));
			m_text_box.InputBindings.Add(new KeyBinding(_majorIncreaseValueCommand, new KeyGesture(Key.PageUp)));
			m_text_box.InputBindings.Add(new KeyBinding(_majorDecreaseValueCommand, new KeyGesture(Key.PageDown)));
			m_text_box.InputBindings.Add(new KeyBinding(_updateValueStringCommand, new KeyGesture(Key.Enter)));
			m_text_box.InputBindings.Add(new KeyBinding(_cancelChangesCommand, new KeyGesture(Key.Escape)));
		}

		#endregion

		#region · Methods ·

		private void CancelChanges()
		{
			m_text_box.Undo();
		}

#endregion
	}
}