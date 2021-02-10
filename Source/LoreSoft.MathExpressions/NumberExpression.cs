using System;
using System.Globalization;

namespace MathExpressions
{
	/// <summary>
	/// Class representing a constant number expression.
	/// </summary>
	public class NumberExpression : ExpressionBase
	{
		/// <summary>Initializes a new instance of the <see cref="NumberExpression"/> class.</summary>
		/// <param name="value">The number value for this expression.</param>
		public NumberExpression(double value)
		{
			Value = value;
			base.Evaluate = (double[] numbers) => Value;
		}

		/// <summary>Gets the number of arguments this expression uses.</summary>
		/// <value>The argument count.</value>
		public override int ArgumentCount => 0;

		/// <summary>Gets the number value for this expression.</summary>
		public double Value { get; }

		/// <summary>Determines whether the specified character is a number.</summary>
		/// <param name="c">The character to test.</param>
		/// <returns><c>true</c> if the specified character is a number; otherwise, <c>false</c>.</returns>
		/// <remarks>This method checks if the character is a digit or a decimal separator.</remarks>
		public static bool IsNumber(char c)
		{
			NumberFormatInfo f = CultureInfo.CurrentUICulture.NumberFormat;
			//BUG: This will not work for a multi-character decimal separator.
			return Char.IsDigit(c) || f.NumberDecimalSeparator.IndexOf(c) >= 0;
		}

		/// <summary>Determines whether the specified char is negative sign.</summary>
		/// <param name="c">The char to check.</param>
		/// <returns><c>true</c> if the specified char is negative sign; otherwise, <c>false</c>.</returns>
		public static bool IsNegativeSign(char c)
		{
			NumberFormatInfo f = CultureInfo.CurrentUICulture.NumberFormat;
			//BUG: This will not work for a multi-character negative sign.
			return f.NegativeSign.IndexOf(c) >= 0;
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
