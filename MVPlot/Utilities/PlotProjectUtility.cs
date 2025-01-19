using Google.Protobuf;
using MVPlot.Managers;
using MVPlot.Proto;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Windows;

namespace MVPlot.Utilities
{
    /// <summary>
    /// 剧本项目工具类
    /// </summary>
    public static class PlotProjectUtility
    {
        /// <summary>
        /// 剧本项目文件夹路径
        /// </summary>
        static string PlotProjectFolderPath = "";

        /// <summary>
        /// 剧本项目名称
        /// </summary>
        static string PlotProjectName = "";

        /// <summary>
        /// 剧本项目主文件路径
        /// </summary>
        public static string PlotProjectMainFilePath => PlotProjectFolderPath + "\\" + PlotProjectName;

        /// <summary>
        /// 设置剧本项目路径
        /// </summary>
        /// <param name="path">项目路径</param>
        public static void SetPlotProjectFolderPath(string path) => PlotProjectFolderPath = path;

        /// <summary>
        /// 设置剧本项目名称
        /// </summary>
        /// <param name="name">项目名称</param>
        public static void SetPlotProjectName(string name) => PlotProjectName = name;

        /// <summary>
        /// 根据路径创建剧本项目
        /// </summary>
        public static void CreatePlotProject()
        {   
            if (!CheckPlotProjectCreation())
            {
                MessageBoxResult result = HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["PlotProjectCreation_CreateWarning"], 
                                                                                LanguageManager.Instance["Warning"], 
                                                                                MessageBoxButton.YesNo, 
                                                                                MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        break;
                    default:
                        return;
                }
            }
            PlotProject project = new()
            {
                Name = ByteString.CopyFrom(Encoding.Default.GetBytes(PlotProjectName))
            };
            if (File.Exists(PlotProjectMainFilePath)) File.Delete(PlotProjectMainFilePath);
            using FileStream stream = new(PlotProjectMainFilePath, FileMode.Create);
            project.WriteTo(stream);
        }

        /// <summary>
        /// 检查创建目录是否合法
        /// </summary>
        /// <returns>目录合法性</returns>
        public static bool CheckPlotProjectCreation()
        {
            if (File.Exists(PlotProjectMainFilePath)) return false;
            return true;
        }
    }
}
