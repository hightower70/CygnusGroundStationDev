using System.Windows.Controls;

namespace OccupancyGrid
{
	/// <summary>
	/// Interaction logic for LidarDisplay.xaml
	/// </summary>
	public partial class OccupancyGridDisplayPanel : UserControl
	{
		//private OccupancyDisplay m_lidar_display;

		public OccupancyGridDisplayPanel()
		{
			InitializeComponent();

			//m_lidar_display = new OccupancyGridDisplay();
			//m_lidar_display.Initialize(OccupancyGridDisplayCanvas);

			//OccupancyGridDisplayCanvas.DataContext = m_lidar_display;

			//m_lidar_display.PositionX = 10;
			//m_lidar_display.PositionY = 11;
			//m_lidar_display.Heading = -45;
		}
	}
}
