using MathExpressions.Properties;
using System;
using System.Diagnostics;
using System.Linq;

namespace MathExpressions
{
	/// <summary>
	/// Class representing a math operator expression.
	/// </summary>
	/// <remarks>
	/// The inherent inability of hardware to maintain accurate precision
	/// for doubles requires us to use `decimal` for subtraction.
	/// REF: https://stackoverflow.com/questions/2741903/double-minus-double-giving-precision-problems
	/// REF: https://www.exceptionnotfound.net/decimal-vs-double-and-other-tips-about-number-types-in-net/
	/// </remarks>
	[DebuggerDisplay("op: {_operator,nq}")]
	public class OperatorExpression : IExpression
	{
		/// <summary>The supported math operators by this class.</summary>
		private static readonly char[] operatorSymbols = new char[] { '+', '-', '*', '/', '%', '^' };

		/// <summary>
		/// Returns a higher value to represent the higher precedence of the passed operator.
		/// Operators with lower precedence have lower values.
		/// </summary>
		/// <param name="c">Operator to check</param>
		/// <returns>Returns a higher value for a higher operator precedence.</returns>
		public static int Precedence(string c)
		{
			System.Diagnostics.Debug.Assert(c.Length == 1);
			System.Diagnostics.Debug.Assert(IsSymbol(c.First()));

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


		private readonly string _operator;

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

			_operator = @operator;
		}

		/// <summary>Gets the number of arguments this expression uses.</summary>
		/// <value>The argument count.</value>
		public int ArgumentCount => 2;

		public PreciseNumber Evaluate(PreciseNumber[] operands)
		{
			((IExpression)this).Validate(operands);

			return _operator switch
			{
				"+" => Add(operands),
				"-" => Subtract(operands),
				"*" => Multiply(operands),
				"/" => Divide(operands),
				"%" => Modulo(operands),
				"^" => Power(operands),
				_ => throw new ArgumentException(String.Format(Resources.InvalidOperator1, _operator), nameof(_operator)),
			};
		}


		/// <summary>Adds the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private static PreciseNumber Add(PreciseNumber[] numbers)
		{
			// if they are all actual numbers, we perform the operation with Decimals for precision.
			if (numbers.All(n => n.HasValue))
			{
				return numbers.Aggregate(new PreciseNumber(0m), (accumulate, n) => new PreciseNumber(accumulate.Value + n.Value));
			}
			else
			{
				return numbers.Aggregate(new PreciseNumber(0m), (accumulate, n) => new PreciseNumber(accumulate.AsDouble + n.AsDouble));
			}
		}

		/// <summary>Subtracts the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private static PreciseNumber Subtract(PreciseNumber[] numbers)
		{
			// if they are all actual numbers, we perform the operation with Decimals for precision.
			if (numbers.All(n => n.HasValue))
			{
				return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => new PreciseNumber(accumulate.Value - n.Value));
			}
			else
			{
				return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => new PreciseNumber(accumulate.AsDouble - n.AsDouble));
			}
		}

		/// <summary>Multiples the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private static PreciseNumber Multiply(PreciseNumber[] numbers)
		{
			// if they are all actual numbers, we perform the operation with Decimals for precision.
			if (numbers.All(n => n.HasValue))
			{
				return numbers.Aggregate(new PreciseNumber(1m), (accumulate, n) => new PreciseNumber(accumulate.Value * n.Value));
			}
			else
			{
				return numbers.Aggregate(new PreciseNumber(1m), (accumulate, n) => new PreciseNumber(accumulate.AsDouble * n.AsDouble));
			}
		}

		/// <summary>Divides the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private static PreciseNumber Divide(PreciseNumber[] numbers)
		{
			// if they are all actual numbers, we perform the operation with Decimals for precision.
			if (numbers.All(n => n.HasValue))
			{
				/// If the Decimal operation throws DivideByZeroException, we perform the operation with
				/// Doubles to determine if the result is Double.PositiveInfinity or Double.NegativeInfinity.
				try
				{
					return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => new PreciseNumber(accumulate.Value / n.Value));
				}
				catch (DivideByZeroException)
				{
					return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => new PreciseNumber(accumulate.AsDouble / n.AsDouble));
				}
			}
			else
			{
				return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => new PreciseNumber(accumulate.AsDouble / n.AsDouble));
			}
		}

		/// <summary>Modulo the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private static PreciseNumber Modulo(PreciseNumber[] numbers)
		{
			// if they are all actual numbers, we perform the operation with Decimals for precision.
			if (numbers.All(n => n.HasValue))
			{
				return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => new PreciseNumber(accumulate.Value % n.Value));
			}
			else
			{
				return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => new PreciseNumber(accumulate.AsDouble % n.AsDouble));
			}
		}

		/// <summary>Power for the specified numbers.</summary>
		/// <param name="numbers">The numbers.</param>
		/// <returns>The result of the operation.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		private static PreciseNumber Power(PreciseNumber[] numbers)
		{
			// We always use Doubles for this operation, so no need to test for errors.
			return numbers.Skip(1).Aggregate(numbers.First(), (accumulate, n) => new PreciseNumber(Math.Pow(accumulate.AsDouble, n.AsDouble)));
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
