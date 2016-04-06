using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CygnusControls
{
	/// <summary>
	/// Interaction logic for SevenSegment.xaml
	/// </summary>
	public partial class SevenSegmentDigit : UserControl
	{
		public SevenSegmentDigit()
		{
			InitializeComponent();
			SetValue(0);
		}

		#region · Member variables ·
		private int m_digit_index = 0;
		private bool m_disable_zero_blanking = false;

		#endregion

		#region · Properties ·

		/// <summary>
		/// Radius of the segment blur effect
		/// </summary>
		public double BlurRadius
		{
			get { return (double)GetValue(BlurRadiusProperty); }
			set { SetValue(BlurRadiusProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BlurRadius.  
		public static readonly DependencyProperty BlurRadiusProperty =
				DependencyProperty.Register("BlurRadius", typeof(double), typeof(SevenSegmentDigit), new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsRender));

		/// <summary>
		/// Color of the display segments
		/// </summary>
		public Brush SegmentColor
		{
			get { return (Brush)GetValue(SegmentColorProperty); }
			set { SetValue(SegmentColorProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SegmentColor.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SegmentColorProperty =
				DependencyProperty.Register("SegmentColor", typeof(Brush), typeof(SevenSegmentDigit), new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));


		/// <summary>
		/// Disables zero blanking of the given digit
		/// </summary>
		public bool DisableZeroBlanking
		{
			get { return m_disable_zero_blanking; }
			set { m_disable_zero_blanking = value; }
		}

		/// <summary>
		/// Index of the digit (0=10^0, 1=10^1 digit, etc.)
		/// </summary>
		public int DigitIndex
		{
			get { return m_digit_index; }
			set { m_digit_index = value; }
		}
		
		/// <summary>
		/// Value to display
		/// </summary>
		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ValueProperty =
				DependencyProperty.Register("Value", typeof(double), typeof(SevenSegmentDigit), new PropertyMetadata(0.0, OnValueChanged));


		private static void OnValueChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
		{
			SevenSegmentDigit digit = source as SevenSegmentDigit;

			if (digit != null && e.NewValue is double)
				digit.SetValue((double)e.NewValue);
		}
		
		#endregion

		#region · Display functions ·

		/// <summary>
		/// Sets value to display
		/// </summary>
		/// <param name="in_value"></param>
		public void SetValue(double in_value)
		{
			int number = (int)(in_value / Math.Pow(10, DigitIndex));
			int digit_number = number % 10;

			if (number > 9 || digit_number != 0 || m_digit_index == 0 || m_disable_zero_blanking)
			{
				SetDigit(digit_number);
			}
			else
			{
				SetDigit(10);
			}
		}

		/// <summary>
		/// Sets segments to certain number display
		/// </summary>
		/// <param name="in_number"></param>
		public void SetDigit(int in_number)
		{
			if (in_number < 0)
				in_number = 0;
			if (in_number > 10)
				in_number = 10;

			segA.Opacity = m_segment_table[in_number, 0];
			segB.Opacity = m_segment_table[in_number, 1];
			segC.Opacity = m_segment_table[in_number, 2];
			segD.Opacity = m_segment_table[in_number, 3];
			segE.Opacity = m_segment_table[in_number, 4];
			segF.Opacity = m_segment_table[in_number, 5];
			segG.Opacity = m_segment_table[in_number, 6];
		}
		#endregion


		#region · Segment table ·
		private static readonly int[,] m_segment_table = 
		{ 
			//a  b  c  d  e  f  g
			{ 1, 1, 1, 1, 1, 1, 0 }, // 0
			{ 0, 1, 1, 0, 0, 0, 0 }, // 1
			{ 1, 1, 0, 1, 1, 0, 1 }, // 2
			{ 1, 1, 1, 1, 0, 0, 1 }, // 3
			{ 0, 1, 1, 0, 0, 1, 1 }, // 4
			{ 1, 0, 1, 1, 0, 1, 1 }, // 5
			{ 1, 0, 1, 1, 1, 1, 1 }, // 6
			{ 1, 1, 1, 0, 0, 0, 0 }, // 7
			{ 1, 1, 1, 1, 1, 1, 1 }, // 8
			{ 1, 1, 1, 1, 0, 1, 1 }, // 9
			{ 0, 0, 0, 0, 0, 0, 0 }	 // blank
		};
		#endregion
	}
}
