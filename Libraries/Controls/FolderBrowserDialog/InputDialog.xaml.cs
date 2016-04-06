using System.ComponentModel;
using System.Windows;

namespace CygnusControls
{
	/// <summary>
	/// Interaction logic for InputDialog.xaml
	/// </summary>
	public partial class InputDialog : Window, INotifyPropertyChanged
	{
		#region · Constructor ·

		/// <summary>
		/// default constructor
		/// </summary>
		public InputDialog()
		{
			InitializeComponent();
		}

		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets/sets message property
		/// </summary>
		public string Message
		{
			get
			{
				return m_message;
			}
			set
			{
				m_message = value;
				RaisePropertyChanged("Message");
			}
		}
		private string m_message;


		/// <summary>
		/// Gets/sets InputText property
		/// </summary>
		public string InputText
		{
			get
			{
				return m_input_text;
			}
			set
			{
				m_input_text = value;
				RaisePropertyChanged("InputText");
			}
		}
		private string m_input_text;
		#endregion

		#region · Event handler ·

		/// <summary>
		/// Button click handler
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
		#endregion

		#region · INotifyPropertyChanged ·

		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string in_property_name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(in_property_name));
			}
		}

		#endregion
	}
}
								 