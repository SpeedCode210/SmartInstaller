using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmartInstaller
{
    public partial class SettingsWindow : Window
    {
        private MainWindow ParentWindow;
        public SettingsWindow(MainWindow parent)
        {
            InitializeComponent();
            this.ParentWindow = parent;
            desktopShortcut.IsChecked = parent.desktopShortcut;
            autoStart.IsChecked = parent.autoStart;
            path.Text = parent.InstallationPath;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }


        private void pathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if ((bool)dialog.ShowDialog())
            {
                path.Text = dialog.SelectedPath;
            }
        }

        private void validate_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.desktopShortcut = (bool)desktopShortcut.IsChecked;
            ParentWindow.autoStart = (bool)autoStart.IsChecked;
            ParentWindow.InstallationPath = path.Text;
            this.Close();
        }
    }
}
