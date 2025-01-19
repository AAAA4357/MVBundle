using MVPlot.Managers;
using MVPlot.Proto;
using MVPlot.Windows;

namespace MVPlot.Utilities
{
    public sealed class AddPlotUserOperation(MainEditWindowViewModel viewModel, Plot plot) : UserOperationBase
    {
        MainEditWindowViewModel BaseObject = viewModel;

        Plot Argument = plot;

        public override void ReDO()
        {
            BaseObject.PlotList.Add(Argument);
            PlotProjectManager.CurrentProject!.Plots.Add(Argument);
        }

        public override void UnDO()
        {
            BaseObject.PlotList.Remove(Argument);
            PlotProjectManager.CurrentProject!.Plots.Remove(Argument);
        }
    }

    public sealed class RemovePlotUserOperation(MainEditWindowViewModel viewModel, Plot plot) : UserOperationBase
    {
        MainEditWindowViewModel BaseObject = viewModel;

        Plot Argument = plot;

        public override void ReDO()
        {
            BaseObject.PlotList.Remove(Argument);
            PlotProjectManager.CurrentProject!.Plots.Remove(Argument);
        }

        public override void UnDO()
        {
            BaseObject.PlotList.Add(Argument);
            PlotProjectManager.CurrentProject!.Plots.Add(Argument);
        }
    }

    public sealed class RemovePlotRowUserOperation(MainEditWindowViewModel viewModel, PlotRow row, int plotIndex, int rowIndex) : UserOperationBase
    {
        MainEditWindowViewModel BaseObject = viewModel;

        PlotRow Argument = row;

        int PlotIndex = plotIndex;
        int RowIndex = rowIndex;

        public override void ReDO()
        {
            BaseObject.PlotRowList.Remove(Argument);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Remove(Argument);
        }

        public override void UnDO()
        {
            BaseObject.PlotRowList.Insert(RowIndex, Argument);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Insert(RowIndex, Argument);
        }
    }

    public sealed class OverwritePlotRowUserOperation(MainEditWindowViewModel viewModel, PlotRow oldRow, PlotRow newEow, int plotIndex, int rowIndex) : UserOperationBase
    {
        MainEditWindowViewModel BaseObject = viewModel;

        PlotRow Argument1 = oldRow;
        PlotRow Argument2 = newEow;

        int PlotIndex = plotIndex;
        int RowIndex = rowIndex;

        public override void ReDO()
        {
            BaseObject.PlotRowList.Remove(Argument1);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Remove(Argument1);
            BaseObject.PlotRowList.Insert(RowIndex, Argument2);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Insert(RowIndex, Argument2);
        }

        public override void UnDO()
        {
            BaseObject.PlotRowList.Remove(Argument2);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Remove(Argument2);
            BaseObject.PlotRowList.Insert(RowIndex, Argument1);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Insert(RowIndex, Argument1);
        }
    }

    public sealed class InsertPlotRowUserOperation(MainEditWindowViewModel viewModel, PlotRow row, int plotIndex, int rowIndex) : UserOperationBase
    {
        MainEditWindowViewModel BaseObject = viewModel;

        PlotRow Argument = row;

        int PlotIndex = plotIndex;
        int RowIndex = rowIndex;

        public override void ReDO()
        {
            BaseObject.PlotRowList.Insert(RowIndex, Argument);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Insert(RowIndex, Argument);
        }

        public override void UnDO()
        {
            BaseObject.PlotRowList.Remove(Argument);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Remove(Argument);
        }
    }

    public sealed class MovePlotRowUserOperation(MainEditWindowViewModel viewModel, PlotRow row, int plotIndex, int oldRowIndex, int newRowIndex) : UserOperationBase
    {
        MainEditWindowViewModel BaseObject = viewModel;

        PlotRow Argument = row;

        int PlotIndex = plotIndex;
        int OldRowIndex = oldRowIndex;
        int NewRowIndex = newRowIndex;

        public override void ReDO()
        {
            BaseObject.PlotRowList.Remove(Argument);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Remove(Argument);
            BaseObject.PlotRowList.Insert(NewRowIndex, Argument);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Insert(NewRowIndex, Argument);
        }

        public override void UnDO()
        {
            BaseObject.PlotRowList.Remove(Argument);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Remove(Argument);
            BaseObject.PlotRowList.Insert(OldRowIndex, Argument);
            PlotProjectManager.CurrentProject!.Plots[PlotIndex].Rows.Insert(OldRowIndex, Argument);
        }
    }
}
