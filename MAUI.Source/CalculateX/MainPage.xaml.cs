using CalculateX.ViewModels;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CalculateX;

public partial class MainPage : ContentPage, IQueryAttributable
{
	public MainPage()
	{
		InitializeComponent();
	}

	private void WorkspacesPage_NavigatedTo(object sender, NavigatedToEventArgs e)
	{
		workspacesCollection.SelectedItem = null;
	}

	public async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (GetWorkspaceViewModelFromQuery(query, WorkspaceViewModel.QUERY_DATA_DELETE, out WorkspaceViewModel? workspaceDeleteVM))
		{
			if (!await DisplayAlert(
							"Delete Workspace",
							$"Do you want to delete workspace {workspaceDeleteVM.Name}?",
							"Yes", "No"))
			{
				return;
			}

			ViewModel.DeleteWorkspace(workspaceDeleteVM);
		}
		else if (GetWorkspaceViewModelFromQuery(query, WorkspaceViewModel.QUERY_DATA_RENAME, out WorkspaceViewModel? workspaceRenameVM))
		{
			string? name = await DisplayPromptAsync(
											"Rename Workspace",
											$"Please enter the new name for workspace {workspaceRenameVM.Name}:",
											initialValue: workspaceRenameVM.Name);
			if (name is null)
			{
				return;
			}

			ViewModel.RenameWorkspace(workspaceRenameVM, name);
		}

		// https://stackoverflow.com/q/73755717/4858
		query.Clear();

		////
		//// https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation
		////
		bool GetWorkspaceViewModelFromQuery(IDictionary<string, object> query,
														  string key,
														  [NotNullWhen(true)] out WorkspaceViewModel? workspaceVM)
		{
			if (query.TryGetValue(key, out object? valueID))
			{
				Debug.Assert(valueID is not null);

				string workspaceID = valueID.ToString()!;

				workspaceVM = ViewModel.TheWorkspaceViewModels.FirstOrDefault(x => x.ID == workspaceID);
			}
			else
			{
				workspaceVM = null;
			}

			return (workspaceVM is not null);
		}
	}
}
