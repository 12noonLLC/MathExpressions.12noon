using MathExpressions.Properties;
using System;

namespace MathExpressions;

/// <summary>
/// The interface used when running expressions
/// </summary>
public interface IExpression
{
	/// <summary>Gets the number of arguments this expression uses.</summary>
	public int ArgumentCount { get; }

	/// <summary>Evaluates this expression.</summary>
	/// <param name="operands">The numbers to evaluate.</param>
	/// <returns>The result of the evaluated numbers.</returns>
	public PreciseNumber Evaluate(PreciseNumber[] operands);

	/// <summary>Validates the specified numbers for the expression.</summary>
	/// <param name="operands">The numbers to validate.</param>
	/// <exception cref="ArgumentNullException">When numbers is null.</exception>
	/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
	public void Validate(PreciseNumber[] operands)
	{
		if (operands is null)
		{
			throw new ArgumentNullException(nameof(operands));
		}
		if (operands.Length != ArgumentCount)
		{
			throw new ArgumentException(Resources.InvalidLengthOfArray, nameof(operands));
		}
	}
}
