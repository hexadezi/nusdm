using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace nusdm
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		#region Private Fields

		private readonly MainWindowViewModel mainWindowViewModel;

		#endregion Private Fields

		#region Public Constructors

		public MainWindow()
		{
			InitializeComponent();

			mainWindowViewModel = new MainWindowViewModel();

			DataContext = mainWindowViewModel;
		}

		#endregion Public Constructors

		#region Public Methods

		// https://stackoverflow.com/a/1080012/4859698
		public static DependencyObject GetScrollViewer(DependencyObject o)
		{
			if (o is ScrollViewer)
			{ return o; }

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
			{
				var child = VisualTreeHelper.GetChild(o, i);

				var result = GetScrollViewer(child);
				if (result == null)
				{
					continue;
				}
				else
				{
					return result;
				}
			}

			return null;
		}

		#endregion Public Methods

		#region Private Methods

		private void FocusFirstInListBox()
		{
			ScrollViewer scrollViewer = GetScrollViewer(lbxTitles) as ScrollViewer;
			scrollViewer.ScrollToTop();

			if (lbxTitles.Items.Count > 0)
			{
				lbxTitles.SelectedIndex = 0;
				lbxTitles.Focus();
				var listBoxItem = (ListBoxItem)lbxTitles.ItemContainerGenerator.ContainerFromItem(lbxTitles.SelectedItem);
				listBoxItem.Focus();
			}
		}

		private void ListBoxItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			e.Handled = true;
		}

		private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			sv.ScrollToBottom();
		}

		private void TxtFilter_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Down)
			{
				FocusFirstInListBox();
				e.Handled = true;
			}
		}

		private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (Key.Escape == e.Key)
			{
				Close();
			}
			else if (Keyboard.Modifiers != ModifierKeys.Control)
			{
				// Allow alphanumeric and space.
				if (e.Key >= Key.D0 && e.Key <= Key.D9 ||
					e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 ||
					e.Key >= Key.A && e.Key <= Key.Z ||
					e.Key == Key.Space ||
					e.Key == Key.Back)
				{
					txtFilter.Focus();
					e.Handled = false;
				}
			}
		}

		#endregion Private Methods
	}
}