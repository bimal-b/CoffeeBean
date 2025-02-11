using CoffeeBean.Utils;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CoffeeBean
{
    // https://stackoverflow.com/a/19326/7585517
    // https://github.com/microsoft/WPF-Samples/tree/master/Application%20Management/SingleInstanceDetection
    internal sealed class SingleInstanceManager : WindowsFormsApplicationBase
    {
        private ScreenLockPreventer screenLockPreventer;
        private NotifyIcon trayIcon;

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs e)
        {
            screenLockPreventer = new ScreenLockPreventer();
            trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(System.Windows.Application.GetResourceStream(new Uri("/Icon/icon.ico", UriKind.Relative)).Stream),
                Visible = true,
                Text = "CoffeeBean",
                ContextMenuStrip = new ContextMenu(screenLockPreventer)
            };

            // When application starts, the screen lock preventing functionality should be enabled by default, unless there are command line arguments saying otherwise.
            screenLockPreventer.Enabled = true;

            var commandLineParametersAreCorrect = ProcessCommandLineArguments(e.CommandLine);

            if (commandLineParametersAreCorrect)
            {
                // Start the event loop
                new System.Windows.Application().Run();
            }

            // On shutdown
            trayIcon.Dispose();

            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs e)
        {
            _ = ProcessCommandLineArguments(e.CommandLine);

            e.BringToForeground = false;
        }

        private bool ProcessCommandLineArguments(IReadOnlyList<string> args)
        {
            try
            {
                var parsedArgs = CommandLineParser.Parse(args);
                if (parsedArgs.IsScreenLockEnabled != null)
                {
                    screenLockPreventer.Enabled = (bool)parsedArgs.IsScreenLockEnabled;
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorDialog.Show($"{ex.Message}.\nUsage:\nCoffeeBean.exe [enable|disable]");
                return false;
            }
        }
    }

    internal static class EntryPoint
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }
}
