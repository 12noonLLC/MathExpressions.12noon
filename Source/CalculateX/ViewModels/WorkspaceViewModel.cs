using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathExpressions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace CalculateX.ViewModels;

[DebuggerDisplay("{Name} ({History.Count})")]
internal class WorkspaceViewModel : ObservableObject, Shared.EditableTabHeaderControl.IEditableTabHeaderControl
{
	public const string QUERY_DATA_WORKSPACE = "workspace";
	public const string QUERY_DATA_DELETE = "delete";
	public const string QUERY_DATA_RENAME = "rename";
	public const string QUERY_DATA_VARIABLES = "variables";
	public const string QUERY_DATA_VARIABLE_NAME = "variable";

	/// <summary>
	/// This is public only so the workspaces view-model can extract
	/// the Workspace when we need to add it to the workspaces model.
	/// </summary>
	public Models.Workspace _workspace { get; private init; }

	public string ID => _workspace.ID;
	public string Name
	{
		get => _workspace.Name;
		set => SetProperty(_workspace.Name, value, _workspace, (model, newValue) => model.Name = newValue);
	}
	public bool CanCloseTab
	{
		get => _canCloseTab;
		set => SetProperty(ref _canCloseTab, value);
	}
	private bool _canCloseTab;

	public ObservableCollection<Models.Workspace.HistoryEntry> History => _workspace.History;
	public VariableDictionary Variables => _workspace.Variables;

	public string Input
	{
		get => _input;
		set
		{
			SetProperty(ref _input, value);
			EvaluateCommand.NotifyCanExecuteChanged();
		}
	}
	private string _input = string.Empty;

	public bool ShowHelp
	{
		get => _showHelp;
		set => SetProperty(ref _showHelp, value);
	}
	private bool _showHelp = false;



	public event EventHandler? RequestDelete;
	private void RaiseRequestDelete() => RequestDelete?.Invoke(this, EventArgs.Empty);

	public event EventHandler? WorkspaceChanged;
	public void RaiseWorkspaceChanged() => WorkspaceChanged?.Invoke(this, EventArgs.Empty);


	public ICommand DeleteWorkspaceCommand { get; }
	public ICommand ClearInputCommand { get; }
	public RelayCommand ClearHistoryCommand { get; }
	public ICommand ToggleHelpCommand { get; }
	public RelayCommand EvaluateCommand { get; }


	/// Empty ctor required for the Designer
	public WorkspaceViewModel()
	{
		DeleteWorkspaceCommand = new RelayCommand(DeleteWorkspace);
		ClearInputCommand = new RelayCommand(ClearInput);
		ClearHistoryCommand = new RelayCommand(ClearHistory, ClearHistory_CanExecute);
		ToggleHelpCommand = new RelayCommand(ToggleHelp);
		EvaluateCommand = new RelayCommand(Evaluate, Evaluate_CanExecute);

		_workspace = new("Designer");
		CanCloseTab = true;
	}

	/// <summary>
	/// Called when adding "+" workspace.
	/// </summary>
	public WorkspaceViewModel(bool canCloseTab)
	{
		Debug.Assert(!canCloseTab, "Only called when adding + workspace.");

		DeleteWorkspaceCommand = new RelayCommand(DeleteWorkspace);
		ClearInputCommand = new RelayCommand(ClearInput);
		ClearHistoryCommand = new RelayCommand(ClearHistory, ClearHistory_CanExecute);
		ToggleHelpCommand = new RelayCommand(ToggleHelp);
		EvaluateCommand = new RelayCommand(Evaluate, Evaluate_CanExecute);

		_workspace = new("+");
		CanCloseTab = false;
	}

	/// <summary>
	/// Called when loading workspaces.
	/// </summary>
	/// <param name="workspace"></param>
	public WorkspaceViewModel(Models.Workspace workspace)
	{
		DeleteWorkspaceCommand = new RelayCommand(DeleteWorkspace);
		ClearInputCommand = new RelayCommand(ClearInput);
		ClearHistoryCommand = new RelayCommand(ClearHistory, ClearHistory_CanExecute);
		ToggleHelpCommand = new RelayCommand(ToggleHelp);
		EvaluateCommand = new RelayCommand(Evaluate, Evaluate_CanExecute);

		_workspace = workspace;
		CanCloseTab = true;
	}

	public void DeleteWorkspace()
	{
		RaiseRequestDelete();
	}

	public void ClearInput()
	{
		Input = string.Empty;
	}

	private bool ClearHistory_CanExecute() => History.Any();
	private void ClearHistory()
	{
		_workspace.ClearHistory();

		RaiseWorkspaceChanged();
	}

	private void ToggleHelp()
	{
		ShowHelp = !ShowHelp;
	}

	private bool Evaluate_CanExecute() => !string.IsNullOrWhiteSpace(Input);
	private void Evaluate()
	{
		Debug.Assert(!string.IsNullOrWhiteSpace(Input));

		_workspace.Evaluate(Input);
		ClearHistoryCommand.NotifyCanExecuteChanged();
		CollectionViewSource.GetDefaultView(Variables).Refresh();

		// Clear input when we're done.
		ClearInput();

		RaiseWorkspaceChanged();
	}

	/// <summary>
	/// Insert the passed text at the cursor and return the new cursor position.
	/// </summary>
	/// <param name="insertText">Text to insert</param>
	/// <param name="cursorPosition">Current cursor position</param>
	/// <returns>New cursor position</returns>
	public int InsertTextAtCursor(string insertText, int cursorPosition)
	{
		// Insert variable name into input field
		Input = Input.Insert(cursorPosition, insertText);

		// return cursor position after the inserted variable name
		return cursorPosition + insertText.Length;
	}
}
