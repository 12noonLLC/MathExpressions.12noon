using MathExpressions.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MathExpressions
{
	/// <summary>
	/// A class representing the System.Math function expressions
	/// </summary>
	public class FunctionExpression : IExpression
	{
		private class MathFunction
		{
			public readonly string MathMethodName;

			public delegate double TMathFunction(PreciseNumber[] operands);
			private readonly TMathFunction _function;

			public int ArgumentCount { get; }

			public MathFunction(string methodName, TMathFunction function, int argumentCount)
			{
				MathMethodName = methodName;
				_function = function;
				ArgumentCount = argumentCount;
			}

			public PreciseNumber Invoke(PreciseNumber[] operands)
			{
				Debug.Assert(operands.Length == ArgumentCount, "The number of arguments is validated by the caller.");

				// Note: Some Math functions can return Double.NaN, Double.PositiveInfinity, etc. which cannot cast to Decimal.
				double result;
				result = _function(operands);
				return new PreciseNumber(result);
			}
		}

		private static readonly Dictionary<string, MathFunction> MathFunctions = new();
		static FunctionExpression()
		{
			// Note: The keys must be uppercase for <see cref="FunctionExpression"/>.

			// Basic functions
			MathFunctions.Add("SQRT",		new MathFunction(nameof(Math.Sqrt),		(operands) => Math.Sqrt(operands[0].AsDouble), 1));
			MathFunctions.Add("CBRT",		new MathFunction(nameof(Math.Cbrt),		(operands) => Math.Cbrt(operands[0].AsDouble), 1));
			MathFunctions.Add("ABS",		new MathFunction(nameof(Math.Abs),		(operands) => Math.Abs(operands[0].AsDouble), 1));
			MathFunctions.Add("POW",		new MathFunction(nameof(Math.Pow),		(operands) => Math.Pow(operands[0].AsDouble, operands[1].AsDouble), 2));
			MathFunctions.Add("MIN",		new MathFunction(nameof(Math.Min),		(operands) => Math.Min(operands[0].AsDouble, operands[1].AsDouble), 2));
			MathFunctions.Add("MAX",		new MathFunction(nameof(Math.Max),		(operands) => Math.Max(operands[0].AsDouble, operands[1].AsDouble), 2));
			MathFunctions.Add("ROUND",		new MathFunction(nameof(Math.Round),		(operands) => Math.Round(operands[0].AsDouble, (int)operands[1].AsDouble), 2));
			MathFunctions.Add("TRUNCATE",	new MathFunction(nameof(Math.Truncate),	(operands) => Math.Truncate(operands[0].AsDouble), 1));
			MathFunctions.Add("FLOOR",		new MathFunction(nameof(Math.Floor),		(operands) => Math.Floor(operands[0].AsDouble), 1));
			MathFunctions.Add("CEILING",	new MathFunction(nameof(Math.Ceiling),	(operands) => Math.Ceiling(operands[0].AsDouble), 1));

			// Trigonometric functions
			MathFunctions.Add("COS",		new MathFunction(nameof(Math.Cos),		(operands) => Math.Cos(operands[0].AsDouble), 1));
			MathFunctions.Add("COSH",		new MathFunction(nameof(Math.Cosh),		(operands) => Math.Cosh(operands[0].AsDouble), 1));
			MathFunctions.Add("ACOS",		new MathFunction(nameof(Math.Acos),		(operands) => Math.Acos(operands[0].AsDouble), 1));
			MathFunctions.Add("SIN",		new MathFunction(nameof(Math.Sin),		(operands) => Math.Sin(operands[0].AsDouble), 1));
			MathFunctions.Add("SINH",		new MathFunction(nameof(Math.Sinh),		(operands) => Math.Sinh(operands[0].AsDouble), 1));
			MathFunctions.Add("ASIN",		new MathFunction(nameof(Math.Asin),		(operands) => Math.Asin(operands[0].AsDouble), 1));
			MathFunctions.Add("TAN",		new MathFunction(nameof(Math.Tan),		(operands) => Math.Tan(operands[0].AsDouble), 1));
			MathFunctions.Add("TANH",		new MathFunction(nameof(Math.Tanh),		(operands) => Math.Tanh(operands[0].AsDouble), 1));
			MathFunctions.Add("ATAN",		new MathFunction(nameof(Math.Atan),		(operands) => Math.Atan(operands[0].AsDouble), 1));
			MathFunctions.Add("ATAN2",		new MathFunction(nameof(Math.Atan2),		(operands) => Math.Atan2(operands[0].AsDouble, operands[1].AsDouble), 2));
			MathFunctions.Add("ATANH",		new MathFunction(nameof(Math.Atanh),		(operands) => Math.Atanh(operands[0].AsDouble), 1));

			// Logarithmic functions
			MathFunctions.Add("EXP",		new MathFunction(nameof(Math.Exp),		(operands) => Math.Exp(operands[0].AsDouble), 1));
			MathFunctions.Add("LOG",		new MathFunction(nameof(Math.Log),		(operands) => Math.Log(operands[0].AsDouble), 1));
			MathFunctions.Add("LOG10",		new MathFunction(nameof(Math.Log10),		(operands) => Math.Log10(operands[0].AsDouble), 1));
			MathFunctions.Add("LOG2",		new MathFunction(nameof(Math.Log2),		(operands) => Math.Log2(operands[0].AsDouble), 1));
		}


		private readonly MathFunction _mathFunction;

		/// <summary>Initializes a new instance of the <see cref="FunctionExpression"/> class.</summary>
		/// <param name="function">The function.</param>
		internal FunctionExpression(string function)
		{
			// REF: https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1308
			if (!MathFunctions.TryGetValue(function.ToUpperInvariant(), out _mathFunction!))
			{
				throw new ArgumentException(String.Format(Resources.InvalidFunctionName1, function), nameof(function));
			}
		}

		/// <summary>Executes the function on specified numbers.</summary>
		/// <param name="operands">The numbers used in the function.</param>
		/// <returns>The result of the function execution. May be Double.NaN, Double.PositiveInfinity, etc.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		public PreciseNumber Evaluate(PreciseNumber[] operands)
		{
			((IExpression)this).Validate(operands);

			return _mathFunction.Invoke(operands);
		}

		/// <summary>Gets the number of arguments this expression uses.</summary>
		/// <value>The argument count.</value>
		public int ArgumentCount => _mathFunction.ArgumentCount;

		/// <summary>Determines whether the specified function name is a function.</summary>
		/// <param name="function">The function name.</param>
		/// <returns><c>true</c> if the specified name is a function; otherwise, <c>false</c>.</returns>
		public static bool IsFunction(string function) => MathFunctions.ContainsKey(function.ToUpperInvariant());

		/// <summary>Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.</summary>
		/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.</returns>
		/// <filterPriority>2</filterPriority>
		public override string ToString() => _mathFunction.MathMethodName;

		/// <summary>
		/// Gets the function names.
		/// </summary>
		/// <returns>An array of function names.</returns>
		public static string[] GetFunctionNames() => MathFunctions.Keys.ToArray();
	}
}
