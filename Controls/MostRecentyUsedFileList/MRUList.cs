using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CygnusControls
{
	public class MRUList : DependencyObject
	{
		#region · Data members ·
		private ObservableCollection<MenuItem> m_mru_list;
		#endregion

		#region · Constructor ·
		/// <summary>
		/// Default constructor
		/// </summary>
		public MRUList()
		{
			m_mru_list = new ObservableCollection<MenuItem>();

			MenuItem empty_item = new MenuItem();
			empty_item.Header = "(emptylist)";
			m_mru_list.Add(empty_item);

			SetValue(MRUListProperty, m_mru_list);
		}
		#endregion

		#region · List maintenance functions ·
		public void UpdateList()
		{
			MRUMenuCommand command = new MRUMenuCommand(this);
			List<MenuItem> list = new List<MenuItem>();

			MenuItem menu_item = new MenuItem();
			menu_item.Header = "menu1";
			menu_item.Command = command;
			menu_item.CommandParameter = 1;

			list.Add(menu_item);

			menu_item = new MenuItem();
			menu_item.Header = "menu2";
			menu_item.Command = command;
			menu_item.CommandParameter = 2;

			list.Add(menu_item);


		}
		#endregion

		#region · Properties ·
		public ObservableCollection<MenuItem> Items
		{
			get { return (ObservableCollection<MenuItem>)GetValue(MRUListProperty); }
		}

			// Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
			public static readonly DependencyProperty MRUListProperty =
					DependencyProperty.Register("Items", typeof(ObservableCollection<MenuItem>), typeof(MRUList), new UIPropertyMetadata(null));

		#endregion
	}
}
