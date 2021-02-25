using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MathExpressions.UnitTests
{
	[TestClass]
	public class VariablesTest
	{
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
		public void TestAnswer()
		{
			Assert.AreEqual((3 + 4) * 2, eval.Evaluate("(3 + 4) * 2"));
			Assert.AreEqual(eval.Answer, eval.Evaluate("(3 + 4) * 2"));
			Assert.AreEqual(eval.Evaluate("answer"), eval.Evaluate("(3 + 4) * 2"));

			Assert.AreEqual(3 + 4 * 4, eval.Evaluate("3 + 4 ^ 2"));
			Assert.AreEqual((3 + 4 * 4) * Math.Abs(5 - 8) - 7, eval.Evaluate("answer * abs(5-8) - 7"));
		}

		[TestMethod]
		public void TestConstants()
		{
			Assert.AreEqual(Math.PI, eval.Evaluate("pi"));
			Assert.AreEqual(Math.E, eval.Evaluate("e"));
		}

		[TestMethod]
		public void TestVariables()
		{
			var y = 46d;
			Assert.AreEqual(y, eval.Evaluate("y = 46"));
			Assert.AreEqual(y, eval.Evaluate("y"));

			var z = y + 9d;
			Assert.AreEqual(z, eval.Evaluate("z = y + 9"));
			Assert.AreEqual(y, eval.Evaluate("y"));
			Assert.AreEqual(z, eval.Evaluate("z"));

			z = y - Math.Sqrt(16);
			Assert.AreEqual(z, eval.Evaluate("z = y - sqrt(16)"));
			Assert.AreEqual(y, eval.Evaluate("y"));
			Assert.AreEqual(z, eval.Evaluate("z"));
		}


		[TestMethod]
		public void TestVariableNames()
		{
			var x = 8d;
			var y = 5d;
			Assert.AreEqual(x, eval.Evaluate("x = 8"));
			Assert.AreEqual(y, eval.Evaluate("y = 5"));
			var answer = x + y;
			Assert.AreEqual(answer, eval.Evaluate("x+y"));
			Assert.AreEqual(x + y + answer, eval.Evaluate("x + y+answer"));
		}

		[TestMethod]
		public void TestVariableNamesNumbers()
		{
			var x1 = 18d;
			var x2 = 22d;
			Assert.AreEqual(x1, eval.Evaluate("x1=18"));
			Assert.AreEqual(x2, eval.Evaluate("x2=22"));
			var answer = x1 * x2;
			Assert.AreEqual(answer, eval.Evaluate("x1 * x2"));
			Assert.AreEqual(answer + x1 - x2, eval.Evaluate("answer+x1 -x2"));
		}

		[TestMethod]
		public void TestVariableNameConflicts()
		{
			//Assert.AreEqual(sqrt, eval.Evaluate("sqrt=3"));
			Assert.ThrowsException<ParseException>(() => eval.Evaluate("sqrt=3"));
			Assert.ThrowsException<ParseException>(() => eval.Evaluate("x = sqrt+3"));

			Assert.AreEqual(Math.Sqrt(49), eval.Evaluate("sqrt(49)"));
			var s = Math.Sqrt(49);
			Assert.AreEqual(s, eval.Evaluate("s = sqrt(49)"));
		}

		[TestMethod]
		public void TestVariablesNegativeNumber()
		{
			var a = -18d;
			Assert.AreEqual(a, eval.Evaluate("a = -18"));
			Assert.AreEqual(a, eval.Evaluate("a =-18"));
		}
	}
}
