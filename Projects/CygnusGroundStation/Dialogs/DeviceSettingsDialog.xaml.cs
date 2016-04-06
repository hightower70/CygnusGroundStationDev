using CommonClassLibrary.DeviceSettings;
using CygnusControls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace CygnusGroundStation.Dialogs
{
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

				DeviceSettingValue.ValueType value_type = (DeviceSettingValue.ValueType)property.GetValue(obj);

				switch (value_type)
				{
					case DeviceSettingValue.ValueType.IntValue:
						return IntegerTemplate;

					case DeviceSettingValue.ValueType.StringValue:
						return StringTemplate;

					default:
						return base.SelectTemplate(item, container);
				}
			}
			else
				return base.SelectTemplate(item, container);
		}
	}

	/// <summary>
	/// Interaction logic for DeviceSettings.xaml
	/// </summary>
	public partial class DeviceSettingsDialog : Window
	{
		#region · Types ·
		public class DeviceSettingsGroupInfo : TreeViewItemBase
		{
			private DeviceSettingsGroup m_device_settings_group;

			public DeviceSettingsGroupInfo(DeviceSettingsGroup in_group)
			{
				m_device_settings_group = in_group;
			}

			public string DisplayName { get { return m_device_settings_group.DisplayName; } }
			public DeviceSettingsGroup Group { get { return m_device_settings_group; } }
		}

		public class DeviceSettingsValueInfo : INotifyPropertyChanged
		{
			private DeviceSettingValue m_device_settings_value;

			public event PropertyChangedEventHandler PropertyChanged;

			public DeviceSettingsValueInfo(DeviceSettingValue in_value)
			{
				m_device_settings_value = in_value;
			}

			public string DisplayName { get { return m_device_settings_value.DisplayName; } }

			public string Units { get { return m_device_settings_value.Units; } }

			public string Description { get { return m_device_settings_value.Description; } }

			public DeviceSettingValue.ValueType Type { get { return m_device_settings_value.Type; } }

			public int Min { get { return m_device_settings_value.Min; } }
			public int Max { get { return m_device_settings_value.Max; } }

			public object Value 
			{ 
				get { return m_device_settings_value.Value; }
				set 
				{ 
					m_device_settings_value.Value = value;
					OnPropertyChanged("Value");
				}
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


		#endregion


		private DeviceSettings m_device_settings;
		private volatile bool m_updating;
		private ObservableCollection<DeviceSettingsGroupInfo> m_device_settings_group_info = new ObservableCollection<DeviceSettingsGroupInfo>();
		private ObservableCollection<DeviceSettingsValueInfo> m_device_settings_value_info = new ObservableCollection<DeviceSettingsValueInfo>();

		public ObservableCollection<DeviceSettingsGroupInfo> TreeInfo
		{
			get { return m_device_settings_group_info; }
		}

		public ObservableCollection<DeviceSettingsValueInfo> ValueInfo
		{
			get { return m_device_settings_value_info; }
		}

		public DeviceSettingsDialog()
		{
			m_device_settings = new DeviceSettings();

			m_device_settings.ParseXMLFile("/Settings/*", @"d:\Projects\CygnusGroundStation\Projects\DeviceSettingsParser\setting.xml");

			UpdateDisplayedTree();

			InitializeComponent();
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

				for(int index = 0; index < selected_item.Group.Values.Count && m_updating; index++)
				{
					DeviceSettingsValueInfo info = new DeviceSettingsValueInfo(selected_item.Group.Values[index]);

					m_device_settings_value_info.Add(info);
				}
				//object t = sender.SelectedItem;
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//gSetupFormContainer.Children.Clear();

			//SetupDialogSettings settings = m_current_settings.GetSettings<SetupDialogSettings>();

			//settings.SetupDialogPos.SaveWindowPosition(this);

			//m_current_settings.SetSettings(settings);
		}

		private void Window_Initialized(object sender, System.EventArgs e)
		{
			//SetupDialogSettings settings = FrameworkSettingsFile.Default.GetSettings<SetupDialogSettings>();

			//settings.SetupDialogPos.LoadWindowPosition(this);

			//ModuleManager.Default.GenerateModuleSetupTreeInfo();

			//tvSetupTree.DataContext = ModuleManager.Default;
			tvDeviceSetupTree.DataContext = this;
			dgValueList.DataContext = this;

		}

		private void UpdateDisplayedTree()
		{
			m_updating = true;
			int index;


			index = 0;
			while (index < m_device_settings.DeviceSettingsRoot.Groups.Count && m_updating)
			{
				DeviceSettingsGroupInfo group_info = new DeviceSettingsGroupInfo(m_device_settings.DeviceSettingsRoot.Groups[index]);

				UpdateDisplayedTreeRecursively(group_info, m_device_settings.DeviceSettingsRoot.Groups[index]);

				m_device_settings_group_info.Add(group_info);

				index++;
			}
		}

		private void UpdateDisplayedTreeRecursively(DeviceSettingsGroupInfo in_parent, DeviceSettingsGroup in_group)
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

	}
}
