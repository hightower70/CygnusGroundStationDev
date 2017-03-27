using CommonClassLibrary.DeviceCommunication;
using CygnusControls;
using System.Windows;

namespace CygnusGroundStation.Dialogs
{
	/// <summary>
	/// Interaction logic for AboutControl.xaml
	/// </summary>
	public partial class SetupCommunication : SetupPageBase
	{
		private SetupCommunicationDataProvider m_data_provider;

		public SetupCommunication()
		{
			InitializeComponent();
		}

		public override void OnSetupPageActivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			// setup data provider
			m_data_provider = new SetupCommunicationDataProvider();
			this.DataContext = m_data_provider;
		}

		public override void OnSetupPageDeactivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
			m_data_provider.Save();
		}
	}
}
