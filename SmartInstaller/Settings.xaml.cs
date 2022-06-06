using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SmartInstaller
{
    public partial class SettingsWindow : Window
    {
        private MainWindow ParentWindow;

        //Brushes that changes with Light or Dark theme
        public Brush BackgroundColorBrush { get; private set; }
        public Brush ButtonBackgroundColorBrush { get; private set; }
        public Brush SecondBackgroundColorBrush { get; private set; }
        public Brush AccentColorBrush { get; private set; }
        public Brush SeparatorColorBrush { get; private set; }
        public Brush ForegroundColorBrush { get; private set; }
        public Brush ButtonContrastColorBrush { get; private set; }

        public SettingsWindow(MainWindow parent)
        {
            InitializeComponent();
            this.ParentWindow = parent;
            desktopShortcut.IsChecked = parent.desktopShortcut;
            autoStart.IsChecked = parent.autoStart;
            path.Text = parent.InstallationPath;
            InitTheme();
        }

        //function that gets windows'default theme from parent
        private void InitTheme()
        {
            this.BackgroundColorBrush = ParentWindow.BackgroundColorBrush;
            this.ButtonBackgroundColorBrush = ParentWindow.ButtonBackgroundColorBrush;
            this.SeparatorColorBrush = ParentWindow.SeparatorColorBrush;
            this.SecondBackgroundColorBrush = ParentWindow.SecondBackgroundColorBrush;
            this.ForegroundColorBrush = ParentWindow.ForegroundColorBrush;
            this.ButtonContrastColorBrush = ParentWindow.ButtonContrastColorBrush;
            this.AccentColorBrush = ParentWindow.AccentColorBrush;

            this.DataContext = this;
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

        private void shortcutbutton_Click(object sender, RoutedEventArgs e)
        {
            desktopShortcut.IsChecked = !desktopShortcut.IsChecked;
        }

        private void autostartbutton_Click(object sender, RoutedEventArgs e)
        {
            autoStart.IsChecked = !autoStart.IsChecked;
        }
    }
}
