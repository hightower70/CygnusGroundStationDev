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
// Class for drawig circular scale of WPF virtual gauge
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace CygnusControls
{
	public class CircularScale : FrameworkElement
	{
		#region · Constants ·
		const double LabelAngleThreshold = 15;
		#endregion

		#region · Types ·
		public enum LabelAngleType
		{
			Relative,
			Absolute
		}

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
				DependencyProperty.Register("MinorTickCount", typeof(int), typeof(CircularScale), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		
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
				DependencyProperty.Register("MinorTickColor", typeof(Brush), typeof(CircularScale), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));


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
				DependencyProperty.Register("MinorTickLength", typeof(int), typeof(CircularScale), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Gap of the minor tick line.
		/// </summary>
		public int MinorTickGap
		{
				get { return (int)GetValue(MinorTickGapProperty); }
				set { SetValue(MinorTickGapProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MinorTickGap.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MinorTickGapProperty = 
				DependencyProperty.Register("MinorTickGap", typeof(int), typeof(CircularScale), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

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
				DependencyProperty.Register("MinorTickThickness", typeof(double), typeof(CircularScale), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

		

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

		/// <summary>
		/// Start angle of the scale (first major tick mark)
		/// </summary>
		public double StartAngle
		{
			get { return (double)GetValue(StartAngleProperty); }
			set { SetValue(StartAngleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StartAngle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StartAngleProperty =
				DependencyProperty.Register("StartAngle", typeof(double), typeof(CircularScale), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// End angle of the scale (last major tick mark)
		/// </summary>
		public double EndAngle
		{
			get { return (double)GetValue(EndAngleProperty); }
			set { SetValue(EndAngleProperty, value); }
		}

		// Using a DependencyProperty as the backing store for EndAngle.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty EndAngleProperty =
				DependencyProperty.Register("EndAngle", typeof(double), typeof(CircularScale), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
		
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

			if(MajorTickLabelAngleMode == LabelAngleType.Absolute)
				label_angle = MajorTickLabelAngle;
			
			if(Font == null)
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
						if (m_labels[i].FontHeight!= 0)
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

				if(MajorTickLabelAngleMode == LabelAngleType.Relative)
					label_angle = major_angle +  MajorTickLabelAngle;

				if(label_angle != 0.0)
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

}
