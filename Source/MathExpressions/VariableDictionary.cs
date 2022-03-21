using MathExpressions.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathExpressions;

/// <summary>
/// Class representing a collection of variable names and values.
/// </summary>
/// <remarks>
/// Variable names can only contain letters and numbers. Symbols are not allowed.
/// Must store Double for all results, which may be Double.NaN, Double.PositiveInfinity, etc.
/// </remarks>
public class VariableDictionary : SortedDictionary<string, double>
{
	private readonly MathEvaluator _evaluator;

	/// <summary>
	/// Initializes a new instance of the <see cref="VariableDictionary"/> class.
	/// </summary>
	/// <param name="evaluator">The evaluator for comparing variable names with function names.</param>
	public VariableDictionary(MathEvaluator evaluator)
		: base(StringComparer.OrdinalIgnoreCase)
	{
		_evaluator = evaluator;
		Initialize();
	}

	public void Initialize()
	{
		base.Clear();
		base.Add(MathEvaluator.AnswerVariable, 0);
		base.Add("pi", Math.PI);
		base.Add("e", Math.E);
	}


	/// <summary>Adds the specified variable and value to the dictionary.</summary>
	/// <param name="name">The name of the variable to add.</param>
	/// <param name="value">The value of the variable.</param>
	/// <exception cref="ArgumentNullException">When variable name is null.</exception>
	/// <exception cref="ArgumentException">When variable name contains non-letters or the name exists in the <see cref="MathEvaluator.Functions"/> list.</exception>
	/// <seealso cref="MathEvaluator"/>
	/// <seealso cref="MathEvaluator.Variables"/>
	/// <seealso cref="MathEvaluator.Functions"/>
	public new void Add(string name, double value)
	{
		Validate(name);
		base.Add(name, value);
	}

	public new double this[string name]
	{
		get => base[name];
		set
		{
			Validate(name);
			base[name] = value;
		}
	}


	public bool IsValidVariableName(string name)
	{
		try
		{
			Validate(name);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
	private void Validate(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException(nameof(name));
		}

		if (_evaluator.IsFunction(name))
		{
			throw new ArgumentException(String.Format(Resources.VariableNameConflict1, name), nameof(name));
		}

		// Allow letters, digits, and underscores, but first character must be a letter.
		if (!Char.IsLetter(name.First()) || name.Any(c => !IsValidVariableNameCharacter(c)))
		{
			throw new ArgumentException(Resources.VariableNameContainsLetters, nameof(name));
		}
	}
	public static bool IsValidVariableNameCharacter(char c) => Char.IsLetterOrDigit(c) || (c == '_');
}
