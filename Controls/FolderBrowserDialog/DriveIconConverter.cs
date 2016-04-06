using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CygnusControls
{
	public class DriveIconConverter : IValueConverter
	{
		private static BitmapImage removable;
		private static BitmapImage drive;
		private static BitmapImage netDrive;
		private static BitmapImage cdrom;
		private static BitmapImage ram;
		private static BitmapImage folder;

		public DriveIconConverter()
		{
			if (removable == null)
				removable = CreateImage("pack://application:,,,/WpfControls;component/CommonDialog/FolderBrowserDialog/Images/shell32_8.ico");

			if (drive == null)
				drive = CreateImage("pack://application:,,,/WpfControls;component/CommonDialog/FolderBrowserDialog/Images/shell32_9.ico");

			if (netDrive == null)
				netDrive = CreateImage("pack://application:,,,/WpfControls;component/CommonDialog/FolderBrowserDialog/Images/shell32_10.ico");

			if (cdrom == null)
				cdrom = CreateImage("pack://application:,,,/WpfControls;component/CommonDialog/FolderBrowserDialog/Images/shell32_12.ico");

			if (ram == null)
				ram = CreateImage("pack://application:,,,/WpfControls;component/CommonDialog/FolderBrowserDialog/Images/shell32_303.ico");

			if (folder == null)
				folder = CreateImage("pack://application:,,,/WpfControls;component/CommonDialog/FolderBrowserDialog/Images/shell32_264.ico");
		}

		private BitmapImage CreateImage(string uri)
		{
			BitmapImage img = new BitmapImage();
			img.BeginInit();
			img.UriSource = new Uri(uri);
			img.EndInit();
			return img;
		}

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var treeItem = value as TreeItem;
			if (treeItem == null)
				throw new ArgumentException("Illegal item type");

			if (treeItem is DriveTreeItem)
			{
				DriveTreeItem driveItem = treeItem as DriveTreeItem;
				switch (driveItem.DriveType)
				{
					case DriveType.CDRom:
						return cdrom;
					case DriveType.Fixed:
						return drive;
					case DriveType.Network:
						return netDrive;
					case DriveType.NoRootDirectory:
						return drive;
					case DriveType.Ram:
						return ram;
					case DriveType.Removable:
						return removable;
					case DriveType.Unknown:
						return drive;
				}
			}
			else
			{
				return folder;
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
