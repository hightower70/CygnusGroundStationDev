using System.Windows;
using System.Windows.Interop;

namespace CygnusControls
{
    /// <summary>
    /// Interaction logic for FolderPickerDialog.xaml
    /// </summary>
    public partial class FolderBrowserDialog : Window
    {
        #region Dependency properties

        public static readonly DependencyProperty ItemContainerStyleProperty =
            DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(FolderBrowserDialog));

        public Style ItemContainerStyle
        {
            get
            {
                return (Style)GetValue(ItemContainerStyleProperty);
            }
            set
            {
                SetValue(ItemContainerStyleProperty, value);
            }
        }

        private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as FolderBrowserDialog;
            if (control != null)
            {
                control.ItemContainerStyle = e.NewValue as Style;
            }
        }

        #endregion

        public string SelectedPath { get; private set; }

        public string InitialPath
        {
            get
            {
							return fbcBrowser.InitialPath;
            }
            set
            {
							fbcBrowser.InitialPath = value;
            }
        }

        public FolderBrowserDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
					SelectedPath = fbcBrowser.SelectedPath;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ComponentDispatcher.IsThreadModal)
            {
                DialogResult = false;
            }
            else
            {
                Close();
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
					fbcBrowser.CreateNewFolder();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
					fbcBrowser.RefreshTree();
        }
    }
}
