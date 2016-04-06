using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CygnusGroundStation
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

			// load modules
			ModuleManager.Default.LoadModules();

			// init controls
			InitializeComponent();

			FormManager.Default.SetFormParent(FormContainer);

			// load startup form
			SetupFormSettings form_settings = FrameworkSettingsFile.Default.GetSettings<SetupFormSettings>();
			FormManager.Default.LoadForm(form_settings.StartupForm);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Window dialog = new SetupDialog();
			dialog.Owner = this;
			dialog.ShowDialog();
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			Window about = new AboutDialog();
			about.Owner = this;

			about.ShowDialog();
		}

		private void Options_Click(object sender, RoutedEventArgs e)
		{
			Window setup = new SetupDialog();
			setup.Owner = this;
			if (setup.ShowDialog() ?? false)
			{
				// save settings if dialog result was success
				FrameworkSettingsFile.Default.CopySettingsFrom(SetupDialog.CurrentSettings);
				FrameworkSettingsFile.Default.Save();

				// reload startup form
				SetupFormSettings form_settings = FrameworkSettingsFile.Default.GetSettings<SetupFormSettings>();
				FormManager.Default.LoadForm(form_settings.StartupForm);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			MainGeneralSettings settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			settings.MainWindowPos.SaveWindowPosition(this);

			FrameworkSettingsFile.Default.SetSettings(settings);
			FrameworkSettingsFile.Default.Save();
		}

		private void Window_Initialized(object sender, EventArgs e)
		{
			MainGeneralSettings settings = FrameworkSettingsFile.Default.GetSettings<MainGeneralSettings>();

			settings.MainWindowPos.LoadWindowPosition(this);
		}
	}
}
