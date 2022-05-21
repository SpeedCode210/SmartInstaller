
using System;
using System.ComponentModel;
using System.Reflection;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Text.Json;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Media;
using File = System.IO.File;

namespace SmartInstaller
{

    public partial class MainWindow : Window
    {
        public string ApplicationName;
        public string ProgressMessage;
        private string TempDir;
        private Button btn;
        public int Progress;
        private string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        public string InstallationPath;
        private ProgramData _programData;

        //Brushes that changes with Light or Dark theme
        public Brush BackgroundColorBrush { get; private set; }
        public Brush ButtonBackgroundColorBrush { get; private set; }
        public Brush SecondBackgroundColorBrush { get; private set; }
        public Brush AccentColorBrush { get; private set; }
        public Brush SeparatorColorBrush { get; private set; }
        public Brush ForegroundColorBrush { get; private set; }
        public Brush ButtonContrastColorBrush { get; private set; }

        public bool autoStart = false;
        public bool desktopShortcut = true;

        public MainWindow()
        {
            InitializeComponent();
            InitializeInstaller();
            InitTheme();
        }

        //Function that initialize the installer
        private void InitializeInstaller()
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var resourceName = "SmartInstaller.package.json";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)!)
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonFile = reader.ReadToEnd();
                _programData = JsonSerializer.Deserialize<ProgramData>(jsonFile)!;
            }

            ApplicationName = _programData.Name;
            title.Content = ApplicationName;
            version.Content = "Version " + _programData.VersionName;
            Progress = 0;
            ProgressMessage = "Click on \"Install\"";
            txt.Content = ProgressMessage;
            InstallationPath = ProgramFiles;
        }

        //function that gets windows'default theme (Light theme for windows 8.1 and older)
        private void InitTheme()
        {
            bool AppsUseLightTheme = true;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null && key.GetValue("AppsUseLightTheme") != null)
                    {
                        Int64 value = Convert.ToInt64(key.GetValue("AppsUseLightTheme").ToString());
                        if (value == 0)
                            AppsUseLightTheme = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }

            if (AppsUseLightTheme)
            {
                this.BackgroundColorBrush = new SolidColorBrush(Color.FromRgb(249, 249, 249));
                this.ButtonBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(249, 249, 249));
                this.SeparatorColorBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229));
                this.SecondBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(238, 238, 238));
                this.ForegroundColorBrush = new SolidColorBrush(Color.FromRgb(16, 16, 16));
                this.AccentColorBrush = new SolidColorBrush(Color.FromRgb(0, 95, 184));
                this.ButtonContrastColorBrush = Brushes.White;
            }
            else
            {
                this.BackgroundColorBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                this.ButtonBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(52, 52, 52));
                this.SeparatorColorBrush = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                this.SecondBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(39, 39, 39));
                this.ForegroundColorBrush = new SolidColorBrush(Color.FromRgb(250, 250, 250));
                this.AccentColorBrush = new SolidColorBrush(Color.FromRgb(96, 205, 255));
                this.ButtonContrastColorBrush = Brushes.Black;

            }

            this.DataContext = this;
        }

        //Click event for canceling the installer
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.IO.File.Delete(TempDir + "arch.zip");
            }
            catch { }
            try
            {
                Directory.Delete(TempDir);
            }
            catch { }
            if (autoStart && (string)btn.Content == "Quit")
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = InstallationPath + "\\" + ApplicationName + "\\" + _programData.MainExe;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
            }
            Application.Current.Shutdown();
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            btn = (Button)sender;
            btn.Content = "Cancel";
            btn.Click -= btnDownload_Click;
            btn.Click += Button_Click;
            //Creating temp directory for the download
            string result = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            Directory.CreateDirectory(result + "\\TempSmartInstaller");
            TempDir = result + "\\TempSmartInstaller\\";
            ProgressMessage = "Preparing";
            txt.Content = ProgressMessage;
            await Task.Delay(100);
            //Start extracting
            //Copy file from resource
            await File.WriteAllBytesAsync(TempDir + "arch.zip", ReadResource("package.zip"));
            Progress = 100;
            pb.Value = Progress;
            Completed();
        }


        private async void Completed()
        {
            //Extracting the archive
            await Task.Delay(100);
            bool b = true;
            ProgressMessage = "Extracting";
            txt.Content = ProgressMessage;
            await Task.Delay(100);
            try
            {
                ZipFile.ExtractToDirectory(TempDir + "arch.zip", TempDir);

            }
            catch (FileNotFoundException)
            {
                b = false;
            }
            catch (InvalidDataException)
            {
                b = false;
            }
            if (b)
            {
                string js = System.IO.File.ReadAllText(TempDir + "package.json");
                _programData = JsonSerializer.Deserialize<ProgramData>(js);
                Progress = 150;
                pb.Value = Progress;
                ProgressMessage = "Installing...";
                txt.Content = ProgressMessage;
                await Task.Delay(100);


                //Installing the app and creating shortcuts and registery keys
                Directory.CreateDirectory(InstallationPath + "\\" + ApplicationName);
                DirectoryInfo dir = new DirectoryInfo(InstallationPath + "\\" + ApplicationName);

                if (dir.Exists)
                {

                    foreach (FileInfo file in dir.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dirt in dir.GetDirectories())
                    {
                        dirt.Delete(true);
                    }
                }
                DirectoryCopy(TempDir + "bin", InstallationPath + "\\" + ApplicationName, true);
                var remover = GetStreamFromFile("Remove.exe");
                SaveStreamAsFile(InstallationPath + "\\" + ApplicationName + "\\" + "Remove.exe", remover);

                System.IO.File.Move(TempDir + "package.json", InstallationPath + "\\" + ApplicationName + "\\package.json");
                System.IO.DirectoryInfo di = new DirectoryInfo(TempDir);

                //Deleting temp files
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dirt in di.GetDirectories())
                {
                    dirt.Delete(true);
                }
                Directory.Delete(TempDir);
                //Creating shortcuts
                if (desktopShortcut) CreateShortcut(ApplicationName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), InstallationPath + "\\" + ApplicationName + "\\" + _programData.MainExe);
                CreateShortcut(ApplicationName, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), InstallationPath + "\\" + ApplicationName + "\\" + _programData.MainExe);
                CreateShortcut(ApplicationName + " Uninstaller", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), InstallationPath + "\\" + ApplicationName + "\\" + "Remove.exe");
                UninstallRegistery();
                Progress = 200;
                pb.Value = Progress;
                ProgressMessage = "Finished installing";
                txt.Content = ProgressMessage;
                btn.Content = "Quitter";
            }
            else
            {
                try
                {
                    System.IO.File.Delete(TempDir + "arch.zip");

                }
                catch { }
                try
                { Directory.Delete(TempDir); }
                catch { }
                Progress = 0;
                pb.Value = Progress;
                btn.Content = "Install";
                btn.Click -= Button_Click;
                btn.Click += btnDownload_Click;
            }
        }


        //function that creates registery keys for the uninstaller
        private void UninstallRegistery()
        {
            RegistryKey UninstallKey = Registry.LocalMachine.OpenSubKey("SOFTWARE", true).OpenSubKey("Microsoft", true)
                .OpenSubKey("Windows", true).OpenSubKey("CurrentVersion", true).OpenSubKey("Uninstall", true)
                .CreateSubKey(ApplicationName, true);
            UninstallKey.SetValue("DisplayIcon", InstallationPath + "\\" + ApplicationName + "\\" + _programData.MainExe, RegistryValueKind.String);
            UninstallKey.SetValue("DisplayName", ApplicationName, RegistryValueKind.String);
            UninstallKey.SetValue("DisplayVersion", _programData.VersionName, RegistryValueKind.String);
            UninstallKey.SetValue("UninstallPath", InstallationPath + "\\" + ApplicationName + "\\" + "Remove.exe", RegistryValueKind.String);
            UninstallKey.SetValue("UninstallString", InstallationPath + "\\" + ApplicationName + "\\" + "Remove.exe", RegistryValueKind.String);
            UninstallKey.SetValue("InstallLocation", InstallationPath + "\\" + ApplicationName, RegistryValueKind.String);
            UninstallKey.SetValue("NoModify", 1, RegistryValueKind.DWord);
            UninstallKey.SetValue("NoRepair", 1, RegistryValueKind.DWord);
        }

        public static void SaveStreamAsFile(string filePath, Stream inputStream)
        {

            string path = filePath;
            using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
            {
                inputStream.CopyTo(outputFileStream);
            }
        }




        public static void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            string shortcutLocation = Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcut.TargetPath = targetFileLocation;
            shortcut.Save();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        //Click event for settings button
        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(this);
            settings.Show();
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }



        private static Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(MainWindow).GetTypeInfo().Assembly;

            var stream = assembly.GetManifestResourceStream("SmartInstaller." + filename);

            return stream;
        }

        private static byte[] ReadResource(string resourceName)
        {
            System.Reflection.Assembly a = typeof(MainWindow).GetTypeInfo().Assembly;
            ;
            string fileName = a.GetName().Name + "." + resourceName;

            using (Stream resFilestream = a.GetManifestResourceStream(fileName))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                var byteArray = ba;
                return byteArray;
            }

        }
    }

    //Class of package.json
    class ProgramData
    {
        public string Name { get; set; }
        public string MainExe { get; set; }
        public string VersionName { get; set; }
        public int VersionCode { get; set; }
        public string Date { get; set; }
    }


}
