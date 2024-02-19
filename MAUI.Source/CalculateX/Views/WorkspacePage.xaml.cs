using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
//using Microsoft.Maui.Controls.Platform;
//using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace CalculateX.Views;

public partial class WorkspacePage : ContentPage, IQueryAttributable
{
	public WorkspacePage()
	{
		InitializeComponent();

		// MAUI BUG?
		// We need to set Entry.Text because it does not
		// update when we change Input in the view-model.
		ViewModel.InputChanged += (sender, e) =>
		{
			CtlEntry.Text = ViewModel.Input;

			// Position cursor at the end of the text
			CtlEntry.CursorPosition = CtlEntry.Text.Length;

			_ = CtlEntry.Focus();
		};
	}

	private async void ContentPage_Loaded(object sender, EventArgs e)
	{
		//BUG Cannot get this to work.
		//IPlatformElementConfiguration<Microsoft.Maui.Controls.PlatformConfiguration.Android, Microsoft.Maui.Controls.Entry>
		//	x = CtlEntry.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>();
		//CtlEntry.OnThisPlatform<Microsoft.Maui.Controls.PlatformConfiguration.Android>();

		//// BUG This does not work: this.Window, this, CtlEntry
		//// Ensure that THIS page will resize when the keyboard pops up.
		//Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
		//	.Application
		//	.SetWindowSoftInputModeAdjust(CtlEntry, WindowSoftInputModeAdjust.Resize);

		CtlEntry.Focus();
		await CtlEntry.ShowKeyboardAsync(CancellationToken.None);
	}

	/// <summary>
	/// We need to scroll the entry-history control after the keyboard is shown.
	/// It does not work to do this in the Loaded event because--it seems--that
	/// it happens before the adjustment for the keyboard is made.
	/// </summary>
	/// <remarks>
	/// This is needed despite the CollectionView.ItemsUpdatingScrollMode setting.
	/// </remarks>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ContentPage_LayoutChanged(object sender, EventArgs e)
	{
		CtlHistoryEntries.ScrollTo(index: ViewModel.History.Count - 1, position: ScrollToPosition.End, animate: false);
	}

	private void EvaluateEntry_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (ViewModel is null)
		{
			return;
		}

		if (ViewModel.EvaluateCommand is RelayCommand evaluateCommand)
		{
			evaluateCommand.NotifyCanExecuteChanged();
		}
	}

	private async void HistoryEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.Count == 0)
		{
			return;
		}

		Models.Workspace.HistoryEntry entry = ((Models.Workspace.HistoryEntry)e.CurrentSelection[0]);
		string? result = entry.Result;

		ViewModel.SetInput(entry.Input);

		if (result is not null)
		{
			await Clipboard.Default.SetTextAsync(result);
		}

		// Clear the selection so the entry is not highlighted in the list of entries.
		CollectionView collection = (CollectionView)sender;
		collection.SelectedItem = null;
	}

	public async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		// Note: We cannot this to the view-model because we have to access
		// the text control directly in order to modify the cursor position.
		if (query.TryGetValue(ViewModels.WorkspaceViewModel.QUERY_DATA_VARIABLE_NAME, out object? valueName))
		{
			Debug.Assert(valueName is not null);
			string variableName = (string)valueName;

			// Insert variable name into input field
			int curPos = CtlEntry.CursorPosition;
			string s = CtlEntry.Text.Insert(CtlEntry.CursorPosition, variableName);
			ViewModel.Input = s;
			CtlEntry.CursorPosition = curPos + variableName.Length;
			CtlEntry.Focus();

			if (query.TryGetValue(ViewModels.WorkspaceViewModel.QUERY_DATA_VARIABLE_VALUE, out object? valueValue))
			{
				Debug.Assert(valueValue is not null);
				string variableValue = (string)valueValue;
				await Clipboard.Default.SetTextAsync(variableValue);
			}

			// https://stackoverflow.com/q/73755717/4858
			query.Clear();
		}
	}
}
