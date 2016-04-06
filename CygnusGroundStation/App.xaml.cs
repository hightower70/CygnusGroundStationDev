using CygnusGroundStation.Dialogs;
using System.Windows;

namespace CygnusGroundStation
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
			: base()
		{
			this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;
		}

		void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			ExceptionDialog dialog = new ExceptionDialog(e);

			if (MainWindow != null && MainWindow.IsVisible)
				dialog.Owner = MainWindow;

			dialog.ShowDialog();

			e.Handled = true;
		}
	}
}
