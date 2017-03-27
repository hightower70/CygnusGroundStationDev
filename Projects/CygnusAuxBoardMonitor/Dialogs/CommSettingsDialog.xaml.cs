using CommonClassLibrary.Settings;
using CygnusAuxBoardMonitor.Settings;
using System.Windows;

namespace CygnusAuxBoardMonitor.Dialogs
{
	/// <summary>
	/// Interaction logic for CommSettings.xaml
	/// </summary>
	public partial class CommSettingsDialog : Window
	{
		private static SettingsFileBase m_current_settings;
		private SetupCommunicationDataProvider m_data_provider;

		public static SettingsFileBase CurrentSettings
		{
			get { return m_current_settings; }
		}

		public CommSettingsDialog()
		{
			// copy current settings
			m_current_settings = new SettingsFileBase();
			m_current_settings.CopySettingsFrom(FrameworkSettingsFile.Default);

			InitializeComponent();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			SetupDialogSettings settings = m_current_settings.GetSettings<SetupDialogSettings>();

			settings.DialogPos.SaveWindowPositionAndSize(this);

			m_current_settings.SetSettings(settings);
		}

		private void Window_Initialized(object sender, System.EventArgs e)
		{
			SetupDialogSettings settings = m_current_settings.GetSettings<SetupDialogSettings>();

			settings.DialogPos.LoadWindowPosition(this);

			// setup data provider
			m_data_provider = new SetupCommunicationDataProvider();
			this.DataContext = m_data_provider;
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			m_data_provider.Save();

			DialogResult = true;
		}
	}
}
