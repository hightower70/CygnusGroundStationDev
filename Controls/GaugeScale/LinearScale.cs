///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2015 Laszlo Arvai. All rights reserved.
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
// Class for drawig linear scale of WPF virtual gauge
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CygnusControls
{
	public class LinearScale : Canvas
	{
		#region · Types ·

		public enum LinearScaleOrientation
		{
			Horizontal,
			Vertical
		}

		public enum LinearScalePosition
		{
			TopOrLeft,
			BottomOrRight
		}

		#endregion

		#region · Data members ·
		private Canvas m_canvas = null;
		private List<TextBlock> m_major_tick_labels = new List<TextBlock>();
		private List<MajorTickLabel> m_labels = new List<MajorTickLabel>();
		private int m_first_major_tick_index = 0;
		private Line m_scale_line;
		#endregion

		#region · Event handlers ·

		/// <summary>
		/// Property changed event
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="e"></param>
		private static void OnLinearScalePropertyChanged(DependencyObject in_object, DependencyPropertyChangedEventArgs in_event)
		{
			if(in_object is LinearScale)
			{
				LinearScale obj = (LinearScale)in_object;

				if (obj != null)
				{
					obj.InvalidateScaleGraphics();
				}
			}
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property.Name == "Width" || e.Property.Name == "Height")
			{
				InvalidateScaleGraphics();

				InvalidateVisual();
			}
		}


		protected override void OnRender(DrawingContext dc)
		{
			if (m_canvas == null)
				CreateScaleGraphics();

			base.OnRender(dc);
		}


		private void InvalidateScaleGraphics()
		{
			if (this.m_canvas != null)
			{
				this.m_canvas.Children.Clear();
				this.m_canvas = null;
			}

			m_major_tick_labels.Clear();
		}
		#endregion

		#region · Major tick properties ·


		/// <summary>
		/// Major tickline color
		/// </summary>
		public Brush MajorTickColor
		{
			get { return (Brush)GetValue(MajorTickColorProperty); }
			set { SetValue(MajorTickColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickColorProperty =
				DependencyProperty.Register("MajorTickColor", typeof(Brush), typeof(LinearScale), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));


		/// <summary>
		/// Distance in pixels of major ticks
		/// </summary>
		public int MajorTickCount
		{
			get { return (int)GetValue(MajorTickCountProperty); }
			set { SetValue(MajorTickCountProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickCount.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickCountProperty =
				DependencyProperty.Register("MajorTickCount", typeof(int), typeof(LinearScale), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));


		/// <summary>
		/// Major tick tickness
		/// </summary>
		public double MajorTickThickness
		{
			get { return (double)GetValue(MajorTickThicknessProperty); }
			set { SetValue(MajorTickThicknessProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickTickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickThicknessProperty =
				DependencyProperty.Register("MajorTickThickness", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));

		/// <summary>
		/// Length of the major tick line
		/// </summary>
		public int MajorTickLength
		{
			get { return (int)GetValue(MajorTickLengthProperty); }
			set { SetValue(MajorTickLengthProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickLength.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickLengthProperty =
				DependencyProperty.Register("MajorTickLength", typeof(int), typeof(LinearScale), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));

		/// <summary>
		/// Font color used for scale display
		/// </summary>
		public Brush FontColor
		{
			get { return (Brush)GetValue(FontColorProperty); }
			set { SetValue(FontColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FontColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FontColorProperty =
				DependencyProperty.Register("FontColor", typeof(Brush), typeof(LinearScale), new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));


		/// <summary>
		/// Height of the scale font
		/// </summary>
		public double FontHeight
		{
			get { return (double)GetValue(FontHeightProperty); }
			set { SetValue(FontHeightProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FontHeight.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FontHeightProperty =
				DependencyProperty.Register("FontHeight", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));

		/// <summary>
		/// Font of the scale
		/// </summary>
		public FontFamily Font
		{
			get { return (FontFamily)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Font.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FontProperty =
				DependencyProperty.Register("Font", typeof(FontFamily), typeof(LinearScale), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));

		public double MajorTickLabelGap
		{
			get { return (double)GetValue(MajorTickLabelGapProperty); }
			set { SetValue(MajorTickLabelGapProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickLabelGap.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickLabelGapProperty =
				DependencyProperty.Register("MajorTickLabelGap", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));

		/// <summary>
		/// Tick label overide
		/// </summary>
		public List<MajorTickLabel> Labels
		{
			get { return m_labels; }
		}


		#endregion

		#region · Minor tick properties ·

		/// <summary>
		/// Number of minor tick between two major ticks
		/// </summary>
		public int MinorTickCount
		{
			get { return (int)GetValue(MinorTickCountProperty); }
			set { SetValue(MinorTickCountProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MinorTickCount.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinorTickCountProperty =
				DependencyProperty.Register("MinorTickCount", typeof(int), typeof(LinearScale), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));


		/// <summary>
		/// Minor tick line color
		/// </summary>
		public Brush MinorTickColor
		{
			get { return (Brush)GetValue(MinorTickColorProperty); }
			set { SetValue(MinorTickColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MinorTickColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinorTickColorProperty =
				DependencyProperty.Register("MinorTickColor", typeof(Brush), typeof(LinearScale), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));


		/// <summary>
		/// Length of the minor tick line
		/// </summary>
		public int MinorTickLength
		{
			get { return (int)GetValue(MinorTickLengthProperty); }
			set { SetValue(MinorTickLengthProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MinorTickLength.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinorTickLengthProperty =
				DependencyProperty.Register("MinorTickLength", typeof(int), typeof(LinearScale), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));

		/// <summary>
		/// Thiuckness of the minor tick line
		/// </summary>
		public double MinorTickThickness
		{
			get { return (double)GetValue(MinorTickThicknessProperty); }
			set { SetValue(MinorTickThicknessProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MinorTickThickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinorTickThicknessProperty =
				DependencyProperty.Register("MinorTickThickness", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));

		#endregion
 
		#region · Scale properties ·



		public LinearScaleOrientation Orientation
		{
			get { return (LinearScaleOrientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty OrientationProperty =
				DependencyProperty.Register("Orientation", typeof(LinearScaleOrientation), typeof(LinearScale), new FrameworkPropertyMetadata(LinearScaleOrientation.Vertical, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));

		/// <summary>
		/// Scale minmum displayed value
		/// </summary>
		public double ScaleMin
		{
			get { return (double)GetValue(ScaleMinProperty); }
			set { SetValue(ScaleMinProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ScaleMin.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScaleMinProperty =
				DependencyProperty.Register("ScaleMin", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));

		/// <summary>
		/// Scale maximum value
		/// </summary>
		public double ScaleMax
		{
			get { return (double)GetValue(ScaleMaxProperty); }
			set { SetValue(ScaleMaxProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ScaleMax.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScaleMaxProperty =
				DependencyProperty.Register("ScaleMax", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));




		public double VisibleRange
		{
			get { return (double)GetValue(VisibleRangeProperty); }
			set { SetValue(VisibleRangeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for VisibleRange.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty VisibleRangeProperty =
				DependencyProperty.Register("VisibleRange", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));




		public bool LoopedScale
		{
			get { return (bool)GetValue(LoopedScaleProperty); }
			set { SetValue(LoopedScaleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CircularScale.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty LoopedScaleProperty =
				DependencyProperty.Register("LoopedScale", typeof(bool), typeof(LinearScale), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));




		public LinearScalePosition ScalePosition
		{
			get { return (LinearScalePosition)GetValue(ScalePositionProperty); }
			set { SetValue(ScalePositionProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ScalePosition.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScalePositionProperty =
				DependencyProperty.Register("ScalePosition", typeof(LinearScalePosition), typeof(LinearScale), new FrameworkPropertyMetadata(LinearScalePosition.TopOrLeft, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));




		public double ScaleLineThickness
		{
			get { return (double)GetValue(ScaleLineThicknessProperty); }
			set { SetValue(ScaleLineThicknessProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ScaleLineThickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScaleLineThicknessProperty =
				DependencyProperty.Register("ScaleLineThickness", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));




		public Brush ScaleLineColor
		{
			get { return (Brush)GetValue(ScaleLineColorProperty); }
			set { SetValue(ScaleLineColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ScaleLineColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScaleLineColorProperty =
				DependencyProperty.Register("ScaleLineColor", typeof(Brush), typeof(LinearScale), new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));




		public double ScaleOffset
		{
			get { return (double)GetValue(ScaleOffsetProperty); }
			set { SetValue(ScaleOffsetProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ScaleOffset.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScaleOffsetProperty =
				DependencyProperty.Register("ScaleOffset", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLinearScalePropertyChanged));
		

		#endregion

		#region · Value property ·

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ValueProperty =
				DependencyProperty.Register("Value", typeof(double), typeof(LinearScale), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.None, OnValueChangedProperty));

		private static void OnValueChangedProperty(DependencyObject in_object, DependencyPropertyChangedEventArgs in_event)
		{
			if (in_object is LinearScale)
			{
				LinearScale obj = (LinearScale)in_object;

				obj.UpdateValue((double)in_event.NewValue, (double)in_event.OldValue);
			}
		}		


		#endregion 

		private void UpdateValue(double in_new_value, double in_old_value)
		{
			int i;

			if (m_canvas != null)
			{
				double major_tick_division = (ScaleMax - ScaleMin) / MajorTickCount;
				int major_tick_index = (int)(Math.Floor((in_new_value - ScaleMin) / major_tick_division));
				double new_canvas_offset = (in_new_value - ScaleMin) - (major_tick_index * major_tick_division);
				double major_tick_division_in_pixels;

				if (Orientation == LinearScaleOrientation.Horizontal)
				{
					// horizontal scale
					double new_canvas_left = -new_canvas_offset / VisibleRange * Width;
					Canvas.SetLeft(m_canvas, new_canvas_left);
					UpdateMajorTickScaleLabels(in_new_value);

				}
				else
				{
					// vertical scale
					double new_canvas_top = new_canvas_offset / VisibleRange * Height;
					Canvas.SetTop(m_canvas, new_canvas_top);
					UpdateMajorTickScaleLabels(in_new_value);

					// update scale visibility for non looped scale
					major_tick_division_in_pixels = major_tick_division / VisibleRange * Height;
					if (!LoopedScale)
					{
						// lower end of the scale
						if (in_new_value < ScaleOffset)
						{
							double y = Height - ValueToCoordinateY(ScaleOffset - (major_tick_index * major_tick_division));

							m_scale_line.Y2 = y;

							for (i = 0; i < m_canvas.Children.Count; i++)
							{
								dynamic obj = m_canvas.Children[i];

								if (GetGraphicsElementY(obj) > (y + major_tick_division_in_pixels / 2))
									m_canvas.Children[i].Visibility = System.Windows.Visibility.Hidden;
								else
									m_canvas.Children[i].Visibility = System.Windows.Visibility.Visible;
							}
						}
						else
						{
							if (in_old_value < ScaleOffset)
							{
								m_scale_line.Y2 = Height + major_tick_division_in_pixels;

								// all items are visible
								for (i = 0; i < m_canvas.Children.Count; i++)
								{
									m_canvas.Children[i].Visibility = System.Windows.Visibility.Visible;
								}
							}
						}
					}

					// upper end of the scale
					if (in_new_value + (VisibleRange - ScaleOffset) > ScaleMax)
					{
						double y = ValueToCoordinateY(VisibleRange - (ScaleMax - (major_tick_index * major_tick_division)) - ScaleOffset);

						m_scale_line.Y1 = y;

						for (i = 0; i < m_canvas.Children.Count; i++)
						{
							dynamic obj = m_canvas.Children[i];

							if (GetGraphicsElementY(obj) < (y - major_tick_division_in_pixels / 2))
								m_canvas.Children[i].Visibility = System.Windows.Visibility.Hidden;
							else
								m_canvas.Children[i].Visibility = System.Windows.Visibility.Visible;
						}
					}
					else
					{
						if (in_old_value + (VisibleRange - ScaleOffset) > ScaleMax)
						{
							m_scale_line.Y1 = 0 - major_tick_division_in_pixels;

							// all items are visible
							for (i = 0; i < m_canvas.Children.Count; i++)
							{
								m_canvas.Children[i].Visibility = System.Windows.Visibility.Visible;
							}
						}
					}
				}
			}
		}

		private double GetGraphicsElementY(Line in_line)
		{
			return (in_line.Y1 + in_line.Y2) / 2;
		}

		private double GetGraphicsElementY(TextBlock in_text_block)
		{
			return Canvas.GetTop(in_text_block) + in_text_block.FontSize / 2;
		}

		private void UpdateMajorTickScaleLabels(double in_value)
		{
			double major_tick_division = (ScaleMax - ScaleMin) / MajorTickCount;
			int major_tick_index = (int)Math.Floor(in_value / major_tick_division);
			double tick_label_value = (major_tick_index - 1) * major_tick_division - ScaleOffset;

			if (major_tick_index != m_first_major_tick_index)
			{
				m_first_major_tick_index = major_tick_index;

				for (int index = 0; index < m_major_tick_labels.Count; index++)
				{
					UpdateMajorTickLabelText(m_major_tick_labels[index], tick_label_value); 
					tick_label_value += major_tick_division;
				}
			}
		}

		private void CreateScaleGraphics()
		{
			double x, y;
			double value;
			double major_tick_division_in_pixels;

			// create clipping rect
			this.ClipToBounds = true;

			m_first_major_tick_index = 0;
			m_canvas = new Canvas();

			this.Children.Add(m_canvas);

			// calculate division 
			double major_tick_division = (ScaleMax - ScaleMin) / MajorTickCount;
			if (Orientation == LinearScaleOrientation.Horizontal)
				major_tick_division_in_pixels = major_tick_division / VisibleRange * Width;
			else
				major_tick_division_in_pixels = major_tick_division / VisibleRange * Height;

			// draw scale line
			if (ScaleLineThickness > 0.0)
			{
				m_scale_line = new Line();
				m_scale_line.Stroke = ScaleLineColor;

				if (Orientation == LinearScaleOrientation.Horizontal)
				{
					m_scale_line.X1 = -major_tick_division_in_pixels;
					m_scale_line.X2 = Width + major_tick_division_in_pixels;

					if (ScalePosition == LinearScalePosition.TopOrLeft)
					{
						m_scale_line.Y1 = 0;
						m_scale_line.Y2 = 0;
					}
					else
					{
						m_scale_line.Y1 = Height - 1;
						m_scale_line.Y2 = Height - 1;
					}
				}
				else
				{
					// vertical scale
					m_scale_line.Y1 = 0 - major_tick_division_in_pixels;
					m_scale_line.Y2 = Height + major_tick_division_in_pixels;

					if (ScalePosition == LinearScalePosition.TopOrLeft)
					{
						m_scale_line.X1 = 0;
						m_scale_line.X2 = 0;
					}
					else
					{
						m_scale_line.X1 = Width - 1;
						m_scale_line.X2 = Width - 1;
					}
				}

				m_scale_line.StrokeThickness = ScaleLineThickness;
				m_canvas.Children.Add(m_scale_line);
			}


			// draw ticks
			value = 0 - major_tick_division;

			if (Orientation == LinearScaleOrientation.Horizontal)
			{
				if (ScalePosition == LinearScalePosition.TopOrLeft)
				{
					y = 0;
				}
				else
				{
					y = Height - 1;
				}

				// draw horizontal scale
				while (value <= VisibleRange + major_tick_division)
				{
					x = ValueToCoordinateX(value);
					if (ScalePosition == LinearScalePosition.TopOrLeft)
						CreateMajorTick(x, 0, x, MajorTickLength);
					else
						CreateMajorTick(x, Height - 1, x, Height - 1 - MajorTickLength);

					CreateMajorTickLabel(x, 0, value - ScaleOffset);

					// draw minor tick marks
					if (MinorTickThickness > 0 && MinorTickCount > 0)
					{
						double minor_x;
						for (int minor_tick_mark = 1; minor_tick_mark <= MinorTickCount; minor_tick_mark++)
						{
							// draw minor tick line
							minor_x = x + major_tick_division_in_pixels * minor_tick_mark / (MinorTickCount + 1);
							if (ScalePosition == LinearScalePosition.TopOrLeft)
								CreateMinorTick(minor_x, 0, minor_x, MinorTickLength);
							else
								CreateMinorTick(minor_x, Height - 1, minor_x, Height - 1 - MinorTickLength);
						}
					}

					value += major_tick_division;
				}
			}
			else
			{
				// draw vertical scale
				while (value <= VisibleRange + major_tick_division)
				{
					y = Height - ValueToCoordinateY(value);
					if (ScalePosition == LinearScalePosition.TopOrLeft)
						CreateMajorTick(0, y, MajorTickLength, y);
					else
						CreateMajorTick(Width - 1, y, Width - 1 - MajorTickLength, y);

					CreateMajorTickLabel(0, y, value - ScaleOffset);
					value += major_tick_division;
				}
			}

			UpdateValue(Value, Value);
		}

		private void UpdateMajorTickLabelText(TextBlock in_text_block, double in_value)
		{
			int i;
			double font_height;
			Brush font_color;
			string major_tickmark_label;

			font_color = FontColor;
			font_height = FontHeight;

			// handle looped scale
			if (LoopedScale)
			{
				if (in_value > ScaleMax)
					in_value -= (ScaleMax - ScaleMin);

				if (in_value < ScaleMin)
					in_value += (ScaleMax - ScaleMin);
			}

			major_tickmark_label = in_value.ToString();


			// find special label values
			i = 0;
			while (i < m_labels.Count)
			{
				if (m_labels[i].Value == major_tickmark_label)
				{
					// change label text
					major_tickmark_label = m_labels[i].Label;

					// font color
					if (m_labels[i].FontColor != null)
						font_color = m_labels[i].FontColor;

					// font size
					if (m_labels[i].FontHeight != 0)
						font_height = m_labels[i].FontHeight;

					break;
				}
				i++;
			}

			// update text properties
			in_text_block.Text = major_tickmark_label;
			in_text_block.FontSize = font_height;
			in_text_block.Foreground = font_color;
		}

		private void CreateMajorTickLabel(double in_x, double in_y, double in_value)
		{
			double x, y;
			double major_tick_distance = (ScaleMax - ScaleMin) / MajorTickCount / VisibleRange * Width;

			TextBlock label = new TextBlock();
			FontFamily font_family;

			// set font
			if (Font == null)
				font_family = new FontFamily();
			else
				font_family = Font;

			label.FontFamily = font_family;

			// set position
			if (Orientation == LinearScaleOrientation.Horizontal)
			{
				// horizontal scale
				label.Width = major_tick_distance;
				label.TextAlignment = TextAlignment.Center;
				x = in_x - major_tick_distance / 2;

				if (ScalePosition == LinearScalePosition.TopOrLeft)
					y = in_y + MajorTickLength + MajorTickLabelGap;
				else
					y = in_y + Height - MajorTickLength - FontHeight - MajorTickLabelGap;
			}
			else
			{
				// vertical scale
				label.Width = Width - 1; 
				if (ScalePosition == LinearScalePosition.TopOrLeft)
				{
					label.TextAlignment = TextAlignment.Left;
					x = in_x + MajorTickLength + MajorTickLabelGap;
					y = in_y - FontHeight / 2;
				}
				else
				{
					label.TextAlignment = TextAlignment.Right;
					x = in_x - MajorTickLength - MajorTickLabelGap;
					y = in_y - FontHeight / 2;
				}
			}

			Canvas.SetLeft(label, x);
			Canvas.SetTop(label, y);

			// set text and color
			UpdateMajorTickLabelText(label, in_value);

			// add to parents
			m_canvas.Children.Add(label);
			m_major_tick_labels.Add(label);
		}

		private double ValueToCoordinateX(double in_value)
		{
			double coordinate;

			coordinate = in_value / VisibleRange * Width;

			return coordinate;
		}

		private double ValueToCoordinateY(double in_value)
		{
			double coordinate;

			coordinate = in_value / VisibleRange * Height;

			return coordinate;
		}

		private void CreateMajorTick(double in_x1, double in_y1, double in_x2, double in_y2)
		{
			Line line = new Line();
			line.Stroke = MajorTickColor;

			line.X1 = in_x1;
			line.X2 = in_x2;
			line.Y1 = in_y1;
			line.Y2 = in_y2;

			line.StrokeThickness = MajorTickThickness;
			m_canvas.Children.Add(line);
		}

		private void CreateMinorTick(double in_x1, double in_y1, double in_x2, double in_y2)
		{
			Line line = new Line();
			line.Stroke = MinorTickColor;

			line.X1 = in_x1;
			line.X2 = in_x2;
			line.Y1 = in_y1;
			line.Y2 = in_y2;

			line.StrokeThickness = MinorTickThickness;
			m_canvas.Children.Add(line);
		}

#if false
		#region · Constants ·
		const double LabelAngleThreshold = 15;
		#endregion



		#region · Data members ·

		private List<MajorTickLabel> m_labels = new List<MajorTickLabel>();

		#endregion

		#region · Major tick properties ·

		/// <summary>
		/// Major tickline color
		/// </summary>
		public Brush MajorTickColor
		{
			get { return (Brush)GetValue(MajorTickColorProperty); }
			set { SetValue(MajorTickColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickColorProperty =
				DependencyProperty.Register("MajorTickColor", typeof(Brush), typeof(CircularScale), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Number of major ticks
		/// </summary>
		public int MajorTickCount
		{
			get { return (int)GetValue(MajorTickCountProperty); }
			set { SetValue(MajorTickCountProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickCount.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickCountProperty =
				DependencyProperty.Register("MajorTickCount", typeof(int), typeof(CircularScale), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Major tick tickness
		/// </summary>
		public double MajorTickThickness
		{
			get { return (double)GetValue(MajorTickThicknessProperty); }
			set { SetValue(MajorTickThicknessProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickTickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickThicknessProperty =
				DependencyProperty.Register("MajorTickThickness", typeof(double), typeof(CircularScale), new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Length of the major tick line
		/// </summary>
		public int MajorTickLength
		{
			get { return (int)GetValue(MajorTickLengthProperty); }
			set { SetValue(MajorTickLengthProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickLength.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickLengthProperty =
				DependencyProperty.Register("MajorTickLength", typeof(int), typeof(CircularScale), new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender));


		/// <summary>
		/// Gap for major tick line (distance between line end and border of the circular scale)
		/// </summary>
		public int MajorTickGap
		{
			get { return (int)GetValue(MajorTickGapProperty); }
			set { SetValue(MajorTickGapProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickGap.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickGapProperty =
				DependencyProperty.Register("MajorTickGap", typeof(int), typeof(CircularScale), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));



		#endregion

		#region · Major tick label properties ·

		/// <summary>
		/// Font color used for scale display
		/// </summary>
		public Brush FontColor
		{
			get { return (Brush)GetValue(FontColorProperty); }
			set { SetValue(FontColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FontColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FontColorProperty =
				DependencyProperty.Register("FontColor", typeof(Brush), typeof(CircularScale), new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));


		/// <summary>
		/// Height of the scale font
		/// </summary>
		public double FontHeight
		{
			get { return (double)GetValue(FontHeightProperty); }
			set { SetValue(FontHeightProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FontHeight.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FontHeightProperty =
				DependencyProperty.Register("FontHeight", typeof(double), typeof(CircularScale), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Font of the scale
		/// </summary>
		public FontFamily Font
		{
			get { return (FontFamily)GetValue(FontProperty); }
			set { SetValue(FontProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Font.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FontProperty =
				DependencyProperty.Register("Font", typeof(FontFamily), typeof(CircularScale), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Tick label overide
		/// </summary>
		public List<MajorTickLabel> Labels
		{
			get { return m_labels; }
		}

		/// <summary>
		/// Major tick label angle mode
		/// </summary>
		public LabelAngleType MajorTickLabelAngleMode
		{
			get { return (LabelAngleType)GetValue(MajorTickLabelAngleModeProperty); }
			set { SetValue(MajorTickLabelAngleModeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickMarkLabelAngleMode.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickLabelAngleModeProperty =
				DependencyProperty.Register("MajorTickLabelAngleMode", typeof(LabelAngleType), typeof(CircularScale), new FrameworkPropertyMetadata(LabelAngleType.Absolute, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Angle of the major tickmark
		/// </summary>
		public double MajorTickLabelAngle
		{
			get { return (double)GetValue(MajorTickLabelAngleProperty); }
			set { SetValue(MajorTickLabelAngleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MajorTickmarkAngle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MajorTickLabelAngleProperty =
				DependencyProperty.Register("MajorTickLabelAngle", typeof(double), typeof(CircularScale), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));




		#endregion


		#region · Scale properties ·

		/// <summary>
		/// Scale minmum displayed value
		/// </summary>
		public double ScaleMin
		{
			get { return (double)GetValue(ScaleMinProperty); }
			set { SetValue(ScaleMinProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ScaleMin.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScaleMinProperty =
				DependencyProperty.Register("ScaleMin", typeof(double), typeof(CircularScale), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Scale maximum value
		/// </summary>
		public double ScaleMax
		{
			get { return (double)GetValue(ScaleMaxProperty); }
			set { SetValue(ScaleMaxProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ScaleMax.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ScaleMaxProperty =
				DependencyProperty.Register("ScaleMax", typeof(double), typeof(CircularScale), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

		#endregion

		#region · Rendering function ·

		/// <summary>
		/// Render circular scale
		/// </summary>
		/// <param name="dc"></param>
		protected override void OnRender(DrawingContext dc)
		{
			double width = RenderSize.Width;
			double height = RenderSize.Height;
			Point p1, p2;
			Point label_position, label_center;
			int major_tick_mark;
			int minor_tick_mark;
			double major_angle, minor_angle;
			double major_angle_range = (EndAngle - StartAngle) / (MajorTickCount);
			int major_tick_marklabel_value;
			string major_tickmark_label;
			Typeface scale_font;
			int i;
			double label_angle = 0;
			Brush font_color;
			double font_height;

			if (MajorTickLabelAngleMode == LabelAngleType.Absolute)
				label_angle = MajorTickLabelAngle;

			if (Font == null)
				scale_font = new Typeface(SystemFonts.MessageFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
			else
				scale_font = new Typeface(Font, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

			Pen major_tick_pen = new Pen(MajorTickColor, MajorTickThickness);
			Pen minor_tick_pen;

			if (MinorTickColor == null)
				minor_tick_pen = new Pen(MajorTickColor, MinorTickThickness);
			else
				minor_tick_pen = new Pen(MinorTickColor, MinorTickThickness);

			p1 = new Point();
			p2 = new Point();
			label_position = new Point();
			label_center = new Point();

			for (major_tick_mark = 0; major_tick_mark <= MajorTickCount; major_tick_mark++)
			{
				// major tickmark angle
				major_angle = major_tick_mark * major_angle_range + StartAngle;

				// draw minor tick lines
				if (major_tick_mark < MajorTickCount)
				{
					for (minor_tick_mark = 1; minor_tick_mark < MinorTickCount; minor_tick_mark++)
					{
						minor_angle = major_angle + minor_tick_mark * major_angle_range / (MinorTickCount);

						// draw minor tick line
						CalculateCircularPoint(minor_angle, width / 2 - MinorTickGap, height / 2 - MinorTickGap, ref p1);
						CalculateCircularPoint(minor_angle, width / 2 - MinorTickLength - MinorTickGap, height / 2 - MinorTickLength - MinorTickGap, ref p2);

						dc.DrawLine(minor_tick_pen, p1, p2);
					}
				}

				// label value
				major_tick_marklabel_value = (int)(major_tick_mark * (ScaleMax - ScaleMin) / (MajorTickCount) + ScaleMin);
				major_tickmark_label = major_tick_marklabel_value.ToString();

				// find label override
				font_color = FontColor;
				font_height = FontHeight;
				i = 0;
				while (i < m_labels.Count)
				{
					if (m_labels[i].Value == major_tickmark_label)
					{
						// change label text
						major_tickmark_label = m_labels[i].Label;

						// font color
						if (m_labels[i].FontColor != null)
							font_color = m_labels[i].FontColor;

						// font size
						if (m_labels[i].FontHeight != 0)
							font_height = m_labels[i].FontHeight;

						break;
					}
					i++;
				}

				// draw major tick line
				CalculateCircularPoint(major_angle, width / 2 - MajorTickGap, height / 2 - MajorTickGap, ref p1);
				CalculateCircularPoint(major_angle, width / 2 - MajorTickLength - MajorTickGap, height / 2 - MajorTickLength - MajorTickGap, ref p2);

				dc.DrawLine(major_tick_pen, p1, p2);

				FormattedText ft = new FormattedText(major_tickmark_label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, scale_font, font_height, font_color);
				CalculateMajorTickmarkLabelPosition(major_angle, width / 2 - MajorTickLength - MajorTickGap - 2, height / 2 - MajorTickLength - MajorTickGap - 2, label_angle, ft, ref label_position, ref label_center);

				if (MajorTickLabelAngleMode == LabelAngleType.Relative)
					label_angle = major_angle + MajorTickLabelAngle;

				if (label_angle != 0.0)
					dc.PushTransform(new RotateTransform(label_angle, label_center.X, label_center.Y));

				dc.DrawText(ft, label_position);

				if (label_angle != 0.0)
					dc.Pop();
			}
		}

		#endregion

		#region · Helper functions ·

		private void CalculateMajorTickmarkLabelPosition(double in_angle, double in_radius_x, double in_radius_y, double in_label_angle, FormattedText in_text, ref Point out_position, ref Point out_center)
		{
			double angle_in_deg;
			double angle_in_rad;
			double width = RenderSize.Width;
			double height = RenderSize.Height;
			double x, y;
			double text_width = in_text.Width;
			double text_height = in_text.Height;

			// angle calculation
			angle_in_deg = NormalizeAngle(in_angle);
			angle_in_rad = angle_in_deg / 180 * Math.PI;

			if (MajorTickLabelAngleMode == LabelAngleType.Relative)
			{
				double label_angle_in_rad = NormalizeAngle(in_label_angle) / 180 * Math.PI;

				// circumference X,Y coordinate calculation
				x = width / 2 + Math.Cos(angle_in_rad) * (in_radius_x - text_height / 2);
				y = height / 2 + Math.Sin(angle_in_rad) * (in_radius_y - text_height / 2);

				out_position.X = x - text_width / 2;
				out_position.Y = y - text_height / 2;

				out_center.X = x;
				out_center.Y = y;
			}
			else
			{
				// circumference X,Y coordinate calculation
				x = width / 2 + Math.Cos(angle_in_rad) * in_radius_x;
				y = height / 2 + Math.Sin(angle_in_rad) * in_radius_y;

				// determine position and text center
				if (Math.Abs(angle_in_deg - 0) < LabelAngleThreshold)
				{
					out_center.X = x - text_width / 2;
					out_center.Y = y;

					x -= text_width;
					y -= text_height / 2;
				}
				else
				{
					if (Math.Abs(angle_in_deg - 90) < LabelAngleThreshold)
					{
						out_center.X = x;
						out_center.Y = y - text_height / 2;

						x -= text_width / 2;
						y -= text_height;
					}
					else
					{
						if (Math.Abs(angle_in_deg - 180) < LabelAngleThreshold)
						{
							out_center.X = x + text_width / 2;
							out_center.Y = y;

							y -= text_height / 2;
						}
						else
						{
							if (Math.Abs(angle_in_deg - 270) < LabelAngleThreshold)
							{
								out_center.X = x;
								out_center.Y = y + text_height / 2;

								x -= text_width / 2;
							}
							else
							{
								if (angle_in_deg < 90)
								{
									out_center.X = x - text_width / 2;
									out_center.Y = y - text_height / 2;

									x -= text_width;
									y -= text_height;
								}
								else
								{
									if (angle_in_deg < 180)
									{
										out_center.X = x + text_width / 2;
										out_center.Y = y - text_height / 2;

										y -= text_height;
									}
									else
									{
										if (angle_in_deg < 270)
										{
											out_center.X = x + text_width / 2;
											out_center.Y = y + text_height / 2;
										}
										else
										{
											out_center.X = x - text_width / 2;
											out_center.Y = y + text_height / 2;

											x -= text_width;
										}
									}
								}
							}
						}
					}
				}

				out_position.X = x;
				out_position.Y = y;
			}
		}

		/// <summary>
		/// Calculate X,Y coordinates of the point on a circumference
		/// </summary>
		/// <param name="in_angle">Angle where the point is located</param>
		/// <param name="in_radius_x">Radius in horizintal direction</param>
		/// <param name="in_radius_y">Radius in vertical direection</param>
		/// <param name="out_point">Calcualted coordinates</param>
		private void CalculateCircularPoint(double in_angle, double in_radius_x, double in_radius_y, ref Point out_point)
		{
			double angle = in_angle / 180 * Math.PI;
			double width = RenderSize.Width;
			double height = RenderSize.Height;

			out_point.X = width / 2 + Math.Cos(angle) * in_radius_x;
			out_point.Y = height / 2 + Math.Sin(angle) * in_radius_y;
		}

		/// <summary>
		/// Normalizes angle (angle will be in 0..360degree range)
		/// </summary>
		/// <param name="in_angle">Input angle in degree</param>
		/// <returns>Normalized angle</returns>
		private double NormalizeAngle(double in_angle)
		{
			double angle = (in_angle % 360);

			if (angle < 0)
				return angle + 360;
			else
				return angle;
		}

		#endregion
	}
#endif
	}
}