using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;

namespace CygnusControls
{
	public class TreeItem : INotifyPropertyChanged
	{
		#region · Constructor ·

		/// <summary>
		/// Creates a new TreeItem class for folder browser
		/// </summary>
		/// <param name="in_name">Name of the folder</param>
		/// <param name="in_parent">Parent of the folder</param>
		public TreeItem(string in_name, TreeItem in_parent)
		{
			Name = in_name;
			IsFullyLoaded = false;
			Parent = in_parent;

			Children = new ObservableCollection<TreeItem>();
		}
		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets/sets IsFullyLoaded property.
		/// </summary>
		public bool IsFullyLoaded { get; set; }

		/// <summary>
		/// Gets/sets name property
		/// </summary>
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
				RaisePropertyChanged("Name");
			}
		}
		private string m_name;

		/// <summary>
		/// Gets/sets parent folder
		/// </summary>
		public TreeItem Parent
		{
			get
			{
				return m_parent;
			}
			set
			{
				m_parent = value;
				RaisePropertyChanged("Parent");
			}
		}
		private TreeItem m_parent;

		/// <summary>
		/// Gets/sets child folder collection
		/// </summary>
		public ObservableCollection<TreeItem> Children
		{
			get
			{
				return m_children;
			}
			set
			{
				m_children = value;
				RaisePropertyChanged("Children");
			}
		}
		private ObservableCollection<TreeItem> m_children;

		public string FullPath
		{
			get { return GetFullPath(); }
		}

		#endregion

		#region · Public members ·

		/// <summary>
		/// Gets full path of the directory
		/// </summary>
		/// <returns></returns>
		public string GetFullPath()
		{
			Stack<string> stack = new Stack<string>();

			var ti = this;

			while (ti.Parent != null)
			{
				stack.Push(ti.Name);
				ti = ti.Parent;
			}

			string path = stack.Pop();

			while (stack.Count > 0)
			{
				path = Path.Combine(path, stack.Pop());
			}

			return path;
		}
		#endregion

		#region · INotifyPropertyChanged ·
		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged(string in_property_name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(in_property_name));
			}
		}
		#endregion
	}

	/// <summary>
	/// Drive tree item
	/// </summary>
	public class DriveTreeItem : TreeItem
	{
		public DriveType DriveType { get; set; }

		public DriveTreeItem(string name, DriveType driveType, TreeItem parent)
			: base(name, parent)
		{
			DriveType = driveType;
		}
	}
}
