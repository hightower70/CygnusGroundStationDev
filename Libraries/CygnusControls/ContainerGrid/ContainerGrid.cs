///////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2016 Laszlo Arvai. All rights reserved.
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
// Border control which handles zoom and pan operation
///////////////////////////////////////////////////////////////////////////////

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CygnusControls
{
	public class ContainerGrid : Grid
	{
		public string ChildPanel
		{
			get { return (string)GetValue(ChildFormProperty); }
			set { SetValue(ChildFormProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ChildForm.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ChildFormProperty =
				DependencyProperty.Register("ChildForm", typeof(string), typeof(ContainerGrid), new FrameworkPropertyMetadata("", new PropertyChangedCallback(OnChildFormPropertyChanged)));

		private static void OnChildFormPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			ContainerGrid container_grid = sender as ContainerGrid;
			if (container_grid != null)
			{
				container_grid.UpdateChildPanel((string)e.NewValue);
			}
		}

		private void UpdateChildPanel(string in_panel_name)
		{
			FrameworkElement child_panel;

			this.Children.Clear();

			child_panel = ModuleManager.Default.GetModuleDisplayPanel(in_panel_name);
			if (child_panel != null)
				this.Children.Add(child_panel);
		}

		public void Initialize(UIElement element)
		{

		}
	}
}