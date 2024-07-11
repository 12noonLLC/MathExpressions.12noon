using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
#if MY_WINDOWS_WPF
using MathExpressions;
using System.Linq;
using System.Windows.Data;
#endif

namespace CalculateX.ViewModels;

[DebuggerDisplay("{Name} {ID} ({History.Count})")]
public class WorkspaceViewModel : ObservableObject, ICloneable, IEquatable<WorkspaceViewModel>
#if !MY_WINDOWS_WPF
, IQueryAttributable
#endif
{
	public const string QUERY_DATA_WORKSPACE = "workspace";
	public const string QUERY_DATA_DELETE = "delete";
	public const string QUERY_DATA_RENAME = "rename";
	public const string QUERY_DATA_VARIABLES = "variables";
	public const string QUERY_DATA_VARIABLE_NAME = "variable";
#if !MY_WINDOWS_WPF
	public const string QUERY_DATA_VARIABLE_VALUE = "value";
#endif

	/// <summary>
	/// This is public only so the workspaces view-model can extract
	/// the Workspace when we need to add it to the workspaces model.
	/// </summary>
#if MY_WINDOWS_WPF
	public Models.Workspace _workspace { get; private init; }
#else
	public Models.Workspace _workspace { get; private set; }
#endif
	public Models.Workspace TEST_workspace => _workspace;

	public string ID => _workspace.ID;
	public string Name
	{
		get => _workspace.Name;
		set => SetProperty(_workspace.Name, value, _workspace, (model, newValue) => model.Name = newValue);
	}

	public ObservableCollection<Models.Workspace.HistoryEntry> History => _workspace.History;
#if MY_WINDOWS_WPF
	public VariableDictionary Variables => _workspace.Variables;
#endif

	public string Input
	{
		get => _input;
		set
		{
			SetProperty(ref _input, value);
#if MY_WINDOWS_WPF
			EvaluateCommand.NotifyCanExecuteChanged();
#endif
		}
	}
	private string _input = string.Empty;

#if MY_WINDOWS_WPF
	public bool ShowHelp
	{
		get => _showHelp;
		set => SetProperty(ref _showHelp, value);
	}
	private bool _showHelp = false;


	public event EventHandler? RequestDelete;
	private void RaiseRequestDelete() => RequestDelete?.Invoke(this, EventArgs.Empty);
#endif

	public event EventHandler? WorkspaceChanged;
	public void RaiseWorkspaceChanged() => WorkspaceChanged?.Invoke(this, EventArgs.Empty);


	public RelayCommand EvaluateCommand { get; }
	public ICommand DeleteWorkspaceCommand { get; }
#if MY_WINDOWS_WPF
	public ICommand ClearInputCommand { get; }
	public RelayCommand ClearHistoryCommand { get; }
	public ICommand ToggleHelpCommand { get; }
#else
	public ICommand RenameWorkspaceCommand { get; }
	public ICommand VariablesWorkspaceCommand { get; }
	public ICommand HelpCommand { get; }
#endif


	/// <summary>
	/// Windows: Empty ctor required for the Designer.
	/// Android: Called when navigating to a workspace.
	/// </summary>
	public WorkspaceViewModel()
	{
		EvaluateCommand = new RelayCommand(Evaluate, Evaluate_CanExecute);
#if MY_WINDOWS_WPF
		DeleteWorkspaceCommand = new RelayCommand(DeleteWorkspace);
		ClearInputCommand = new RelayCommand(ClearInput);
		ClearHistoryCommand = new RelayCommand(ClearHistory, ClearHistory_CanExecute);
		ToggleHelpCommand = new RelayCommand(ToggleHelp);

		_workspace = new("Designer");
#else
		DeleteWorkspaceCommand = new AsyncRelayCommand(DeleteWorkspaceAsync);
		RenameWorkspaceCommand = new AsyncRelayCommand(RenameWorkspaceAsync);
		VariablesWorkspaceCommand = new AsyncRelayCommand(VariablesWorkspaceAsync);
		HelpCommand = new AsyncRelayCommand(Help);

		// We always go on to ApplyQueryAttributes() to select a workspace.
		// We don't need this workspace, but we do not want to allow null.
		_workspace = new("temporary");
#endif
	}

	/// <summary>
	/// Called when loading workspaces.
	/// </summary>
	/// <param name="workspace"></param>
	public WorkspaceViewModel(Models.Workspace workspace)
	{
		EvaluateCommand = new RelayCommand(Evaluate, Evaluate_CanExecute);
#if MY_WINDOWS_WPF
		DeleteWorkspaceCommand = new RelayCommand(DeleteWorkspace);
		ClearInputCommand = new RelayCommand(ClearInput);
		ClearHistoryCommand = new RelayCommand(ClearHistory, ClearHistory_CanExecute);
		ToggleHelpCommand = new RelayCommand(ToggleHelp);
#else
		DeleteWorkspaceCommand = new AsyncRelayCommand(DeleteWorkspaceAsync);
		RenameWorkspaceCommand = new AsyncRelayCommand(RenameWorkspaceAsync);
		VariablesWorkspaceCommand = new AsyncRelayCommand(VariablesWorkspaceAsync);
		HelpCommand = new AsyncRelayCommand(Help);
#endif

		_workspace = workspace;
	}

	#region Implement IEquatable and Equals Override

	public bool Equals(WorkspaceViewModel? other)
	{
		return _workspace.Equals(other?._workspace);
	}

	public override bool Equals(object? obj) => Equals(obj as WorkspaceViewModel);

	public override int GetHashCode() => _workspace.GetHashCode();

	// Optionally, override the == and != operators
	public static bool operator ==(WorkspaceViewModel left, WorkspaceViewModel right)
	{
		if (left is null)
		{
			return (right is null);
		}
		return left.Equals(right);
	}

	public static bool operator !=(WorkspaceViewModel left, WorkspaceViewModel right)
	{
		return !(left == right);
	}

	#endregion Implement IEquatable and Equals Override

	#region Implement ICloneable Override

	/// <summary>
	/// Clone the workspace view-model.
	/// </summary>
	/// <param name="idNew">ID to use in the clone or null to use the same ID</param>
	/// <returns>Copy of the workspace view-model</returns>
	public WorkspaceViewModel CloneWorkspaceViewModel(Guid? idNew)
	{
		WorkspaceViewModel wvm;
		if (idNew is null)
		{
			wvm = (WorkspaceViewModel)Clone();
		}
		else
		{
			wvm = new(_workspace.CloneWorkspace(idNew));
		}
		return wvm;
	}

	/// <summary>
	/// Copy the workspace view-model.
	/// </summary>
	/// <remarks>
	/// This also COPIES the ID, so there may be circumstances where the caller
	/// needs to change the ID of the cloned workspace.
	/// </remarks>
	/// <returns></returns>
	public object Clone()
	{
		WorkspaceViewModel wvm = new(_workspace.CloneWorkspace(idNew: null));
		return wvm;
	}

	#endregion Implement ICloneable Override

#if MY_WINDOWS_WPF
	public void DeleteWorkspace()
	{
		RaiseRequestDelete();
	}
#endif

#if MY_WINDOWS_WPF
	public void ClearInput()
#else
	private void ClearInput()
#endif
	{
		Input = string.Empty;
	}

#if MY_WINDOWS_WPF
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
#endif

	private bool Evaluate_CanExecute() => !string.IsNullOrWhiteSpace(Input);
	private void Evaluate()
	{
		Debug.Assert(!string.IsNullOrWhiteSpace(Input));

		_workspace.Evaluate(Input);
#if MY_WINDOWS_WPF
		_workspace.LastModifiedUTC = DateTimeOffset.UtcNow;
		ClearHistoryCommand.NotifyCanExecuteChanged();
		CollectionViewSource.GetDefaultView(Variables).Refresh();
#else
		SemanticScreenReader.Announce(_workspace.Variables[MathExpressions.MathEvaluator.AnswerVariable].ToString());
#endif

		// Clear input when we're done.
		ClearInput();

		RaiseWorkspaceChanged();
	}

	/// <summary>
	/// Delete the selection at the cursor, insert the passed
	/// text at the cursor and return the new cursor position.
	/// </summary>
	/// <param name="insertText">Text to insert</param>
	/// <param name="cursorPosition">Where to insert text</param>
	/// <param name="selectionLength">How many characters to replace</param>
	/// <returns>New cursor position</returns>
	public int InsertTextAtCursor(string insertText, int cursorPosition, int selectionLength)
	{
		// Delete selected text and insert new text into the string
		Input = Input
					.Remove(cursorPosition, selectionLength)
					.Insert(cursorPosition, insertText);

		// return cursor position after the inserted variable name
		return cursorPosition + insertText.Length;
	}

#if !MY_WINDOWS_WPF
	/*
		When a page, or the binding context of a page, implements this interface,
		the query string parameters used in navigation are passed to the
		ApplyQueryAttributes method. This view-model is used as the binding context
		for the view. When the view is navigated to, the view's binding context
		(this view-model) is passed the query string parameters used during navigation.

		This code checks if the load key was provided in the query dictionary.
		If this key is found, the value should be the identifier of the model
		object to load. That note is loaded and set as the underlying model
		object of this view-model instance.
	 */
	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue(QUERY_DATA_WORKSPACE, out object? value))
		{
			Debug.Assert(value is not null);

			WorkspaceViewModel viewModel = (WorkspaceViewModel)value;

			// KLUDGE XAML creates a view-model using the default ctor, so the
			// workspaces view-model does not have a chance to add an event handler.
			WorkspaceChanged += viewModel.WorkspaceChanged;

			_workspace = viewModel._workspace;

			// We need to refresh anything based on _workspace.
			RefreshProperties();

			// https://stackoverflow.com/q/73755717/4858
			query.Clear();
		}
	}

	private void RefreshProperties()
	{
		OnPropertyChanged(nameof(Name));
		OnPropertyChanged(nameof(History));
	}

	private async Task DeleteWorkspaceAsync()
	{
		await Shell.Current.GoToAsync($"..?{QUERY_DATA_DELETE}={_workspace.ID}");
	}

	private async Task RenameWorkspaceAsync()
	{
		await Shell.Current.GoToAsync($"..?{QUERY_DATA_RENAME}={_workspace.ID}");
	}

	private async Task VariablesWorkspaceAsync()
	{
#if !MAUI_UNITTESTS
		// Navigate to page that displays the variables from this workspace
		await Shell.Current.GoToAsync(nameof(Views.VariablesPage),
			new Dictionary<string, object>()
			{
				{
					QUERY_DATA_VARIABLES,
					_workspace.Variables
				}
			});
#else
		await Task.Delay(0);
#endif
	}

	private async Task Help()
	{
#if !MAUI_UNITTESTS
		await Shell.Current.GoToAsync(nameof(Views.HelpPage));
#else
		await Task.Delay(0);
#endif
	}
#endif
}
