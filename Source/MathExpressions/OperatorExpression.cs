using MathExpressions.Properties;
using System;
using System.Linq;

namespace MathExpressions
{
	/// <summary>
	/// Class representing a math operator expression.
	/// </summary>
	public class OperatorExpression : ExpressionBase
	{
		/// <summary>The supported math operators by this class.</summary>
		private static readonly char[] operatorSymbols = new char[] { '+', '-', '*', '/', '%', '^' };

		/// <summary>
		/// Determine whether the passed operator has higher or lower precedence.
		/// </summary>
		/// <param name="c">Operator to check</param>
		/// <returns>Returns an integer indicating the precedence of the passed operator.</returns>
		public static int Precedence(string c)
		{
			if (c.Length != 1)
			{
				return 1;
			}

			if (c[0] == '^')
			{
				return 3;
			}

			if ((c[0] == '*') || (c[0] == '/') || (c[0] == '%'))
			{
				return 2;
			}

			return 1;
		}

		/// <summary>Initializes a new instance of the <see cref="OperatorExpression"/> class.</summary>
		/// <param name="operator">The operator to use for this class.</param>
		/// <exception cref="ArgumentNullException">When the operator is null or empty.</exception>
		/// <exception cref="ArgumentException">When the operator is invalid.</exception>
		public OperatorExpression(string @operator)
		{
			if (string.IsNullOrEmpty(@operator))
			{
				throw new ArgumentNullException(nameof(@operator));
			}

			switch (@operator)
			{
				case "+":	base.Evaluate = Add;			break;
				case "-":	base.Evaluate = Subtract;	break;
				case "*":	base.Evaluate = Multiply;	break;
				case "/":	base.Evaluate = Divide;		break;
				case "%":	base.Evaluate = Modulo;		break;
				case "^":	base.Evaluate = Power;		break;
				default:
					throw new ArgumentException(String.Format(Resources.InvalidOperator1, @operator), nameof(@operator));
			}
		}

		/// <summary>Gets the number of arguments this expression uses.</summary>
		/// <value>The argument count.</value>
		public override int ArgumentCount => 2;

		/// <summary>Adds the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private double Add(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Aggregate(0d, (accumulate, n) => accumulate + n);
		}

		/// <summary>Subtracts the specified numbers.</summary>
		/// <remarks>
		/// The inherent inability of hardware to maintain accurate precision
		/// for doubles requires us to use `decimal` for subtraction.
		/// REF: https://stackoverflow.com/questions/2741903/double-minus-double-giving-precision-problems
		/// </remarks>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private double Subtract(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => (double)((decimal)accumulate - (decimal)n));
		}

		/// <summary>Multiples the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private double Multiply(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Aggregate(1d, (accumulate, n) => accumulate * n);
		}

		/// <summary>Divides the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private double Divide(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => accumulate / n);
		}

		/// <summary>Modulo the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private double Modulo(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => accumulate % n);
		}

		/// <summary>Power for the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private double Power(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => Math.Pow(accumulate, n));
		}

		/// <summary>Determines whether the specified string is a math symbol.</summary>
		/// <param name="s">The string to check.</param>
		/// <returns><c>true</c> if the specified string is a math symbol; otherwise, <c>false</c>.</returns>
		public static bool IsSymbol(string s)
		{
			if ((s is null) || (s.Length != 1))
			{
				return false;
			}

			return IsSymbol(s[0]);
		}

		/// <summary>Determines whether the specified char is a math symbol.</summary>
		/// <param name="c">The char to check.</param>
		/// <returns><c>true</c> if the specified char is a math symbol; otherwise, <c>false</c>.</returns>
		public static bool IsSymbol(char c) => operatorSymbols.Contains(c);
	}
}
