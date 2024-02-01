using MathExpressions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Media;

namespace CalculateX;

[DebuggerDisplay("{Name} ({InputRecord.Count})")]
public class Workspace : Shared.EditableTabHeaderControl.IEditableTabHeaderControl, Shared.IRaisePropertyChanged
{
	private readonly Shared.NotifyProperty<string> _name;
	public string Name { get => _name.Value; set => _name.Value = value; }

	private readonly Shared.NotifyProperty<bool> _canCloseTab;
	public bool CanCloseTab { get => _canCloseTab.Value; set => _canCloseTab.Value = value; }

	// separate history because it is rearranged based on MRU entry.
	public readonly CircularHistory EntryHistory = new();


	// This is an instance variable to maintain the state of its variable dictionary.
	public readonly MathEvaluator TheEvaluator = new();

	public VariableDictionary Variables { get => TheEvaluator.Variables; private set { } }

	public string Input { get => _input.Value; set => _input.Value = value; }
	private readonly Shared.NotifyProperty<string> _input;

	public bool ShowHelp { get => _showHelp.Value; set => _showHelp.Value = value; }
	private readonly Shared.NotifyProperty<bool> _showHelp;

	public record class HistoryEntry(string Input, string? Result)
	{
		public string Input { get; set; } = Input;
		public string? Result { get; set; } = Result;
	}
	public ObservableCollection<HistoryEntry> History { get; init; } = new();


	public Workspace() : this("New", canCloseTab: true) { }	/// Empty ctor required for the Designer
	public Workspace(string name, bool canCloseTab)
	{
		_canCloseTab = new Shared.NotifyProperty<bool>(this, nameof(CanCloseTab), initialValue: true);
		_name = new Shared.NotifyProperty<string>(this, nameof(Name), initialValue: string.Empty);
		_input = new Shared.NotifyProperty<string>(this, nameof(Input), initialValue: string.Empty);
		_showHelp = new Shared.NotifyProperty<bool>(this, nameof(ShowHelp), initialValue: false);

		Name = name;
		CanCloseTab = canCloseTab;
	}


	public void ClearHistory()
	{
		History.Clear();
		EntryHistory.Reset();

		// Clear variables (because they will not exist when the app restarts).
		Variables.Initialize();
		CollectionViewSource.GetDefaultView(Variables).Refresh();
	}


	public void EvaluateInputAndSave()
	{
		Debug.Assert(!string.IsNullOrWhiteSpace(Input));

		Evaluate(Input);
		CollectionViewSource.GetDefaultView(Variables).Refresh();

		// Clear input when we're done.
		Input = string.Empty;
	}

	public void Evaluate(string input)
	{
		try
		{
			double? d = TheEvaluator.Evaluate(input);
			// If a variable was deleted, modify the history entry.
			if (d is null)
			{
				AppendHistoryEntry(input, string.Empty, Colors.Black);
			}
			else
			{
				AppendHistoryEntry(input, Shared.Numbers.FormatNumberWithGroupingSeparators(d.Value), Colors.Blue);
			}
		}
		catch (Exception ex)
		{
			AppendHistoryEntry(input, ex.Message, Colors.Red);
		}

		EntryHistory.AddNewEntry(input);
	}


	/// <summary>
	///
	/// </summary>
	/// <param name="input">The user's typed expression</param>
	/// <param name="answer">The value of the expression. (Empty if variable was deleted.)</param>
	/// <param name="fgBrush">Color to use if there's an answer.</param>
	private void AppendHistoryEntry(string input, string answer, Color fgColor)
	{
		input = input.Trim();
		History.Add(new HistoryEntry(input, string.IsNullOrEmpty(answer) ? null : answer));
	}

	#region Implement IRaisePropertyChanged

	public event PropertyChangedEventHandler? PropertyChanged;

	public void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	#endregion Implement IRaisePropertyChanged
}
