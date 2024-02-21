using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace CalculateX.ViewModels;

internal class WorkspaceViewModel : ObservableObject, IQueryAttributable
{
	public const string QUERY_DATA_WORKSPACE = "workspace";
	public const string QUERY_DATA_DELETE = "delete";
	public const string QUERY_DATA_RENAME = "rename";
	public const string QUERY_DATA_VARIABLES = "variables";
	public const string QUERY_DATA_VARIABLE_NAME = "variable";
	public const string QUERY_DATA_VARIABLE_VALUE = "value";

	private Models.Workspace _workspace;

	public string ID => _workspace.ID;
	public string Name
	{
		get => _workspace.Name;
		set => SetProperty(_workspace.Name, value, _workspace, (model, newValue) => model.Name = newValue);
	}
	//TODO: We do not need this.
	public bool CanCloseTab
	{
		get => _canCloseTab;
		private set => SetProperty(ref _canCloseTab, value);
	}
	private bool _canCloseTab = true;

	public ObservableCollection<Models.Workspace.HistoryEntry> History => _workspace.History;

	private string _input = string.Empty;
	public string Input
	{
		get => _input;
		set => SetProperty(ref _input, value);
	}

	public event EventHandler? WorkspaceChanged;
	public void RaiseWorkspaceChanged() => WorkspaceChanged?.Invoke(this, EventArgs.Empty);

	/*
	 * MAUI BUG
	 * Unable to set Input and have it change Entry.Text.
	 * So, we raise an event to update the Entry control's text.
	 */
	public event EventHandler? InputChanged;
	public void RaiseInputChanged() => InputChanged?.Invoke(this, EventArgs.Empty);

	public ICommand EvaluateCommand { get; }
	//public ICommand InputBackwardCommand { get; }
	//public ICommand InputForwardCommand { get; }
	public ICommand DeleteWorkspaceCommand { get; }
	public ICommand RenameWorkspaceCommand { get; }
	public ICommand VariablesWorkspaceCommand { get; }
	public ICommand HelpCommand { get; }


	/// <summary>
	/// Called when navigating to a workspace.
	/// </summary>
	public WorkspaceViewModel()
	{
		EvaluateCommand = new RelayCommand(Evaluate, Evaluate_CanExecute);
		//InputBackwardCommand = new RelayCommand(InputBackward);
		//InputForwardCommand = new RelayCommand(InputForward);
		DeleteWorkspaceCommand = new AsyncRelayCommand(DeleteWorkspaceAsync);
		RenameWorkspaceCommand = new AsyncRelayCommand(RenameWorkspaceAsync);
		VariablesWorkspaceCommand = new AsyncRelayCommand(VariablesWorkspaceAsync);
		HelpCommand = new AsyncRelayCommand(Help);

		// We always go on to ApplyQueryAttributes() to select a workspace.
		// We don't need this workspace, but we do not want to allow null.
		_workspace = new("temporary");
	}

	/// <summary>
	/// Called when loading workspaces.
	/// </summary>
	/// <param name="workspace"></param>
	public WorkspaceViewModel(Models.Workspace workspace)
	{
		EvaluateCommand = new RelayCommand(Evaluate, Evaluate_CanExecute);
		//InputBackwardCommand = new RelayCommand(InputBackward);
		//InputForwardCommand = new RelayCommand(InputForward);
		DeleteWorkspaceCommand = new AsyncRelayCommand(DeleteWorkspaceAsync);
		RenameWorkspaceCommand = new AsyncRelayCommand(RenameWorkspaceAsync);
		VariablesWorkspaceCommand = new AsyncRelayCommand(VariablesWorkspaceAsync);
		HelpCommand = new AsyncRelayCommand(Help);

		_workspace = workspace;
	}

	public void SetInput(string s)
	{
		Input = s;

		/*
		 * BUG This does not work. MAUI bug.
		 */
		//OnPropertyChanged(nameof(Input));

		RaiseInputChanged();
	}

	//private void InputBackward()
	//{
	//	string entry = _workspace.InputBackward(Input);
	//	SetInput(entry);
	//}

	//private void InputForward()
	//{
	//	string entry = _workspace.InputForward(Input);
	//	SetInput(entry);
	//}

	public bool Evaluate_CanExecute() => !string.IsNullOrWhiteSpace(Input);
	private void Evaluate()
	{
		if (string.IsNullOrWhiteSpace(Input))
		{
			return;
		}

		_workspace.Evaluate(Input);

		SemanticScreenReader.Announce(_workspace.Variables[MathExpressions.MathEvaluator.AnswerVariable].ToString());

		// Clear input when we're done.
		SetInput(string.Empty);

		RaiseWorkspaceChanged();
	}

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
		OnPropertyChanged(nameof(CanCloseTab));
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
		// Navigate to page that displays the variables from this workspace
		await Shell.Current.GoToAsync(nameof(Views.VariablesPage),
			new Dictionary<string, object>()
			{
				{
					QUERY_DATA_VARIABLES,
					_workspace.Variables
				}
			});
	}

	private async Task Help()
	{
		await Shell.Current.GoToAsync(nameof(Views.HelpPage));
	}
}
