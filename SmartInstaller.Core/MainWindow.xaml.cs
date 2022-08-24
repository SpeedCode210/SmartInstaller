
using System;
using System.Reflection;
using System.IO;
using System.Windows;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Text.Json;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Media;
using File = System.IO.File;
using System.Globalization;

namespace SmartInstaller
{

    public partial class MainWindow : Window
    {
        public string ApplicationName;
        private string TempDir;
        public int Progress;
        private ProgramData _programData;
        private static string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        //Change this line to change default installation path
        public string InstallationPath = ProgramFiles;

        //Change this line to change installer theme
        public Theme Theme = Theme.Auto;

        //Change this line to change if the installer adds the program to PATH by default
        public bool AddToPath = false;

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
            if (CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ar")
                FlowDirection = FlowDirection.RightToLeft;
            InitializeInstaller();
            InitTheme();
        }

        //Function that initialize the installer
        private void InitializeInstaller()
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var resourceName = assembly.GetName().Name + ".package.json";
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
            txt.Content = GetString("click-on-install");

            //check if program is installed
            if(File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\" + ApplicationName + ".lnk"))
            {
                btnDownload.Content = GetString("update");
                txt.Content = GetString("click-on-update");
            }
        }

        //function that gets windows'default theme (Light theme for windows 8.1 and older)
        private void InitTheme()
        {
            bool AppsUseLightTheme = Theme is not Theme.Dark;
            
            if(Theme is Theme.Auto)
            {
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
            }

            this.AccentColorBrush = GetAccentColor(AppsUseLightTheme);

            if (AppsUseLightTheme)
            {
              
                this.BackgroundColorBrush = new SolidColorBrush(Color.FromRgb(249, 249, 249));
                this.ButtonBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(249, 249, 249));
                this.SeparatorColorBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229));
                this.SecondBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(238, 238, 238));
                this.ForegroundColorBrush = new SolidColorBrush(Color.FromRgb(16, 16, 16));
                this.ButtonContrastColorBrush = Brushes.White;
            }
            else
            {
                this.BackgroundColorBrush = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                this.ButtonBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(52, 52, 52));
                this.SeparatorColorBrush = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                this.SecondBackgroundColorBrush = new SolidColorBrush(Color.FromRgb(39, 39, 39));
                this.ForegroundColorBrush = new SolidColorBrush(Color.FromRgb(250, 250, 250));
                this.ButtonContrastColorBrush = Brushes.Black;

            }

            this.DataContext = this;
        }

        private static Brush GetAccentColor(bool light)
        {
            System.Drawing.Color systemAccent = System.Drawing.Color.FromArgb(255, SystemParameters.WindowGlassColor.R, SystemParameters.WindowGlassColor.G, SystemParameters.WindowGlassColor.B);

            if (light)
            {
                if (systemAccent.GetBrightness() <= 0.5)
                    return SystemParameters.WindowGlassBrush;
                else
                {
                    var color = new HslColor(SystemParameters.WindowGlassColor);
                    var color2 = new HslColor(color.h, color.s, 0.45f, color.a);
                    return new SolidColorBrush(color2.ToRgb());
                }
            }
            else
            {
                if (systemAccent.GetBrightness() >= 0.5)
                    return SystemParameters.WindowGlassBrush;
                else
                {
                    var color = new HslColor(SystemParameters.WindowGlassColor);
                    var color2 = new HslColor(color.h, color.s, 0.65f, color.a);
                    return new SolidColorBrush(color2.ToRgb());
                }
            }
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
            if (autoStart && (string)btnDownload.Content == GetString("quit"))
            {
                Process p = new Process();
                p.StartInfo.FileName = InstallationPath + "\\" + ApplicationName + "\\" + _programData.MainExe;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();
            }
            Application.Current.Shutdown();
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            btnDownload.Content = GetString("cancel");
            btnDownload.Click -= btnDownload_Click;
            btnDownload.Click += Button_Click;
            //Creating temp directory for the download
            string result = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            Directory.CreateDirectory(result + "\\TempSmartInstaller");
            TempDir = result + "\\TempSmartInstaller\\";
            txt.Content = GetString("preparing");
            await Task.Delay(100);
            //Start extracting
            //Copy file from resource
            File.WriteAllBytes(TempDir + "arch.zip", ReadResource("package.zip"));
            Progress = 100;
            pb.Value = Progress;
            Completed();
        }


        private async void Completed()
        {
            //Extracting the archive
            await Task.Delay(100);
            bool b = true;
            txt.Content = GetString("extracting");
            await Task.Delay(100);
            try
            {
#if NETCOREAPP
                ZipFile.ExtractToDirectory(TempDir + "arch.zip", TempDir, true);
#else
                ZipFile.ExtractToDirectory(TempDir + "arch.zip", TempDir);
#endif

            }
            catch (FileNotFoundException)
            {
                b = false;
            }
            catch (InvalidDataException)
            {
                b = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            if (b)
            {
                string js = File.ReadAllText(TempDir + "package.json");
                _programData = JsonSerializer.Deserialize<ProgramData>(js)!;
                Progress = 150;
                pb.Value = Progress;
                txt.Content = GetString("installing");
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
                var remover = ReadEmbeddedResource("Remove.exe");
                SaveStreamAsFile(InstallationPath + "\\" + ApplicationName + "\\" + "Remove.exe", remover);

                File.Move(TempDir + "package.json", InstallationPath + "\\" + ApplicationName + "\\package.json");
                DirectoryInfo di = new DirectoryInfo(TempDir);

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
                //Adding to PATH
                if (AddToPath)
                {
                    var pathToAdd = InstallationPath + "\\" + ApplicationName + "\\";
                    var name = "PATH";
                    var scope = EnvironmentVariableTarget.Machine;
                    var oldValue = Environment.GetEnvironmentVariable(name, scope)!;
                    if (!oldValue.Contains(pathToAdd))
                    {
                        Debug.WriteLine("ADDING TO PATH : " + oldValue);
                        var newValue = oldValue + ";" + pathToAdd;
                        Environment.SetEnvironmentVariable(name, newValue, scope);
                    }
                }
                //Creating shortcuts
                if (desktopShortcut) CreateShortcut(ApplicationName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), InstallationPath + "\\" + ApplicationName + "\\" + _programData.MainExe);
                CreateShortcut(ApplicationName, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), InstallationPath + "\\" + ApplicationName + "\\" + _programData.MainExe);
                CreateShortcut(ApplicationName + " Uninstaller", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), InstallationPath + "\\" + ApplicationName + "\\" + "Remove.exe");
                UninstallRegistery();
                Progress = 200;
                pb.Value = Progress;
                txt.Content = GetString("finished");
                btnDownload.Content = GetString("quit");
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
                btnDownload.Content = GetString("install");
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\" + ApplicationName + ".lnk"))
                {
                    btnDownload.Content = GetString("update");
                }
                btnDownload.Click -= Button_Click;
                btnDownload.Click += btnDownload_Click;
            }
        }

        static Stream ReadEmbeddedResource(string res)
        {
            var asm = Assembly.GetEntryAssembly();
            var stream = asm.GetManifestResourceStream(asm.GetName().Name + "." + res.Replace('/', '.'))!;
            return stream;
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


        private static byte[] ReadResource(string res)
        {
            using (Stream resFilestream = ReadEmbeddedResource(res))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                var byteArray = ba;
                return byteArray;
            }
        }

        private static string GetString(string key)
        {
            object resource = Application.Current.TryFindResource(key);
            string? settingsString = resource as string;
            if (settingsString != null)
            {
                return settingsString;
            }
            throw new FileNotFoundException($"Resource {key} not found");
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
