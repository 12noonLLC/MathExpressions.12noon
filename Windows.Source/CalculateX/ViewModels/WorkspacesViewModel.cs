using CalculateX.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CalculateX.ViewModels;

public class WorkspacesViewModel : ObservableObject
{
	private readonly Workspaces _workspaces;
	public ObservableCollection<WorkspaceViewModel> TheWorkspaceViewModels { get; private init; }
	private readonly Func<WorkspaceViewModel, WorkspaceViewModel, bool> SortByName = (WorkspaceViewModel item1, WorkspaceViewModel item2) => (item1.Name.CompareTo(item2.Name) < 0);

	public WorkspaceViewModel SelectedWorkspaceVM
	{
		get => _selectedWorkspaceVM;
		set => SetProperty(ref _selectedWorkspaceVM, value);
	}
	private WorkspaceViewModel _selectedWorkspaceVM;

	private int _windowNumber = 0;

	//TODO: [RelayCommand] https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/relaycommand
	/// <summary>
	/// Commands for this view-model.
	/// </summary>
	public ICommand AddWorkspaceCommand { get; }
	public ICommand DeleteWorkspaceCommand { get; }
	public RelayCommand SelectPreviousWorkspaceCommand { get; }
	public RelayCommand SelectNextWorkspaceCommand { get; }


	public WorkspacesViewModel(string pathWorkspacesFile)
	{
		_workspaces = new(pathWorkspacesFile);

		AddWorkspaceCommand = new RelayCommand(AddWorkspace);
		DeleteWorkspaceCommand = new RelayCommand(DeleteWorkspace);
		SelectPreviousWorkspaceCommand = new RelayCommand(SelectPreviousWorkspace);
		SelectNextWorkspaceCommand = new RelayCommand(SelectNextWorkspace);

//TODO: Handle CollectionChanged and add the below handlers for NewItems and remove them for OldItems (see MSJ article)
//Have to create empty collection first and THEN add vms to it. Use Lazy<>?
		TheWorkspaceViewModels = new(
			_workspaces.TheWorkspaces
			.Select(model => new WorkspaceViewModel(model))
			.OrderBy(vm => vm.Name)
		);
		foreach (var viewModel in TheWorkspaceViewModels)
		{
			SubscribeViewModelEvents(viewModel);
		}

		if (!TheWorkspaceViewModels.Any())
		{
			AddWorkspace();
		}

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


	/// <summary>
	/// Called when the user presses a toolbar button or shortcut to create a new workspace.
	/// </summary>
	private void AddWorkspace()
	{
		WorkspaceViewModel newViewModel = CreateWorkspace();
		TheWorkspaceViewModels.AddSorted(newViewModel, SortByName);

		SelectedWorkspaceVM = newViewModel;
		SaveWorkspaces();
	}

	private WorkspaceViewModel CreateWorkspace()
	{
		Workspace newWorkspace = new(FormWorkspaceName());
		_workspaces.AddWorkspace(newWorkspace);

		WorkspaceViewModel viewModel = new(newWorkspace);
		SubscribeViewModelEvents(viewModel);

		return viewModel;
	}

	public void RenameWorkspace(WorkspaceViewModel workspaceVM, string name)
	{
		workspaceVM.Name = name;

		// If the sorted location is different, move the workspace VM to the sorted location.
		int ixSorted = TheWorkspaceViewModels.FindSortedIndex(workspaceVM, SortByName);
		int ixCur = TheWorkspaceViewModels.IndexOf(workspaceVM);
		if (ixSorted != ixCur)
		{
			// The destination index is the position within the existing list,
			// so we have to pretend the item has been removed from the list.
			if (ixSorted > ixCur)
			{
				--ixSorted;
			}

			// If they're still different, move the view-model.
			if (ixSorted != ixCur)
			{
				TheWorkspaceViewModels.Move(ixCur, ixSorted);
			}
		}

		SaveWorkspaces();
	}

	private void OnRequestDelete(object? /*WorkspaceViewModel*/ sender, EventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		WorkspaceViewModel deletedWorkspaceVM = (WorkspaceViewModel)sender;

		DeleteWorkspace(deletedWorkspaceVM);
	}

	private void DeleteWorkspace()
	{
		DeleteWorkspace(SelectedWorkspaceVM);
	}

	private void DeleteWorkspace(WorkspaceViewModel deletedWorkspaceVM)
	{
		if (MessageBox.Show($"Do you want to delete the \"{SelectedWorkspaceVM.Name}\" workspace?", "Delete Workspace", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
		{
			return;
		}

		// Delete workspace model
		/// Note: We must remove the workspace model first so <see cref="FormWorkspaceName"/> knows to reset the workspaceNumber.
		_workspaces.DeleteWorkspace(deletedWorkspaceVM.ID);

		if (deletedWorkspaceVM.ID == SelectedWorkspaceVM.ID)
		{
			int indexSelectedWorkspace = TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM);
			bool isLastWorkspace = (deletedWorkspaceVM.ID == TheWorkspaceViewModels[^1].ID);

			/// If closing last workspace, create one.
			if (!_workspaces.TheWorkspaces.Any())
			{
				Debug.Assert(TheWorkspaceViewModels.Count == 1);

				/// [deleted]
				WorkspaceViewModel newViewModel = CreateWorkspace();
				TheWorkspaceViewModels.Insert(0, newViewModel);
				/// [new][deleted]
			}

			/// Select the workspace we want to be selected AFTER we delete the workspace.

			/// Select workspace that will be in the same position.
			if (isLastWorkspace)
			{
				/// [a]...[z][deleted]
				SelectedWorkspaceVM = TheWorkspaceViewModels[^2];
			}
			else
			{
				/// [a]...[m][deleted][n]...[z]
				SelectedWorkspaceVM = TheWorkspaceViewModels[indexSelectedWorkspace + 1];
			}
		}
		else
		{
			// Note: No need to change the selected workspace.
		}

		// Note: We must remove the view-model last because the binding changes the selected tab.
		// Delete workspace view-model
		TheWorkspaceViewModels.Remove(deletedWorkspaceVM);

		SelectPreviousWorkspaceCommand.NotifyCanExecuteChanged();
		SelectNextWorkspaceCommand.NotifyCanExecuteChanged();

		SaveWorkspaces();
	}

	private void SelectPreviousWorkspace()
	{
		Debug.Assert(SelectedWorkspaceVM is not null);
		Debug.Assert(TheWorkspaceViewModels.Count > 0);

		if (SelectedWorkspaceVM == TheWorkspaceViewModels.First())
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels.Last();
		}
		else
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels[TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM) - 1];
		}
	}

	private void SelectNextWorkspace()
	{
		Debug.Assert(SelectedWorkspaceVM is not null);
		Debug.Assert(TheWorkspaceViewModels.Count > 0);

		if (SelectedWorkspaceVM == TheWorkspaceViewModels.Last())
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels.First();
		}
		else
		{
			SelectedWorkspaceVM = TheWorkspaceViewModels[TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM) + 1];
		}
	}

	private string FormWorkspaceName()
	{
		// If we have closed the last tab, reset the window ID.
		// (This prevents the ID from incrementing when we close the last tab.)
		if (!_workspaces.TheWorkspaces.Any())
		{
			_windowNumber = 0;
		}

		IEnumerable<string> bannedNames = _workspaces.TheWorkspaces.Select(w => w.Name);

		string name = string.Empty;
		do
		{
			++_windowNumber;
			name = $"{nameof(Workspace)}{_windowNumber}";
		} while (bannedNames.Any(n => n == name));

		return name;
	}
}
