using System;
using System.Linq;
using LoreSoft.MathExpressions.Properties;
using System.Diagnostics.CodeAnalysis;

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
                throw new ArgumentNullException("operator");

            switch (@operator)
            {
                case "+":
                    base.Evaluate = new MathEvaluate(Add);
                    _mathOperator = MathOperator.Add;
                    break;
                case "-":
                    base.Evaluate = new MathEvaluate(Subtract);
                    _mathOperator = MathOperator.Subtract;
                    break;
                case "*":
                case "x":
                    base.Evaluate = new MathEvaluate(Multiply);
                    _mathOperator = MathOperator.Multiply;
                    break;
                case "/":
                    base.Evaluate = new MathEvaluate(Divide);
                    _mathOperator = MathOperator.Divide;
                    break;
                case "%":
                    base.Evaluate = new MathEvaluate(Modulo);
                    _mathOperator = MathOperator.Modulo;
                    break;
                case "^":
                    base.Evaluate = new MathEvaluate(Power);
                    _mathOperator = MathOperator.Power;
                    break;

                default:
                    throw new ArgumentException(String.Format(Resources.InvalidOperator1, @operator), "operator");
            }
        }

        private MathOperator _mathOperator;

        /// <summary>Gets the math operator.</summary>
        /// <value>The math operator.</value>
        public MathOperator MathOperator
        {
            get { return _mathOperator; }
        }

        /// <summary>Gets the number of arguments this expression uses.</summary>
        /// <value>The argument count.</value>
        public override int ArgumentCount
        {
            get { return 2; }
        }

        /// <summary>Adds the specified numbers.</summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentNullException">When numbers is null.</exception>
        /// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
        public double Add(double[] numbers)
        {
            base.Validate(numbers);
            double result = 0;
            foreach (double n in numbers)
                result += n;

            return result;
        }

        /// <summary>Subtracts the specified numbers.</summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentNullException">When numbers is null.</exception>
        /// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
        public double Subtract(double[] numbers)
        {
            base.Validate(numbers);
            double? result = null;
            foreach (double n in numbers)
                if (result.HasValue)
                    result -= n;
                else
                    result = n;

            return result ?? 0;
        }

        /// <summary>Multiples the specified numbers.</summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentNullException">When numbers is null.</exception>
        /// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
        public double Multiply(double[] numbers)
        {
            base.Validate(numbers);
            double? result = null;
            foreach (double n in numbers)
                if (result.HasValue)
                    result *= n;
                else
                    result = n;

            return result ?? 0;
        }

        /// <summary>Divides the specified numbers.</summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentNullException">When numbers is null.</exception>
        /// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
        public double Divide(double[] numbers)
        {
            base.Validate(numbers);
            double? result = null;
            foreach (double n in numbers)
                if (result.HasValue)
                    result /= n;
                else
                    result = n;

            return result ?? 0;
        }

        /// <summary>Modulo the specified numbers.</summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentNullException">When numbers is null.</exception>
        /// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
        public double Modulo(double[] numbers)
        {
            base.Validate(numbers);
            double? result = null;
            foreach (double n in numbers)
                if (result.HasValue)
                    result %= n;
                else
                    result = n;

            return result ?? 0;
        }

        /// <summary>Power for the specified numbers.</summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentNullException">When numbers is null.</exception>
        /// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
        public double Power(double[] numbers)
        {
            base.Validate(numbers);
            return Math.Pow(numbers[0], numbers[1]);
        }

        /// <summary>Determines whether the specified string is a math symbol.</summary>
        /// <param name="s">The string to check.</param>
        /// <returns><c>true</c> if the specified string is a math symbol; otherwise, <c>false</c>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static bool IsSymbol(string s)
        {
            if (s == null || s.Length != 1)
                return false;
            
            char c = s[0];
            return IsSymbol(c);
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
        public override string ToString()
        {
            return _mathOperator.ToString();
        }
    }
}