using MathExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace CalculateX.Models;

[DebuggerDisplay("{Name} ({History.Count})")]
public class Workspace
{
	public string ID { get; private init; }

	public string Name { get; set; }


	// This is an instance variable to maintain the state of its variable dictionary.
	public readonly MathEvaluator TheEvaluator = new();

	public VariableDictionary Variables => TheEvaluator.Variables;

	[DebuggerDisplay("{Input} = {Result} Cleared = {IsCleared}, Error = {IsError}")]
	public record class HistoryEntry(string Input, string? Result, bool IsCleared, bool IsError)
	{
		// Raw expression without equals sign (for XAML).
		public string Input { get; private init; } = Input;
		public string? Result { get; private init; } = Result;
		public bool IsCleared { get; private init; } = IsCleared;
		public bool IsError { get; private init; } = IsError;


		// Normal expression
		public HistoryEntry(string input, string result)
			: this(input, result, IsCleared: false, IsError: false)
		{
		}

		// Clear a variable
		public HistoryEntry(string Input)
			: this(Input, Result: null, IsCleared: true, IsError: false)
		{
		}

		// Expression error
		public HistoryEntry(string input, string errorMessage, bool _)
			: this(input, Result: errorMessage, IsCleared: false, IsError: true)
		{
		}

		// If the expression cleared a variable, we need to append the equals sign.
		public string GetInput()
		{
			return IsCleared ? Input + '=' : Input;
		}
	}
	public ObservableCollection<HistoryEntry> History { get; private init; } = new();


	/// <summary>
	/// Called when loading workspaces.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="name"></param>
	public Workspace(string id, string name)
	{
		ID = id;
		Name = name;
	}
	/// <summary>
	/// Called when navigating to or adding a workspace.
	/// </summary>
	/// <param name="name"></param>
	public Workspace(string name) : this(Guid.NewGuid().ToString(), name) { }

	public void ClearHistory()
	{
		History.Clear();

		// Clear variables (because they will not exist when the app restarts).
		Variables.Initialize();
	}

	//public void Reevaluate()
	//{
	//	foreach (var entry in History)
	//	{
	//		double? d = TheEvaluator.Evaluate(entry.Input);
	//		if (d is null)
	//		{
	//			entry.Result = null;
	//		}
	//		else
	//		{
	//			entry.Result = Shared.Numbers.FormatNumberWithGroupingSeparators(d.Value);
	//		}
	//	}
	//}

	public void Evaluate(string input)
	{
		Debug.Assert(!string.IsNullOrWhiteSpace(input));

		try
		{
			double? d = TheEvaluator.Evaluate(input);
			// If a variable was deleted, modify the history entry.
			if (d is null)
			{
				// Remove trailing equals sign (to avoid "var = = cleared").
				input = input.TrimEnd();

				Debug.Assert(input.EndsWith('='), "Clearing a variable always ends in an equals sign.");
				if (!input.EndsWith('='))
				{
					// This should never happen.
					return;
				}

				input = input[..^1].TrimEnd();
				AppendHistoryClearVariable(input);
			}
			else
			{
				AppendHistoryEntry(input, Shared.Numbers.FormatNumberWithGroupingSeparators(d.Value));
			}
		}
		catch (Exception ex)
		{
			AppendHistoryError(input, ex.Message);
		}
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="input">The user's typed expression</param>
	/// <param name="answer">The value of the expression. (Empty if variable was deleted.)</param>
	private void AppendHistoryEntry(string input, string answer)	=> History.Add(new HistoryEntry(input.Trim(), answer));
	private void AppendHistoryClearVariable(string input)				=> History.Add(new HistoryEntry(input.Trim()));
	private void AppendHistoryError(string input, string msg)		=> History.Add(new HistoryEntry(input.Trim(), msg, /*IsError*/ true));
}
