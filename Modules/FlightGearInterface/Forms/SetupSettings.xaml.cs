using CygnusControls;
using CygnusGroundStation.Dialogs;
using System.Windows;

namespace FlightGearInterface
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class SetupSettings : SetupPageBase
	{
		private FlightGearSettings m_settings;

		public SetupSettings()
		{
			InitializeComponent();

			m_settings = new FlightGearSettings();
			this.DataContext = m_settings;
		}

		public override void OnSetupPageActivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			// setup data provider
			//m_settings = SetupDialog.CurrentSettings.GetSettings<FlightGearSetings>();
			//this.DataContext = m_settings;
		}

		public override void OnSetupPageDeactivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			SetupDialog.CurrentSettings.SetSettings(m_settings);
		}
	}
}
