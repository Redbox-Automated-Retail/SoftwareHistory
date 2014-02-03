using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console;

internal class ListViewFactory
{
	internal static ListViewFactory Instance => Singleton<ListViewFactory>.Instance;

	internal void MakeTab(ListViewNames tab, bool enable)
	{
		switch (tab)
		{
		case ListViewNames.Errors:
			CreateErrorListViewTab(enable);
			break;
		case ListViewNames.Job:
			CreateJobListViewTab(enable);
			break;
		case ListViewNames.OutputWindow:
			CreateOutputListViewTab(enable: true);
			break;
		case ListViewNames.ProgramEvents:
			CreateProgramEventListViewTab(enable);
			break;
		case ListViewNames.Results:
			CreateResultListViewTab(enable);
			break;
		case ListViewNames.Stack:
			CreateStackListViewTab(enable);
			break;
		case ListViewNames.Symbols:
			CreateSymbolListViewTab(enable);
			break;
		case ListViewNames.Immediate:
			break;
		}
	}

	private void CreateJobListViewTab(bool enable)
	{
		string[,] headers = new string[8, 2]
		{
			{ "Job ID", "ID" },
			{ "Connection State", "ConnectionState" },
			{ "Label", "Label" },
			{ "Running Program", "ProgramName" },
			{ "Priority", "Priority" },
			{ "Status", "Status" },
			{ "Start Time", "StartTime" },
			{ "Execution Time", "ExecutionTime" }
		};
		JobListView<HardwareJobWrapper> control = new JobListView<HardwareJobWrapper>(headers, JobHelper.JobList);
		ListViewTabControl.Instance.Add(ListViewNames.Job, control, null, enable);
	}

	private void CreateStackListViewTab(bool enable)
	{
		string[,] headers = new string[1, 2] { { "Stack Values", "Value" } };
		StackListView<StringWrapper> stackListView = new StackListView<StringWrapper>(headers, JobHelper.StackList);
		ListViewTabControl.Instance.Add(ListViewNames.Stack, stackListView, stackListView.RefreshData, enable);
	}

	private void CreateSymbolListViewTab(bool enable)
	{
		string[,] headers = new string[2, 2]
		{
			{ "Name", "Name" },
			{ "Value", "Value" }
		};
		SymbolListView<Symbol> symbolListView = new SymbolListView<Symbol>(headers, JobHelper.SymbolList);
		ListViewTabControl.Instance.Add(ListViewNames.Symbols, symbolListView, symbolListView.RefreshData, enable);
	}

	private void CreateResultListViewTab(bool enable)
	{
		string[,] headers = new string[1, 2] { { "Results", "Value" } };
		ResultListView<StringWrapper> resultListView = new ResultListView<StringWrapper>(headers, JobHelper.ResultList);
		ListViewTabControl.Instance.Add(ListViewNames.Results, resultListView, resultListView.RefreshData, enable);
	}

	private void CreateOutputListViewTab(bool enable)
	{
		ListViewTabControl.Instance.Add(ListViewNames.OutputWindow, OutputWindow.Instance, null, enabled: true);
	}

	private void CreateErrorListViewTab(bool enable)
	{
		ListViewTabControl.Instance.Add(ListViewNames.Errors, ErrorListView.Instance, null, enabled: true);
	}

	private void CreateProgramEventListViewTab(bool enable)
	{
		string[,] headers = new string[2, 2]
		{
			{ "Date", "EventTime" },
			{ "Message", "Message" }
		};
		ProgramEventListView<ProgramEvent> programEventListView = new ProgramEventListView<ProgramEvent>(headers, JobHelper.ProgramEventList);
		ListViewTabControl.Instance.Add(ListViewNames.ProgramEvents, programEventListView, programEventListView.RefreshData, enable);
	}

	private ListViewFactory()
	{
	}
}
