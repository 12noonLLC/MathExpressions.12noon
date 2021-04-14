using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MathExpressions.UnitTests
{
	[TestClass]
	public class MathEvaluatorTest
	{
		//public TestContext TestContext { get; set; }

		private readonly MathEvaluator eval = new();

		[ClassInitialize]
		public static void ClassSetup(TestContext _)
		{
		}

		[ClassCleanup]
		public static void ClassTeardown()
		{
		}


		[TestInitialize]
		public void TestSetup()
		{
		}

		[TestCleanup]
		public void TestTeardown()
		{
		}


      [TestMethod]
      public void EvaluateNegative()
      {
         double expected = 2d + -1d;
         double result = eval.Evaluate("2 + -1").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = -2d + 1d;
         result = eval.Evaluate("-2 + 1").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = (2d + -1d) * (-1d + 2d);
         result = eval.Evaluate("(2 + -1) * (-1 + 2)").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         // this failed due to a bug in parsing whereby the minus sign was erroneously mistaken for a negative sign.  
         // which left the -4 on the calculationStack at the end of evaluation. 
         expected = (-4 - 3) * 5;
         result = eval.Evaluate("(-4-3) *5").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateLog10()
      {
         double result = eval.Evaluate("log10(10)").GetValueOrDefault();
         Assert.AreEqual(1d, result);

         result = eval.Evaluate("log10(100)").GetValueOrDefault();
         Assert.AreEqual(2d, result);
      }

      [TestMethod]
      public void EvaluateSimple()
      {
         double expected = (2d + 1d) * (1d + 2d);
         double result = eval.Evaluate("(2 + 1) * (1 + 2)").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = 2d + 1d * 1d + 2d;
         result = eval.Evaluate("2 + 1 * 1 + 2").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = 1d / 2d;
         result = eval.Evaluate("1/2").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateComplex()
      {
         double expected = ((1d + 2d) + 3d) * 2d - 8d / 4d;
         double result = eval.Evaluate("((1 + 2) + 3) * 2 - 8 / 4").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = 3d + 4d / 5d - 8d;
         result = eval.Evaluate("3+4/5-8").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = Math.Pow(1, 2) + 5 * 1 + 14;
         result = eval.Evaluate("1 ^ 2 + 5 * 1 + 14").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateComplexPower()
      {
         double expected = Math.Pow(1, 2) + 5 * 1 + 14;
         double result = eval.Evaluate("pow(1,2) + 5 * 1 + 14").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = Math.Pow(13, 7) + 5 * 1 + 14;
         result = eval.Evaluate("pow(13 , 7) + 5 * 1 + 14").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionSin()
      {
         double expected = Math.Sin(45);
         double result = eval.Evaluate("sin(45)").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionSinMath()
      {
         double expected = Math.Sin(45) + 45;
         double result = eval.Evaluate("sin(45) + 45").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionPow()
      {
         double expected = Math.Pow(45, 2);
         double result = eval.Evaluate("pow(45, 2)").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMin()
      {
         double expected = Math.Min(45, 50);
         double result = eval.Evaluate("min(45, 50)").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionRound()
      {
         double expected = Math.Round(1.23456789, 4);
         double result = eval.Evaluate("round(1.23456789, 4)").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMinMath()
      {
         double expected = Math.Min(45, 50) + 45;
         double result = eval.Evaluate("min(45, 50) + 45").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMinNested()
      {
         Assert.AreEqual(Math.Min(3, Math.Min(45, 50)), eval.Evaluate("min(3, min(45,50))"));
         Assert.AreEqual(Math.Min(Math.Min(45, 50), 3), eval.Evaluate("min(min(45,50), 3)"));
      }

      [TestMethod]
      public void EvaluateFunctionMinWithEmbeddedParenthesis()
      {
         double expected = Math.Min(3, (45 + 50));
         double result = eval.Evaluate("min(3, (45+50))").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = Math.Max((5), 3 * Math.Min(45, 50));
         result = eval.Evaluate("Max((5), 3 * Min(45, 50))").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      /// <summary>
      /// The parser needs to track whether commas are at the same depth as a function.
      /// </summary>
      /// <see cref="MathEvaluator.TryComma"/>
      /// <seealso cref="MathEvaluator.CountFunctionArguments"/>
      [TestMethod]
      public void EvaluateFunctionsWithinParenthesis()
      {
         double expected;
         double result;

         /// This used to fail in <see cref="MathExpressions.TryComma()"/> because the function is INSIDE a group. [functionDepth = 1; groupDepth = 2]
         expected = (3 * Math.Min(45, 50));
			result = eval.Evaluate("(3 * Min(45, 50))").GetValueOrDefault();
			Assert.AreEqual(expected, result);

			expected = Math.Sqrt((3 * Math.Min(45,50)));
			result = eval.Evaluate("Sqrt((3 * Min(45,50)))").GetValueOrDefault();
			Assert.AreEqual(expected, result);

			expected = Math.Max((5), (3 * Math.Min(45, 50)));
         result = eval.Evaluate("Max((5), (3 * Min(45, 50)))").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = (double)((3m * (decimal)Math.Min(45d, 50d)) + (decimal)Math.Sqrt((double)(3m * (decimal)Math.Min(45d, 50d))));
         result = eval.Evaluate("(3 * Min(45, 50)) + Sqrt((3 * Min(45,50)))").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }


      [TestMethod]
      public void TestAbsolute()
      {
         double expected = Math.Abs(2.4);
         double result = eval.Evaluate("Abs(2.4)").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = Math.Abs(-2.5);
         result = eval.Evaluate("Abs(-2.5)").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void TestSquareRoot()
      {
         double expected = Math.Sqrt(26.4);
         double result = eval.Evaluate("Sqrt(26.4)").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         // Note: Cannot cast Double.NaN to Decimal.
         expected = Math.Sqrt(-2.5);
         result = eval.Evaluate("Sqrt(-2.5)").GetValueOrDefault();
         Assert.AreEqual(Double.NaN, expected);
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void TestCeiling()
      {
         double expected = Math.Ceiling(2.4);
         double result = eval.Evaluate("Ceiling(2.4)").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = Math.Ceiling(2.5);
         result = eval.Evaluate("Ceiling(2.5)").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = Math.Ceiling(2.6);
         result = eval.Evaluate("Ceiling(2.6)").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      public void TestFloor()
      {
         double expected = Math.Floor(2.4);
         double result = eval.Evaluate("Floor(2.4)").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = Math.Floor(2.5);
         result = eval.Evaluate("Floor(2.5)").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = Math.Floor(2.6);
         result = eval.Evaluate("Floor(2.6)").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMax()
      {
         double expected = Math.Max(45, 50);
         double result = eval.Evaluate("max(45, 50)").GetValueOrDefault();

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMaxNested()
      {
         double expected = Math.Max(3, Math.Max(45, 50));
         double result = eval.Evaluate("max(3, max(45,50))").GetValueOrDefault();

         Assert.AreEqual(expected, result);
      }

      [DataTestMethod]
      [DataRow("(1,2)")]
      [ExpectedException(typeof(ParseException))]
      public void EvaluateBadSyntax(string expr)
      {
         eval.Evaluate(expr);
      }


      [DataTestMethod]
      [DataRow("2*45,")]
      [DataRow("min(,2,3)")]
      [DataRow("sin(3,)")]
      [DataRow("min(min(3,4),,4)")]
      [ExpectedException(typeof(ParseException))]
      public void EvaluateMisplacedComma(string expr)
      {
         eval.Evaluate(expr);
      }


      [DataTestMethod]
      // This results in 3 things being added to expression queue, when only 2 are expected by MIN function
      [DataRow("min((1,2),3)")]
      // This results in 4 things being added to expression queue, when only 2 are expected by MAX function
      [DataRow("max(1,2,3,4)")]
      [ExpectedException(typeof(ParseException))]
      public void EvaluateBadArguments(string expr)
      {
         eval.Evaluate(expr);
      }


      [DataTestMethod, ExpectedException(typeof(ParseException))]
      [DataRow("min((1,2))")]
      [DataRow("min((1))")]
      public void EvaluateFunctionHasTooFewArguments(string expr)
      {
         eval.Evaluate(expr);
      }


      [TestMethod]
      public void EvaluateFunctionMaxMath()
      {
         double expected = Math.Max(45, 50) + 45;
         double result = eval.Evaluate("max(45, 50) + 45").GetValueOrDefault();

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionSinComplex()
      {
         double expected = (double)(10m * (decimal)Math.Sin(35 + 10) + 10m);
         double result = eval.Evaluate("10 * sin(35 + 10) + 10").GetValueOrDefault();
         Assert.AreEqual(expected, result);

         expected = 10 * (double)((decimal)Math.Sin(35 + 10) / (decimal)Math.Sin(2));
         result = eval.Evaluate("10 * sin(35 + 10) / sin(2)").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateVariableComplex()
      {
         int i = 10;
         eval.Variables.Add("i", i);

         double expected = Math.Pow(i, 2) + 5 * i + 14;
         double result = eval.Evaluate("i^2+5*i+14").GetValueOrDefault();

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateVariableLoop()
      {
         eval.Variables.Add("i", 0);
         double expected = 0;
         double result = 0;

			foreach (var i in System.Linq.Enumerable.Range(0, 100_000))
         {
            eval.Variables["i"] = i;
            expected += Math.Pow(i, 2) + 5 * i + 14;
            result += eval.Evaluate("i^2+5*i+14").GetValueOrDefault();
         }

         Assert.AreEqual(expected, result);
      }


      [TestMethod]
      public void EvaluateConvert()
      {
         Assert.AreEqual(12, eval.Evaluate("1 [ft->in]"));
         Assert.AreEqual(132, eval.Evaluate("11 [ft -> in]"));
      }

      [TestMethod]
      public void EvaluateFunctionOverFunction()
      {
         double expected = (double)((decimal)Math.Sin(5d) / (decimal)Math.Sin(2d));

			double result = eval.Evaluate("(sin(5)) / (sin(2))").GetValueOrDefault();
			Assert.AreEqual(expected, result);

         result = eval.Evaluate("sin(5) / sin(2)").GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      public class MultiplyBy10Expr : IExpression
      {
			public int ArgumentCount => 1;

			public PreciseNumber Evaluate(PreciseNumber[] numbers) => new(10m * numbers[0].Value);
      }

      [DataTestMethod]
      [DataRow("MB10(5)", 50d)]
      [DataRow("(MB10(5))", 50d)]
      [DataRow("MB10(5) + 10", 60d)]
      [DataRow("MB10(MB10(5))", 500d)]
      public void EvaluateCustomUnaryFunction(string expr, double expected)
      {
         eval.RegisterFunction("MB10", new MultiplyBy10Expr());
         double result = eval.Evaluate(expr).GetValueOrDefault();
         Assert.AreEqual(expected, result);
      }

      public class AddThreeNumbers : IExpression
      {
			public int ArgumentCount => 3;

         public PreciseNumber Evaluate(PreciseNumber[] numbers) => new(numbers[0].Value + numbers[1].Value + numbers[2].Value);
		}

      [TestMethod]
      public void EvaluateCustomTernaryFunction()
      {
         eval.RegisterFunction("A3", new AddThreeNumbers());
         double result = eval.Evaluate("A3(1,2,3)").GetValueOrDefault();
         Assert.AreEqual(1d + 2d + 3d, result);
      }
   }
}
