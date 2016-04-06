using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace CygnusControls
{
	/// <summary>
	/// Interaction logic for FolderPicker.xaml
	/// </summary>
	public partial class FolderBrowserControl : UserControl, INotifyPropertyChanged
	{
		#region · Constants ·
		private const string EmptyItemName = "Empty";
		private const string NewFolderName = "New Folder";
		private const int MaxNewFolderSuffix = 10000;
		#endregion

		#region · Data members ·

		#endregion

		#region · Properties ·

		/// <summary>
		/// Gets/sets Root folder item
		/// </summary>
		public TreeItem Root
		{
			get
			{
				return root;
			}
			private set
			{
				root = value;
				RaisePropertyChanged("Root");
			}
		}

		/// <summary>
		/// Gets/sets selected folder item
		/// </summary>
		public TreeItem SelectedItem
		{
			get
			{
				return selectedItem;
			}
			private set
			{
				selectedItem = value;
				RaisePropertyChanged("SelectedItem");
			}
		}

		public string SelectedPath { get; private set; }

		public string CurrentPath
		{
			get { return (string)GetValue(CurrentPathProperty); }
			set { SetValue(CurrentPathProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentPath.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentPathProperty =
				DependencyProperty.Register("CurrentPath", typeof(string), typeof(FolderBrowserControl), new PropertyMetadata("", CurrentPathChangedCallback));

		private static void CurrentPathChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
		}



		public string CurrentPathEdit
		{
			get { return (string)GetValue(CurrentPathEditProperty); }
			set { SetValue(CurrentPathEditProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentPathEdit.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentPathEditProperty =
				DependencyProperty.Register("CurrentPathEdit", typeof(string), typeof(FolderBrowserControl), new PropertyMetadata(""));


		/// <summary>
		/// Gets/sets initial path
		/// </summary>
		public string InitialPath
		{
			get
			{
				return initialPath;
			}
			set
			{
				initialPath = value;
				UpdateInitialPathUI();
			}
		}

		/// <summary>
		/// Gets/sets Item container style
		/// </summary>
		public Style ItemContainerStyle
		{
			get
			{
				return itemContainerStyle;
			}
			set
			{
				itemContainerStyle = value;
				RaisePropertyChanged("ItemContainerStyle");
			}
		}

		#endregion

		public FolderBrowserControl()
		{
			InitializeComponent();

			Init();
		}

		public void CreateNewFolder()
		{
			CreateNewFolderImpl(SelectedItem);
		}

		public void RefreshTree()
		{
			Root = null;
			Init();
		}

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

		#region Private methods

		private void Init()
		{
			DriveTreeItem item;
			root = new TreeItem("root", null);
			var systemDrives = DriveInfo.GetDrives();

			foreach (var sd in systemDrives)
			{
				item = new DriveTreeItem(sd.Name, sd.DriveType, root);
				item.Children.Add(new TreeItem(EmptyItemName, item));

				root.Children.Add(item);
			}

			item = new DriveTreeItem("Network", DriveType.Network, root);
			item.Children.Add(new TreeItem(EmptyItemName, item));
			root.Children.Add(item);

			Root = root; // to notify UI
		}

		private void TreeView_Selected(object sender, RoutedEventArgs e)
		{
			var tvi = e.OriginalSource as TreeViewItem;
			if (tvi != null)
			{
				SelectedItem = tvi.DataContext as TreeItem;
				string full_path = SelectedItem.GetFullPath();
				SelectedPath = full_path;
				CurrentPathEdit = full_path;
			}
		}

		private void TreeView_Expanded(object sender, RoutedEventArgs e)
		{
			var tvi = e.OriginalSource as TreeViewItem;
			var treeItem = tvi.DataContext as TreeItem;

			if (treeItem != null)
			{
				if (!treeItem.IsFullyLoaded)
				{
					treeItem.Children.Clear();

					// Handle network computers
					if (treeItem is DriveTreeItem && (treeItem as DriveTreeItem).DriveType == DriveType.Network)
					{
						// browse for network computers
						List<string> list = null;

						Mouse.OverrideCursor = Cursors.Wait;
						try
						{
							list = NetworkBrowser.GetNetworkComputers();
						}
						finally
						{
							Mouse.OverrideCursor = null;
						}

						if (list != null)
						{
							foreach (string computer in list)
							{
								TreeItem item = new TreeItem("\\\\" + computer, treeItem);
								item.Children.Add(new TreeItem(EmptyItemName, item));

								treeItem.Children.Add(item);
							}

							treeItem.IsFullyLoaded = true;

							CurrentPath = treeItem.GetFullPath();
						}
					}
					else
					{
						// handle shared directory on network computers
						string path = treeItem.GetFullPath();

						if (treeItem.Parent is DriveTreeItem && (treeItem.Parent as DriveTreeItem).DriveType == DriveType.Network)
						{
							Mouse.OverrideCursor = Cursors.Wait;

							try
							{
								List<string> shared_folders = NetworkBrowser.GetSharedFolders(treeItem.Name);
								if (shared_folders != null)
								{
									foreach (string folder in shared_folders)
									{
										TreeItem item = new TreeItem(folder, treeItem);
										item.Children.Add(new TreeItem(EmptyItemName, item));

										treeItem.Children.Add(item);
									}
								}
							}
							catch {}
							finally
							{
								Mouse.OverrideCursor = null;
							}

							treeItem.IsFullyLoaded = true;

							CurrentPath = treeItem.GetFullPath();
						}
						else
						{
							DirectoryInfo dir = new DirectoryInfo(path);

							try
							{
								var subDirs = dir.GetDirectories();
								foreach (var sd in subDirs)
								{
									TreeItem item = new TreeItem(sd.Name, treeItem);
									item.Children.Add(new TreeItem(EmptyItemName, item));

									treeItem.Children.Add(item);
								}
							}
							catch { }
						}

						treeItem.IsFullyLoaded = true;

						CurrentPath = treeItem.GetFullPath();
					}
				}
			}
			else
				throw new Exception();
		}

		private void UpdateInitialPathUI()
		{
			if (!Directory.Exists(InitialPath))
				return;

			var initialDir = new DirectoryInfo(InitialPath);

			if (!initialDir.Exists)
				return;

			var stack = TraverseUpToRoot(initialDir);
			var containerGenerator = TreeView.ItemContainerGenerator;
			var uiContext = TaskScheduler.FromCurrentSynchronizationContext();
			DirectoryInfo currentDir = null;
			var dirContainer = Root;

			AutoResetEvent waitEvent = new AutoResetEvent(true);

			Task processStackTask = Task.Factory.StartNew(() =>
					{
						while (stack.Count > 0)
						{
							waitEvent.WaitOne();

							currentDir = stack.Pop();

							Task waitGeneratorTask = Task.Factory.StartNew(() =>
							{
								if (containerGenerator == null)
									return;

								while (containerGenerator.Status != GeneratorStatus.ContainersGenerated)
									Thread.Sleep(50);
							}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

							Task updateUiTask = waitGeneratorTask.ContinueWith((r) =>
							{
								try
								{
									var childItem = dirContainer.Children.Where(c => c.Name == currentDir.Name).FirstOrDefault();
									var tvi = containerGenerator.ContainerFromItem(childItem) as TreeViewItem;
									dirContainer = tvi.DataContext as TreeItem;
									tvi.IsExpanded = true;

									tvi.Focus();

									containerGenerator = tvi.ItemContainerGenerator;
								}
								catch { }

								waitEvent.Set();
							}, uiContext);
						}

					}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
		}

		private Stack<DirectoryInfo> TraverseUpToRoot(DirectoryInfo child)
		{
			if (child == null)
				return null;

			if (!child.Exists)
				return null;

			Stack<DirectoryInfo> queue = new Stack<DirectoryInfo>();
			queue.Push(child);
			DirectoryInfo ti = child.Parent;

			while (ti != null)
			{
				queue.Push(ti);
				ti = ti.Parent;
			}

			return queue;
		}

		private void CreateNewFolderImpl(TreeItem parent)
		{
			try
			{
				if (parent == null)
					return;

				var parentPath = parent.GetFullPath();
				var newDirName = GenerateNewFolderName(parentPath);
				var newPath = Path.Combine(parentPath, newDirName);

				Directory.CreateDirectory(newPath);

				var childs = parent.Children;
				var newChild = new TreeItem(newDirName, parent);
				childs.Add(newChild);
				parent.Children = childs.OrderBy(c => c.Name).ToObservableCollection();
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Can't create new folder. Error: {0}", ex.Message));
			}
		}

		private string GenerateNewFolderName(string parentPath)
		{
			string result = NewFolderName;

			if (Directory.Exists(Path.Combine(parentPath, result)))
			{
				for (int i = 1; i < MaxNewFolderSuffix; ++i)
				{
					var nameWithIndex = String.Format(NewFolderName + " {0}", i);

					if (!Directory.Exists(Path.Combine(parentPath, nameWithIndex)))
					{
						result = nameWithIndex;
						break;
					}
				}
			}

			return result;
		}

		private void CreateMenuItem_Click(object sender, RoutedEventArgs e)
		{
			var item = sender as MenuItem;
			if (item != null)
			{
				var context = item.DataContext as TreeItem;
				CreateNewFolderImpl(context);
			}
		}

		private void RenameMenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var item = sender as MenuItem;
				if (item != null)
				{
					var context = item.DataContext as TreeItem;
					if (context != null && !(context is DriveTreeItem))
					{
						var dialog = new InputDialog()
						{
							Message = "New folder name:",
							InputText = context.Name,
							Title = String.Format("Do you really want to rename folder {0}?", context.Name)
						};

						if (dialog.ShowDialog() == true)
						{
							var newFolderName = dialog.InputText;

							/*
							 * Parent for context is always not null due to the fact
							 * that we don't allow to change the name of DriveTreeItem
							 */
							var newFolderFullPath = Path.Combine(context.Parent.GetFullPath(), newFolderName);
							if (Directory.Exists(newFolderFullPath))
							{
								MessageBox.Show(String.Format("Directory already exists: {0}", newFolderFullPath));
							}
							else
							{
								Directory.Move(context.GetFullPath(), newFolderFullPath);
								context.Name = newFolderName;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Can't rename folder. Error: {0}", ex.Message));
			}
		}

		private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var item = sender as MenuItem;
				if (item != null)
				{
					var context = item.DataContext as TreeItem;
					if (context != null && !(context is DriveTreeItem))
					{
						var confirmed =
								MessageBox.Show(
										String.Format("Do you really want to delete folder {0}?", context.Name),
										"Confirm folder removal",
										MessageBoxButton.YesNo);

						if (confirmed == MessageBoxResult.Yes)
						{
							Directory.Delete(context.GetFullPath());
							var parent = context.Parent;
							parent.Children.Remove(context);
						}
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("Can't delete folder. Error: {0}", ex.Message));
			}
		}

		private void UpdateCurrentPath()
		{

		}

		#endregion

		#region · Commands ·

		/// <summary>
		/// Gets CurrentPathChanged command
		/// </summary>
		public ICommand CurrentPathChangedCommand
		{
			get { return new FolderBrowserControlCommand(FolderBrowserControlCommand.CommandType.UpdateCurrentPath, this); }
		}

		/// <summary>
		/// Execute command
		/// </summary>
		/// <param name="in_command"></param>
		/// <param name="in_parameter"></param>
		internal void ExecuteCommand(FolderBrowserControlCommand in_command, object in_parameter)
		{
			switch (in_command.Type)
			{
				case FolderBrowserControlCommand.CommandType.UpdateCurrentPath:
					{
						string root = Path.GetPathRoot(CurrentPathEdit);

						if (!string.IsNullOrEmpty(root))
						{

						}
					}
					break;
			}
		}
		#endregion

		#region Private fields

		private TreeItem root;
		private TreeItem selectedItem;
		private string initialPath;
		private Style itemContainerStyle;

		#endregion
	}

	public class NullToBoolConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return false;

			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	public static class LinqExtensions
	{
		public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
		{
			var result = new ObservableCollection<T>();

			foreach (var ci in source)
			{
				result.Add(ci);
			}

			return result;
		}
	}
}
