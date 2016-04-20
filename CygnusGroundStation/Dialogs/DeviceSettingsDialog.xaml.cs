using CommonClassLibrary.DeviceCommunication;
using CommonClassLibrary.DeviceSettings;
using CygnusControls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace CygnusGroundStation.Dialogs
{
	#region · Types ·
	public class DeviceSettingsTemplateSelector : DataTemplateSelector
	{
		public DataTemplate StringTemplate
		{ get; set; }

		public DataTemplate IntegerTemplate
		{ get; set; }

		public string PropertyName
		{ get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			DeviceSettingsDialog.DeviceSettingsValueInfo obj = item as DeviceSettingsDialog.DeviceSettingsValueInfo;

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
					case ParserDeviceSettingValue.ValueType.IntValue:
						return IntegerTemplate;

					case ParserDeviceSettingValue.ValueType.StringValue:
						return StringTemplate;

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
		/// device settings group information
		/// </summary>
		public class DeviceSettingsGroupInfo : TreeViewItemBase
		{
			private ParserDeviceSettingsGroup m_device_settings_group;

			public DeviceSettingsGroupInfo(ParserDeviceSettingsGroup in_group)
			{
				m_device_settings_group = in_group;
			}

			public string DisplayName { get { return m_device_settings_group.DisplayName; } }
			public ParserDeviceSettingsGroup Group { get { return m_device_settings_group; } }
		}

		public delegate void DeviceSettingsValueChangedCallback(DeviceSettingsValueInfo in_value_info);

		/// <summary>
		/// Device settings values info
		/// </summary>
		public class DeviceSettingsValueInfo : INotifyPropertyChanged
		{
			private ParserDeviceSettingValue m_device_settings_value;
			private DeviceSettingsValueChangedCallback m_value_changed_callback;

			public event PropertyChangedEventHandler PropertyChanged;

			public DeviceSettingsValueInfo(ParserDeviceSettingValue in_value, DeviceSettingsValueChangedCallback in_callback)
			{
				m_device_settings_value = in_value;
				m_value_changed_callback = in_callback;
			}

			public string DisplayName { get { return m_device_settings_value.Name; } }

			public string Units { get { return m_device_settings_value.Units; } }

			public string Description { get { return m_device_settings_value.Description; } }

			public ParserDeviceSettingValue.ValueType Type { get { return m_device_settings_value.Type; } }

			public int Min { get { return m_device_settings_value.Min; } }
			public int Max { get { return m_device_settings_value.Max; } }

			public object Value
			{
				get { return m_device_settings_value.Value; }
				set
				{
					m_device_settings_value.Value = value;
					OnPropertyChanged("Value");
					if (m_value_changed_callback != null)
						m_value_changed_callback(this);
				}
			}

			public ParserDeviceSettingValue ValueInfo
			{
				get { return m_device_settings_value; }
			}

			protected void OnPropertyChanged(string name)
			{
				PropertyChangedEventHandler handler = PropertyChanged;
				if (handler != null)
				{
					handler(this, new PropertyChangedEventArgs(name));
				}
			}
		}

		/// <summary>
		/// Files (used for device settings) information
		/// </summary>
		private class FileInfo
		{
			public string Name;
			public string FullPath;

			public FileInfo(string in_name)
			{
				Name = in_name;
				FullPath = string.Empty;
			}
		}

		#endregion

		#region · Data members ·
		private FileInfo[] m_files_info = { new FileInfo("ConfigurationXML"), new FileInfo("ConfigurationData") };
		private int m_current_file_index;

		private readonly SynchronizationContext m_synchronization_context;

		private ParserDeviceSettings m_device_settings;
		private DeviceSettingsBinaryData m_device_settings_binary_data = new DeviceSettingsBinaryData();
		static SettingsFileBase m_current_settings;
		private volatile bool m_updating;
		private ObservableCollection<DeviceSettingsGroupInfo> m_device_settings_group_info = new ObservableCollection<DeviceSettingsGroupInfo>();
		private ObservableCollection<DeviceSettingsValueInfo> m_device_settings_value_info = new ObservableCollection<DeviceSettingsValueInfo>();

		#endregion

		#region · Properties ·
		public static SettingsFileBase CurrentSettings
		{
			get { return m_current_settings; }
		}

		public ObservableCollection<DeviceSettingsGroupInfo> TreeInfo
		{
			get { return m_device_settings_group_info; }
		}

		public ObservableCollection<DeviceSettingsValueInfo> ValueInfo
		{
			get { return m_device_settings_value_info; }
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

			// copy current settings
			m_current_settings = new SettingsFileBase();
			m_current_settings.CopySettingsFrom(FrameworkSettingsFile.Default);

			m_device_settings = new ParserDeviceSettings();

			InitializeComponent();

			gFileTransferIndicator.Visibility = Visibility.Visible;
			gSettings.Visibility = Visibility.Hidden;

			FormManager.RealtimeObjectProviderAdd(this);

			// start file download
			m_current_file_index = 0;
			OnPropertyChanged("TotalFileCount");
			OnPropertyChanged("CurrentFileIndex");

			//CommunicationManager.Default.StartFileDownload(m_files_info[m_current_file_index].Name, OnFileOperationFinished);

			m_device_settings.ParseXMLFile("/Settings/*", @"d:\Projects\CygnusFCS\Resources\ConfigurationXML.xml");
			//m_device_settings_binary_data.Load(m_files_info[1].FullPath);

			//m_device_current_values
			UpdateDisplayedTree();

			//m_device_settings.UpdateValueOffset();

		}

		private void OnFileOperationFinished(CommunicationManager.FileOperationResult in_result, string in_file_path)
		{
			if (in_result == CommunicationManager.FileOperationResult.Success)
			{
				m_files_info[m_current_file_index].FullPath = in_file_path;

				if (m_current_file_index < m_files_info.Length - 1)
				{
					m_current_file_index++;
					OnPropertyChanged("CurrentFileIndex");

					CommunicationManager.Default.StartFileDownload(m_files_info[m_current_file_index].Name, OnFileOperationFinished);
				}
				else
				{
					m_synchronization_context.Send(FileOperationFinishedSync, null);
				}
			}
			else
			{
				// TODO error handling
			}
		}

		private void FileOperationFinishedSync(object in_param)
		{
			gFileTransferIndicator.Visibility = Visibility.Hidden;
			gSettings.Visibility = Visibility.Visible;

			// load files
			m_device_settings.ParserXMLFileGZIP("/Settings/*", m_files_info[0].FullPath);
			m_device_settings_binary_data.Load(m_files_info[1].FullPath);

			//m_device_current_values
			UpdateDisplayedTree();

			m_device_settings.UpdateValueOffset();
		}

		private void DeviceSettingsValueChanged(DeviceSettingsValueInfo in_value_info)
		{

		}


		private void bOK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void tvSetupTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (e.NewValue is DeviceSettingsGroupInfo)
			{
				DeviceSettingsGroupInfo selected_item = (DeviceSettingsGroupInfo)e.NewValue;
				m_device_settings_value_info.Clear();

				for (int index = 0; index < selected_item.Group.Values.Count && m_updating; index++)
				{
					DeviceSettingsValueInfo info = new DeviceSettingsValueInfo(selected_item.Group.Values[index], DeviceSettingsValueChanged);

					m_device_settings_value_info.Add(info);
				}
				//object t = sender.SelectedItem;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			FormManager.RealtimeObjectProviderRemove(this);

			DeviceSettinsgDialogSettings settings = m_current_settings.GetSettings<DeviceSettinsgDialogSettings>();

			settings.DialogPos.SaveWindowPosition(this);

			m_current_settings.SetSettings(settings);
		}

		private void Window_Initialized(object sender, System.EventArgs e)
		{
			DeviceSettinsgDialogSettings settings = m_current_settings.GetSettings<DeviceSettinsgDialogSettings>();

			settings.DialogPos.LoadWindowPosition(this);

			tvDeviceSetupTree.DataContext = this;
			dgValueList.DataContext = this;
		}

		private void UpdateDisplayedTree()
		{
			if (m_device_settings.DeviceSettingsRoot == null)
				return;

			m_updating = true;

			int index;

			index = 0;
			while (index < m_device_settings.DeviceSettingsRoot.Groups.Count && m_updating)
			{
				DeviceSettingsGroupInfo group_info = new DeviceSettingsGroupInfo(m_device_settings.DeviceSettingsRoot.Groups[index]);

				if (index == 0)
				{
					group_info.IsSelected = true;
				}

				UpdateDisplayedTreeRecursively(group_info, m_device_settings.DeviceSettingsRoot.Groups[index]);

				m_device_settings_group_info.Add(group_info);

				index++;
			}

			tvDeviceSetupTree.Focus();
		}

		private void UpdateDisplayedTreeRecursively(DeviceSettingsGroupInfo in_parent, ParserDeviceSettingsGroup in_group)
		{
			int index;

			index = 0;
			while (index < in_group.Groups.Count && m_updating)
			{
				DeviceSettingsGroupInfo group_info = new DeviceSettingsGroupInfo(m_device_settings.DeviceSettingsRoot.Groups[index]);

				UpdateDisplayedTreeRecursively(group_info, in_group.Groups[index]);

				in_parent.AddChild(group_info);

				index++;
			}
		}

		#region · INotifyPropertyChange ·
		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null && !string.IsNullOrEmpty(propertyName))
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

	}
}
