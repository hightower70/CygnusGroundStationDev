using CygnusControls;
using System.Windows;

namespace CygnusGroundStation
{
	/// <summary>
	/// Interaction logic for SetupPath.xaml
	/// </summary>
	public partial class SetupGeneral : SetupPageBase
	{
		private SetupDataProvider m_data_provider;

		public SetupGeneral()
		{
			InitializeComponent();
		}

		public override void OnSetupPageActivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			// init control values
			m_data_provider = new SetupDataProvider();
			m_data_provider.ConfigFilePath = FrameworkSettingsFile.Default.ConfigFileName;
			m_data_provider.GeneralSettings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();
			this.DataContext = m_data_provider;
		}
	}
}
