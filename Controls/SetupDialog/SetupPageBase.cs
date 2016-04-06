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
// Base class for Setup Dialog Pages
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CygnusControls
{
	/// <summary>
	/// Base class for Setup Dialog Page
	/// </summary>
	public class SetupPageBase : UserControl
	{
		/// <summary>
		/// Page changed event arguments
		/// </summary>
		public class SetupPageEventArgs
		{
			public FrameworkElement OldPage { get; set; }
			public FrameworkElement NewPage { get; set; }
		}

		/// <summary>
		/// Event triggered when page inside the setup dialog has been activated
		/// </summary>
		public virtual void OnSetupPageActivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
		}

		/// <summary>
		/// Event triggered when page inside the setup dialog has been deactivated
		/// </summary>
		public virtual void OnSetupPageDeactivating(Window in_parent, SetupPageEventArgs in_event_info)
		{
		}
	}
}
