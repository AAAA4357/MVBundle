using Google.Protobuf;
using MVPlot.Proto;
using MVPlot.Utilities;
using MVPlot.Windows;
using System.IO;

namespace MVPlot.Managers
{
    /// <summary>
    /// 剧本项目管理器
    /// </summary>
    public static class PlotProjectManager
    {
        /// <summary>
        /// 剧本项目
        /// </summary>
        public static PlotProject? CurrentProject { get; private set; }

        /// <summary>
        /// 剧本文件流
        /// </summary>
        static FileStream? ProjectFileStream;

        /// <summary>
        /// 打开剧本项目
        /// </summary>
        public static void OpenPlotProject()
        {
            FileStream stream = File.Open(PlotProjectUtility.PlotProjectMainFilePath, FileMode.Open);
            PlotProject project = PlotProject.Parser.ParseFrom(stream);
            CurrentProject = project;
            ProjectFileStream = stream;
            WindowManager.Hide(nameof(WelcomeWindow));
            MainEditWindow window = new();
            window.ShowDialog();
        }

        /// <summary>
        /// 关闭剧本项目
        /// </summary>
        public static void ClosePlotProject()
        {
            CurrentProject = null;
            ProjectFileStream?.Close();
            ProjectFileStream?.Dispose();
            ProjectFileStream = null;
            WindowManager.Close(nameof(MainEditWindow));
            WindowManager.Show(nameof(WelcomeWindow));
        }

        /// <summary>
        /// 保存剧本项目
        /// </summary>
        public static void SavePlotProject()
        {
            ProjectFileStream!.Seek(0, SeekOrigin.Begin);
            ProjectFileStream!.SetLength(0);
            Stream stream = ProjectFileStream!;
            CurrentProject!.WriteTo(stream);
        }
    }
}
