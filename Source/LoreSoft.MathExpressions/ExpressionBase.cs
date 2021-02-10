using LoreSoft.MathExpressions.Properties;
using System;

namespace LoreSoft.MathExpressions
{
	/// <summary>The base class for expressions</summary>
	public abstract class ExpressionBase : IExpression
	{
		/// <summary>Gets the number of arguments this expression uses.</summary>
		public abstract int ArgumentCount { get; }

		/// <summary>Gets or sets the evaluate delegate.</summary>
		public virtual MathEvaluate Evaluate { get; set; }

		/// <summary>Validates the specified numbers for the expression.</summary>
		/// <param name="numbers">The numbers to validate.</param>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		protected void Validate(double[] numbers)
		{
			if (numbers is null)
			{
				throw new ArgumentNullException(nameof(numbers));
			}
			if (numbers.Length != ArgumentCount)
			{
				throw new ArgumentException(Resources.InvalidLengthOfArray, nameof(numbers));
			}
		}
	}
}
