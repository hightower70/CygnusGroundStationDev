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
using System.Windows.Shapes;

namespace CygnusGroundStation
{
	/// <summary>
	/// Interaction logic for AddModule.xaml
	/// </summary>
	public partial class AddModuleDialog : Window
	{
		public AddModuleDialog()
		{
			InitializeComponent();
		}

		private void bAdd_Click(object sender, RoutedEventArgs e)
		{
			foreach (ModuleInfo module in lbModules.SelectedItems)
			{
				ModuleManager.Default.AddModule(module);
			}

			Close();
		}

		private void lbModules_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			foreach (ModuleInfo module in lbModules.SelectedItems)
			{
				ModuleManager.Default.AddModule(module);
			}

			Close();
		}
	}
}

