using System;

namespace SmartInstaller.NetCore
{
    public class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
