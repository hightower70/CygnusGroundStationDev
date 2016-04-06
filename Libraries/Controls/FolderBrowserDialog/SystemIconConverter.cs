using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CygnusControls
{
	public class SystemIconConverter : IValueConverter
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct SHFILEINFO
		{
			public IntPtr hIcon;
			public IntPtr iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		};

			public const uint SHGFI_ICON = 0x100;
			public const uint SHGFI_LARGEICON = 0x0; // Large icon
			public const uint SHGFI_SMALLICON = 0x1; // Small icon
			public const uint USEFILEATTRIBUTES = 0x000000010; // when the full path isn't available
			public const uint SHGFI_OPENICON = 0x000000002;

			public const uint SHGFI_LINKOVERLAY = 0x000008000;

			public const int FILE_ATTRIBUTE_NORMAL = 0x80;
			[DllImport("shell32.dll")]
			public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
			[DllImport("User32.dll")]
			public static extern int DestroyIcon(IntPtr hIcon);


			public static ImageSource GetSmallIcon(string fileName)
		{
			fileName = fileName.TrimEnd('\\');

			IntPtr hImgSmall; //the handle to the system image list
			SHFILEINFO shinfo = new SHFILEINFO();
			hImgSmall = SHGetFileInfo(fileName, FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_SMALLICON | USEFILEATTRIBUTES | SHGFI_OPENICON );
			//The icon is returned in the hIcon member of the shinfo struct
			//Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
			//DestroyIcon(shinfo.hIcon);
			ImageSource img = Imaging.CreateBitmapSourceFromHIcon(shinfo.hIcon,Int32Rect.Empty,BitmapSizeOptions.FromWidthAndHeight(16,16));
			DestroyIcon(shinfo.hIcon);
			return img;
		}


		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return GetSmallIcon((string)value);
			
			/*
			var treeItem = value as TreeItem;
			if (treeItem == null)
				throw new ArgumentException("Illegal item type");

			if (treeItem is DriveTreeItem)
			{
				DriveTreeItem driveItem = treeItem as DriveTreeItem;
			}				*/
			//return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
