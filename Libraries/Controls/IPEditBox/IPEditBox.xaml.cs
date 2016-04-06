using System;
using System.ComponentModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CygnusControls
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class IPEditBox : UserControl, INotifyPropertyChanged
	{
		#region · Class Variables ·

		private IPAddress m_ip_address = new IPAddress(0);

		#endregion

		#region · Constructor ·

		public IPEditBox()
		{
			InitializeComponent();

			gEditBoxes.DataContext = this;
		}

		#endregion

		#region · Properties ·

		/// <summary>
		/// Current IP Address value
		/// </summary>
		public IPAddress IPAddressValue
		{
			get { return (IPAddress)GetValue(IPAddressValueProperty); }
			set { SetValue(IPAddressValueProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IPAddressValue.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IPAddressValueProperty =
				DependencyProperty.Register("IPAddressValue", typeof(IPAddress), typeof(IPEditBox), new FrameworkPropertyMetadata(new IPAddress(new byte[] {0,0,0,0}), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIPAddressValueProperty));

		private static void OnIPAddressValueProperty(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			((IPEditBox)obj).m_ip_address = (IPAddress)(e.NewValue); 

			((IPEditBox)obj).NotifyPropertyChanged("Octet1");
			((IPEditBox)obj).NotifyPropertyChanged("Octet2");
			((IPEditBox)obj).NotifyPropertyChanged("Octet3");
			((IPEditBox)obj).NotifyPropertyChanged("Octet4");
		}

		/// <summary>
		/// First octet of the IP Address
		/// </summary>
		public Int32 Octet1
		{
			get
			{
				if (m_ip_address == null)
					return 0;
				else
					return m_ip_address.GetAddressBytes()[0];
			}
			set
			{
				byte[] address_bytes = m_ip_address.GetAddressBytes();
				address_bytes[0] = (byte)value;
				IPAddressValue = new IPAddress(address_bytes);
				NotifyPropertyChanged("Octet1");
			}
		}

		/// <summary>
		/// Second octet of the IP address
		/// </summary>
		public Int32 Octet2
		{
			get
			{
				if (m_ip_address == null)
					return 0;
				else
					return m_ip_address.GetAddressBytes()[1];
			}
			set
			{
				byte[] address_bytes = m_ip_address.GetAddressBytes();
				address_bytes[1] = (byte)value;
				IPAddressValue = new IPAddress(address_bytes);
				NotifyPropertyChanged("Octet2");
			}
		}

		/// <summary>
		/// Third octet of the IP address
		/// </summary>
		public Int32 Octet3
		{
			get
			{
				if (m_ip_address == null)
					return 0;
				else
					return m_ip_address.GetAddressBytes()[2];
			}
			set
			{
				byte[] address_bytes = m_ip_address.GetAddressBytes();
				address_bytes[2] = (byte)value;
				IPAddressValue = new IPAddress(address_bytes);
				NotifyPropertyChanged("Octet3");
			}
		}

		/// <summary>
		/// Forth octet of the IP address
		/// </summary>
		public Int32 Octet4
		{
			get
			{
				if (m_ip_address == null)
					return 0;
				else
					return m_ip_address.GetAddressBytes()[3];
			}
			set
			{
				byte[] address_bytes = m_ip_address.GetAddressBytes();
				address_bytes[3] = (byte)value;
				IPAddressValue = new IPAddress(address_bytes);
				NotifyPropertyChanged("Octet4");
			}
		}

		#endregion

		#region · Overrides ·

		public override string ToString()
		{
			return m_ip_address.ToString();// String.Format("{0}.{1}.{2}.{3}", Octet1, Octet2, Octet3, Octet4);
		}


		#endregion

		#region · Events ·

		private void TextOctet_GotFocus(object sender, RoutedEventArgs e)
		{
			TextBox tb = (TextBox)sender;
			tb.SelectAll();
		}

		private void TextOctet_TextChanged(object sender, TextChangedEventArgs e)
		{

			String outString = String.Empty;

			String[] text = Regex.Split(((TextBox)sender).Text, "");
			foreach (string s in text)
				if (!Regex.IsMatch(s, "[^0-9]"))
					outString += s;

			if (String.IsNullOrWhiteSpace(outString))
				outString = "";
			else if (int.Parse(outString) > 255)
				outString = "255";

			((TextBox)sender).Text = outString;
			((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

		private void FirstOctet_TextChanged(object sender, TextChangedEventArgs e)
		{

			String outString = String.Empty;

			String[] text = Regex.Split(((TextBox)sender).Text, "");
			foreach (string s in text)
				if (!Regex.IsMatch(s, "[^0-9]"))
					outString += s;

			if (String.IsNullOrWhiteSpace(outString))
				outString = "";
			else if (int.Parse(outString) > 255)
				outString = "255";
			
			((TextBox)sender).Text = outString;
			((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();

		}

		private void TextOctet_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			TextBox tb = (TextBox)sender;
			tb.SelectAll();
		}

		private void TextOctetDecimalArrows_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Decimal)
			{
				TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
				UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

				if (keyboardFocus != null)
				{
					keyboardFocus.MoveFocus(tRequest);
				}

				e.Handled = true;
			}
			else if (e.Key == Key.Right && ((TextBox)sender).Name != "TextOctet4")
			{
				if (((TextBox)sender).CaretIndex == ((TextBox)sender).Text.Length)
				{
					TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
					UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

					if (keyboardFocus != null)
					{
						keyboardFocus.MoveFocus(tRequest);
					}

					e.Handled = true;
				}
			}
			else if (e.Key == Key.Left && ((TextBox)sender).Name != "TextOctet1")
			{
				if (((TextBox)sender).CaretIndex == 0)
				{
					TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Previous);
					UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

					if (keyboardFocus != null)
					{
						keyboardFocus.MoveFocus(tRequest);
					}

					e.Handled = true;
				}
			}
		}

		private void TextOctetArrowsOnly_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Right && ((TextBox)sender).Name != "TextOctet4")
			{
				if (((TextBox)sender).CaretIndex == ((TextBox)sender).Text.Length)
				{
					TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
					UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

					if (keyboardFocus != null)
					{
						keyboardFocus.MoveFocus(tRequest);
					}

					e.Handled = true;
				}
			}
			else if (e.Key == Key.Left && ((TextBox)sender).Name != "TextOctet1")
			{
				if (((TextBox)sender).CaretIndex == 0)
				{
					TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Previous);
					UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

					if (keyboardFocus != null)
					{
						keyboardFocus.MoveFocus(tRequest);
					}

					e.Handled = true;
				}
			}
		}

		private void TextOctet1_LostFocus(object sender, RoutedEventArgs e)
		{
			if (String.IsNullOrWhiteSpace(((TextBox)sender).Text))
				((TextBox)sender).Text = "0";
		}

		#endregion

		#region · INotifyPropertyChanged ·

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}

		#endregion
	}
}
