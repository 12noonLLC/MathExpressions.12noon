using CommunityToolkit.Maui.Core.Platform;
using System.Diagnostics;
//using Microsoft.Maui.Controls.Platform;
//using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace CalculateX.Views;

public partial class WorkspacePage : ContentPage, IQueryAttributable
{
	public WorkspacePage()
	{
		InitializeComponent();
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
#if !WINDOWS
		await CtlEntry.ShowKeyboardAsync(CancellationToken.None);
#endif
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
		if (ViewModel.History.Count == 0)
		{
			return;
		}

		CtlHistoryEntries.ScrollTo(index: ViewModel.History.Count - 1, position: ScrollToPosition.End, animate: false);
	}

	/// <summary>
	/// If the first character the user enters is an operator symbol,
	/// prepend it with the name of the "answer" variable.
	/// </summary>
	/// <param name="sender">TextBox for input</param>
	/// <param name="e"></param>
	private void Entry_TextChanged(object /*Entry*/ sender, TextChangedEventArgs e)
	{
		// This is true at startup.
		if (ViewModel is null)
		{
			return;
		}

		if ((ViewModel.Input.Length == 1) && (e.NewTextValue.Length == 1))
		{
			char op = ViewModel.Input.First();
			if (MathExpressions.OperatorExpression.IsSymbol(op))
			{
				ViewModel.InsertTextAtCursor(MathExpressions.MathEvaluator.AnswerVariable, cursorPosition: 0, selectionLength: 0);

				// Position cursor at the end of the text (instead of after 'answer')
				// We cannot just set the cursor position here. It stays after first character.
				// So, we delay moving the cursor position until Text is set.
				Task.Factory.StartNew(() => {
					Thread.Sleep(50);
					MainThread.BeginInvokeOnMainThread(() =>
					{
						// Code to run on the main thread
						Entry textBox = (Entry)sender;
						textBox.CursorPosition = ViewModel.Input.Length;
					});
				});
			}
		}

		ViewModel.EvaluateCommand.NotifyCanExecuteChanged();
	}

	private async void HistoryEntries_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.Count == 0)
		{
			return;
		}

		Models.Workspace.HistoryEntry entry = ((Models.Workspace.HistoryEntry)e.CurrentSelection[0]);
		string? result = entry.Result;

		ViewModel.Input = entry.GetInput();

		// The user can press the UP-ARROW to navigate to the history list and press
		// SPACE to select an entry, so we need to restore focus to the entry field.
		CtlEntry.Focus();

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
		// User selected a variable.
		// Note: We cannot move this to the view-model because we have to access
		// the text control directly in order to modify the cursor position.
		if (query.TryGetValue(ViewModels.WorkspaceViewModel.QUERY_DATA_VARIABLE_NAME, out object? valueName))
		{
			Debug.Assert(valueName is not null);
			string variableName = (string)valueName;

			// Insert variable name into input field
			CtlEntry.CursorPosition = ViewModel.InsertTextAtCursor(variableName, CtlEntry.CursorPosition, CtlEntry.SelectionLength);

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
