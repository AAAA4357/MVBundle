using HandyControl.Controls;
using MVPlot.Managers;
using MVPlot.Utilities;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace MVPlot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length == 0) return;
            string path = e.Args[0];
            FileInfo info = new(path);
            PlotProjectUtility.SetPlotProjectFolderPath(info.DirectoryName!);
            PlotProjectUtility.SetPlotProjectName(info.Name);
            PlotProjectManager.OpenPlotProject();
        }
    }

}
