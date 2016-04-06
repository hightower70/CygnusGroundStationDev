using CygnusControls;
using CygnusGroundStation.Dialogs;
using System.Windows;

namespace XPlaneInterface
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class SetupInterfaceSettings : SetupPageBase
	{
		private XPlaneInterfaceSettings m_settings;

		public SetupInterfaceSettings()
		{
			// init data provider
			m_settings = new XPlaneInterfaceSettings();
			this.DataContext = m_settings;

			// Init components
			InitializeComponent();
		}

		public override void OnSetupPageActivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			// setup data provider
			m_settings = SetupDialog.CurrentSettings.GetSettings<XPlaneInterfaceSettings>();
			this.DataContext = m_settings;
		}

		public override void OnSetupPageDeactivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			// save settings
			SetupDialog.CurrentSettings.SetSettings(m_settings);
		}
	}
}
