using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CalculateX;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private const string CalculateX_FileName = "CalculateX.xml";
	private static readonly string _pathWorkspacesFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), CalculateX_FileName);

	// We cannot create the view-model in XAML because we need to pass it the path to storage.
	public ViewModels.WorkspacesViewModel ViewModel { get; private init; } = new(_pathWorkspacesFile);

	// This class provides features through event handling.
	private readonly Shared.WindowPosition _windowPosition;


	 public MainWindow()
	{
		_windowPosition = new(this, "MainWindowPosition");

		/// XAML constructs WorkspacesViewModel and sets DataContext to it.
		InitializeComponent();

		DataContext = ViewModel;
	}

	private void InputControlTextBox_Loaded(object /*TextBox*/ sender, RoutedEventArgs e)
	{
		TextBox inputControl = (TextBox)sender;

		DataObject.AddPastingHandler(inputControl, SanitizeTextPastingHandler);
	}


	private void Workspaces_SelectionChanged(object /*ListBox*/ sender, SelectionChangedEventArgs e)
	{
		ViewModel.SaveWorkspaces();

		CtlInputTextBox.Focus();

		// We cannot set Handled because that prevents the behavior's event handler from being called.
		//e.Handled = true;
	}

	private void SanitizeTextPastingHandler(object /*TextBox*/ sender, DataObjectPastingEventArgs e)
	{
		e.Handled = true;

		// If any pasting is to be done, we will do it manually.
		e.CancelCommand();

		if (e.DataObject.GetData(typeof(string)) is not string pasteText)
		{
			return;
		}

		pasteText = Shared.Numbers.RemoveCurrencySymbolAndGroupingSeparators(pasteText);

		TextBox textBox = (TextBox)sender;

		textBox.CaretIndex = ViewModel.SelectedWorkspaceVM.InsertTextAtCursor(pasteText, textBox.CaretIndex, textBox.SelectionLength);
	}

	private void RenameWorkspace_Clicked(object sender, RoutedEventArgs e)
	{
		string newName = new Shared.MessageInput(this, "Enter the new name for the workspace:", "Rename Workspace", ViewModel.SelectedWorkspaceVM.Name).ShowDialog() ?? ViewModel.SelectedWorkspaceVM.Name;
		if ((newName == ViewModel.SelectedWorkspaceVM.Name) || string.IsNullOrWhiteSpace(newName))
		{
			// Name has not changed or is blank
			return;
		}

		ViewModel.RenameWorkspace(ViewModel.SelectedWorkspaceVM, newName);
	}

	/// <summary>
	/// If the first character the user enters is an operator symbol,
	/// prepend it with the name of the "answer" variable.
	/// </summary>
	/// <param name="sender">TextBox for input</param>
	/// <param name="e"></param>
	private void InputTextBox_TextChanged(object /*TextBox*/ sender, TextChangedEventArgs e)
	{
		// We cannot set Handled because that prevents the behavior's event handler from being called.
		//e.Handled = true;

		if ((ViewModel.SelectedWorkspaceVM.Input.Length == 1) && (e.Changes.First().AddedLength == 1) && (e.Changes.First().Offset == 0))
		{
			char op = ViewModel.SelectedWorkspaceVM.Input.First();
			if (MathExpressions.OperatorExpression.IsSymbol(op))
			{
				ViewModel.SelectedWorkspaceVM.InsertTextAtCursor(MathExpressions.MathEvaluator.AnswerVariable, cursorPosition: 0, selectionLength: 0);

				// Position cursor at the end of the text (instead of after 'answer')
				TextBox textBox = (TextBox)e.Source;
				textBox.CaretIndex = textBox.Text.Length;
			}
		}
	}

	/// <summary>
	/// When the user selects a history entry, copy it to the input control.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void HistoryListBox_SelectionChanged(object /*ListBox*/ sender, SelectionChangedEventArgs e)
	{
		e.Handled = true;

		ListBox historyControl = (ListBox)sender;
		if (historyControl.Items.Count == 0)
		{
			return;
		}

		if (historyControl.SelectedItem is null)
		{
			return;
		}

		var entry = (Models.Workspace.HistoryEntry)historyControl.SelectedItem;
		string input = entry.GetInput();
		string? result = entry.Result;
		TextBox ctlInputTextBox = (TextBox)historyControl.Tag;

		ViewModel.SelectedWorkspaceVM.ClearInput();
		Debug.Assert(ctlInputTextBox.CaretIndex == 0);
		ctlInputTextBox.CaretIndex = ViewModel.SelectedWorkspaceVM.InsertTextAtCursor(input, ctlInputTextBox.CaretIndex, ctlInputTextBox.SelectionLength);

		if (result is not null)
		{
			Clipboard.SetText(result);
		}

		// This scrolls back up a ways for some reason. Plus, focus is left in the input field anyway.
		/// https://stackoverflow.com/questions/5756448/in-wpf-how-can-i-get-the-next-control-in-the-tab-order
		//historyControl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

		// set focus to the input field
		ctlInputTextBox.Focus();

		// clear the history selection
		historyControl.SelectedItem = null;
	}

	/// <summary>
	/// When the user selects a variable entry, copy it to the input control.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void VariablesListView_SelectionChanged(object /*ListView*/ sender, SelectionChangedEventArgs e)
	{
		e.Handled = true;

		ListView variablesControl = (ListView)sender;
		if (variablesControl.Items.Count == 0)
		{
			return;
		}

		if (variablesControl.SelectedItem is null)
		{
			return;
		}

		var entry = (KeyValuePair<string, double>)variablesControl.SelectedItem;
		string variableName = entry.Key;
		string variableValue = ((double)entry.Value).ToString();
		TextBox ctlInputTextBox = (TextBox)variablesControl.Tag;

		ctlInputTextBox.CaretIndex = ViewModel.SelectedWorkspaceVM.InsertTextAtCursor(variableName, ctlInputTextBox.CaretIndex, ctlInputTextBox.SelectionLength);

		Clipboard.SetText(variableValue);

		// set focus to the input field
		ctlInputTextBox.Focus();

		// clear the history selection
		variablesControl.SelectedItem = null;
	}
}
