using CygnusControls;
using System.Windows;

namespace CygnusGroundStation.Dialogs
{
	/// <summary>
	/// Interaction logic for SetupPath.xaml
	/// </summary>
	public partial class SetupGeneral : SetupPageBase
	{
		private MainGeneralSettings m_data_provider;

		public SetupGeneral()
		{
			InitializeComponent();
		}

		public override void OnSetupPageActivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			// setup data provider
			m_data_provider = SetupDialog.CurrentSettings.GetSettings<MainGeneralSettings>();
			this.DataContext = m_data_provider;
		}

		public override void OnSetupPageDeactivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			SetupDialog.CurrentSettings.SetSettings(m_data_provider);
		}

	}
}
