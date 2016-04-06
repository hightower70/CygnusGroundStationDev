using CygnusControls;
using System.Windows;

namespace CygnusGroundStation.Dialogs
{
	/// <summary>
	/// Interaction logic for SetupForms.xaml
	/// </summary>
	public partial class SetupForms : SetupPageBase
	{
		private SetupFormsDataProvider m_data_provider;

		public SetupForms()
		{
			InitializeComponent();
		}

		public override void OnSetupPageActivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			// setup data provider
			m_data_provider = new SetupFormsDataProvider();
			this.DataContext = m_data_provider;
		}

		public override void OnSetupPageDeactivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			m_data_provider.Save();
		}
	}
}
