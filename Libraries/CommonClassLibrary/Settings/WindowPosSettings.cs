///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2014 Laszlo Arvai. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the GNU Lesser General Public License as published
// by the Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
// MA 02110-1301  USA
///////////////////////////////////////////////////////////////////////////////
// File description
// ----------------
// Class for handling window (Form) position and size settings
///////////////////////////////////////////////////////////////////////////////
using System.Windows;

namespace CommonClassLibrary.Settings
{
	public class WindowPosSettings
	{
		// Main window position and size
		public WindowState State;
		public double Width { get; set; }
		public double Height { get; set; }
		public double Left { get; set; }
		public double Top { get; set; }

		/// <summary>
		/// Sets window size to the given value and centers window on the screen
		/// </summary>
		/// <param name="in_width"></param>
		/// <param name="in_height"></param>
		public void SetDefault(int in_width, int in_height)
		{
			State = WindowState.Normal;

			// Window position and size
			Width = in_width;
			Height = in_height;

			Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
			Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
		}

		/// <summary>
		/// Loads window positon settings (must be called from OnLoaded event handler)
		/// </summary>
		/// <param name="in_window"></param>
		public void LoadWindowPositionAndSize(Window in_window)
		{
			// set state
			in_window.WindowState = State;

			if (in_window.WindowState == WindowState.Normal)
			{
				in_window.Left = Left;
				in_window.Top = Top;
				in_window.Width = Width;
				in_window.Height = Height;
			}
		}

		/// <summary>
		/// Saves current window settings (must be called from OnClosed event handler)
		/// </summary>
		/// <param name="in_window"></param>
		public void SaveWindowPositionAndSize(Window in_window)
		{
			State = in_window.WindowState;

			Left = in_window.Left;
			Top = in_window.Top;
			Width = in_window.Width;
			Height = in_window.Height;
		}

		/// <summary>
		/// Loads window positon settings (must be called from OnLoaded event handler)
		/// </summary>
		/// <param name="in_window"></param>
		public void LoadWindowPosition(Window in_window)
		{
			// set state
			in_window.WindowState = State;

			if (in_window.WindowState == WindowState.Normal)
			{
				in_window.Left = Left;
				in_window.Top = Top;
			}
		}
	}
}
