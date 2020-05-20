using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Elite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!App.IsShuttingDown)
            {
                Properties.Settings.Default.Visible = false;
                Properties.Settings.Default.Save();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
