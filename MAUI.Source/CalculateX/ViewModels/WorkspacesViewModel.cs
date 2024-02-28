using CalculateX.Models;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CalculateX.ViewModels;

public class WorkspacesViewModel
{
	private readonly Workspaces _workspaces;
	public ObservableCollection<WorkspaceViewModel> TheWorkspaceViewModels { get; private init; }

	private string? _selectedWorkspaceID;

	private int _windowId = 0;

	public ICommand AddWorkspaceCommand { get; }
	public ICommand AboutCommand { get; }
	public ICommand SelectWorkspaceCommand { get; }


	public WorkspacesViewModel(string pathWorkspacesFile)
	{
		_workspaces = new(pathWorkspacesFile);

		AddWorkspaceCommand = new RelayCommand(AddWorkspace);
		AboutCommand = new AsyncRelayCommand(About);
		SelectWorkspaceCommand = new AsyncRelayCommand<WorkspaceViewModel>(SelectWorkspaceAsync);

		TheWorkspaceViewModels = new(_workspaces.TheWorkspaces.Select(model => new WorkspaceViewModel(model)));
		foreach (var workspace in TheWorkspaceViewModels)
		{
			workspace.WorkspaceChanged += OnWorkspaceChanged;
		}

		if (!TheWorkspaceViewModels.Any())
		{
			AddWorkspace();
			SaveWorkspaces();
		}
	}

	private void AddWorkspace()
	{
		Workspace newWorkspace = new(FormWorkspaceName(TheWorkspaceViewModels.Select(w => w.Name)));
		_workspaces.AddWorkspace(newWorkspace);

		WorkspaceViewModel viewModel = new(newWorkspace);
		viewModel.WorkspaceChanged += OnWorkspaceChanged;
		TheWorkspaceViewModels.Add(viewModel);

		_selectedWorkspaceID ??= newWorkspace.ID;
	}

	private async Task About()
	{
		await Shell.Current.GoToAsync(nameof(Views.AboutPage));
	}

	public void SaveWorkspaces() => _workspaces.SaveWorkspaces(_selectedWorkspaceID);

	public void DeleteWorkspace(WorkspaceViewModel workspaceVM)
	{
		// Delete workspace model
		_workspaces.DeleteWorkspace(workspaceVM.ID);

		// Delete workspace view-model
		TheWorkspaceViewModels.Remove(workspaceVM);

		// If closing last tab (except for "+" tab), create one.
		if (!TheWorkspaceViewModels.Any(w => w.CanCloseTab))
		{
			AddWorkspace();
		}

		SaveWorkspaces();
	}

	public void RenameWorkspace(WorkspaceViewModel workspaceVM, string name)
	{
		workspaceVM.Name = name;

		SaveWorkspaces();
	}

	private void OnWorkspaceChanged(object? sender, EventArgs e) => SaveWorkspaces();

	public async Task SelectWorkspaceAsync(WorkspaceViewModel? workspaceVM)
	{
		if (workspaceVM is null)
		{
			return;
		}

		_selectedWorkspaceID = workspaceVM.ID;

		// Should navigate to "NotePage?ItemId=path\on\device\XYZ.notes.txt"
		//Shell.Current.GoToAsync($"{nameof(Views.WorkspacePage)}?select={workspace.ID}");
		await Shell.Current.GoToAsync(nameof(Views.WorkspacePage),
			new Dictionary<string, object>()
			{
				{
					WorkspaceViewModel.QUERY_DATA_WORKSPACE,
					TheWorkspaceViewModels.First(workspaceView => workspaceView.ID == workspaceVM.ID)
				}
			});
	}

	private string FormWorkspaceName(IEnumerable<string> bannedNames)
	{
		// If we have closed the last tab, reset the window ID.
		// (This prevents the ID from incrementing when we close the last tab.)
		if (!TheWorkspaceViewModels.Any(w => w.CanCloseTab))
		{
			_windowId = 0;
		}

		string name = string.Empty;
		do
		{
			++_windowId;
			name = $"{nameof(Workspace)}{_windowId}";
		} while (bannedNames.Any(n => n == name));

		return name;
	}
}
