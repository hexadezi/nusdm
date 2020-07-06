using System.Windows;
using System.Windows.Controls;

namespace nusdm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel mainWindowViewModel;
        public MainWindow()
        {
            InitializeComponent();

            mainWindowViewModel = new MainWindowViewModel();

            DataContext = mainWindowViewModel;
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private void ProjectListView_OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
