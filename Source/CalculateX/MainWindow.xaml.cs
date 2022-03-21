using MathExpressions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;

namespace CalculateX;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, Shared.IRaisePropertyChanged
{
	/// <summary>
	/// Commands for this window.
	/// </summary>
	public static RoutedUICommand ClearInput { get; } = new("Clear Input", nameof(ClearInput), typeof(MainWindow),
		new InputGestureCollection()
		{
			new KeyGesture(Key.Escape, ModifierKeys.None),
		}
	);
	public static RoutedUICommand ClearHistory { get; } = new("Clear", nameof(ClearHistory), typeof(MainWindow),
		new InputGestureCollection()
		{
			// Note: Ctrl+Del and Ctrl+Backspace do not work.
			new KeyGesture(Key.D, ModifierKeys.Control),
		}
	);
	public static RoutedCommand WorkspacePrevious { get; } = new(nameof(WorkspacePrevious), typeof(MainWindow));
	public static RoutedCommand WorkspaceNext { get; } = new(nameof(WorkspaceNext), typeof(MainWindow));
	public static RoutedCommand CmdNewWorkspace { get; } = new(nameof(CmdNewWorkspace), typeof(MainWindow));
	public static RoutedCommand CmdCloseWorkspace { get; } = new(nameof(CmdCloseWorkspace), typeof(MainWindow));
	public static RoutedCommand HistoryPrevious { get; } = new(nameof(HistoryPrevious), typeof(MainWindow));
	public static RoutedCommand HistoryNext { get; } = new(nameof(HistoryNext), typeof(MainWindow));

	// This class provides features through event handling.
	private readonly Shared.WindowPosition _windowPosition;

	private int _windowId = 0;
	public Workspace CurrentWorkspace { get; set; }
	public ObservableCollection<Workspace> Workspaces { get; set; } = new();

	private const string NAME_ELEMENT_ROOT = "calculatex";
	private const string NAME_ELEMENT_WORKSPACES = "workspaces";
	private const string NAME_ATTRIBUTE_SELECTED = "selected";
	private const string NAME_ELEMENT_WORKSPACE = "workspace";
	private const string NAME_ATTRIBUTE_NAME = "name";
	private const string NAME_ELEMENT_INPUTS = "inputs";
	private const string NAME_ELEMENT_KEY = "key";
	private const string NAME_ATTRIBUTE_ORDINAL = "ordinal";


	public MainWindow()
	{
		Workspace? selectedWorkspace = LoadWorkspaces();
		if (!Workspaces.Any())
		{
			Workspaces.Add(new(FormWindowName(), canCloseTab: true));
		}
		Workspaces.Add(new("+", canCloseTab: false));
		CurrentWorkspace = selectedWorkspace ?? Workspaces.First();

		_windowPosition = new(this, "MainWindowPosition");

		InitializeComponent();
		DataContext = this;

		EventManager.RegisterClassHandler(typeof(TabItem), Shared.RoutedEventHelper.CloseTabEvent, new RoutedEventHandler(OnCloseTab));
		EventManager.RegisterClassHandler(typeof(TabItem), Shared.RoutedEventHelper.HeaderChangedEvent, new RoutedEventHandler(OnWorkspaceNameChanged));
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		foreach (var workspace in Workspaces)
		{
			// Re-evaluate historical inputs
			workspace.InputRecord.ForEach(i => workspace.Evaluate(i));
			CollectionViewSource.GetDefaultView(workspace.Variables).Refresh();
		}
	}

	private void InputControlTextBox_Loaded(object /*TextBox*/ sender, RoutedEventArgs e)
	{
		TextBox inputControl = (TextBox)sender;

		DataObject.AddPastingHandler(inputControl, SanitizeTextPastingHandler);
	}


	private void WorkspaceTabControl_SelectionChanged(object /*TabControl*/ sender, SelectionChangedEventArgs e)
	{
		/// If the user selected a closable tab, select it.
		if (CurrentWorkspace.CanCloseTab)
		{
			SaveWorkspaces();
			return;
		}

		/// Change the non-closable tab to closable and name it.
		CurrentWorkspace.Name = FormWindowName();
		CurrentWorkspace.CanCloseTab = true;

		// Create new non-closable tab.
		Workspaces.Add(new("+", canCloseTab: false));

		SaveWorkspaces();
	}


	// OriginalSource == TextBox or TabControl (depending on focus when user pressed key binding)
	// Source == TabControl
	private void WorkspacePrevious_CanExecute(object /*MainWindow*/ sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = (Workspaces.Count(w => w.CanCloseTab) > 1);	// Do not count the "+" tab
	}
	private void WorkspacePrevious_Executed(object /*Window*/ sender, ExecutedRoutedEventArgs e)
	{
		Debug.Assert(Workspaces.Count(w => w.CanCloseTab) > 1);

		SelectPreviousWorkspace();

		e.Handled = true;
	}

	// OriginalSource == TextBox or TabControl (depending on focus when user pressed key binding)
	// Source == TabControl
	private void WorkspaceNext_CanExecute(object /*MainWindow*/ sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = (Workspaces.Count(w => w.CanCloseTab) > 1);	// Do not count the "+" tab
	}
	private void WorkspaceNext_Executed(object /*MainWindow*/ sender, ExecutedRoutedEventArgs e)
	{
		Debug.Assert(Workspaces.Count(w => w.CanCloseTab) > 1);

		SelectNextWorkspace();

		e.Handled = true;
	}

	private void SelectPreviousWorkspace()
	{
		if (CurrentWorkspace == Workspaces.First())
		{
			var ixLastClosable = Workspaces.IndexOf(Workspaces.Last(w => w.CanCloseTab));
			CollectionViewSource.GetDefaultView(Workspaces).MoveCurrentToPosition(ixLastClosable);
		}
		else
		{
			CollectionViewSource.GetDefaultView(Workspaces).MoveCurrentToPrevious();
		}
	}

	private void SelectNextWorkspace()
	{
		if (CurrentWorkspace == Workspaces.Last(w => w.CanCloseTab))
		{
			CollectionViewSource.GetDefaultView(Workspaces).MoveCurrentToFirst();
		}
		else
		{
			CollectionViewSource.GetDefaultView(Workspaces).MoveCurrentToNext();
		}
	}


	private void NewWorkspace_Executed(object /*MainWindow*/ sender, ExecutedRoutedEventArgs e)
	{
		//var textBox = (TextBox)e.OriginalSource;
		//var tabControl = (TabControl)e.Source;

		Workspaces.Insert(Workspaces.IndexOf(CurrentWorkspace) + 1, new(FormWindowName(), canCloseTab: true));
		SelectNextWorkspace();

		SaveWorkspaces();

		e.Handled = true;
	}

	public void OnCloseTab(object /*TabItem*/ sender, RoutedEventArgs e)
	{
		var tabItem = (TabItem)sender;
		var closedWorkspace = (Workspace)tabItem.DataContext;

		CloseWorkspace(closedWorkspace);
	}
	// OriginalSource == TextBox, Button, or TabControl (depending on focus when user pressed key binding)
	// Source == TabControl
	private void CloseWorkspace_Executed(object /*MainWindow*/ sender, ExecutedRoutedEventArgs e)
	{
		// Determine which workspace was closed.
		var currentControl = (Control)e.OriginalSource;
		var closedWorkspace = (Workspace)currentControl.DataContext;

		CloseWorkspace(closedWorkspace);

		e.Handled = true;
	}
	private void CloseWorkspace(Workspace closedWorkspace)
	{
		if (closedWorkspace == CurrentWorkspace)
		{
			/// If closing last tab (except for "+" tab), create one.
			if (Workspaces.Count(w => w.CanCloseTab) == 1)
			{
				/// [closed][+]
				Workspaces.Insert(0, new Workspace(FormWindowName(), canCloseTab: true));
				/// [new][closed][+]
			}

			/// Select next tab (unless it's the "+" tab, then select previous tab).
			if (closedWorkspace == Workspaces[^2])
			{
				/// [a]...[z][closed][+]
				SelectPreviousWorkspace();
			}
			else
			{
				/// [a]...[z][closed][a]...[z][+]
				SelectNextWorkspace();
			}
		}

		Workspaces.Remove(closedWorkspace);

		SaveWorkspaces();
	}


	private string FormWindowName()
	{
		string name = string.Empty;
		do
		{
			++_windowId;
			name = $"{nameof(Workspace)}{_windowId}";
		} while (Workspaces.Any(w => w.Name == name));

		return name;
	}


	private void OnWorkspaceNameChanged(object /*TabItem*/ sender, RoutedEventArgs e)
	{
		SaveWorkspaces();
	}


	private void SanitizeTextPastingHandler(object /*TextBox*/ sender, DataObjectPastingEventArgs e)
	{
		// If any pasting is to be done, we will do it manually.
		e.CancelCommand();

		TextBox textBox = (TextBox)sender;

		if (e.DataObject.GetData(typeof(string)) is not string pasteText)
		{
			return;
		}

		pasteText = Shared.Numbers.RemoveCurrencySymbolAndGroupingSeparators(pasteText);

		// Insert text at the cursor
		int saveSelectionStart = textBox.SelectionStart;

		var s = string.Concat(
			textBox.Text.AsSpan(0, textBox.SelectionStart),
			pasteText,
			textBox.Text.AsSpan(textBox.SelectionStart + textBox.SelectionLength)
		);
		textBox.Text = s;

		// Position cursor at the end of the new text
		textBox.Select(saveSelectionStart + pasteText.Length, 0);
	}


	private void HelpButton_Click(object sender, RoutedEventArgs e)
	{
		CurrentWorkspace.ShowHelp = !CurrentWorkspace.ShowHelp;
	}


	private void ClearInput_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = !String.IsNullOrEmpty(CurrentWorkspace.Input);
	}
	private void ClearInput_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		CurrentWorkspace.Input = String.Empty;
	}


	private void ClearHistory_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = CurrentWorkspace.InputRecord.Any();
	}
	private void ClearHistory_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (MessageBox.Show("This will clear the input history and all variables. Do you want to continue?", nameof(CalculateX), MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
		{
			return;
		}

		CurrentWorkspace.InputRecord.Clear();
		CurrentWorkspace.ClearHistory();
		// Clear variables (because they will not exist when the app restarts).
		CurrentWorkspace.Variables.Initialize();
		CollectionViewSource.GetDefaultView(CurrentWorkspace.Variables).Refresh();

		SaveWorkspaces();
	}


	private void HistoryPrevious_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = !CurrentWorkspace.EntryHistory.IsEmpty;
	}
	private void HistoryPrevious_Executed(object /*Window*/ sender, ExecutedRoutedEventArgs e)
	{
		Debug.Assert(!CurrentWorkspace.EntryHistory.IsEmpty);

		// Source == TabControl
		TextBox textBox = (TextBox)e.OriginalSource;

		string entry = CurrentWorkspace.EntryHistory.PreviousEntry(CurrentWorkspace.Input);
		SetInput(entry, textBox);

		e.Handled = true;
	}

	private void HistoryNext_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = !CurrentWorkspace.EntryHistory.IsEmpty;
	}
	private void HistoryNext_Executed(object /*Window*/ sender, ExecutedRoutedEventArgs e)
	{
		Debug.Assert(!CurrentWorkspace.EntryHistory.IsEmpty);

		// Source == TabControl
		TextBox textBox = (TextBox)e.OriginalSource;

		string entry = CurrentWorkspace.EntryHistory.NextEntry(CurrentWorkspace.Input);
		SetInput(entry, textBox);

		e.Handled = true;
	}

	private void SetInput(string s, TextBox textBox)
	{
		CurrentWorkspace.Input = s;

		// Position cursor at the end of the text
		textBox.Select(textBox.Text.Length, 0);
	}


	/// <summary>
	/// If the first character the user enters is an operator symbol,
	/// prepend it with the name of the "answer" variable.
	/// </summary>
	/// <param name="sender">TextBox for input</param>
	/// <param name="e"></param>
	private void InputTextBox_TextChanged(object /*TextBox*/ sender, TextChangedEventArgs e)
	{
		if ((CurrentWorkspace.Input.Length == 1) && (e.Changes.First().AddedLength == 1) && (e.Changes.First().Offset == 0))
		{
			TextBox textBox = (TextBox)e.Source;
			char op = CurrentWorkspace.Input.First();
			if (OperatorExpression.IsSymbol(op))
			{
				CurrentWorkspace.Input = MathEvaluator.AnswerVariable + CurrentWorkspace.Input;

				// Position cursor at the end of the text
				textBox.Select(textBox.Text.Length, 0);
			}
		}
	}


	private void EvaluateButton_Click(object sender, RoutedEventArgs e)
	{
		if (string.IsNullOrWhiteSpace(CurrentWorkspace.Input))
		{
			return;
		}

		CurrentWorkspace.EvaluateInputAndSave();
		SaveWorkspaces();
	}


	/// <summary>
	/// When the user selects text, copy it to the clipboard.
	/// We no longer automatically paste it to the input field because
	/// it can be an unwanted selection or unwanted action altogether.
	/// We do, however, set focus to the input field so the user can quickly
	/// paste it if desired.
	/// </summary>
	/// <remarks>
	/// The MouseUp and MouseLeftButtonUp events are not called, so we use Preview.
	/// The SelectionChanged event is called while the mouse moves, so it's not useful.
	/// The MouseDoubleClick event works but is redundant because this is called for
	/// a double-click, too.
	/// </remarks>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void HistoryDisplay_MouseUp(object /*RichTextBox*/ sender, MouseButtonEventArgs e)
	{
		RichTextBox historyDisplay = (RichTextBox)sender;
		if (historyDisplay.Selection.IsEmpty)
		{
			return;
		}

		string selected = historyDisplay.Selection.Text;
		Clipboard.SetText(selected);

		/// https://stackoverflow.com/questions/5756448/in-wpf-how-can-i-get-the-next-control-in-the-tab-order
		historyDisplay.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
	}


	/// <summary>
	/// 
	/// </summary>
	/// <example>
	///	<calculatex>
	///		<workspaces>
	///			<workspace name="...">
	///				<inputs>
	///					<key ordinal="1">cat</key>
	///					<key ordinal="2">dog</key>
	///				</inputs>
	///			</workspace>
	///			<workspace>
	///				...
	///			</workspace>
	///		</workspaces>
	///	</calculatex>
	/// </example>
	public void SaveWorkspaces()
	{
		Shared.MyStorage.WriteXDocument(NAME_ELEMENT_ROOT,
			new XDocument(
				new XElement(NAME_ELEMENT_WORKSPACES,
					Workspaces
					.Take(0..^1)   // Do not save last "+" tab.
					.Select(w =>
						new XElement(NAME_ELEMENT_WORKSPACE,
							new XAttribute(NAME_ATTRIBUTE_NAME, w.Name),
							new XAttribute(NAME_ATTRIBUTE_SELECTED, object.ReferenceEquals(CurrentWorkspace, w)),
							w.InputRecord.Aggregate(
								seed: (new XElement(NAME_ELEMENT_INPUTS), 0),
								func:
								((XElement root, int n) t, string input) =>
								{
									++t.n;	// advance ordinal
									t.root.Add(
										new XElement(NAME_ELEMENT_KEY,
											new XAttribute(NAME_ATTRIBUTE_ORDINAL, t.n),
											input
										)
									);
									return t;
								}
							)
							.Item1
						)
					))
				)
			);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>Selected workspace (or null)</returns>
	private Workspace? LoadWorkspaces()
	{
		Workspace? selectedWorkspace = null;

		/// TODO: We can remove the legacy load after everyone upgrades.
		XElement? root = ReadXElementLegacy("inputs");
		if (root is not null)
		{
			Workspace workspace = new("Legacy", canCloseTab: true);
			workspace.InputRecord.AddRange(
				root.Elements()
					.Select(e => (ordinal: (int)e.Attribute(NAME_ATTRIBUTE_ORDINAL)!, value: e.Value))
					.OrderBy(t => t.ordinal)
					.Select(t => t.value)
			);
			Workspaces.Add(workspace);

			Shared.MyStorage.Delete("inputs");

			selectedWorkspace = workspace;
		}
		else
		{
			XDocument? xdoc = Shared.MyStorage.ReadXDocument(NAME_ELEMENT_ROOT);
			if (xdoc is null)
			{
				return null;
			}

			foreach (XElement xWorkspace in xdoc.Element(NAME_ELEMENT_WORKSPACES)?.Elements(NAME_ELEMENT_WORKSPACE) ?? Enumerable.Empty<XElement>())
			{
				Workspace workspace = new(xWorkspace.Attribute(NAME_ATTRIBUTE_NAME)?.Value ?? "New", canCloseTab: true);
				bool selected = (bool?)xWorkspace.Attribute(NAME_ATTRIBUTE_SELECTED) ?? false;
				if (selected)
				{
					selectedWorkspace = workspace;
				}
				workspace.InputRecord.AddRange(
					xWorkspace
					.Element(NAME_ELEMENT_INPUTS)!
					.Elements(NAME_ELEMENT_KEY)
					.Select(e => (ordinal: (int)e.Attribute(NAME_ATTRIBUTE_ORDINAL)!, value: e.Value))
					.OrderBy(t => t.ordinal)
					.Select(t => t.value));
				Workspaces.Add(workspace);
			}
		}

		return selectedWorkspace;
	}
	private static XElement? ReadXElementLegacy(string tag)
	{
		try
		{
			using IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			if (!isf.FileExists(tag))
			{
				return null;
			}

			using IsolatedStorageFileStream stm = new(tag, FileMode.Open, isf);
			if (stm is null)
			{
				return null;
			}

			using StreamReader stmReader = new(stm);

			// If this hasn't been created yet, EOS is true.
			if (stmReader.EndOfStream)
			{
				return null;
			}

			try
			{
				return XElement.Load(stmReader);
			}
			catch (XmlException)
			{
				stm.SetLength(0);
				return null;
			}

			//This calls Dispose, so we don't need to. stmReader.Close();
			//This calls Dispose, so we don't need to. stm.Close();
			// http://stackoverflow.com/questions/1065168/does-disposing-streamreader-close-the-stream
		}
		catch (NotSupportedException)
		{
			return null;
		}
	}

	#region Implement IRaisePropertyChanged

	public event PropertyChangedEventHandler? PropertyChanged;

	public void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	#endregion Implement IRaisePropertyChanged
}
