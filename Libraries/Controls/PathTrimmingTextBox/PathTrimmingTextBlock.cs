using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;																																																																						 
using System.Windows.Controls;
using System.Windows.Media;

namespace CygnusControls
{
	public class PathTrimmingTextBlock : FrameworkElement
	{
 		#region · Properties ·

		public string FilePath
		{
			get { return (string)GetValue(FilePathProperty); }
			set { SetValue(FilePathProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FilePath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FilePathProperty =
				DependencyProperty.Register("FilePath", typeof(string), typeof(PathTrimmingTextBlock), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

						 
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FontSizeProperty =
				DependencyProperty.Register("FontSize", typeof(double), typeof(PathTrimmingTextBlock), new PropertyMetadata(SystemFonts.MessageFontSize));

						 


		public FontFamily FontFamily
		{
			get { return (FontFamily)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		// Using a DependencyProperty as the backing store for FontFamily.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty FontFamilyProperty =
				DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(PathTrimmingTextBlock), new PropertyMetadata(SystemFonts.MessageFontFamily));




		public Brush Foreground
		{
			get { return (Brush)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Foreground.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ForegroundProperty =
				DependencyProperty.Register("Foreground", typeof(Brush), typeof(PathTrimmingTextBlock), new PropertyMetadata(Brushes.Black));



		public Brush Background
		{
			get { return (Brush)GetValue(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Background.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BackgroundProperty =
				DependencyProperty.Register("Background", typeof(Brush), typeof(PathTrimmingTextBlock), new PropertyMetadata(Brushes.Transparent));




		public Brush Stroke
		{
			get { return (Brush)GetValue(StrokeProperty); }
			set { SetValue(StrokeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Stroke.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeProperty =
				DependencyProperty.Register("Stroke", typeof(Brush), typeof(PathTrimmingTextBlock), new PropertyMetadata(Brushes.Transparent));




		public double StrokeThickness
		{
			get { return (double)GetValue(StrokeThicknessProperty); }
			set { SetValue(StrokeThicknessProperty, value); }
		}

		// Using a DependencyProperty as the backing store for StrokeThickness.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty StrokeThicknessProperty =
				DependencyProperty.Register("StrokeThickness", typeof(double), typeof(PathTrimmingTextBlock), new PropertyMetadata(0.0));

		

		#endregion

		// Override the default Measure method of Panel 
		protected override Size MeasureOverride(Size availableSize)
		{
			Size panelDesiredSize = new Size();

			if (!string.IsNullOrEmpty(FilePath))
			{
				Typeface tf = GetTypeface();

				FormattedText formatted = new FormattedText(FilePath,
							CultureInfo.CurrentCulture,
							FlowDirection.LeftToRight,
							tf,
							FontSize,
							Foreground);

				panelDesiredSize.Width = MinWidth;

				if (panelDesiredSize.Height < formatted.Height)
					panelDesiredSize.Height = formatted.Height;
			}
			else
			{
				panelDesiredSize = availableSize;
			}

			return panelDesiredSize;
		}

		private Typeface GetTypeface()
		{
			return new Typeface(SystemFonts.MessageFontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
		}

		#region · Rendering ·
		protected override void OnRender(DrawingContext dc)
		{
			// setup clipping
			RectangleGeometry clip_area = new RectangleGeometry(new Rect(0, 0, RenderSize.Width, RenderSize.Height));

			dc.PushClip(clip_area);


			Pen pen = new Pen(Stroke, StrokeThickness);

			dc.DrawRectangle(Background, pen, new Rect(0, 0, RenderSize.Width, RenderSize.Height));

			Typeface tf = GetTypeface();

			string path = GetTrimmedPath(RenderSize.Width, tf);


			// draw tick value
			FormattedText ft ;
			if (!string.IsNullOrEmpty(FilePath))
			{
				ft = new FormattedText(path, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, tf, FontSize, Foreground);

				// Draw the text at a location
				double x = 0, y = 0;

				switch (VerticalAlignment)
				{
					case System.Windows.VerticalAlignment.Top:
						y = 0;
						break;

					case System.Windows.VerticalAlignment.Center:
						y = (RenderSize.Height - ft.Height) / 2;
						break;

					case System.Windows.VerticalAlignment.Bottom:
						y = RenderSize.Height - ft.Height;
						break;
				}

				switch (HorizontalAlignment)
				{
					case System.Windows.HorizontalAlignment.Left:
						x = 0;
						break;

					case System.Windows.HorizontalAlignment.Center:
						x = (RenderSize.Width - ft.Width) / 2;
						break;

					case System.Windows.HorizontalAlignment.Right:
						x = RenderSize.Width - ft.Width;
						break;
				}

				dc.DrawText(ft, new Point(x, y));
			}

			dc.Pop();
		}
		#endregion

		string GetTrimmedPath(double width, Typeface in_typeface)
		{
			if (string.IsNullOrEmpty(FilePath))
				return string.Empty;

			string filename = System.IO.Path.GetFileName(FilePath);
			string directory = System.IO.Path.GetDirectoryName(FilePath);

			if (string.IsNullOrEmpty(filename))
			{
				int pos = directory.LastIndexOfAny("\\/".ToCharArray());

				if (pos > 0)
				{
					filename = directory.Substring(pos + 1);
					directory = directory.Remove(pos);
				}
			}

			FormattedText formatted;
			bool width_ok = false;
			bool width_changed = false;
			string current_path;

			do
			{
				current_path = string.Format("{0}...\\{1}", directory, filename);

				formatted = new FormattedText(
						current_path,
						CultureInfo.CurrentCulture,
						FlowDirection.LeftToRight,
						in_typeface,
						FontSize,
						Foreground
				);

				width_ok = formatted.Width < width;

				if (!width_ok)
				{
					width_changed = true;
					directory = directory.Substring(0, directory.Length - 1);

					if (directory.Length == 0) return "...\\" + filename;
				}

			} while (!width_ok);

			if (!width_changed)
			{
				return System.IO.Path.Combine(directory, filename);
			}

			return current_path;
		}
 	}
}

