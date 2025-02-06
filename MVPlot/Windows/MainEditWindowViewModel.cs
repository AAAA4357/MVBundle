using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf;
using Microsoft.Win32;
using MVPlot.Managers;
using MVPlot.Proto;
using MVPlot.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace MVPlot.Windows
{
    /// <summary>
    /// 主编辑窗口视图模型
    /// </summary>
    public partial class MainEditWindowViewModel : ObservableObject
    {
        public string WindowTitle => string.Format(LanguageManager.Instance["MainEditWindow_WindowTitle"]!, PlotProjectManager.CurrentProject!.Name.ToString(Encoding.Default)) + (IsSaved ? "" : "*");

        [ObservableProperty]
        public int _PlotListSelectedIndex = -1;

        [ObservableProperty]
        public int _PlotRowListSelectedIndex = -1;

        [ObservableProperty]
        public PlotRow _PlotRowListSelectedItem = null!;

        [ObservableProperty]
        public string _MainSender_NormalPlotRowName = "";

        [ObservableProperty]
        public string _MainSender_NormalPlotRowContent = "";

        [ObservableProperty]
        public int _MainSender_AdditionalPlotRowType_SelectedIndex = 0;

        [ObservableProperty]
        public string _MainSender_AdditionalPlotRowContent = "";

        [ObservableProperty]
        public int _MainSender_ActionPlotRowType_SelectedIndex = 0;

        [ObservableProperty]
        public string _MainSender_ActionPlotRowContent = "";

        [ObservableProperty]
        public int _PlotConditionSelectedIndex = -1;

        [ObservableProperty]
        public bool _IsMultiSelect = false;

        public ObservableCollection<Plot> PlotList { get; set; } = [.. PlotProjectManager.CurrentProject!.Plots];
        public ObservableCollection<PlotRow> PlotRowList { get; set; } = [];
        public ObservableCollection<string> PlotConditionList { get; set; } = [];

        public bool HasPlot => PlotList.Count > 0;
        public bool SelectedPlot => PlotListSelectedIndex != -1;
        public bool IsMultiSelectCheck { get; set; } = false;
        public bool IsMultiSelectUnCheck { get; set; } = false;

        private bool IsSaved = true;
        private bool ClipboardHasContent = false;
        private bool OverwriteFlag = false;
        private bool InsertFlag = false;
        private bool ForwardToFlag = false;
        private bool BackwardToFlag = false;

        [RelayCommand]
        private void AddPlot()
        {
            string name = DialogUtility.ShowInputString(LanguageManager.Instance["CreatePlot_PlotName_WindowTitle"]!, LanguageManager.Instance["CreatePlot_PlotName_Title"]!);
            if (name == "") return;
            string id;
            do
            {
                id = DialogUtility.ShowInputString(LanguageManager.Instance["CreatePlot_PlotID_WindowTitle"]!, LanguageManager.Instance["CreatePlot_PlotID_Title"]!);
                if (id == "") return;
                if (IDCheck().IsMatch(id))
                {
                    HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["CreatePlot_PlotIDInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                if (PlotList.Count != 0 && PlotList.Where(x => x.ID == id).Count() != 0)
                {
                    HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["CreatePlot_PlotIDDuplicate"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                break;
            }
            while (true);
            Plot plot = new()
            {
                PlotName = ByteString.CopyFrom(Encoding.Default.GetBytes(name)),
                ID = id
            };
            PlotProjectManager.CurrentProject!.Plots.Add(plot);
            PlotList.Add(plot);
            AddPlotUserOperation operation = new(this, plot);
            OperationManager.Add(operation);
            OnPropertyChanged(nameof(HasPlot));
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        private bool AddNormalPlotRowCheck()
        {
            if (MainSender_NormalPlotRowContent == "") return false;
            return true;
        }

        [RelayCommand(CanExecute = nameof(AddNormalPlotRowCheck))]
        private void AddNormalPlotRow()
        {
            if (MainSender_NormalPlotRowContent == "")
            {
                return;
            }
            PlotRow row = new()
            {
                Type = MainSender_NormalPlotRowName == "" ? PlotRowType.Narrator : PlotRowType.Normal
            };
            row.Content.Add(ByteString.CopyFrom(Encoding.Default.GetBytes(MainSender_NormalPlotRowContent)));
            if (MainSender_NormalPlotRowName != "") row.Content.Add(ByteString.CopyFrom(Encoding.Default.GetBytes(MainSender_NormalPlotRowName)));
            int index = PlotRowListSelectedIndex == -1 ? PlotRowList.Count : InsertFlag ? PlotRowListSelectedIndex : PlotRowListSelectedIndex + 1;
            if (OverwriteFlag)
            {
                index--;
                PlotRow oldRow = PlotRowList[index];
                PlotRowList.RemoveAt(index);
                PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.RemoveAt(index);
                OverwritePlotRowUserOperation operation = new(this, oldRow, row, PlotListSelectedIndex, index);
                OperationManager.Add(operation);
            }
            else
            {
                InsertPlotRowUserOperation operation = new(this, row, PlotListSelectedIndex, index);
                OperationManager.Add(operation);
            }
            PlotRowList.Insert(index, row);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Insert(index, row);
            MainSender_NormalPlotRowContent = "";
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        private bool AddAdditionalPlotRowCheck()
        {
            if (MainSender_AdditionalPlotRowContent == "") return false;
            if (PlotRowListSelectedItem is null) return false;
            if (PlotRowListSelectedItem.Type == PlotRowType.Normal) return true;
            return false;
        }

        [RelayCommand(CanExecute = nameof(AddAdditionalPlotRowCheck))]
        private void AddAdditionalPlotRow()
        {
            if (MainSender_AdditionalPlotRowContent == "")
            {
                return;
            }
            PlotRow row = new()
            {
                Type = MainSender_AdditionalPlotRowType_SelectedIndex == 0 ? PlotRowType.Se : PlotRowType.Bubble
            };
            row.Content.Add(ByteString.CopyFrom(Encoding.Default.GetBytes(MainSender_AdditionalPlotRowContent)));
            int index = PlotRowListSelectedIndex == -1 ? PlotRowList.Count : InsertFlag ? PlotRowListSelectedIndex : PlotRowListSelectedIndex + 1;
            if (OverwriteFlag)
            {
                index--;
                PlotRow oldRow = PlotRowList[index];
                PlotRowList.RemoveAt(index);
                PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.RemoveAt(index);
                OverwritePlotRowUserOperation operation = new(this, oldRow, row, PlotListSelectedIndex, index);
                OperationManager.Add(operation);
            }
            else
            {
                InsertPlotRowUserOperation operation = new(this, row, PlotListSelectedIndex, index);
                OperationManager.Add(operation);
            }
            MainSender_AdditionalPlotRowContent = "";
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        private bool AddActionPlotRowCheck()
        {
            if (MainSender_ActionPlotRowContent == "") return false;
            return true;
        }

        [RelayCommand(CanExecute = nameof(AddActionPlotRowCheck))]
        private void AddActionPlotRow()
        {
            if (MainSender_ActionPlotRowContent == "")
            {
                return;
            }
            PlotRow row = new()
            {
                Type = MainSender_ActionPlotRowType_SelectedIndex == 0 ? PlotRowType.ShakeScreen : PlotRowType.ChangeBgm
            };
            row.Content.Add(ByteString.CopyFrom(Encoding.Default.GetBytes(MainSender_ActionPlotRowContent)));
            int index = PlotRowListSelectedIndex == -1 ? PlotRowList.Count : InsertFlag ? PlotRowListSelectedIndex : PlotRowListSelectedIndex + 1;
            if (OverwriteFlag)
            {
                index--;
                PlotRow oldRow = PlotRowList[index];
                PlotRowList.RemoveAt(index);
                PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.RemoveAt(index);
                OverwritePlotRowUserOperation operation = new(this, oldRow, row, PlotListSelectedIndex, index);
                OperationManager.Add(operation);
            }
            else
            {
                InsertPlotRowUserOperation operation = new(this, row, PlotListSelectedIndex, index);
                OperationManager.Add(operation);
            }
            MainSender_ActionPlotRowContent = "";
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand]
        private void Save()
        {
            PlotProjectManager.SavePlotProject();
            IsSaved = true;
            OnPropertyChanged(nameof(WindowTitle));
        }

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
            PlotProjectManager.ClosePlotProject();
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
            PlotProjectManager.ClosePlotProject();
            PlotProjectManager.OpenPlotProject();
        }

        [RelayCommand]
        private void Close()
        {
            PlotProjectManager.ClosePlotProject();
        }

        [RelayCommand]
        private void Undo()
        {
            OperationManager.Undo();
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand]
        private void Redo()
        {
            OperationManager.Redo();
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        private bool SelectedPlotRowCheck()
        {
            if (PlotRowListSelectedIndex == -1) return false;
            return true;
        }

        private bool SelectedPlotCheck()
        {
            if (PlotListSelectedIndex == -1) return false;
            return true;
        }

        [RelayCommand(CanExecute = nameof(SelectedPlotRowCheck))]
        private void Cut()
        {
            CutPlotRow();
        }

        [RelayCommand(CanExecute = nameof(SelectedPlotRowCheck))]
        private void Copy()
        {
            CopyPlotRow();
        }

        private bool PasteCheck()
        {
            if (!ClipboardHasContent) return false;
            string content = Clipboard.GetText();
            try
            {
                JsonConvert.DeserializeObject<SerializablePlotRow>(content);
            }
            catch
            {
                return false;
            }
            return SelectedPlotRowCheck();
        }

        [RelayCommand(CanExecute = nameof(PasteCheck))]
        private void Paste()
        {
            InsertPastePlotRow();
        }

        [RelayCommand]
        private void SelectAll()
        {
            //TODO:
        }

        [RelayCommand]
        private void Find()
        {
            string query = DialogUtility.ShowInputString(LanguageManager.Instance["MainEditWindow_FindWindowTitle"]!, LanguageManager.Instance["MainEditWindow_FindTitle"]!);
            for (int i = PlotListSelectedIndex == -1 ? 0 : PlotListSelectedIndex; i < PlotProjectManager.CurrentProject!.Plots.Count; i++)
            {
                for (int j = PlotRowListSelectedIndex == -1 ? 0 : PlotRowListSelectedIndex; j < PlotProjectManager.CurrentProject!.Plots[i].Rows.Count; j++)
                {
                    foreach (var bs in PlotProjectManager.CurrentProject!.Plots[i].Rows[j].Content)
                    {
                        if (bs.ToString(Encoding.Default).Contains(query))
                        {
                            PlotListSelectedIndex = i;
                            PlotRowListSelectedIndex = j;
                            return;
                        }
                    }
                }
            }
            HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_FindNothing"]!, LanguageManager.Instance["Information"]!, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void FindRegex()
        {
            string query = DialogUtility.ShowInputString(LanguageManager.Instance["MainEditWindow_FindWindowTitle"]!, LanguageManager.Instance["MainEditWindow_FindTitle"]!);
            for (int i = PlotListSelectedIndex == -1 ? 0 : PlotListSelectedIndex; i < PlotProjectManager.CurrentProject!.Plots.Count; i++)
            {
                for (int j = PlotRowListSelectedIndex == -1 ? 0 : PlotRowListSelectedIndex; j < PlotProjectManager.CurrentProject!.Plots[i].Rows.Count; j++)
                {
                    foreach (var bs in PlotProjectManager.CurrentProject!.Plots[i].Rows[j].Content)
                    {
                        if (Regex.IsMatch(bs.ToString(Encoding.Default),query))
                        {
                            PlotListSelectedIndex = i;
                            PlotRowListSelectedIndex = j;
                            return;
                        }
                    }
                }
            }
            HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_FindNothing"]!, LanguageManager.Instance["Information"]!, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void NewPlot()
        {
            AddPlot();
        }

        [RelayCommand]
        private void DeletePlot()
        {
            MessageBoxResult result = HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_DeletePlotAsk"], LanguageManager.Instance["Warning"], MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Plot plot = PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex];
                    PlotProjectManager.CurrentProject!.Plots.RemoveAt(PlotListSelectedIndex);
                    PlotList.RemoveAt(PlotListSelectedIndex);
                    RemovePlotUserOperation operation = new(this, plot);
                    OperationManager.Add(operation);
                    IsSaved = false;
                    OnPropertyChanged(nameof(WindowTitle));
                    OnPropertyChanged(nameof(HasPlot));
                    OnPropertyChanged(nameof(SelectedPlot));
                    break;
                default:
                    return;
            }
        }

        private bool PlotConditionSelectedCheck()
        {
            return SelectedPlotCheck() && PlotConditionSelectedIndex != -1;
        }

        [RelayCommand(CanExecute = nameof(SelectedPlotCheck))]
        private void InsertPlotCondition()
        {
            string condition;
            do
            {
                condition = DialogUtility.ShowInputString(LanguageManager.Instance["MainEditWindow_ChangeConditionWindowTitle"]!, LanguageManager.Instance["MainEditWindow_ChangeConditionTitle"]!);
                if (condition == string.Empty) return;
                if (PlotConditionCheck().IsMatch(condition))
                {
                    HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                string[] values = condition.Split(',');
                if (values.Length != 5)
                {
                    HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                if (values[0] != "s" && values[0] != "v")
                {
                    HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                if (ConditionCheck().IsMatch(values[1]))
                {
                    HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                string value2 = values[2];
                string[] targes = value2.Split(';');
                if (targes.Length != 1 && targes.Length != 2)
                {
                    HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                if (ConditionCheck().IsMatch(targes[0]))
                {
                    HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                if (targes.Length == 2)
                {
                    if (ConditionCheck().IsMatch(targes[1]))
                    {
                        HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                    }
                }
                switch (values[3])
                {
                    case ">":
                        if (values[0] == "s")
                        {
                            HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }
                        break;
                    case "<":
                        if (values[0] == "s")
                        {
                            HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }
                        break;
                    case ">=":
                        if (values[0] == "s")
                        {
                            HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }
                        break;
                    case "<=":
                        if (values[0] == "s")
                        {
                            HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }
                        break;
                    case "->":
                        if (values[0] == "s")
                        {
                            HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                            continue;
                        }
                        break;
                    case "==":
                    case "!=":
                        break;
                    default:
                        HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                        continue;
                }
                if (values[4] != "a" && values[4] != "o")
                {
                    HandyControl.Controls.MessageBox.Show(LanguageManager.Instance["MainEditWindow_ConditionInvalid"]!, LanguageManager.Instance["Warning"]!, MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }
                break;
            }
            while (true);
            int index = PlotConditionSelectedIndex == -1 ? PlotConditionList.Count : PlotConditionSelectedIndex + 1;
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Conditions.Insert(index, condition);
            PlotConditionList.Insert(index, condition);
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand(CanExecute = nameof(PlotConditionSelectedCheck))]
        private void DeletePlotCondition()
        {
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Conditions.RemoveAt(PlotConditionSelectedIndex);
            PlotConditionList.RemoveAt(PlotConditionSelectedIndex);
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand]
        private void ExportMVEdit()
        {
            List<Proto.Plot> plots = [];
            foreach (Proto.Plot plot in PlotProjectManager.CurrentProject!.Plots)
            {
                plots.Add(new());
                foreach (PlotRow row in plot.Rows)
                {

                }
            }
        }

        [RelayCommand]
        private void About()
        {
            HandyControl.Controls.MessageBox.Show($"MVPlot - {Assembly.GetEntryAssembly()!.GetName().Version}\n©2025 Alon_. All rights reserved.", LanguageManager.Instance["MainEditWindow_AboutWindowTitle"]!);
        }

        [RelayCommand]
        private void Usage() 
        {
            UsageWindow window = new();
            window.ShowDialog();
        }

        [RelayCommand]
        private void DeletePlotRow()
        {
            int index = PlotRowListSelectedIndex;
            PlotRow item = PlotRowListSelectedItem;
            PlotRowList.Remove(item);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Remove(item);
            RemovePlotRowUserOperation operation = new(this, item, PlotListSelectedIndex, index);
            OperationManager.Add(operation);
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand]
        private void OverwritePlotRow()
        {
            OverwriteFlag = true;
            PlotRow row = PlotRowListSelectedItem;
            switch (row.Type)
            {
                case PlotRowType.Normal:
                    MainSender_NormalPlotRowName = row.Content[1].ToString(Encoding.Default);
                    MainSender_NormalPlotRowContent = row.Content[0].ToString(Encoding.Default);
                    break;
                case PlotRowType.Narrator:
                    MainSender_NormalPlotRowContent = row.Content[0].ToString(Encoding.Default);
                    break;
                case PlotRowType.Bubble:
                    MainSender_ActionPlotRowType_SelectedIndex = 1;
                    MainSender_AdditionalPlotRowContent = row.Content[0].ToString(Encoding.Default);
                    break;
                case PlotRowType.Se:
                    MainSender_ActionPlotRowType_SelectedIndex = 0;
                    MainSender_AdditionalPlotRowContent = row.Content[0].ToString(Encoding.Default);
                    break;
                case PlotRowType.ShakeScreen:
                    MainSender_ActionPlotRowType_SelectedIndex = 0;
                    MainSender_ActionPlotRowContent = row.Content[0].ToString(Encoding.Default);
                    break;
                case PlotRowType.ChangeBgm:
                    MainSender_ActionPlotRowType_SelectedIndex = 1;
                    MainSender_ActionPlotRowContent = row.Content[0].ToString(Encoding.Default);
                    break;
                default:
                    break;
            }
        }

        [RelayCommand]
        private void InsertPlotRow()
        {
            InsertFlag = true;
        }

        private bool MoveForwardPlotRowCheck()
        {
            if (PlotRowListSelectedIndex == 0 && PlotRowListSelectedIndex != -1) return false;
            return true;
        }

        private bool MoveBackwardPlotRowCheck()
        {
            if (PlotRowListSelectedIndex == PlotRowList.Count - 1 && PlotRowListSelectedIndex != -1) return false;
            return true;
        }

        [RelayCommand(CanExecute = nameof(MoveForwardPlotRowCheck))]
        private void MoveForwardPlotRow()
        {
            PlotRow row = PlotRowListSelectedItem;
            int index1 = PlotRowListSelectedIndex;
            int index2 = index1 - 1;
            PlotRowList.Remove(row);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Remove(row);
            PlotRowList.Insert(index2, row);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Insert(index2, row);
            MovePlotRowUserOperation operation = new(this, row, PlotListSelectedIndex, index1, index2);
            OperationManager.Add(operation);
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand(CanExecute = nameof(MoveBackwardPlotRowCheck))]
        private void MoveBackwardPlotRow()
        {
            PlotRow row = PlotRowListSelectedItem;
            int index1 = PlotRowListSelectedIndex;
            int index2 = index1 + 1;
            PlotRowList.Remove(row);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Remove(row);
            PlotRowList.Insert(index2, row);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Insert(index2, row);
            MovePlotRowUserOperation operation = new(this, row, PlotListSelectedIndex, index1, index2);
            OperationManager.Add(operation);
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand(CanExecute = nameof(MoveForwardPlotRowCheck))]
        private void MoveForwardToPlotRow()
        {
            ForwardToFlag = true;
        }

        [RelayCommand(CanExecute = nameof(MoveBackwardPlotRowCheck))]
        private void MoveBackwardToPlotRow()
        {
            BackwardToFlag = true;
        }

        [RelayCommand]
        private void CutPlotRow()
        {
            int index = PlotRowListSelectedIndex;
            PlotRow item = PlotRowListSelectedItem;
            SerializablePlotRow row = new()
            {
                Type = item.Type,
                Content = []
            };
            foreach (var bs in item.Content)
            {
                row.Content.Add(bs.ToString(Encoding.Default));
            }
            Clipboard.SetText(JsonConvert.SerializeObject(row));
            PlotRowList.RemoveAt(index);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.RemoveAt(index);
            ClipboardHasContent = true;
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand]
        private void CopyContentPlotRow()
        {
            Clipboard.SetText(PlotRowListSelectedItem.Content[0].ToString(Encoding.Default));
            ClipboardHasContent = true;
        }

        [RelayCommand]
        private void CopyPlotRow()
        {
            PlotRow item = PlotRowListSelectedItem;
            SerializablePlotRow row = new() 
            { 
                Type = item.Type, 
                Content = []
            };
            foreach (var bs in item.Content)
            {
                row.Content.Add(bs.ToString(Encoding.Default));
            }
            Clipboard.SetText(JsonConvert.SerializeObject(row));
            ClipboardHasContent = true;
        }

        [RelayCommand(CanExecute = nameof(PasteCheck))]
        private void OverwritePastePlotRow()
        {
            SerializablePlotRow item = JsonConvert.DeserializeObject<SerializablePlotRow>(Clipboard.GetText())!;
            PlotRow row = new()
            {
                Type = item.Type
            };
            foreach (var content in item.Content!)
            {
                row.Content.Add(ByteString.CopyFrom(Encoding.Default.GetBytes(content)));
            }
            int index = PlotRowListSelectedIndex;
            PlotRow oldRow = PlotRowListSelectedItem;
            PlotRowList.RemoveAt(index);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.RemoveAt(index);
            PlotRowList.Insert(index, row);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Insert(index, row);
            OverwritePlotRowUserOperation operation = new(this, oldRow, row, PlotListSelectedIndex, index);
            OperationManager.Add(operation);
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand(CanExecute = nameof(PasteCheck))]
        private void InsertPastePlotRow()
        {
            SerializablePlotRow item = JsonConvert.DeserializeObject<SerializablePlotRow>(Clipboard.GetText())!;
            PlotRow row = new()
            {
                Type = item.Type
            };
            foreach (var content in item.Content!)
            {
                row.Content.Add(ByteString.CopyFrom(Encoding.Default.GetBytes(content)));
            }
            int index = PlotRowListSelectedIndex;
            PlotRowList.Insert(index, row);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Insert(index, row);
            InsertPlotRowUserOperation operation = new(this, row, PlotListSelectedIndex, index);
            OperationManager.Add(operation);
            IsSaved = false;
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand(CanExecute = nameof(PasteCheck))]
        private void AppendPastePlotRow()
        {
            SerializablePlotRow item = JsonConvert.DeserializeObject<SerializablePlotRow>(Clipboard.GetText())!;
            PlotRow row = new()
            {
                Type = item.Type
            };
            foreach (var content in item.Content!)
            {
                row.Content.Add(ByteString.CopyFrom(Encoding.Default.GetBytes(content)));
            }
            int index = PlotRowListSelectedIndex + 1;
            PlotRowList.Insert(index, row);
            PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Insert(index, row);
            InsertPlotRowUserOperation operation = new(this, row, PlotListSelectedIndex, index);
            OperationManager.Add(operation);
                    IsSaved = false;
                    OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand]
        private void MultiSelectPlotRow()
        {
            IsMultiSelect = true;
        }

        partial void OnPlotListSelectedIndexChanged(int value)
        {
            PlotRowList.Clear();
            PlotConditionList.Clear();
            if (value == -1) return;
            foreach (var item in PlotProjectManager.CurrentProject!.Plots[value].Rows)
            {
                PlotRowList.Add(item);
            }
            foreach (var item in PlotProjectManager.CurrentProject!.Plots[value].Conditions)
            {
                PlotConditionList.Add(item);
            }
            OnPropertyChanged(nameof(SelectedPlot));
            InsertPlotConditionCommand.NotifyCanExecuteChanged();
        }

        bool operateLock;

        partial void OnPlotRowListSelectedIndexChanging(int oldValue, int newValue)
        {
            if (operateLock) return;
            if (ForwardToFlag)
            {
                operateLock = true;
                PlotRow row = PlotRowList[oldValue];
                int index1 = oldValue;
                int index2 = newValue;
                if (index2 > index1) index2--;
                PlotRowList.Remove(row);
                PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Remove(row);
                PlotRowList.Insert(index2, row);
                PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Insert(index2, row);
                MovePlotRowUserOperation operation = new(this, row, PlotListSelectedIndex, index1, index2);
                OperationManager.Add(operation);
                ForwardToFlag = false;
                operateLock = false;
            }
            if (BackwardToFlag)
            {
                operateLock = true;
                PlotRow row = PlotRowList[oldValue];
                int index1 = oldValue;
                int index2 = newValue + 1;
                if (index2 > index1) index2--;
                PlotRowList.Remove(row);
                PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Remove(row);
                PlotRowList.Insert(index2, row);
                PlotProjectManager.CurrentProject!.Plots[PlotListSelectedIndex].Rows.Insert(index2, row);
                MovePlotRowUserOperation operation = new(this, row, PlotListSelectedIndex, index1, index2);
                OperationManager.Add(operation);
                BackwardToFlag = false;
                operateLock = false;
            }
        }

        partial void OnPlotRowListSelectedIndexChanged(int value)
        {
            MoveForwardPlotRowCommand.NotifyCanExecuteChanged();
            MoveForwardToPlotRowCommand.NotifyCanExecuteChanged();
            MoveBackwardPlotRowCommand.NotifyCanExecuteChanged();
            MoveBackwardToPlotRowCommand.NotifyCanExecuteChanged();
            CutCommand.NotifyCanExecuteChanged();
            CopyCommand.NotifyCanExecuteChanged();
            OverwritePastePlotRowCommand.NotifyCanExecuteChanged();
            InsertPastePlotRowCommand.NotifyCanExecuteChanged();
            AppendPastePlotRowCommand.NotifyCanExecuteChanged();
            OverwriteFlag = false;
            InsertFlag = false;
        }

        partial void OnMainSender_NormalPlotRowContentChanged(string value)
        {
            AddNormalPlotRowCommand.NotifyCanExecuteChanged();
        }

        partial void OnMainSender_AdditionalPlotRowContentChanged(string value)
        {
            AddAdditionalPlotRowCommand.NotifyCanExecuteChanged();
        }

        partial void OnPlotRowListSelectedItemChanged(PlotRow value)
        {
            AddAdditionalPlotRowCommand.NotifyCanExecuteChanged();
        }

        partial void OnMainSender_ActionPlotRowContentChanged(string value)
        {
            AddActionPlotRowCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsMultiSelectChanged(bool value)
        {
            if (value) IsMultiSelectCheck = !IsMultiSelectCheck;
            else IsMultiSelectUnCheck = !IsMultiSelectUnCheck;
        }

        [GeneratedRegex(@"[^a-zA-Z0-9_]")]
        private static partial Regex IDCheck();

        [GeneratedRegex(@"[^0-9]")]
        private static partial Regex ConditionCheck();

        [GeneratedRegex(@"[^0-9sv,><=\-!;]")]
        private static partial Regex PlotConditionCheck();

        public class Plot
        {
            public required ByteString PlotName { get; set; }

            public required string ID { get; set; }

            public static implicit operator Plot(MVPlot.Proto.Plot plot)
            {
                return new() { PlotName = plot.PlotName, ID = plot.ID };
            }

            public static implicit operator MVPlot.Proto.Plot(Plot plot)
            {
                return new() { PlotName = plot.PlotName, ID = plot.ID };
            }
        }

        [Serializable]
        public class SerializablePlotRow
        {
            public PlotRowType Type { get; set; }

            public List<string>? Content { get; set; }
        }
    }
}
