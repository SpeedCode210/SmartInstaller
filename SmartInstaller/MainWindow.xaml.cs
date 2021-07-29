
using System;
using System.ComponentModel;
using System.Reflection;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.IO.Compression;
using System.Threading.Tasks;
using Newtonsoft.Json;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace SmartInstaller
{

    public partial class MainWindow : Window
    {
        public string ApplicationName;
        public string ProgressMessage;
        public string ApplicationUrl;
        private string TempDir;
        private Button btn;
        public int Progress;
        private string a = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        private Foo f;

        public MainWindow()
        {
            InitializeComponent();
            InitializeInstaller("{AppName}", "{PackageUrl}", "{ImageUrl}");
        }

        private void InitializeInstaller(string AppName, string AppUrl, string ImageUrl)
        {
            ApplicationName = AppName;
            title.Content = ApplicationName;
            Progress = 0;
            ProgressMessage = "Appuyez sur \"Installer\"";
            txt.Content = ProgressMessage;
            ApplicationUrl = AppUrl;
            Logo.Source = new BitmapImage(new Uri(ImageUrl));
        }

        private static Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(MainWindow).GetTypeInfo().Assembly;

            var stream = assembly.GetManifestResourceStream("SmartInstaller." + filename);

            return stream;
        }

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
            Application.Current.Shutdown();
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            btn = (Button)sender;
            btn.Content = "Annuler";
            btn.Click -= btnDownload_Click;
            btn.Click += Button_Click;
            string result = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            Directory.CreateDirectory(result + "\\TempSmartInstaller");
            TempDir = result + "\\TempSmartInstaller\\";

            ProgressMessage = "Téléchargement";
            txt.Content = ProgressMessage;
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
            webClient.DownloadFileAsync(new Uri(ApplicationUrl), TempDir + "arch.zip");
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            pb.Value = Progress;
            ProgressMessage = "Téléchargement "+e.ProgressPercentage+"%";
            txt.Content = ProgressMessage;
        }

        private async void Completed(object sender, AsyncCompletedEventArgs e)
        {
            await Task.Delay(500);
            bool b = true;
            ProgressMessage = "Extraction";
            txt.Content = ProgressMessage;
            await Task.Delay(100);
            try
            {
                ZipFile.ExtractToDirectory(TempDir + "arch.zip", TempDir);

            }
            catch(FileNotFoundException)
            {
                b = false;
            }
            catch(InvalidDataException)
            {
                b = false;
            }
            if(b)
            {
                string js = System.IO.File.ReadAllText(TempDir + "package.json");
                f = JsonConvert.DeserializeObject<Foo>(js);
                Progress = 150;
                pb.Value = Progress;
                ProgressMessage = "Installation";
                txt.Content = ProgressMessage;
                await Task.Delay(100);



                Directory.CreateDirectory(a + "\\SmartInstaller");
                DirectoryInfo dir = new DirectoryInfo(a + "\\SmartInstaller\\" + f.Name);

                if(dir.Exists)
                {

                    foreach(FileInfo file in dir.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach(DirectoryInfo dirt in dir.GetDirectories())
                    {
                        dirt.Delete(true);
                    }
                }
                DirectoryCopy(TempDir + "bin", a + "\\SmartInstaller\\" + f.Name, true);
                var remover = GetStreamFromFile("Remove.exe");
                SaveStreamAsFile(a + "\\SmartInstaller\\" + f.Name + "\\" + "Remove.exe", remover);

                System.IO.File.Move(TempDir + "package.json", a + "\\SmartInstaller\\" + f.Name + "\\package.json");
                System.IO.DirectoryInfo di = new DirectoryInfo(TempDir);

                foreach(FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach(DirectoryInfo dirt in di.GetDirectories())
                {
                    dirt.Delete(true);
                }
                Directory.Delete(TempDir);
                CreateShortcut(f.Name, Environment.GetFolderPath(Environment.SpecialFolder.Desktop), a + "\\SmartInstaller\\" + f.Name + "\\" + f.MainExe);
                CreateShortcut(f.Name, Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), a + "\\SmartInstaller\\" + f.Name + "\\" + f.MainExe);
                CreateShortcut(f.Name + " Uninstaller", Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), a + "\\SmartInstaller\\" + f.Name + "\\" + "Remove.exe");
                UninstallRegistery();
                Progress = 200;
                pb.Value = Progress;
                ProgressMessage = "Installation terminée";
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
                ProgressMessage = "Pas d'internet, veuillez réessayer plus tard";
                txt.Content = ProgressMessage;
            }
        }

        private void UninstallRegistery()
        {
            RegistryKey UninstallKey = Registry.LocalMachine.OpenSubKey("SOFTWARE", true).OpenSubKey("Microsoft", true)
                .OpenSubKey("Windows", true).OpenSubKey("CurrentVersion", true).OpenSubKey("Uninstall", true)
                .CreateSubKey(ApplicationName, true);
            UninstallKey.SetValue("DisplayIcon", a + "\\SmartInstaller\\" + f.Name + "\\" + f.MainExe, RegistryValueKind.String);
            UninstallKey.SetValue("DisplayName", ApplicationName, RegistryValueKind.String);
            UninstallKey.SetValue("DisplayVersion", f.VersionName, RegistryValueKind.String);
            UninstallKey.SetValue("Publisher", "Eclipium", RegistryValueKind.String);
            UninstallKey.SetValue("UninstallPath", a + "\\SmartInstaller\\" + f.Name + "\\" + "Remove.exe", RegistryValueKind.String);
            UninstallKey.SetValue("UninstallString", a + "\\SmartInstaller\\" + f.Name + "\\" + "Remove.exe", RegistryValueKind.String);
            UninstallKey.SetValue("InstallLocation", a + "\\SmartInstaller\\" + f.Name, RegistryValueKind.String);
            UninstallKey.SetValue("NoModify", 1, RegistryValueKind.DWord);
            UninstallKey.SetValue("NoRepair", 1, RegistryValueKind.DWord);
        }

        public static void SaveStreamAsFile(string filePath, Stream inputStream)
        {

            string path = filePath;
            using(FileStream outputFileStream = new FileStream(path, FileMode.Create))
            {
                inputStream.CopyTo(outputFileStream);
            }
        }




        public static void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcut.TargetPath = targetFileLocation;
            shortcut.Save();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if(!dir.Exists)
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
            foreach(FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if(copySubDirs)
            {
                foreach(DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }

    class Foo
    {
        public string Name { get; set; }
        public string MainExe { get; set; }
        public string VersionName { get; set; }
        public int VersionCode { get; set; }
        public string Date { get; set; }
    }


}
