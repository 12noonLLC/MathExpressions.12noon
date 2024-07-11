using CalculateX.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.Input;
using Shared;
#if MY_WINDOWS_WPF
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
#else
using Microsoft.Graph;
using Microsoft.Graph.Models;
#endif

namespace CalculateX.ViewModels;

public class WorkspacesViewModel
#if MY_WINDOWS_WPF
	: ObservableObject
#endif
{
	private readonly Workspaces _workspaces;
	public Workspaces TEST_Workspaces => _workspaces;
	public ObservableCollection<WorkspaceViewModel> TheWorkspaceViewModels { get; private init; }
	private readonly Func<WorkspaceViewModel, WorkspaceViewModel, bool> SortByName = (WorkspaceViewModel item1, WorkspaceViewModel item2) => (item1.Name.CompareTo(item2.Name) < 0);

#if MY_WINDOWS_WPF
	private WorkspaceViewModel _selectedWorkspaceVM;
	public WorkspaceViewModel SelectedWorkspaceVM
	{
		get => _selectedWorkspaceVM;
		set => SetProperty(ref _selectedWorkspaceVM, value);
	}
#else
	private string? _selectedWorkspaceID;
#endif
	private int _windowId = 0;

	//TODO: Windows: [RelayCommand] https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/relaycommand
	/// <summary>
	/// Commands for this view-model.
	/// </summary>
	public ICommand AddWorkspaceCommand { get; }
#if MY_WINDOWS_WPF
	public ICommand DeleteSelectedWorkspaceCommand { get; }
	public RelayCommand SelectPreviousWorkspaceCommand { get; }
	public RelayCommand SelectNextWorkspaceCommand { get; }
#else
	public ICommand SynchronizeCommand { get; }
	public ICommand AboutCommand { get; }
	public ICommand SelectWorkspaceCommand { get; }
#endif


	public static WorkspacesViewModel ConstructFromFile(string pathWorkspacesFile)
	{
		Debug.Assert(!string.IsNullOrEmpty(pathWorkspacesFile));

		// Ensure there is at least a default file.
		if (!File.Exists(pathWorkspacesFile))
		{
			// Create a default file with an empty set of workspaces
			new XDocument(
				new XElement(Workspaces.NAME_ELEMENT_ROOT,
					new XAttribute(Workspaces.NAME_ATTRIBUTE_SYNCHRONIZED, DateTimeOffset.UtcNow),
					new XElement(Workspaces.NAME_ELEMENT_WORKSPACES),
					new XElement(Workspaces.NAME_ELEMENT_DELETED_WORKSPACES)
				)
			)
			.Save(pathWorkspacesFile);
		}

		XDocument xdoc = XDocument.Load(pathWorkspacesFile);
		return new WorkspacesViewModel(xdoc);
	}

	public WorkspacesViewModel(XDocument xdocWorkspaces)
	{
		_workspaces = new(xdocWorkspaces);

		AddWorkspaceCommand = new RelayCommand(AddWorkspace);
#if MY_WINDOWS_WPF
		DeleteSelectedWorkspaceCommand = new RelayCommand(DeleteSelectedWorkspace);
		SelectPreviousWorkspaceCommand = new RelayCommand(SelectPreviousWorkspace);
		SelectNextWorkspaceCommand = new RelayCommand(SelectNextWorkspace);
#else
		SynchronizeCommand = new AsyncRelayCommand<string>(SynchronizeWorkspaces);
		AboutCommand = new AsyncRelayCommand(About);
		SelectWorkspaceCommand = new AsyncRelayCommand<WorkspaceViewModel>(SelectWorkspaceAsync);
#endif

		/*
		 * WINDOWS:
		 * TODO: Handle CollectionChanged and add the below handlers for NewItems and remove them for OldItems (see MSJ article)
		 * Have to create empty collection first and THEN add vms to it. Use Lazy<>?
		 */
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
#if MY_WINDOWS_WPF
			//TODO: Should we call SaveWorkspaces() for Windows, too?? AddWorkspace calls it. Should we do that for Android?
#else
			SaveWorkspaces();
#endif
		}

#if MY_WINDOWS_WPF
		SelectPreviousWorkspaceCommand.NotifyCanExecuteChanged();
		SelectNextWorkspaceCommand.NotifyCanExecuteChanged();

		_selectedWorkspaceVM = TheWorkspaceViewModels.FirstOrDefault(wvm => wvm.ID == _workspaces.LoadedSelectedWorkspaceID) ?? TheWorkspaceViewModels.First();
#endif
	}

	private void SubscribeViewModelEvents(WorkspaceViewModel viewModel)
	{
#if MY_WINDOWS_WPF
		viewModel.RequestDelete += OnRequestDelete;
#endif
		viewModel.WorkspaceChanged += OnWorkspaceChanged;
	}

#if MY_WINDOWS_WPF
	public void SaveWorkspaces() => _workspaces.SaveWorkspaces(SelectedWorkspaceVM.ID);
#else
	public void SaveWorkspaces() => _workspaces.SaveWorkspaces(_selectedWorkspaceID ?? string.Empty);
#endif

	private void OnWorkspaceChanged(object? sender, EventArgs e) => SaveWorkspaces();


	/// <summary>
	/// Called when the user presses a toolbar button or shortcut to create a new workspace.
	/// </summary>
	private void AddWorkspace()
	{
		WorkspaceViewModel newViewModel = CreateWorkspace();
		AddWorkspaceViewModel(newViewModel);

#if MY_WINDOWS_WPF
		SelectedWorkspaceVM = newViewModel;
		SaveWorkspaces();
#endif
	}

	private void AddWorkspaceViewModel(WorkspaceViewModel workspaceVM)
	{
		TheWorkspaceViewModels.AddSorted(workspaceVM, SortByName);
	}

	/// <summary>
	/// Create a new workspace and add it to the collection of workspaces.
	/// Create a new workspace view-model and subscribe to its events.
	/// The caller must add the returned workspace view-model to
	/// the collection of workspace view-models.
	/// </summary>
	/// <returns>New workspace view-model</returns>
	private WorkspaceViewModel CreateWorkspace()
	{
		Workspace newWorkspace = new(FormWorkspaceName());
		_workspaces.AddWorkspace(newWorkspace);

		WorkspaceViewModel viewModel = new(newWorkspace);
		SubscribeViewModelEvents(viewModel);

#if MY_WINDOWS_WPF
#else
		_selectedWorkspaceID ??= newWorkspace.ID;
#endif

		return viewModel;
	}

#if MY_WINDOWS_WPF
#else
	private async Task About()
	{
#if !MAUI_UNITTESTS
		await Shell.Current.GoToAsync(nameof(Views.AboutPage));
#else
		await Task.Delay(0);
#endif
	}
#endif

	public void RenameWorkspace(WorkspaceViewModel workspaceVM, string name)
	{
		workspaceVM.Name = name;

		SortExistingWorkspace(workspaceVM);

		SaveWorkspaces();
	}

	private void SortExistingWorkspace(WorkspaceViewModel workspaceVM)
	{
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
	}

#if MY_WINDOWS_WPF
	private void OnRequestDelete(object? /*WorkspaceViewModel*/ sender, EventArgs e)
	{
		ArgumentNullException.ThrowIfNull(sender);

		WorkspaceViewModel deletedWorkspaceVM = (WorkspaceViewModel)sender;

		DeleteWorkspaceWithConfirmation(deletedWorkspaceVM);
	}

	public void TEST_DeleteSelectedWorkspace() => DeleteSelectedWorkspace();
	private void DeleteSelectedWorkspace()
	{
		DeleteWorkspaceWithConfirmation(SelectedWorkspaceVM);
	}

	/// <summary>
	/// Prompt the user to delete the passed workspace.
	/// </summary>
	/// <param name="deletedWorkspaceVM"></param>
	private void DeleteWorkspaceWithConfirmation(WorkspaceViewModel deletedWorkspaceVM)
	{
		if (!App.MyAlertService.PromptForConfirmation($"Do you want to delete the \"{SelectedWorkspaceVM.Name}\" workspace?", "Delete Workspace"))
		{
			return;
		}

		DeleteWorkspace(deletedWorkspaceVM);

		SaveWorkspaces();
	}

	private void DeleteWorkspace(WorkspaceViewModel deletedWorkspaceVM)
	{
		// Delete workspace model
		/// Note: We must remove the workspace model first so <see cref="FormWorkspaceName"/> knows to reset the workspaceNumber.
		_workspaces.DeleteWorkspace(deletedWorkspaceVM.ID);

		if (deletedWorkspaceVM == SelectedWorkspaceVM)
		{
			int indexSelectedWorkspace = TheWorkspaceViewModels.IndexOf(SelectedWorkspaceVM);
			bool isLastWorkspace = (deletedWorkspaceVM == TheWorkspaceViewModels[^1]);

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
#else
	// Android does not delete the selected workspace.
	public void TEST_DeleteWorkspace(WorkspaceViewModel deletedWorkspaceVM) => DeleteWorkspace(deletedWorkspaceVM);

	public void DeleteWorkspace(WorkspaceViewModel deletedWorkspaceVM)
	{
		// Delete workspace model
		_workspaces.DeleteWorkspace(deletedWorkspaceVM.ID);

		// Delete workspace view-model
		TheWorkspaceViewModels.Remove(deletedWorkspaceVM);

		// If closing last workspace, create one.
		if (TheWorkspaceViewModels.Count == 0)
		{
			AddWorkspace();
		}

		SaveWorkspaces();
	}

	public async Task SelectWorkspaceAsync(WorkspaceViewModel? workspaceVM)
	{
		if (workspaceVM is null)
		{
			return;
		}

		_selectedWorkspaceID = workspaceVM.ID;

#if !MAUI_UNITTESTS
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
#else
		await Task.Delay(0);
#endif
	}
#endif

	private string FormWorkspaceName()
	{
		// If we have closed the last tab, reset the window ID.
		// (This prevents the ID from incrementing when we close the last tab.)
		if (!_workspaces.TheWorkspaces.Any())
		{
			_windowId = 0;
		}

		IEnumerable<string> bannedNames = _workspaces.TheWorkspaces.Select(w => w.Name);

		string name = string.Empty;
		do
		{
			++_windowId;
			name = $"{nameof(Workspace)}{_windowId}";
		} while (bannedNames.Any(n => n == name));

		return name;
	}

	#region Synchronize Workspaces

#if MY_WINDOWS_WPF
	// Windows needs to synchronize with a file--its own storage file when it's synchronized with OneDrive.
	public void SynchronizeWorkspaces(string pathStorageFile)
	{
		WorkspacesViewModel vmWorkspacesThat = ConstructFromFile(pathStorageFile);
		Synchronize(vmWorkspacesThat);
	}
#elif ANDROID
	// Android needs to sync with OneDrive directly.
	public async Task SynchronizeWorkspaces(string pathStorageFile)
	{
		XDocument? xdocThat = await TheOneDrive.DownloadXDocument(pathStorageFile);
		if (xdocThat is null)
		{
			return;
		}

		WorkspacesViewModel vmWorkspacesThat = new(xdocThat);

		Synchronize(vmWorkspacesThat);
	}
#else
	// We do not support iOS (yet).
	private async Task SynchronizeWorkspaces(string pathStorageFile)
	{
		await Task.Delay(0);
		//return Task.CompletedTask;
	}
#endif

	//private static bool IsThisMoreCurrent(Workspace wThis, Workspace wThat)
	//{
	//	// if This is more current than That
	//	return wThis.LastModifiedUTC.CompareTo(wThat.LastModifiedUTC) > 0;
	//}
	//private static bool IsThisMoreCurrent(Workspace wThis, DateTimeOffset dtoLastSynchronized)
	//{
	//	// if This is more current than (typically) That Workspaces
	//	return wThis.LastModifiedUTC.CompareTo(dtoLastSynchronized) > 0;
	//}

	private static readonly object gate = new(); // prevent simultaneous synchronizations

	public void TEST_Synchronize(WorkspacesViewModel vmWorkspacesThat) => Synchronize(vmWorkspacesThat);
	private void Synchronize(WorkspacesViewModel vmWorkspacesThat)
	{
		lock (gate)
		{
			// Sync deleted workspaces
			SyncRemoveDeletedWorkspaces(this, vmWorkspacesThat);
			SyncRemoveDeletedWorkspaces(vmWorkspacesThat, this);

			// Sync updated workspaces
			SyncWorkspacesInputs(this, vmWorkspacesThat);
			SyncWorkspacesInputs(vmWorkspacesThat, this);

			// Sync added workspaces
			SyncAddNewWorkspaces(this, vmWorkspacesThat);
			SyncAddNewWorkspaces(vmWorkspacesThat, this);

			// update last-synchronized for both workspace sets
			_workspaces.LastSynchronizedUTC = DateTimeOffset.UtcNow;
			vmWorkspacesThat._workspaces.LastSynchronizedUTC = _workspaces.LastSynchronizedUTC;
		}

		SaveWorkspaces();
	}

	/// <summary>
	/// This method is responsible for synchronizing the deleted workspaces between
	/// two instances of the <see cref="WorkspacesViewModel"/> class.
	/// It removes the deleted workspaces from the second instance if they exist
	/// in the first instance.
	// The synchronization is performed by comparing the IDs of the deleted
	// workspaces in `this` with the IDs of the workspaces in `that`.
	// If a match is found, the workspace is removed from `that` and added to the
	// collection of deleted workspaces of `that`.
	/// </summary>
	/// <param name="vmWorkspacesThis">The first instance of the <see cref="WorkspacesViewModel"/> class.</param>
	/// <param name="vmWorkspacesThat">The second instance of the <see cref="WorkspacesViewModel"/> class.</param>
	private static void SyncRemoveDeletedWorkspaces(WorkspacesViewModel vmWorkspacesThis, WorkspacesViewModel vmWorkspacesThat)
	{
		vmWorkspacesThat
			.TheWorkspaceViewModels
			.Where(wvm => vmWorkspacesThis._workspaces._deletedWorkspaces.Any(wDeleted => wDeleted == wvm._workspace))
			.ToList()
			// List of workspace view-models to be deleted from `that`.
			.ForEach(wvmToDelete =>
			{
				vmWorkspacesThat.DeleteWorkspace(wvmToDelete);

				/// This is the same as <see cref="DeleteWorkspace(string)"/>
				/// except that we set `LastModifiedUTC` the same as the
				/// already-deleted one so the unit tests work.
				// copy `LastModifiedUTC`
				Workspace workspaceDeleted = vmWorkspacesThis._workspaces._deletedWorkspaces.First(w => w == wvmToDelete._workspace);
				wvmToDelete._workspace.LastModifiedUTC = workspaceDeleted.LastModifiedUTC;
			});
	}

	/// <summary>
	/// Copy new workspaces from one set of workspaces to another.
	/// </summary>
	/// <param name="wvmThis">The source workspaces view-model containing any new workspaces.</param>
	/// <param name="wvmThat">The target workspaces view-model to synchronize the new workspaces to.</param>
	private static void SyncAddNewWorkspaces(WorkspacesViewModel wvmThis, WorkspacesViewModel wvmThat)
	{
		foreach (WorkspaceViewModel workspacevmThis in wvmThis.TheWorkspaceViewModels)
		{
			// if This workspace VM is NOT in That VM's set of workspaces, copy it over.
			if (!wvmThat.TheWorkspaceViewModels.Any(wvm => wvm == workspacevmThis))
			{
				// copy This workspace view-model to That workspaces view-model
				WorkspaceViewModel workspacevmCopy = workspacevmThis.CloneWorkspaceViewModel(idNew: null);

				// add copy to That set of workspace view-models
				wvmThat.AddWorkspaceViewModel(workspacevmCopy);

				/// Internal Housekeeping <see cref="CreateWorkspace"/>
				wvmThat._workspaces.AddWorkspace(workspacevmCopy._workspace);
				wvmThat.SubscribeViewModelEvents(workspacevmCopy);
			}
		}
	}

	private static void SyncWorkspacesInputs(WorkspacesViewModel wvmThis, WorkspacesViewModel wvmThat)
	{
		List<WorkspaceViewModel> newWorkspaceVMsThis = [];
		List<WorkspaceViewModel> newWorkspaceVMsThat = [];

		/// Add the workspaces that are in This but not in That to newWorkspaceVMsThis.
		newWorkspaceVMsThis.AddRange(wvmThis.TheWorkspaceViewModels.Except(wvmThat.TheWorkspaceViewModels));
		/// Add the workspaces that are in That but not in This to newWorkspaceVMsThat.
		newWorkspaceVMsThat.AddRange(wvmThat.TheWorkspaceViewModels.Except(wvmThis.TheWorkspaceViewModels));

		/// Find the workspaces that are in both This and That. Call SyncWorkspaceInputs on each.
		foreach (WorkspaceViewModel workspacevmThis in wvmThis.TheWorkspaceViewModels)
		{
			WorkspaceViewModel? workspacevmThat = wvmThat.TheWorkspaceViewModels.FirstOrDefault(wvm => wvm == workspacevmThis);
			if (workspacevmThat is not null)
			{
				SyncWorkspaceInputs(workspacevmThis, workspacevmThat, newWorkspaceVMsThis, newWorkspaceVMsThat);
			}
		}

#if MY_WINDOWS_WPF
		WorkspaceViewModel selected = wvmThis.SelectedWorkspaceVM;
#else
		string? selected = wvmThis._selectedWorkspaceID;
#endif

		ReplaceWorkspaceViewModels(wvmThis, newWorkspaceVMsThis);
		ReplaceWorkspaceViewModels(wvmThat, newWorkspaceVMsThat);

#if MY_WINDOWS_WPF
		wvmThis.SelectedWorkspaceVM = wvmThis.TheWorkspaceViewModels.FirstOrDefault(wvm => wvm == selected) ?? wvmThis.TheWorkspaceViewModels.First();
#else
		wvmThis._selectedWorkspaceID = selected;
#endif
	}

	private static void ReplaceWorkspaceViewModels(WorkspacesViewModel wvm, List<WorkspaceViewModel> newWorkspaceVMs)
	{
		wvm.TheWorkspaceViewModels.Clear();
		wvm._workspaces.TheWorkspaces.Clear();
		newWorkspaceVMs.ForEach(vm =>
		{
			wvm.AddWorkspaceViewModel(vm);
			wvm._workspaces.AddWorkspace(vm._workspace);
			wvm.SubscribeViewModelEvents(vm);
		});
	}

	// TODO: How do we know if the inputs have changed since the last sync?
	// TODO: Some inputs may have been deleted (with future feature)

	/// <summary>
	/// The purpose of this method is to synchronize the inputs between two workspaces
	/// (wThis and wThat). It compares the history of inputs in both workspaces and
	/// performs the necessary actions to ensure that both workspaces have the same inputs.
	/// Here's a step-by-step breakdown of what the method does:
	/// 1. It first calculates the additional inputs in wThis and wThat by finding
	/// 	the inputs that exist in one workspace but not in the other. This is done
	/// 	using the Except LINQ method.
	/// 2. If there are no additional inputs in either workspace, the method returns early.
	/// 3. If there are additional inputs in wThis but not in wThat, the method
	/// 	iterates over each additional input in wThis and evaluates it in wThat
	/// 	by calling the Evaluate method of wThat with the input as an argument.
	/// 4. If there are additional inputs in wThat but not in wThis, the method
	/// 	iterates over each additional input in wThat and evaluates it in wThis
	/// 	by calling the Evaluate method of wThis with the input as an argument.
	/// 5. If both workspaces have additional inputs, the method creates a clone of
	///	wThis using the CloneWorkspace method. This clone is then added to wThat
	/// 	using the AddWorkspace method of Workspaces.
	/// 	Similarly, the clone is added to wThis using the same method.
	///
	/// Overall, the SyncWorkspaceInputs method ensures that the inputs in both
	/// workspaces are synchronized by either copying inputs from one workspace
	/// to the other or creating clones of workspaces with additional inputs.
	/// </summary>
	/// <remarks>
	/// We always add the This workspace to the new list of This workspaces.
	///
	/// if This inputs have NOT changed && That inputs have NOT changed:
	///	do nothing
	/// else if This inputs have changed && That inputs have NOT changed:
	/// 	copy This inputs to That workspace
	/// else if That inputs have changed && This inputs have NOT changed:
	///	copy That inputs to This workspace
	/// else if This inputs have changed && That inputs have changed:
	/// 	clone This workspace
	/// 	change This ID to a new GUID
	/// 	add new This workspace to That document
	/// </remarks>
	/// <param name="wvmThis">Workspace 1 to compare</param>
	/// <param name="wvmThat">Workspace 2 to compare</param>
	/// <param name="newWorkspaceVMsThis">New list of This workspace view-models</param>
	/// <param name="newWorkspaceVMsThat">New list of That workspace view-models</param>
	private static void SyncWorkspaceInputs(WorkspaceViewModel wvmThis, WorkspaceViewModel wvmThat,
														List<WorkspaceViewModel> newWorkspaceVMsThis,
														List<WorkspaceViewModel> newWorkspaceVMsThat)
	{
		var additionalInputsThis = wvmThis.History.Except(wvmThat.History);
		var additionalInputsThat = wvmThat.History.Except(wvmThis.History);

		newWorkspaceVMsThis.Add(wvmThis);

		if (!additionalInputsThis.Any() && !additionalInputsThat.Any())
		{
			newWorkspaceVMsThat.Add(wvmThat);
			return;
		}

		// If this workspace has new inputs, copy them to that workspace
		if (additionalInputsThis.Any() && !additionalInputsThat.Any())
		{
			foreach (Workspace.HistoryEntry? input in additionalInputsThis)
			{
				wvmThat._workspace.Evaluate(input.Input);
			}
			newWorkspaceVMsThat.Add(wvmThat);
			return;
		}

		// If that workspace has new inputs, copy them to this workspace
		if (!additionalInputsThis.Any() && additionalInputsThat.Any())
		{
			foreach (Workspace.HistoryEntry? input in additionalInputsThat)
			{
				wvmThis._workspace.Evaluate(input.Input);
			}
			newWorkspaceVMsThat.Add(wvmThat);
			return;
		}

		Debug.Assert(additionalInputsThis.Any() && additionalInputsThat.Any());
		// If both workspaces have new inputs, they cannot be synchronized.
		// So, clone each workspace and add it to both sets of workspaces.
		/*
		 * Each of the same workspace has its own inputs.
		 * This: "Main" id=1 [cat, dog]
		 * That: "Main" id=1 [cat, giraffe]
		 *
		 * Change one workspace's ID and Name because it will be a new workspace--the conflict.
		 * That= "Main [CONFLICT]" id=2 [cat, giraffe]
		 *
		 * Clone both and copy each to the other workspace.
		 * +That: "Main" id=1 [cat, dog]
		 * +This: "Main [CONFLICT]" id=2 [cat, giraffe]
		 */

		// Add This workspace to both workspaces
		WorkspaceViewModel workspacevmCopyThis = wvmThis.CloneWorkspaceViewModel(idNew: null);
		newWorkspaceVMsThat.Add(workspacevmCopyThis);

		// Add That workspace to both workspaces as a conflict.
		// (This is why we pass in new lists of workspaces to build.)
		wvmThat.Name += " [CONFLICT]";
		WorkspaceViewModel workspacevmCopyThat = wvmThat.CloneWorkspaceViewModel(Guid.NewGuid());
		WorkspaceViewModel workspacevmCopyThat2 = workspacevmCopyThat.CloneWorkspaceViewModel(idNew: null);
		newWorkspaceVMsThis.Add(workspacevmCopyThat);
		newWorkspaceVMsThat.Add(workspacevmCopyThat2);
	}

	#endregion Synchronize Workspaces
}
