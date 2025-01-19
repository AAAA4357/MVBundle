using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MVPlot.Managers;
using MVPlot.Utilities;
using System.Globalization;
using System.IO;
using System.Windows.Input;

namespace MVPlot.Windows
{
    /// <summary>
    /// 欢迎窗口视图模型
    /// </summary>
    public partial class WelcomeWindowViewModel : ObservableObject
    {
        private int LanguageIndex = 0;

        [RelayCommand]
        private void New()
        {
            OpenFolderDialog dialog = new()
            {
                Title = LanguageManager.Instance["SelectSaveLocation"]
            };
            dialog.ShowDialog();
            if (dialog.FolderName == string.Empty) return;
            PlotProjectUtility.SetPlotProjectFolderPath(dialog.FolderName);
            string name = DialogUtility.ShowInputString(LanguageManager.Instance["CreatePlotProject_WindowTitle"]!, LanguageManager.Instance["CreatePlotProject_ProjectNameInput"]!, LanguageManager.Instance["CreatePlotProject_DefaultName"]!);
            if (name == "") return;
            PlotProjectUtility.SetPlotProjectName(name + ".mpproj");
            PlotProjectUtility.CreatePlotProject();
            PlotProjectManager.OpenPlotProject();
        }

        [RelayCommand]
        private void Open()
        {
            OpenFileDialog dialog = new()
            {
                Title = LanguageManager.Instance["OpenExisting"],
                Filter = LanguageManager.Instance["SelectFilter"]
            };
            dialog.ShowDialog();
            if (dialog.FileName == string.Empty) return;
            FileInfo info = new(dialog.FileName);
            PlotProjectUtility.SetPlotProjectFolderPath(info.DirectoryName!);
            PlotProjectUtility.SetPlotProjectName(info.Name);
            PlotProjectManager.OpenPlotProject();
        }

        [RelayCommand]
        private void LangChange()
        {
            LanguageIndex++;
            if (LanguageIndex == 2) LanguageIndex = 0;
            switch (LanguageIndex)
            {
                case 0:
                    LanguageManager.Instance.ChangeLanguage(new CultureInfo("zh-CN"));
                    break;
                case 1:
                    LanguageManager.Instance.ChangeLanguage(new CultureInfo("en-US"));
                    break;
                default:
                    break;
            }
        }
    }
}
