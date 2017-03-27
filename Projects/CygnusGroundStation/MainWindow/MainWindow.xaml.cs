using CommonClassLibrary.DeviceCommunication;
using CommonClassLibrary.RealtimeObjectExchange;
using CommonClassLibrary.Settings;
using CygnusControls;
using CygnusGroundStation.Dialogs;
using System;
using System.Windows;

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
			ModuleManager.Default.AddMainModule(new ModuleInterface());
			ModuleManager.Default.ModulesLoad();

			// init controls
			InitializeComponent();

			FormManager.Default.SetFormParent(FormContainer);

			// create communicators
			CreateUDPCommunicator();
			CreateUARTCommunicator();
			CreateUSBCommunicator();

			CommunicationManager.Default.PacketLogCreate("packet_log.txt");

			// create realtime objects
			CreateRealtimeObjects();

			// load startup form
			SetupFormSettings form_settings = FrameworkSettingsFile.Default.GetSettings<SetupFormSettings>();
			MainGeneralSettings main_settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			FormManager.Default.LoadForm(form_settings.StartupForm, main_settings.ModulesPath, main_settings.FormsPath);

			// start modules
			ModuleManager.Default.ModulesInitializeAndStart();

			// start communication manager
			CommunicationManager.Default.Start();

		}

		private void CreateUDPCommunicator()
		{
			SetupCommunicationSettings com_settings = FrameworkSettingsFile.Default.GetSettings<SetupCommunicationSettings>();

			if (!com_settings.UDPEnabled)
				return;

			// init communication manager
			UDPCommunicator udp_communicator = new UDPCommunicator();
			udp_communicator.UDPLocalPort = com_settings.UDPLocalPort;
			udp_communicator.UDPRemotePort = com_settings.UDPRemotePort;

			CommunicationManager.Default.AddCommunicator(udp_communicator);
		}

		private void CreateUARTCommunicator()
		{
			SetupCommunicationSettings com_settings = FrameworkSettingsFile.Default.GetSettings<SetupCommunicationSettings>();

			if (!com_settings.UARTEnabled)
				return;

			// init communication manager
			UARTCommunicator uart_communicator = new UARTCommunicator();
			uart_communicator.PortName = com_settings.UARTPort;
			uart_communicator.BaudRate = com_settings.UARTBaud;

			CommunicationManager.Default.AddCommunicator(uart_communicator);
		}


		private void CreateUSBCommunicator()
		{
			SetupCommunicationSettings usb_settings = FrameworkSettingsFile.Default.GetSettings<SetupCommunicationSettings>();

			if (!usb_settings.USBEnabled)
				return;

			// init communication manager
			USBCommunicator usb_communicator = new USBCommunicator();
			usb_communicator.VID = usb_settings.USBVID;
			usb_communicator.PID = usb_settings.USBPID;

			CommunicationManager.Default.AddCommunicator(usb_communicator);
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
			ModuleManager.Default.ModulesStop();
			CommunicationManager.Default.Stop();
			CommunicationManager.Default.PacketLogClose();
			FormManager.Default.ObjectRefreshStop();
			FormManager.Default.CloseCurrentForm();

			MainGeneralSettings settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			settings.MainWindowPos.SaveWindowPositionAndSize(this);

			FrameworkSettingsFile.Default.SetSettings(settings);
			FrameworkSettingsFile.Default.Save();
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
			MainGeneralSettings settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			settings.MainWindowPos.LoadWindowPositionAndSize(this);
		}

		private void DeviceSettings_Click(object sender, RoutedEventArgs e)
		{
			Window setup = new DeviceSettingsDialog();
			setup.Owner = this;
			if (setup.ShowDialog() ?? false)
			{
				// save settings if dialog result was success
				FrameworkSettingsFile.Default.CopySettingsFrom(DeviceSettingsDialog.CurrentSettings);
				FrameworkSettingsFile.Default.Save();
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
