using CalculateX.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace CalculateX.ViewModels;

internal class WorkspacesViewModel : ObservableObject
{
	private const string CalculateX_FileName = "CalculateX.xml";
	private static readonly string _pathWorkspacesFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), CalculateX_FileName);
	private readonly Workspaces _workspaces = new(_pathWorkspacesFile);

	public ObservableCollection<WorkspaceViewModel> TheWorkspaceViewModels { get; private set; }
	public WorkspaceViewModel SelectedWorkspaceVM
	{
		get => _selectedWorkspaceVM;
		set => SetProperty(ref _selectedWorkspaceVM, value);
	}
	private WorkspaceViewModel _selectedWorkspaceVM;

	private int _windowId = 0;

	//TODO: [RelayCommand] https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/relaycommand
	/// <summary>
	/// Commands for this view-model.
	/// </summary>
	public ICommand NewWorkspaceCommand { get; }
	public RelayCommand SelectPreviousWorkspaceCommand { get; }
	public RelayCommand SelectNextWorkspaceCommand { get; }


	public WorkspacesViewModel()
	{
		NewWorkspaceCommand = new RelayCommand(NewWorkspace);
		SelectPreviousWorkspaceCommand = new RelayCommand(SelectPreviousWorkspace, SelectPreviousWorkspace_CanExecute);
		SelectNextWorkspaceCommand = new RelayCommand(SelectNextWorkspace, SelectNextWorkspace_CanExecute);

//TODO: Handle CollectionChanged and add the below handlers for NewItems and remove them for OldItems (see MSJ article)
//Have to create empty collection first and THEN add vms to it. Use Lazy<>?
		TheWorkspaceViewModels = new(_workspaces.TheWorkspaces.Select(model => new WorkspaceViewModel(model)));
		foreach (var viewModel in TheWorkspaceViewModels)
		{
			SubscribeViewModelEvents(viewModel);
		}

		if (!TheWorkspaceViewModels.Any())
		{
			WorkspaceViewModel newViewModel = CreateWorkspace();
			TheWorkspaceViewModels.Insert(0, newViewModel);
			SelectedWorkspaceVM = newViewModel;
		}

		// Add the "+" tab to the view-model but not to the model.
		TheWorkspaceViewModels.Add(new(canCloseTab: false));

		SelectPreviousWorkspaceCommand.NotifyCanExecuteChanged();
		SelectNextWorkspaceCommand.NotifyCanExecuteChanged();

		_selectedWorkspaceVM = TheWorkspaceViewModels.FirstOrDefault(vm => vm.ID == _workspaces.LoadedSelectedWorkspaceID) ?? TheWorkspaceViewModels.First();
	}

	private void SubscribeViewModelEvents(WorkspaceViewModel viewModel)
	{
		viewModel.RequestDelete += OnRequestDelete;
		viewModel.WorkspaceChanged += OnWorkspaceChanged;
	}

	public void SaveWorkspaces() => _workspaces.SaveWorkspaces(SelectedWorkspaceVM.ID);

	private void OnWorkspaceChanged(object? sender, EventArgs e) => SaveWorkspaces();

	public void SelectWorkspace()
	{
		/// If the user selected a closable tab, select it.
		if (SelectedWorkspaceVM.CanCloseTab)
		{
			SaveWorkspaces();
			return;
		}

		/// The user selected the "+" tab...
		/// Change the non-closable tab to closable and name it.
		SelectedWorkspaceVM.Name = FormWorkspaceName(TheWorkspaceViewModels.Select(w => w.Name));
		SelectedWorkspaceVM.CanCloseTab = true;
		SubscribeViewModelEvents(SelectedWorkspaceVM);
		_workspaces.AddWorkspace(SelectedWorkspaceVM._workspace);

		// Create new non-closable "+" tab (but do not add the workspace to the model).
		TheWorkspaceViewModels.Add(new(canCloseTab: false));

		SelectPreviousWorkspaceCommand.NotifyCanExecuteChanged();
		SelectNextWorkspaceCommand.NotifyCanExecuteChanged();

		SaveWorkspaces();
	}

	private void NewWorkspace()
	{
		Debug.Assert(SelectedWorkspaceVM is not null);
		Debug.Assert(TheWorkspaceViewModels.Count(w => w.CanCloseTab) > 0);
		Debug.Assert(SelectNextWorkspace_CanExecute());

		WorkspaceViewModel newViewModel = CreateWorkspace();

		TheWorkspaceViewModels.Insert(TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM) + 1, newViewModel);

		SelectNextWorkspace();
	}

	private WorkspaceViewModel CreateWorkspace()
	{
		Workspace newWorkspace = new(FormWorkspaceName(TheWorkspaceViewModels.Select(w => w.Name)));
		_workspaces.AddWorkspace(newWorkspace);

		WorkspaceViewModel viewModel = new(newWorkspace);
		SubscribeViewModelEvents(viewModel);

		return viewModel;
	}

	private void OnRequestDelete(object? /*WorkspaceViewModel*/ sender, EventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		WorkspaceViewModel deletedWorkspaceVM = (WorkspaceViewModel)sender;

		if (deletedWorkspaceVM.ID == SelectedWorkspaceVM.ID)
		{
			var remainingWorkspaces = TheWorkspaceViewModels.Except(new[] { deletedWorkspaceVM });

			/// If closing last tab (except for "+" tab), create one.
			if (!remainingWorkspaces.Any(w => w.CanCloseTab))
			{
				/// [deleted][+]
				WorkspaceViewModel newViewModel = CreateWorkspace();
				TheWorkspaceViewModels.Insert(0, newViewModel);
				/// [new][deleted][+]
			}

			/// Select next tab (unless it's the "+" tab, then select previous tab).
			if (deletedWorkspaceVM.ID == TheWorkspaceViewModels[^2].ID)
			{
				/// [a]...[z][deleted][+]
				SelectPreviousWorkspace();
			}
			else
			{
				/// [a]...[z][deleted][a]...[z][+]
				SelectNextWorkspace();
			}
		}

		// Delete workspace model
		_workspaces.DeleteWorkspace(deletedWorkspaceVM.ID);

		// Delete workspace view-model
		TheWorkspaceViewModels.Remove(deletedWorkspaceVM);

		SelectPreviousWorkspaceCommand.NotifyCanExecuteChanged();
		SelectNextWorkspaceCommand.NotifyCanExecuteChanged();

		SaveWorkspaces();
	}

	private bool SelectPreviousWorkspace_CanExecute()
	{
		return (TheWorkspaceViewModels.Count(w => w.CanCloseTab) > 1);  // Do not count the "+" tab
	}
	private void SelectPreviousWorkspace()
	{
		Debug.Assert(SelectedWorkspaceVM is not null);
		Debug.Assert(TheWorkspaceViewModels.Count(w => w.CanCloseTab) > 1);

		if (SelectedWorkspaceVM == TheWorkspaceViewModels.First(w => w.CanCloseTab))
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels.Last(w => w.CanCloseTab);
		}
		else
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels[TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM) - 1];
		}
	}

	private bool SelectNextWorkspace_CanExecute()
	{
		return (TheWorkspaceViewModels.Count(w => w.CanCloseTab) > 1);  // Do not count the "+" tab
	}
	private void SelectNextWorkspace()
	{
		Debug.Assert(SelectedWorkspaceVM is not null);
		Debug.Assert(TheWorkspaceViewModels.Count(w => w.CanCloseTab) > 1);

		if (SelectedWorkspaceVM == TheWorkspaceViewModels.Last(w => w.CanCloseTab))
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels.First(w => w.CanCloseTab);
		}
		else
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels[TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM) + 1];
		}
	}

	private string FormWorkspaceName(IEnumerable<string> bannedNames)
	{
		// If we have closed the last tab, reset the window ID.
		// (This prevents the ID from incrementing when we close the last tab.)
		// N.B. WPF version was this: if (Workspaces.Count(w => w.CanCloseTab) == 1)
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
