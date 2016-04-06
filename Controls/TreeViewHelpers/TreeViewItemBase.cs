using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CygnusControls
{
	/// <summary>
	/// Base class for hiearchical data item (e.q. treeview items)
	/// </summary>
	public class TreeViewItemBase : INotifyPropertyChanged
	{
		#region · Data members ·
		private bool m_is_expanded = false;
		private bool m_is_selected = false;
		internal TreeViewItemBase m_item_parent = null;
		internal ObservableCollection<TreeViewItemBase> m_children = new ObservableCollection<TreeViewItemBase>();

		#endregion

		#region · Constructor&Destructor ·

		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets/Sets expansion flag
		/// </summary>
		public bool IsExpanded
		{
			get { return m_is_expanded; }
			set
			{
				m_is_expanded = value;
				OnPropertyChanged("IsExpanded");
			}
		}

		/// <summary>
		/// Gets/Sets selection flag
		/// </summary>
		public bool IsSelected
		{
			get { return m_is_selected; }
			set
			{
				m_is_selected = value;
				OnPropertyChanged("IsSelected");
			}
		}

		/// <summary>
		/// Gets item parent
		/// </summary>
		public TreeViewItemBase Parent
		{
			get { return m_item_parent; }
		}

		#endregion

		#region · Child handling ·

		/// <summary>
		/// List of the child elements
		/// </summary>
		public ObservableCollection<TreeViewItemBase> Children
		{
			get { return m_children; }
		}

		/// <summary>
		/// Sets item logical parent
		/// </summary>
		/// <param name="in_parent"></param>
		public void SetParent(TreeViewItemBase in_parent)
		{
			m_item_parent = in_parent;
		}

		/// <summary>
		/// Adds a child item
		/// </summary>
		/// <param name="in_child"></param>
		public void AddChild(TreeViewItemBase in_child)
		{
			in_child.SetParent(this);
			m_children.Add(in_child);
		}

		#endregion

		#region · INotifyPropertyChanged Members ·

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}