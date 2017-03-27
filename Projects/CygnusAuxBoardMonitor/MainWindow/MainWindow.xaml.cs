using CommonClassLibrary.DeviceCommunication;
using CommonClassLibrary.RealtimeObjectExchange;
using CommonClassLibrary.Settings;
using CygnusAuxBoardMonitor.Dialogs;
using CygnusAuxBoardMonitor.Settings;
using CygnusControls;
using System.Windows;

namespace CygnusAuxBoardMonitor
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

			CreateUARTCommunicator();

			CommunicationManager.Default.PacketLogCreate("packet_log.txt");

			// create realtime objects
			CreateRealtimeObjects();

			InitializeComponent();

			FormManager.RealtimeObjectProviderAdd(this);

			// start communication manager
			CommunicationManager.Default.Start();
		}

		private void CreateUARTCommunicator()
		{
			SetupCommunicationSettings com_settings = FrameworkSettingsFile.Default.GetSettings<SetupCommunicationSettings>();

			// init communication manager
			UARTCommunicator uart_communicator = new UARTCommunicator();
			uart_communicator.PortName = com_settings.UARTPort;
			uart_communicator.BaudRate = 115200;

			CommunicationManager.Default.AddCommunicator(uart_communicator);
		}

		private void CreateRealtimeObjects()
		{
			// create realtime objects
			RealtimeObjectStorage.Default.ObjectClear();

			CommunicationManager.Default.CreateRealtimeObjects();

			ParserRealtimeObjectExchange rox_parser = new CommonClassLibrary.RealtimeObjectExchange.ParserRealtimeObjectExchange();
			rox_parser.ParseXMLFileFromResource("/RealtimeObjectExchangle/*", "CygnusAuxBoardMonitor.Resources.RealtimeObjects.xml"); // load realtime objects from resource instead of downloading from the device
			RealtimeObjectStorage.Default.CopyParsedObjects(rox_parser.Collection.Objects);
		}

		private void Exit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Options_Click(object sender, RoutedEventArgs e)
		{
			CommSettingsDialog dialog = new CommSettingsDialog();

			dialog.Owner = this;
			if (dialog.ShowDialog() ?? false)
			{
				// save settings if dialog result was success
				FrameworkSettingsFile.Default.CopySettingsFrom(CommSettingsDialog.CurrentSettings);
				FrameworkSettingsFile.Default.Save();
			}
		}

		private void Window_Initialized(object sender, System.EventArgs e)
		{
			MainGeneralSettings settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			settings.MainWindowPos.LoadWindowPositionAndSize(this);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			CommunicationManager.Default.Stop();
			CommunicationManager.Default.PacketLogClose();
			FormManager.RealtimeObjectProviderRemove(this);

			MainGeneralSettings settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			settings.MainWindowPos.SaveWindowPositionAndSize(this);

			FrameworkSettingsFile.Default.SetSettings(settings);
			FrameworkSettingsFile.Default.Save();
		}

		private void DeviceSettings_Click(object sender, RoutedEventArgs e)
		{
			DeviceSettingsDialog dialog = new DeviceSettingsDialog();

			dialog.Owner = this;
			if (dialog.ShowDialog() ?? false)
			{
				// save settings if dialog result was success
				FrameworkSettingsFile.Default.CopySettingsFrom(DeviceSettingsDialog.CurrentSettings);
				FrameworkSettingsFile.Default.Save();
			}
		}
	}
}
