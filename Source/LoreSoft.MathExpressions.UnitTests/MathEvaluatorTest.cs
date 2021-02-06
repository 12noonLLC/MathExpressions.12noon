using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LoreSoft.MathExpressions.UnitTests
{
	[TestClass]
	public class MathEvaluatorTest
	{
		public TestContext TestContext { get; set; }

		private readonly MathEvaluator eval = new MathEvaluator();

		[ClassInitialize]
		public static void ClassSetup(TestContext testContext)
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
         double result = eval.Evaluate("2 + -1");
         Assert.AreEqual(expected, result);

         expected = -2d + 1d;
         result = eval.Evaluate("-2 + 1");
         Assert.AreEqual(expected, result);

         expected = (2d + -1d) * (-1d + 2d);
         result = eval.Evaluate("(2 + -1) * (-1 + 2)");
         Assert.AreEqual(expected, result);

         // this failed due to a bug in parsing whereby the minus sign was erroneously mistaken for a negative sign.  
         // which left the -4 on the calculationStack at the end of evaluation. 
         expected = (-4 - 3) * 5;
         result = eval.Evaluate("(-4-3) *5");
         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateLog10()
      {
         double result = eval.Evaluate("log10(10)");
         Assert.AreEqual(1d, result);
      }

      [TestMethod]
      public void EvaluateSimple()
      {
         double expected = (2d + 1d) * (1d + 2d);
         double result = eval.Evaluate("(2 + 1) * (1 + 2)");

         Assert.AreEqual(expected, result);

         expected = 2d + 1d * 1d + 2d;
         result = eval.Evaluate("2 + 1 * 1 + 2");

         Assert.AreEqual(expected, result);

         expected = 1d / 2d;
         result = eval.Evaluate("1/2");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateComplex()
      {
         double expected = ((1d + 2d) + 3d) * 2d - 8d / 4d;
         double result = eval.Evaluate("((1 + 2) + 3) * 2 - 8 / 4");

         Assert.AreEqual(expected, result);

         expected = 3d + 4d / 5d - 8d;
         result = eval.Evaluate("3+4/5-8");

         Assert.AreEqual(expected, result);

         expected = Math.Pow(1, 2) + 5 * 1 + 14;
         result = eval.Evaluate("1 ^ 2 + 5 * 1 + 14");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateComplexPower()
      {
         double expected = Math.Pow(1, 2) + 5 * 1 + 14;
         double result = eval.Evaluate("pow(1,2) + 5 * 1 + 14");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionSin()
      {
         double expected = Math.Sin(45);
         double result = eval.Evaluate("sin(45)");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionSinMath()
      {
         double expected = Math.Sin(45) + 45;
         double result = eval.Evaluate("sin(45) + 45");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionPow()
      {
         double expected = Math.Pow(45, 2);
         double result = eval.Evaluate("pow(45, 2)");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMin()
      {
         double expected = Math.Min(45, 50);
         double result = eval.Evaluate("min(45, 50)");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionRound()
      {
         double expected = Math.Round(1.23456789, 4);
         double result = eval.Evaluate("round(1.23456789, 4)");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMinMath()
      {
         double expected = Math.Min(45, 50) + 45;
         double result = eval.Evaluate("min(45, 50) + 45");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMinNested()
      {
         double expected = Math.Min(3, Math.Min(45, 50));
         double result = eval.Evaluate("min(3, min(45,50))");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMinWithEmbeddedParenthesis()
      {
         double expected = Math.Min(3, (45 + 50));
         double result = eval.Evaluate("min(3, (45+50))");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      [Ignore]
      public void EvaluateFunctionMinWithinParenthesis()
      {
         // TODO: This should work... but doesn't.
         double expected = (3 * Math.Min(45, 50));
         double result = eval.Evaluate("(3 * Min(45,50))");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMax()
      {
         double expected = Math.Max(45, 50);
         double result = eval.Evaluate("max(45, 50)");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionMaxNested()
      {
         double expected = Math.Max(3, Math.Max(45, 50));
         double result = eval.Evaluate("max(3, max(45,50))");

         Assert.AreEqual(expected, result);
      }

      [DataTestMethod]
      [DataRow("2*45,")]
      [DataRow("min(,2,3)")]
      [DataRow("sin(3,)")]
      [DataRow("min(min(3,4),,4)")]
      [DataRow("min((1,2))")]
      [ExpectedException(typeof(ParseException))]
      public void EvaluateMisplacedComma(string expr)
      {
         eval.Evaluate(expr);
      }

      [TestMethod, ExpectedException(typeof(ParseException))]
      public void EvaluateFunctionHasTooManyArguments()
      {
         // This will result in 4 things being added to expression queue, when only 2 are expected by max function
         eval.Evaluate("max(1,2,3,4)");
      }

      [TestMethod]
      public void EvaluateFunctionMaxMath()
      {
         double expected = Math.Max(45, 50) + 45;
         double result = eval.Evaluate("max(45, 50) + 45");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateFunctionSinComplex()
      {
         double expected = 10 * Math.Sin(35 + 10) + 10;
         double result = eval.Evaluate("10 * sin(35 + 10) + 10");

         Assert.AreEqual(expected, result);

         expected = 10 * Math.Sin(35 + 10) / Math.Sin(2); ;
         result = eval.Evaluate("10 * sin(35 + 10) / sin(2)");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateVariableComplex()
      {
         int i = 10;
         eval.Variables.Add("i", i);

         double expected = Math.Pow(i, 2) + 5 * i + 14;
         double result = eval.Evaluate("i^2+5*i+14");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      public void EvaluateVariableLoop()
      {
         eval.Variables.Add("i", 0);
         double expected = 0;
         double result = 0;

         for (int i = 1; i < 100_000; i++)
         {
            eval.Variables["i"] = i;
            expected += Math.Pow(i, 2) + 5 * i + 14;
            result += eval.Evaluate("i^2+5*i+14");
         }

         Assert.AreEqual(expected, result);
      }


      [TestMethod]
      public void EvaluateConvert()
      {
         double expected = 12;
         double result = eval.Evaluate("1 [ft->in]");

         Assert.AreEqual(expected, result);

         expected = 12;
         result = eval.Evaluate("1 [ft -> in]");

         Assert.AreEqual(expected, result);
      }

      [TestMethod]
      [Ignore]
      public void EvaluateFunctionOverFunction()
      {
         double expected = Math.Sin(5) / Math.Sin(2);
         double result;
         result = eval.Evaluate("(sin(5)) / (sin(2))");
         Assert.AreEqual(expected, result);

         result = eval.Evaluate("sin(5) / sin(2)");
         Assert.AreEqual(expected, result);
      }

      class MultiplyBy10Expr : IExpression
      {
			public int ArgumentCount => 1;

			private double Test(double[] numbers) => 10 * numbers[0];

			public MathEvaluate Evaluate
			{
				get => Test;
				set => throw new NotImplementedException();
			}
		}

      [DataTestMethod]
      [DataRow("MB10(5)", 50d)]
      [DataRow("(MB10(5))", 50d)]
      [DataRow("MB10(5) + 10", 60d)]
      [DataRow("MB10(MB10(5))", 500d)]
      public void EvaluateCustomUnaryFunction(string expr, double expected)
      {
         eval.RegisterFunction("MB10", new MultiplyBy10Expr());
         double result = eval.Evaluate(expr);
         Assert.AreEqual(expected, result);
      }

      class AddThreeNumbers : IExpression
      {
			public int ArgumentCount => 3;

			private double Test(double[] numbers) => numbers[0] + numbers[1] + numbers[2];

			public MathEvaluate Evaluate
			{
				get => Test;
				set => throw new NotImplementedException();
			}
		}

      [TestMethod]
      public void EvaluateCustomTernaryFunction()
      {
         eval.RegisterFunction("A3", new AddThreeNumbers());
         double result = eval.Evaluate("A3(1,2,3)");
         Assert.AreEqual(6d, result);
      }
   }
}
