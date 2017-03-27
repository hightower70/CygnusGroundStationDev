using CommonClassLibrary.DeviceCommunication;
using CommonClassLibrary.DeviceSettings;
using CommonClassLibrary.Settings;
using CygnusAuxBoardMonitor.Settings;
using CygnusControls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CygnusAuxBoardMonitor.Dialogs
{
	#region · Types ·
	public class DeviceSettingsTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StringTemplate
		{ get; set; }

		public DataTemplate IntegerTemplate
		{ get; set; }

		public DataTemplate EnumTemplate
		{ get; set; }

		public DataTemplate FloatTemplate
		{ get; set; }

		public string PropertyName
		{ get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			ParserDeviceSettingValue obj = item as ParserDeviceSettingValue;

			if (obj != null)
			{
				//use reflection to retrieve property
				Type type = obj.GetType();
				PropertyInfo property = type.GetProperty(this.PropertyName);

				if (property == null)
					return base.SelectTemplate(item, container);

				ParserDeviceSettingValue.ValueType value_type = (ParserDeviceSettingValue.ValueType)property.GetValue(obj);

				switch (value_type)
				{
					case ParserDeviceSettingValue.ValueType.Int32Value:
						return IntegerTemplate;

					case ParserDeviceSettingValue.ValueType.StringValue:
						return StringTemplate;

					case ParserDeviceSettingValue.ValueType.EnumValue:
						return EnumTemplate;

					case ParserDeviceSettingValue.ValueType.Int16FixedValue:
					case ParserDeviceSettingValue.ValueType.FloatValue:
						return FloatTemplate;

					case ParserDeviceSettingValue.ValueType.UInt16Value:
					case ParserDeviceSettingValue.ValueType.UInt8Value:
						return IntegerTemplate;

					default:
						return base.SelectTemplate(item, container);
				}
			}
			else
				return base.SelectTemplate(item, container);
		}
	}
	#endregion

	/// <summary>
	/// Interaction logic for DeviceSettings.xaml
	/// </summary>
	public partial class DeviceSettingsDialog : Window, INotifyPropertyChanged
	{
		#region · Types ·
		/// <summary>
		/// Files (used for device settings) information
		/// </summary>
		private class FileInfo
		{
			public string Name;
			public string FullPath;
			public byte FileID;

			public FileInfo(string in_name)
			{
				Name = in_name;
				FullPath = string.Empty;
				FileID = 0;
			}
		}

		#endregion

		#region · Data members ·
		private FileInfo[] m_files_info = { new FileInfo("ConfigurationData") };
		private int m_current_file_index;

		private readonly SynchronizationContext m_synchronization_context;

		static SettingsFileBase m_current_settings;

		private ParserDeviceSettings m_device_settings;
		private ObservableCollection<ParserDeviceSettingsGroup> m_device_settings_group = new ObservableCollection<ParserDeviceSettingsGroup>();
		private ObservableCollection<ParserDeviceSettingValue> m_device_settings_value = new ObservableCollection<ParserDeviceSettingValue>();
		private DeviceSettingsBinaryDataFile m_device_settings_binary_data = new DeviceSettingsBinaryDataFile();

		private DeviceSettinsgDialogSettings m_dialog_settings;

		private bool? m_dialog_result = null;

		private bool m_dialog_initializing;
		#endregion

		#region · Properties ·

		public DeviceSettinsgDialogSettings DialogSettings
		{
			get { return m_dialog_settings; }
		}

		public static SettingsFileBase CurrentSettings
		{
			get { return m_current_settings; }
		}

		public ObservableCollection<ParserDeviceSettingsGroup> Groups
		{
			get { return m_device_settings_group; }
		}

		public ObservableCollection<ParserDeviceSettingValue> Values
		{
			get { return m_device_settings_value; }
		}

		public int TotalFileCount
		{
			get { return m_files_info.Length; }
		}

		public int CurrentFileIndex
		{
			get { return m_current_file_index + 1; }
		}

		private FileInfo[] Files_info
		{
			get
			{
				return m_files_info;
			}

			set
			{
				m_files_info = value;
			}
		}

		#endregion

		public DeviceSettingsDialog()
		{
			m_synchronization_context = SynchronizationContext.Current;
			m_dialog_initializing = true;

			// copy current settings
			m_current_settings = new SettingsFileBase();
			m_current_settings.CopySettingsFrom(FrameworkSettingsFile.Default);

			m_dialog_settings = m_current_settings.GetSettings<DeviceSettinsgDialogSettings>();

			m_device_settings = new ParserDeviceSettings();
			m_device_settings.SetValueChangedCallback(OnDeviceSettingValueChanged);

			InitializeComponent();
			DataContext = this;

			gFileTransferIndicator.Visibility = Visibility.Visible;
			gSettings.Visibility = Visibility.Hidden;

			FormManager.RealtimeObjectProviderAdd(this);

			// start file download
			m_current_file_index = 0;
			OnPropertyChanged("TotalFileCount");
			OnPropertyChanged("CurrentFileIndex");

			CommunicationManager.Default.FileTransfer.FileDownloadStart(m_files_info[m_current_file_index].Name, OnFileReadOperationFinished);
		}

		private void OnFileReadOperationFinished(FileTransferManager.FileTransferResultInfo in_result)
		{
			if (in_result.State == FileTransferManager.FileTransferResult.Success)
			{
				m_files_info[m_current_file_index].FullPath = in_result.FullPath;
				m_files_info[m_current_file_index].FileID = in_result.FileID;

				if (m_current_file_index < m_files_info.Length - 1)
				{
					m_current_file_index++;
					OnPropertyChanged("CurrentFileIndex");

					CommunicationManager.Default.FileTransfer.FileDownloadStart(m_files_info[m_current_file_index].Name, OnFileReadOperationFinished);
				}
				else
				{
					m_synchronization_context.Send(FileReadOperationFinishedSync, null);
				}
			}
			else
			{
				// TODO error handling
			}
		}

		private void FileReadOperationFinishedSync(object in_param)
		{
			gFileTransferIndicator.Visibility = Visibility.Hidden;
			gSettings.Visibility = Visibility.Visible;

			// load files
			m_device_settings.ParseXMLFileFromResource("/Settings/*", "CygnusAuxBoardMonitor.Resources.ConfigurationXML.xml"); // load config from resource instead of downloading from the device
			m_device_settings_binary_data.Load(m_files_info[0].FullPath); // download setting values from the device
			//m_device_settings.ParserXMLFileGZIP("/Settings/*", m_files_info[0].FullPath);
			//m_device_settings_binary_data.Load(m_files_info[1].FullPath);

			m_device_settings.GenerateBinaryValueOffset();
			m_device_settings.UpdateValuesFromBinaryFile(m_device_settings_binary_data.BinaryDataFile);

			// update group list
			m_device_settings_group.Clear();
			for (int i = 0; i < m_device_settings.DeviceSettingsRoot.Groups.Count; i++)
			{
				m_device_settings_group.Add(m_device_settings.DeviceSettingsRoot.Groups[i]);
			}

			// update selected index
			if (m_device_settings_group.Count != 0)
				lbDeviceSetupGroup.SelectedIndex = 0;

			m_dialog_initializing = false;
		}

		private void Window_Initialized(object sender, System.EventArgs e)
		{
			m_dialog_settings.DialogPos.LoadWindowPositionAndSize(this);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			FormManager.RealtimeObjectProviderRemove(this);

			m_dialog_settings.DialogPos.SaveWindowPositionAndSize(this);

			m_current_settings.SetSettings(m_dialog_settings);
		}

		private void lbDeviceSetupGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count == 1)
			{
				ParserDeviceSettingsGroup group = (ParserDeviceSettingsGroup)e.AddedItems[0];
				m_device_settings_value.Clear();

				for (int index = 0; index < group.Values.Count; index++)
				{
					m_device_settings_value.Add(group.Values[index]);
				}
			}
		}

		private void OnDeviceSettingValueChanged(ParserDeviceSettingValue in_value_info)
		{
			if(!m_dialog_initializing)
				CommunicationManager.Default.FileTransfer.FileUploadStart(m_files_info[0].FileID, (uint)in_value_info.BinaryOffset, in_value_info.GetBinaryData(), null);
		}

		#region · INotifyPropertyChange ·
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null && !string.IsNullOrEmpty(propertyName))
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		private void bOK_Click(object sender, RoutedEventArgs e)
		{
			m_dialog_result = true;
			CommunicationManager.Default.FileTransfer.SendFileFinishRequest(FileOperationFinishMode.Success, OnFileOperationFinished);
		}

		private void bCancel_Click(object sender, RoutedEventArgs e)
		{
			m_dialog_result = true;
			CommunicationManager.Default.FileTransfer.SendFileFinishRequest(FileOperationFinishMode.Cancel, OnFileOperationFinished);
		}

		private void OnFileOperationFinished(FileTransferManager.FileTransferResultInfo in_result)
		{
			if (in_result.State == FileTransferManager.FileTransferResult.Success)
			{
				m_synchronization_context.Send(FileOperationFinishedSync, null);
			}
			else
			{
				//TODO: error handling
			}

		}

		private void FileOperationFinishedSync(object in_param)
		{
			this.DialogResult = m_dialog_result;
		}
	}
}
