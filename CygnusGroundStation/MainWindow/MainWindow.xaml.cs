using CommonClassLibrary.DeviceCommunication;
using CommonClassLibrary.RealtimeObjectExchange;
using CygnusControls;
using CygnusGroundStation.Dialogs;
using System;
using System.Windows;
using System.Windows.Threading;
using CommonClassLibrary.DeviceCommunication;

namespace CygnusGroundStation
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			// load config file and get settings
			FrameworkSettingsFile.Default.Load();

			// load modules
			ModuleManager.Default.ModulesLoad();

			// init controls
			InitializeComponent();

			FormManager.Default.SetFormParent(FormContainer);


			// init communication manager
			UDPCommunicator udp_communicator = new UDPCommunicator();
			udp_communicator.UDPReceiverPort = 9602;
			udp_communicator.UDPTransmiterPort = 9601;
			udp_communicator.UDPDeviceReceiverPort = 9601;

			CommunicationManager.Default.AddCommunicator(udp_communicator);

			// create realtime objects
			CreateRealtimeObjects();

			// load startup form
			SetupFormSettings form_settings = FrameworkSettingsFile.Default.GetSettings<SetupFormSettings>();
			MainGeneralSettings main_settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			FormManager.Default.LoadForm(form_settings.StartupForm, main_settings.ModulesPath, main_settings.FormsPath);

			// start communication manager
			CommunicationManager.Default.Start();

		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			Window about = new AboutDialog();
			about.Owner = this;

			about.ShowDialog();
		}

		private void Options_Click(object sender, RoutedEventArgs e)
		{
			Window setup = new SetupDialog();
			setup.Owner = this;
			if (setup.ShowDialog() ?? false)
			{
				using (new WaitCursor())
				{
					// stop modules and dispatcher timer
					FormManager.Default.ObjectRefreshStop();
					ModuleManager.Default.ModulesStop();

					// save settings if dialog result was success
					FrameworkSettingsFile.Default.CopySettingsFrom(SetupDialog.CurrentSettings);
					FrameworkSettingsFile.Default.Save();

					// reload modules
					ModuleManager.Default.ModulesLoad();
					ModuleManager.Default.ModulesInitializeAndStart();

					// reload startup form
					SetupFormSettings form_settings = FrameworkSettingsFile.Default.GetSettings<SetupFormSettings>();
					MainGeneralSettings main_settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

					FormManager.Default.LoadForm(form_settings.StartupForm, main_settings.ModulesPath, main_settings.FormsPath);

					// restart modules and dispatcher timer
					FormManager.Default.ObjectRefreshStart();
				}
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			CommunicationManager.Default.Stop();
			FormManager.Default.ObjectRefreshStop();
			FormManager.Default.CloseCurrentForm();

			MainGeneralSettings settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			settings.MainWindowPos.SaveWindowPosition(this);

			FrameworkSettingsFile.Default.SetSettings(settings);
			FrameworkSettingsFile.Default.Save();
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
			MainGeneralSettings settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			settings.MainWindowPos.LoadWindowPosition(this);
		}

		private void DeviceSettings_Click(object sender, RoutedEventArgs e)
		{
			Window setup = new DeviceSettingsDialog();
			setup.Owner = this;
			if (setup.ShowDialog() ?? false)
			{
			}
		}

		private void CreateRealtimeObjects()
		{
			// create realtime objects
			RealtimeObjectStorage.Default.ObjectClear();

			CommunicationManager.Default.CreateRealtimeObjects();
		}
	}
}
