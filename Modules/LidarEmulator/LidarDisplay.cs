using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LidarEmulator
{
	public class LidarDisplay : INotifyPropertyChanged
	{
		#region · Data members ·
		private double m_heading;
		private double m_pixel_position_x;
		private double m_pixel_position_y;
		private double m_position_x;
		private double m_position_y;
		private double m_pixels_per_meter;
		private int m_circular_resolution;
		private Canvas m_canvas;
		private Line[] m_lines;
		#endregion

		#region · Properties ·

		public double Heading
		{
			get
			{
				return m_heading;
			}
			set
			{
				m_heading = value;
				NotifyPropertyChanged();
			}
		}

		public double PixelPositionX
		{
			get
			{
				return m_pixel_position_x;
			}
			set
			{
				m_pixel_position_x = value;
				NotifyPropertyChanged();
			}
		}

		public double PixelPositionY
		{
			get
			{
				return m_pixel_position_y;
			}
			set
			{
				m_pixel_position_y = value;
				NotifyPropertyChanged();
			}
		}

		public double PositionX
		{
			get
			{
				return m_position_x;
			}
			set
			{
				m_position_x = value;
				PixelPositionX = m_position_x * m_pixels_per_meter;
				NotifyPropertyChanged();
			}
		}

		public double PositionY
		{
			get
			{
				return m_position_y;
			}
			set
			{
				m_position_y = value;
				PixelPositionY = m_position_y * m_pixels_per_meter;
				NotifyPropertyChanged();
			}
		}
		#endregion

		public LidarDisplay()
		{
			m_heading = 0;
			m_pixels_per_meter = 40;
			m_pixel_position_x = 0;
			m_pixel_position_y = 0;
			m_position_x = 0;
			m_position_y = 0;
			m_circular_resolution = 360;
		}

		public void Initialize(Canvas in_canvas)
		{
			int i;
			SolidColorBrush redBrush = new SolidColorBrush();
			redBrush.Color = Colors.Red;

			m_canvas = in_canvas;

			m_lines = new Line[m_circular_resolution];
			for(i=0; i<m_circular_resolution;i++)
			{
				m_lines[i] = new Line();
				m_lines[i].X1 = 0;
				m_lines[i].Y1 = 0;
				m_lines[i].X2 = 0;
				m_lines[i].Y2 = 0;

				// Set Line's width and color
				m_lines[i].StrokeThickness = 0.5;
				m_lines[i].Stroke = redBrush;

				m_canvas.Children.Add(m_lines[i]);
			}
		}

		public void DisplayScan(double in_position_x, double in_position_y, double in_heading, UInt16[] in_scan)
		{
			int scan_index;
			int center_x;
			int center_y;
			double angle;

			center_x = (int)Math.Round(in_position_x * m_pixels_per_meter, 0);
			center_y = (int)Math.Round(in_position_y * m_pixels_per_meter, 0);
			//center_x = (int)(in_position_x * m_pixels_per_meter);
			//center_y = (int)(in_position_y * m_pixels_per_meter);

			for (scan_index = 0; scan_index < m_circular_resolution; scan_index++)
			{
				angle = in_heading / 180 * Math.PI + Math.PI * 2 * scan_index / m_circular_resolution;

				m_lines[scan_index].X1 = center_x;
				m_lines[scan_index].Y1 = center_y;

				m_lines[scan_index].X2 = center_x + Math.Cos(angle) * in_scan[scan_index] / 1000 * m_pixels_per_meter;
				m_lines[scan_index].Y2 = center_y + Math.Sin(angle) * in_scan[scan_index] / 1000 * m_pixels_per_meter;
			}
		}

		#region · INotifyPropertyChanged members ·
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion
	}
}
