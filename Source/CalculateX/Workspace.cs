using MathExpressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace CalculateX;

[DebuggerDisplay("{Name} ({InputRecord.Count})")]
public class Workspace : Shared.EditableTabHeaderControl.IEditableTabHeaderControl, Shared.IRaisePropertyChanged
{
	private readonly Shared.NotifyProperty<string> _name;
	public string Name { get => _name.Value; set => _name.Value = value; }

	private readonly Shared.NotifyProperty<bool> _canCloseTab;
	public bool CanCloseTab { get => _canCloseTab.Value; set => _canCloseTab.Value = value; }

	// record ordered history of inputs so we can save them and play them back on restart.
	public readonly List<string> InputRecord = new();

	// separate history because it is rearranged based on MRU entry.
	public readonly CircularHistory EntryHistory = new();


	// This is an instance variable to maintain the state of its variable dictionary.
	// No need to dispose it because it's present for the entire lifetime.
	public readonly MathEvaluator TheEvaluator = new();

	public VariableDictionary Variables { get => TheEvaluator.Variables; private set { } }

	public string Input { get => _input.Value; set => _input.Value = value; }
	private readonly Shared.NotifyProperty<string> _input;

	public bool ShowHelp { get => _showHelp.Value; set => _showHelp.Value = value; }
	private readonly Shared.NotifyProperty<bool> _showHelp;

	public FlowDocument ContentFlow { get => _flowContent.Value; set => _flowContent.Value = value; }
	private readonly Shared.NotifyProperty<FlowDocument> _flowContent;


	public Workspace() : this("New", canCloseTab: true) { }	/// Empty ctor required for the Designer
	public Workspace(string name, bool canCloseTab)
	{
		_canCloseTab = new Shared.NotifyProperty<bool>(this, nameof(CanCloseTab), initialValue: true);
		_name = new Shared.NotifyProperty<string>(this, nameof(Name), initialValue: string.Empty);
		_input = new Shared.NotifyProperty<string>(this, nameof(Input), initialValue: string.Empty);
		_showHelp = new Shared.NotifyProperty<bool>(this, nameof(ShowHelp), initialValue: false);
		_flowContent= new Shared.NotifyProperty<FlowDocument>(this, nameof(ContentFlow), initialValue: new());

		Name = name;
		CanCloseTab = canCloseTab;
	}


	public void ClearHistory()
	{
		ContentFlow.Blocks.Clear();
	}


	public void EvaluateInputAndSave()
	{
		Debug.Assert(!string.IsNullOrWhiteSpace(Input));

		// Save input in playback record.
		InputRecord.Add(Input);

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
	public void AppendHistoryEntry(string input, string answer, Color fgColor)
	{
		input = input.Trim();

		Paragraph para = new() { Margin = new Thickness(0) };
		if (string.IsNullOrEmpty(answer))
		{
			// "input " in default color followed by "cleared" in gray.
			para.Inlines.Add(new Run($"{input} "));
			para.Inlines.Add(new Italic(new Run("cleared")) { Foreground = Brushes.Gray });
		}
		else
		{
			// "input = " in default color followed by answer string in the specified color.
			para.Inlines.Add(new Run($"{input} = "));
			para.Inlines.Add(new Run(answer) { Foreground = new SolidColorBrush(fgColor) });
		}
		ContentFlow.Blocks.Add(para);
	}

	#region Implement IRaisePropertyChanged

	public event PropertyChangedEventHandler? PropertyChanged;

	public void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	#endregion Implement IRaisePropertyChanged
}
