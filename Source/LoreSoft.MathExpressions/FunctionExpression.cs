using System;
using System.Collections.Generic;
using System.Reflection;
using LoreSoft.MathExpressions.Properties;
using System.Globalization;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LoreSoft.MathExpressions
{
	/// <summary>
	/// A class representing the System.Math function expressions
	/// </summary>
	public class FunctionExpression : ExpressionBase
	{
		private class MathFunction
		{
			public int ArgumentCount => Arguments.Count();
			public readonly string MathMethodName;

			private readonly List<Type> Arguments = new List<Type>();
			private MethodInfo Method = null;

			public MathFunction(string methodName, Type firstArgument)
			{
				MathMethodName = methodName;
				Arguments.Add(firstArgument);
				Initialize();
			}

			public MathFunction(string methodName, Type firstArgument, Type secondArgument)
			{
				MathMethodName = methodName;
				Arguments.Add(firstArgument);
				Arguments.Add(secondArgument);
				Initialize();
			}

			private void Initialize()
			{
				Method = typeof(Math).GetMethod(
					 MathMethodName,
					 BindingFlags.Static | BindingFlags.Public,
					 binder: null,
					 Arguments.ToArray(),
					 modifiers: null
				);
				if (Method is null)
				{
					throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidFunctionName, MathMethodName));
				}
			}

			public double Invoke(double[] numbers)
			{
				Debug.Assert(numbers.Count() == ArgumentCount, "The number of arguments is validated by the caller.");

				object[] parameters = new object[numbers.Length];
				Array.Copy(sourceArray: numbers, destinationArray: parameters, length: numbers.Length);

				// KLUDGE: One function's second argument is Int32 (not Double).
				// To be more generic, we should do this differently.
				if ((numbers.Count() == 2) && (ArgumentCount == 2) && (Arguments[1].Name == nameof(Int32)))
				{
					parameters[1] = Convert.ToInt32(numbers[1]);
				}

				return (double)Method.Invoke(obj: null, parameters);
			}
		}

		private static readonly Dictionary<string, MathFunction> MathFunctions = new Dictionary<string, MathFunction>();
		static FunctionExpression()
		{
			// Note: The keys must be uppercase for <see cref="FunctionExpression"/>.
			MathFunctions.Add("ABS",		new MathFunction(nameof(Math.Abs),		typeof(Double)));
			MathFunctions.Add("ACOS",		new MathFunction(nameof(Math.Acos),		typeof(Double)));
			MathFunctions.Add("ASIN",		new MathFunction(nameof(Math.Asin),		typeof(Double)));
			MathFunctions.Add("ATAN",		new MathFunction(nameof(Math.Atan),		typeof(Double)));
			MathFunctions.Add("CEILING",	new MathFunction(nameof(Math.Ceiling),	typeof(Double)));
			MathFunctions.Add("COS",		new MathFunction(nameof(Math.Cos),		typeof(Double)));
			MathFunctions.Add("COSH",		new MathFunction(nameof(Math.Cosh),		typeof(Double)));
			MathFunctions.Add("EXP",		new MathFunction(nameof(Math.Exp),		typeof(Double)));
			MathFunctions.Add("FLOOR",		new MathFunction(nameof(Math.Floor),	typeof(Double)));
			MathFunctions.Add("LOG",		new MathFunction(nameof(Math.Log),		typeof(Double)));
			MathFunctions.Add("LOG10",		new MathFunction(nameof(Math.Log10),	typeof(Double)));
			MathFunctions.Add("SIN",		new MathFunction(nameof(Math.Sin),		typeof(Double)));
			MathFunctions.Add("SINH",		new MathFunction(nameof(Math.Sinh),		typeof(Double)));
			MathFunctions.Add("SQRT",		new MathFunction(nameof(Math.Sqrt),		typeof(Double)));
			MathFunctions.Add("TAN",		new MathFunction(nameof(Math.Tan),		typeof(Double)));
			MathFunctions.Add("TANH",		new MathFunction(nameof(Math.Tanh),		typeof(Double)));

			MathFunctions.Add("MAX", new MathFunction(nameof(Math.Max), typeof(Double), typeof(Double)));
			MathFunctions.Add("MIN", new MathFunction(nameof(Math.Min), typeof(Double), typeof(Double)));
			MathFunctions.Add("POW", new MathFunction(nameof(Math.Pow), typeof(Double), typeof(Double)));

			MathFunctions.Add("ROUND", new MathFunction(nameof(Math.Round), typeof(Double), typeof(Int32)));
		}


		private readonly MathFunction _mathFunction;

		/// <summary>Initializes a new instance of the <see cref="FunctionExpression"/> class.</summary>
		/// <param name="function">The function.</param>
		internal FunctionExpression(string function)
		{
			// REF: https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1308
			if (!MathFunctions.TryGetValue(function.ToUpperInvariant(), out _mathFunction))
			{
				throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.InvalidFunctionName, function), nameof(function));
			}

			base.Evaluate = new MathEvaluate(Execute);
		}

		/// <summary>Executes the function on specified numbers.</summary>
		/// <param name="numbers">The numbers used in the function.</param>
		/// <returns>The result of the function execution.</returns>
		/// <exception cref="ArgumentNullException">When numbers is null.</exception>
		/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
		public double Execute(double[] numbers)
		{
			base.Validate(numbers);

			return _mathFunction.Invoke(numbers);
		}

		/// <summary>Gets the number of arguments this expression uses.</summary>
		/// <value>The argument count.</value>
		public override int ArgumentCount => _mathFunction.ArgumentCount;

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
