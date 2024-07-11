using MathExpressions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace CalculateX.Models;

[DebuggerDisplay("\"{Name}\" {ID} ({History.Count})")]
public class Workspace : ICloneable, IEquatable<Workspace>
{
	public string ID { get; private set; }

	public string Name { get; set; }

	public DateTimeOffset LastModifiedUTC { get; set; } = DateTimeOffset.UtcNow;

	public const string NAME_ELEMENT_WORKSPACE = "workspace";
	public const string NAME_ATTRIBUTE_ID = "id";
	private const string NAME_ATTRIBUTE_NAME = "name";
	private const string NAME_ATTRIBUTE_LAST_MODIFIED = "last-modified";
	public const string NAME_ATTRIBUTE_SELECTED = "selected";
	public const string NAME_ELEMENT_INPUTS = "inputs";
	private const string NAME_ELEMENT_KEY = "key";
	private const string NAME_ATTRIBUTE_ORDINAL = "ordinal";

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
	public ObservableCollection<HistoryEntry> History { get; private init; } = [];


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

	public static Workspace FromXML(XElement xWorkspace)
	{
		string id = xWorkspace.Attribute(NAME_ATTRIBUTE_ID)!.Value;
		string name = xWorkspace.Attribute(NAME_ATTRIBUTE_NAME)!.Value;
		Workspace workspace = new(id, name)
		{
			LastModifiedUTC = DateTimeOffset.Parse(xWorkspace.Attribute(NAME_ATTRIBUTE_LAST_MODIFIED)?.Value ?? DateTimeOffset.UtcNow.ToString("o")),
		};
		//bool selected = (bool?)xWorkspace.Attribute(NAME_ATTRIBUTE_SELECTED) ?? false;
		//if (selected)
		//{
		//	Debug.Assert(selectedWorkspaceID is null);
		//	selectedWorkspaceID = workspace.ID;
		//}
		foreach (string input in
										xWorkspace
										.Element(NAME_ELEMENT_INPUTS)!
										.Elements(NAME_ELEMENT_KEY)
										.Select(e => (ordinal: (int)e.Attribute(NAME_ATTRIBUTE_ORDINAL)!, value: e.Value))
										.OrderBy(t => t.ordinal)
										.Select(t => t.value)
		)
		{
			// Evaluate loaded input and add to history
			workspace.Evaluate(input);
		}

		return workspace;
	}

	public XElement ToXML(string selectedWorkspaceID)
	{
		XElement xWorkspace =
			new(NAME_ELEMENT_WORKSPACE,
				new XAttribute(NAME_ATTRIBUTE_ID, ID),
				new XAttribute(NAME_ATTRIBUTE_NAME, Name),
				new XAttribute(NAME_ATTRIBUTE_LAST_MODIFIED, LastModifiedUTC.ToString("o")),
				new XAttribute(NAME_ATTRIBUTE_SELECTED, (ID == selectedWorkspaceID)),
				History
				.Aggregate(
					seed: (new XElement(NAME_ELEMENT_INPUTS), 0),
					func:
					((XElement root, int n) t, HistoryEntry entry) =>
					{
						++t.n;   // advance ordinal
						t.root.Add(
							new XElement(NAME_ELEMENT_KEY,
								new XAttribute(NAME_ATTRIBUTE_ORDINAL, t.n),
								entry.GetInput()
							)
						);
						return t;
					}
				)
				.Item1
			);

		return xWorkspace;
	}

	#region Implement IEquatable and Equals Override

	public bool Equals(Workspace? other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return (ID == other.ID);
	}

	public override bool Equals(object? obj) => Equals(obj as Workspace);

	public override int GetHashCode() => HashCode.Combine(ID);

	// Optionally, override the == and != operators
	public static bool operator ==(Workspace left, Workspace right)
	{
		if (left is null)
		{
			return (right is null);
		}
		return left.Equals(right);
	}

	public static bool operator !=(Workspace left, Workspace right)
	{
		return !(left == right);
	}

	#endregion Implement IEquatable and Equals Override

	#region Implement ICloneable Override

	/// <summary>
	/// Clone the workspace. Optionally, specify a new ID.
	/// </summary>
	/// <param name="idNew">ID to use in the clone or null to use the same ID</param>
	/// <returns></returns>
	public Workspace CloneWorkspace(Guid? idNew)
	{
		Workspace w = (Workspace)Clone();
		w.ID = idNew?.ToString() ?? ID;
		return w;
	}

	/// <summary>
	/// Copy the workspace and its history.
	/// </summary>
	/// <remarks>
	/// This also COPIES the ID, so there may be circumstances where the caller
	/// needs to change the ID of the cloned workspace.
	/// </remarks>
	/// <returns></returns>
	public object Clone()
	{
		Workspace w = new(ID, Name)
		{
			LastModifiedUTC = LastModifiedUTC,
		};
		foreach (HistoryEntry? input in History)
		{
			w.Evaluate(input.Input);
		}
		return w;
	}

	#endregion Implement ICloneable Override

	public void ClearHistory()
	{
		History.Clear();

		// Clear variables (because they will not exist when the app restarts).
		Variables.Initialize();

		LastModifiedUTC = DateTimeOffset.UtcNow;
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

	//	LastModifiedUTC = DateTimeOffset.UtcNow;
	//}

	/// <summary>
	/// Evaluate the input and add to the history.
	/// </summary>
	/// <remarks>
	/// This should NOT update `LastModifiedUTC`.
	/// </remarks>
	/// <param name="input"></param>
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
