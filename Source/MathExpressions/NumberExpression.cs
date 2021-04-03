using System;
using System.Globalization;

namespace MathExpressions
{
	/// <summary>
	/// Class representing a constant number expression.
	/// </summary>
	public class NumberExpression : IExpression
	{
		/// <summary>Initializes a new instance of the <see cref="NumberExpression"/> class.</summary>
		/// <param name="value">The number value for this expression.</param>
		public NumberExpression(decimal value)
		{
			Value = value;
		}

		/// <summary>Gets the number of arguments this expression uses.</summary>
		/// <value>The argument count.</value>
		public int ArgumentCount => 0;

		public PreciseNumber Evaluate(PreciseNumber[] operands) => new(Value);


		/// <summary>Gets the number value for this expression.</summary>
		public decimal Value { get; }

		/// <summary>Determines whether the specified character is a number.</summary>
		/// <param name="c">The character to test.</param>
		/// <returns><c>true</c> if the specified character is a number; otherwise, <c>false</c>.</returns>
		/// <remarks>This method checks if the character is a digit or a decimal separator.</remarks>
		public static bool IsNumber(char c)
		{
			NumberFormatInfo f = CultureInfo.CurrentUICulture.NumberFormat;
			//TODO BUG: This will not work for a multi-character decimal separator.
			return Char.IsDigit(c) || f.NumberDecimalSeparator.Contains(c);
		}

		/// <summary>Determines whether the specified char is negative sign.</summary>
		/// <param name="c">The char to check.</param>
		/// <returns><c>true</c> if the specified char is negative sign; otherwise, <c>false</c>.</returns>
		public static bool IsNegativeSign(char c)
		{
			NumberFormatInfo f = CultureInfo.CurrentUICulture.NumberFormat;
			//TODO BUG: This will not work for a multi-character negative sign.
			return f.NegativeSign.Contains(c);
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		/// <filterPriority>2</filterPriority>
		public override string ToString() => Value.ToString(CultureInfo.CurrentCulture);
	}
}
