using MathExpressions.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MathExpressions
{
	/// <summary>
	/// Evaluate math expressions
	/// </summary>
	/// <example>Using the MathEvaluator to calculate a math expression.
	/// <code>
	/// MathEvaluator eval = new MathEvaluator();
	/// //basic math
	/// decimal result = eval.Evaluate("(2 + 1) * (1 + 2)");
	/// //calling a function
	/// result = eval.Evaluate("sqrt(4)");
	/// //evaluate trigonometric 
	/// result = eval.Evaluate("cos(pi * 45 / 180.0)");
	/// //convert inches to feet
	/// result = eval.Evaluate("12 [in->ft]");
	/// //use variable
	/// result = eval.Evaluate("answer * 10");
	/// </code>
	/// </example>
	public class MathEvaluator
	{
		/// <summary>The name of the answer variable.</summary>
		/// <seealso cref="Variables"/>
		public const string AnswerVariable = "answer";

		private readonly Dictionary<string, IExpression> _expressionCache = new(StringComparer.OrdinalIgnoreCase);
		private readonly List<string> _innerFunctions;

		/// Used only during a call to <see	cref="Evaluate(string)"/>.
		private readonly Queue<IExpression> _expressionQueue = new();
		private readonly Stack<string> _symbolStack = new();
		private uint _nestedFunctionDepth = 0;
		private char _currentChar;


		/// <summary>
		/// Initializes a new instance of the <see cref="MathEvaluator"/> class.
		/// </summary>
		public MathEvaluator()
		{
			Variables = new VariableDictionary(this);
			_innerFunctions = new List<string>(FunctionExpression.GetFunctionNames());
			_innerFunctions.Sort();
			Functions = new ReadOnlyCollection<string>(_innerFunctions);
		}


		/// <summary>
		/// Gets the variables collections.
		/// </summary>
		/// <value>The variables for <see cref="MathEvaluator"/>.</value>
		public VariableDictionary Variables { get; }

		/// <summary>Gets the functions available to <see cref="MathEvaluator"/>.</summary>
		/// <value>The functions for <see cref="MathEvaluator"/>.</value>
		/// <seealso cref="RegisterFunction"/>
		public ReadOnlyCollection<string> Functions { get; }

		/// <summary>Gets the answer from the last evaluation.</summary>
		/// <value>The answer variable value.</value>
		/// <seealso cref="Variables"/>
		public double Answer => Variables[AnswerVariable];

		/// <summary>Evaluates the specified expression.</summary>
		/// <param name="expression">The expression to evaluate.</param>
		/// <returns>The result of the evaluated expression. (Can be NaN, PositiveInfinity, or NegativeInfinity.) Null if a variable is deleted.</returns>
		/// <exception cref="ArgumentNullException">When expression is null or empty.</exception>
		/// <exception cref="ParseException">When there is an error parsing the expression.</exception>
		public double? Evaluate(string expression)
		{
			if (String.IsNullOrEmpty(expression))
			{
				throw new ArgumentNullException(nameof(expression));
			}

			// Remove any leading and trailing spaces.
			// Note: We cannot remove all whitespace because:
			// "41 25" => "4125"
			// "s q r t" => "sqrt"
			// "p i" => "pi"
			expression = expression.Trim();

			// Is the expression assigned to a variable?
			// Parse for `string=`.
			string? variableName = null;
			Match m = Regex.Match(expression, @"^\s*(\w+)\s*=\s*(.*)$");
			if (m.Success)
			{
				variableName = m.Groups[1].Value;
				if (IsFunction(variableName))
				{
					variableName = null;
				}
				else
				{
					expression = m.Groups[2].Value;

					// If no expression follows the assignment operator, clear the variable.
					// (Keep the last result as the answer value.)
					if (String.IsNullOrEmpty(expression))
					{
						Variables.Remove(variableName);
						return null;
					}
				}
			}

			using StringReader expressionReader = new(expression);
			// We need a copy to validate function arguments.
			StringBuilder expressionBuilder = new(expression);
			_expressionQueue.Clear();
			_symbolStack.Clear();
			_nestedFunctionDepth = 0;

			ParseExpressionToQueue(expressionReader, expressionBuilder);

			double result = CalculateFromQueue();

			Variables[AnswerVariable] = result;
			if (variableName != null)
			{
				Variables[variableName] = result;
			}
			return result;
		}

		/// <summary>Registers a function for the <see cref="MathEvaluator"/>.</summary>
		/// <param name="functionName">Name of the function.</param>
		/// <param name="expression">An instance of <see cref="IExpression"/> for the function.</param>
		/// <exception cref="ArgumentNullException">When functionName or expression are null.</exception>
		/// <exception cref="ArgumentException">When IExpression.Evaluate property is null or the functionName is already registered.</exception>
		/// <seealso cref="Functions"/>
		/// <seealso cref="IExpression"/>
		public void RegisterFunction(string functionName, IExpression expression)
		{
			if (String.IsNullOrEmpty(functionName))
			{
				throw new ArgumentNullException(nameof(functionName));
			}
			if (expression is null)
			{
				throw new ArgumentNullException(nameof(expression));
			}
			if (IsFunction(functionName))
			{
				throw new ArgumentException(String.Format(Resources.FunctionNameRegistered1, functionName), nameof(functionName));
			}

			_innerFunctions.Add(functionName);
			_innerFunctions.Sort();
			_expressionCache.Add(functionName, expression);
		}

		/// <summary>Determines whether the specified name is a function.</summary>
		/// <param name="name">The name of the function.</param>
		/// <returns><c>true</c> if the specified name is function; otherwise, <c>false</c>.</returns>
		internal bool IsFunction(string name)
		{
			return (_innerFunctions.BinarySearch(name, StringComparer.OrdinalIgnoreCase) >= 0);
		}

		private void ParseExpressionToQueue(StringReader expressionReader, StringBuilder expressionBuilder)
		{
			char lastChar = '\0';
			_currentChar = '\0';

			do
			{
				// last non whitespace char
				if (!Char.IsWhiteSpace(_currentChar))
				{
					lastChar = _currentChar;
				}

				_currentChar = (char)expressionReader.Read();
				expressionBuilder.Remove(startIndex: 0, length: 1);

				if (Char.IsWhiteSpace(_currentChar))
				{
					continue;
				}

				// Test for number before operator in case of negative sign.
				if (TryNumber(expressionReader, expressionBuilder, lastChar))
				{
					continue;
				}

				if (TryOperator())
				{
					continue;
				}

				if (TryComma(expressionReader, expressionBuilder))
				{
					continue;
				}

				if (TryStartGroup(expressionReader, expressionBuilder))
				{
					continue;
				}

				if (TryEndGroup(expressionReader, expressionBuilder))
				{
					continue;
				}

				if (TryString(expressionReader, expressionBuilder))
				{
					continue;
				}

				if (TryConvert(expressionReader, expressionBuilder))
				{
					continue;
				}

				throw new ParseException(String.Format(Resources.InvalidCharacterEncountered1, _currentChar));
			} while (expressionReader.Peek() != -1);

			ProcessSymbolStack();
		}

		private bool TryConvert(StringReader expressionReader, StringBuilder expressionBuilder)
		{
			if (_currentChar != '[')
			{
				return false;
			}

			StringBuilder buffer = new();
			buffer.Append(_currentChar);

			char p = (char)expressionReader.Peek();
			while (Char.IsLetter(p) || Char.IsWhiteSpace(p) || (p == '-') || (p == '>') || (p == '/') || (p == ']'))
			{
				if (!Char.IsWhiteSpace(p))
				{
					buffer.Append((char)expressionReader.Read());
				}
				else
				{
					expressionReader.Read();
				}
				expressionBuilder.Remove(startIndex: 0, length: 1);

				if (p == ']')
				{
					break;
				}

				p = (char)expressionReader.Peek();
			}

			if (ConvertExpression.IsConvertExpression(buffer.ToString()))
			{
				IExpression e = GetExpressionFromSymbol(buffer.ToString());
				_expressionQueue.Enqueue(e);
				return true;
			}

			throw new ParseException(String.Format(Resources.InvalidConversionExpression1, buffer));
		}

		private bool TryString(StringReader expressionReader, StringBuilder expressionBuilder)
		{
			if (!Char.IsLetter(_currentChar))
			{
				return false;
			}

			StringBuilder buffer = new();
			buffer.Append(_currentChar);

			while (char.IsLetterOrDigit((char)expressionReader.Peek()))
			{
				buffer.Append((char)expressionReader.Read());
				expressionBuilder.Remove(startIndex: 0, length: 1);
			}

			string name = buffer.ToString();

			// Is the string a function?
			// Note: Test for function first to prevent a variable from having a function name.
			if (IsFunction(name))
			{
				_symbolStack.Push(name);
				++_nestedFunctionDepth;

				// Verify the number of arguments is correct.
				var nArgs = CountFunctionArguments(name, expressionBuilder.ToString());
				var fexpr = GetExpressionFromSymbol(name);
				if (nArgs != fexpr.ArgumentCount)
				{
					throw new ParseException(String.Format(Resources.InvalidArgumentCount1, name));
				}
				return true;
			}

			// Is the string a variable?
			if (Variables.ContainsKey(name))
			{
				double value = Variables[name];
				NumberExpression expression = new((decimal)value);
				_expressionQueue.Enqueue(expression);

				// Variable can be followed by a group.
				// If there is no operator, assume multiplication.
				char nextChar = PeekNextNonWhitespaceChar(expressionReader, expressionBuilder);
				if ((nextChar == '('))
				{
					_symbolStack.Push(((char)MathOperator.Multiply).ToString());
				}
				return true;
			}

			throw new ParseException(String.Format(Resources.InvalidVariableEncountered1, buffer));
		}

		/// <summary>
		/// Count the number of commas--at the SAME "group" level.
		/// Add one to get the number of arguments to this function.
		/// </summary>
		/// <example>
		/// (1)
		/// (1, 2)
		/// (1, sin(4))
		/// (max(4, 5), min(4, 7))
		/// (max(4, 5), 7 + 3)
		/// (max(4, 5), (7 + 3))
		/// </example>
		/// <param name="name">The name of the function for exceptions.</param>
		/// <param name="subExpression">The rest of the expression, beginning with the first character after the function name.</param>
		/// <returns>The number of arguments in the function.</returns>
		/// <exception cref="ParseException"/>
		private static int CountFunctionArguments(string name, string subExpression)
		{
			int nArgs = 1; // BUG: We assume one arg, but there could be zero. See below.

			var feeder = subExpression.AsEnumerable();
			// Read whitespace until (
			feeder = feeder.SkipWhile(c => Char.IsWhiteSpace(c));
			if (feeder.First() != '(')
			{
				throw new ParseException(String.Format(Resources.InvalidArgumentCount1, name));
			}
			feeder = feeder.Skip(1);

			int nGroupLevel = 0;

			// If the groups are matched, we should finish before the end of the string.
			while (feeder.Any())
			{
				//read until start-group, end-group, or argument separator: "(),"
				feeder = feeder.SkipWhile(c => (c != '(') && (c != ')') && (c != ','));
				switch (feeder.First())
				{
					case '(':
						feeder = feeder.Skip(1);
						++nGroupLevel;
						break;

					case ')':
						if (nGroupLevel == 0)
						{
							// same level as function, so we're done.
							// BUG: Could be ZERO arguments. (If no non-whitespace found before this.)
							return nArgs;
						}
						feeder = feeder.Skip(1);
						--nGroupLevel;
						break;

					case ',':
						feeder = feeder.Skip(1);
						// If we're at the same level as the function, this is another argument.
						if (nGroupLevel == 0)
						{
							++nArgs;
						}
						else // The comma is NOT inside this function, which is invalid.
						{
							// Unless we have nested functions, which IS valid.
							//throw new ParseException(String.Format(Resources.InvalidArgumentCount1, name));
						}
						break;

					default:
						throw new ParseException(String.Format(Resources.InvalidArgumentCount1, name));
				}
			}

			throw new ParseException(String.Format(Resources.InvalidArgumentCount1, name));
		}

		private bool TryStartGroup(StringReader expressionReader, StringBuilder expressionBuilder)
		{
			if (_currentChar != '(')
			{
				return false;
			}

			if (PeekNextNonWhitespaceChar(expressionReader, expressionBuilder) == ',')
			{
				throw new ParseException(String.Format(Resources.InvalidCharacterEncountered1, ","));
			}

			_symbolStack.Push(_currentChar.ToString());
			return true;
		}

		private bool TryComma(StringReader expressionReader, StringBuilder expressionBuilder)
		{
			if (_currentChar != ',')
			{
				return false;
			}

			// If we are not inside a function, commas are invalid.
			if (_nestedFunctionDepth == 0)
			{
				throw new ParseException(String.Format(Resources.InvalidCharacterEncountered1, _currentChar));
			}

			char nextChar = PeekNextNonWhitespaceChar(expressionReader, expressionBuilder);
			if (nextChar == ')' || nextChar == ',')
			{
				throw new ParseException(String.Format(Resources.InvalidCharacterEncountered1, _currentChar));
			}

			// This is an argument separator, so all cached symbols (that are "inside" this function) must move to the expression queue.
			// (If the stack is empty, it will throw InvalidOperationException.)
			while (_symbolStack.Peek() != "(")
			{
				IExpression e = GetExpressionFromSymbol(_symbolStack.Pop());
				_expressionQueue.Enqueue(e);
			}

			return true;
		}

		private static char PeekNextNonWhitespaceChar(StringReader expressionReader, StringBuilder expressionBuilder)
		{
			int next = expressionReader.Peek();
			while ((next != -1) && Char.IsWhiteSpace((char)next))
			{
				expressionReader.Read();
				expressionBuilder.Remove(startIndex: 0, length: 1);

				next = expressionReader.Peek();
			}
			return (char)next;
		}


		private bool TryEndGroup(StringReader expressionReader, StringBuilder expressionBuilder)
		{
			if (_currentChar != ')')
			{
				return false;
			}

			bool hasStart = false;

			while (_symbolStack.Any())
			{
				string p = _symbolStack.Pop();
				if (p == "(")
				{
					hasStart = true;

					if (!_symbolStack.Any())
					{
						break;
					}

					string n = _symbolStack.Peek();
					if (IsFunction(n))
					{
						p = _symbolStack.Pop();
						IExpression f = GetExpressionFromSymbol(p);
						_expressionQueue.Enqueue(f);
						--_nestedFunctionDepth;
					}
					break;
				}

				IExpression e = GetExpressionFromSymbol(p);
				_expressionQueue.Enqueue(e);
			}

			if (!hasStart)
			{
				throw new ParseException(Resources.UnbalancedParentheses);
			}

			// Group or function can be followed by a group, number >= 0, function, or variable.
			// If there is no operator, assume multiplication.
			char nextChar = PeekNextNonWhitespaceChar(expressionReader, expressionBuilder);
			if ((nextChar == '(') || Char.IsLetterOrDigit(nextChar))
			{
				_symbolStack.Push(((char)MathOperator.Multiply).ToString());
			}

			return true;
		}

		private bool TryOperator()
		{
			if (!OperatorExpression.IsSymbol(_currentChar))
			{
				return false;
			}

			bool repeat;
			string op = _currentChar.ToString();
			do
			{
				repeat = false;
				if (!_symbolStack.Any())
				{
					// No previous operator, so hold operator to see if a future one has a higher precedence.
					_symbolStack.Push(op);
					continue;
				}

				string prevOp = _symbolStack.Peek();
				if (prevOp == "(")
				{
					// No previous operator, so hold operator to see if a future one has a higher precedence.
					_symbolStack.Push(op);
				}
				else if (OperatorExpression.Precedence(op) > OperatorExpression.Precedence(prevOp))
				{
					// This operator has higher precedence, so hold it to see if a future one has higher precedence.
					_symbolStack.Push(op);
				}
				else
				{
					// The previous operator has higher (or same) precedence, so move it from stack to expression queue.
					IExpression e = GetExpressionFromSymbol(_symbolStack.Pop());
					_expressionQueue.Enqueue(e);
					repeat = true;
				}
			} while (repeat);

			return true;
		}

		private bool TryNumber(StringReader expressionReader, StringBuilder expressionBuilder, char lastChar)
		{
			bool isNumber = NumberExpression.IsNumber(_currentChar);
			// only negative when last char is group start, comma, or symbol
			bool isNegative = NumberExpression.IsNegativeSign(_currentChar) &&
									((lastChar == '\0') || (lastChar == '(') || (lastChar == ',') || OperatorExpression.IsSymbol(lastChar));
			if (!isNumber && !isNegative)
			{
				return false;
			}

			StringBuilder buffer = new();
			buffer.Append(_currentChar);

			char p = (char)expressionReader.Peek();
			while (NumberExpression.IsNumber(p))
			{
				_currentChar = (char)expressionReader.Read();
				expressionBuilder.Remove(startIndex: 0, length: 1);
				buffer.Append(_currentChar);
				p = (char)expressionReader.Peek();
			}

			if (!Decimal.TryParse(buffer.ToString(), out decimal value))
			{
				throw new ParseException(String.Format(Resources.InvalidNumberFormat1, buffer));
			}

			NumberExpression expression = new(value);
			_expressionQueue.Enqueue(expression);

			// Number can be followed by a group, function, or variable.
			// If there is no operator, assume multiplication.
			char nextChar = PeekNextNonWhitespaceChar(expressionReader, expressionBuilder);
			if ((nextChar == '(') || Char.IsLetter(nextChar))
			{
				_symbolStack.Push(((char)MathOperator.Multiply).ToString());
			}

			return true;
		}

		private void ProcessSymbolStack()
		{
			while (_symbolStack.Any())
			{
				string p = _symbolStack.Pop();
				if (p == "(")
				{
					throw new ParseException(Resources.UnbalancedParentheses);
				}

				IExpression e = GetExpressionFromSymbol(p);
				_expressionQueue.Enqueue(e);
			}
		}

		private IExpression GetExpressionFromSymbol(string p)
		{
			IExpression e;

			if (_expressionCache.ContainsKey(p))
			{
				return _expressionCache[p];
			}

			if (OperatorExpression.IsSymbol(p))
			{
				e = new OperatorExpression(p);
			}
			else if (FunctionExpression.IsFunction(p))
			{
				e = new FunctionExpression(p);
			}
			else if (ConvertExpression.IsConvertExpression(p))
			{
				e = new ConvertExpression(p);
			}
			else
			{
				throw new ParseException(String.Format(Resources.InvalidSymbolOnStack1, p));
			}

			_expressionCache.Add(p, e);
			return e;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Need to return Decimal and Double.PositiveInfinity and Double.NegativeInfinity and Double.NaN.
		/// </remarks>
		/// <returns></returns>
		private double CalculateFromQueue()
		{
			Stack<PreciseNumber> calculationStack = new();

			foreach (IExpression expression in _expressionQueue)
			{
				if (calculationStack.Count < expression.ArgumentCount)
				{
					throw new ParseException(String.Format(Resources.NotEnoughNumbers1, expression));
				}

				Stack<PreciseNumber> parameters = new(capacity: 2);
				foreach (int _ in Enumerable.Range(0, expression.ArgumentCount))
				{
					parameters.Push(calculationStack.Pop());
				}

				PreciseNumber subResult = expression.Evaluate(parameters.ToArray());
				calculationStack.Push(subResult);
			}

			// The remaining element is the result.
			var result = calculationStack.Pop();

			// If there are more elements, there is a parsing error.
			if (calculationStack.Any())
			{
				throw new ParseException(String.Format("{0} items '{1}' remain on the calculation stack.", calculationStack.Count, String.Join(",", calculationStack)));
			}

			return result.AsDouble;
		}
	}
}
