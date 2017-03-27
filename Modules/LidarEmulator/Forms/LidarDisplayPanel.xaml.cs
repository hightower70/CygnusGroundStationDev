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

namespace LidarEmulator
{
	/// <summary>
	/// Interaction logic for LidarDisplay.xaml
	/// </summary>
	public partial class LidarDisplayPanel : UserControl
	{
		private LidarDisplay m_lidar_display;

		public LidarDisplayPanel()
		{
			InitializeComponent();

			m_lidar_display = new LidarDisplay();
			m_lidar_display.Initialize(LidarDisplayCanvas);

			LidarDisplayCanvas.DataContext = m_lidar_display;

			m_lidar_display.PositionX = 10;
			m_lidar_display.PositionY = 11;
			m_lidar_display.Heading = -45;
		}
	}
}
