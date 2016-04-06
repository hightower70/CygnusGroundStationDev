using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CygnusGroundStation.Dialogs
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class ExceptionDialog : Window
	{
		string m_exception_string = "";

		public ExceptionDialog(DispatcherUnhandledExceptionEventArgs e)
		{
			Exception exception;
#if DEBUG
			System.Diagnostics.Debugger.Break();
#endif

			InitializeComponent();

			TreeViewItem treeViewItem = new TreeViewItem();
			treeViewItem.Header = "Exception";
			treeViewItem.ExpandSubtree();

			if (e.Exception.InnerException == null)
				exception = e.Exception;
			else
				exception = e.Exception.InnerException;

			buildTreeLayer(exception, treeViewItem);
			treeView1.Items.Add(treeViewItem);

			textBox1.Text = m_exception_string;
		}

		void buildTreeLayer(Exception e, TreeViewItem parent)
		{
			String exceptionInformation = e.GetType().ToString() + "\n\n";
			parent.DisplayMemberPath = "Header";
			parent.Items.Add(new TreeViewStringSet() { Header = "Type", Content = e.GetType().ToString() });
			System.Reflection.PropertyInfo[] memberList = e.GetType().GetProperties();
			foreach (PropertyInfo info in memberList)
			{
				var value = info.GetValue(e, null);
				if (value != null)
				{
					if (info.Name == "InnerException")
					{
						TreeViewItem treeViewItem = new TreeViewItem();
						treeViewItem.Header = info.Name;
						buildTreeLayer(e.InnerException, treeViewItem);
						parent.Items.Add(treeViewItem);
					}
					else
					{
						TreeViewStringSet treeViewStringSet = new TreeViewStringSet() { Header = info.Name, Content = value.ToString() };
						parent.Items.Add(treeViewStringSet);
						exceptionInformation += treeViewStringSet.Header + "\n" + treeViewStringSet.Content + "\n\n";
					}
				}
			}

			m_exception_string = exceptionInformation;
		}


		private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (e.NewValue.GetType() == typeof(TreeViewItem)) 
				textBox1.Text = m_exception_string;
			else 
				textBox1.Text = e.NewValue.ToString();
		}

		private class TreeViewStringSet
		{
			public string Header { get; set; }
			public string Content { get; set; }

			public override string ToString()
			{
				return Content;
			}
		}

		private void buttonClipboard_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(m_exception_string);
		}

		private void buttonExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			if (App.Current.MainWindow == null || !App.Current.MainWindow.IsVisible)
				Application.Current.Shutdown();
		}
 	}
}
