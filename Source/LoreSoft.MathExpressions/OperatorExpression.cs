using LoreSoft.MathExpressions.Properties;
using System;
using System.Linq;

namespace LoreSoft.MathExpressions
{
	/// <summary>
	/// Class representing a math operator expression.
	/// </summary>
	public class OperatorExpression : ExpressionBase
	{
		/// <summary>The supported math operators by this class.</summary>
		private static readonly char[] operatorSymbols = new char[] { '+', '-', '*', 'x', '/', '%', '^' };

		/// <summary>
		/// Determine whether the passed operator has higher or lower precedence.
		/// </summary>
		/// <param name="c">Operator to check</param>
		/// <returns>Returns 2 if if the passed operator has a higher precedence; 1 if it is lower.</returns>
		public static int Precedence(string c) => (c.Length == 1) && ((c[0] == '*') || (c[0] == 'x') || (c[0] == '/') || (c[0] == '%')) ? 2 : 1;

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
				case "+":
					base.Evaluate = Add;
					MathOperator = MathOperator.Add;
					break;
				case "-":
					base.Evaluate = Subtract;
					MathOperator = MathOperator.Subtract;
					break;
				case "*":
				case "x":
					base.Evaluate = Multiply;
					MathOperator = MathOperator.Multiply;
					break;
				case "/":
					base.Evaluate = Divide;
					MathOperator = MathOperator.Divide;
					break;
				case "%":
					base.Evaluate = Modulo;
					MathOperator = MathOperator.Modulo;
					break;
				case "^":
					base.Evaluate = Power;
					MathOperator = MathOperator.Power;
					break;

				default:
					throw new ArgumentException(String.Format(Resources.InvalidOperator1, @operator), nameof(@operator));
			}
		}

		/// <summary>Gets the math operator.</summary>
		/// <value>The math operator.</value>
		public MathOperator MathOperator { get; }

		/// <summary>Gets the number of arguments this expression uses.</summary>
		/// <value>The argument count.</value>
		public override int ArgumentCount => 2;

		/// <summary>Adds the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		public double Add(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Aggregate(0d, (accumulate, n) => accumulate + n);
		}

		/// <summary>Subtracts the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		public double Subtract(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => accumulate - n);
		}

		/// <summary>Multiples the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		public double Multiply(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Aggregate(1d, (accumulate, n) => accumulate * n);
		}

		/// <summary>Divides the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		public double Divide(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => accumulate / n);
		}

		/// <summary>Modulo the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		public double Modulo(double[] numbers)
		{
			base.Validate(numbers);
			return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => accumulate % n);
		}

		/// <summary>Power for the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		public double Power(double[] numbers)
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

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		/// <filterPriority>2</filterPriority>
		public override string ToString() => MathOperator.ToString();
	}
}