using MathExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MathExpressions.UnitTests
{
	[TestClass]
	public class BasicMathEvaluationTest
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
		public void TestAddition()
		{
			double expected = 45 + 128;
			double result = eval.Evaluate("45 + 128");
			Assert.AreEqual(expected, result);

			expected = 128 + 45;
			result = eval.Evaluate("128 + 45");
			Assert.AreEqual(expected, result);

			expected = 128 + 45;
			result = eval.Evaluate("(128 + 45)");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestSubtraction()
		{
			double expected = 128 - 45;
			double result = eval.Evaluate("128 - 45");
			Assert.AreEqual(expected, result);

			expected = 45 - 128;
			result = eval.Evaluate("45 - 128");
			Assert.AreEqual(expected, result);

			expected = 45 - 128;
			result = eval.Evaluate("(45 - 128)");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestSubtractionPrecision()
		{
			double expected = (double)(7464.36m - 7391.21m);
			double result = eval.Evaluate("7464.36 - 7391.21");
			Assert.AreEqual(expected, result);

			expected = (double)(7391.21m - 7464.36m);
			result = eval.Evaluate("7391.21 - 7464.36");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestMultiplication()
		{
			double expected = 128 * 45;
			double result = eval.Evaluate("128 * 45");
			Assert.AreEqual(expected, result);

			expected = 45 * 128;
			result = eval.Evaluate("45 * 128");
			Assert.AreEqual(expected, result);

			expected = 45 * 128;
			result = eval.Evaluate("(45 * 128)");
			Assert.AreEqual(expected, result);

			expected = 45 * 128;
			result = eval.Evaluate("45 * (128)");
			Assert.AreEqual(expected, result);

			expected = 45 * 128;
			result = eval.Evaluate("(45 * (128))");
			Assert.AreEqual(expected, result);
		}

		[TestMethod, ExpectedException(typeof(ParseException))]
		public void TestMultiplicationAlternate()
		{
			double expected = 128 * 45;
			Assert.AreEqual(expected, eval.Evaluate("128 x 45"));
			Assert.AreEqual(expected, eval.Evaluate("128x45"));

			expected = 45 * 128;
			Assert.AreEqual(expected, eval.Evaluate("45 x 128"));
			Assert.AreEqual(expected, eval.Evaluate("45x128"));

			expected = 10 + (45 * 128);
			Assert.AreEqual(expected, eval.Evaluate("10 + 45 x 128"));
			Assert.AreEqual(expected, eval.Evaluate("10+45x128"));

			expected = (45 * 128) + 10;
			Assert.AreEqual(expected, eval.Evaluate("45 x 128 + 10"));
			Assert.AreEqual(expected, eval.Evaluate("45x128+10"));

			expected = 45 * Math.Sqrt(25);
			Assert.AreEqual(expected, eval.Evaluate("45 x sqrt(25)"));
			Assert.AreEqual(expected, eval.Evaluate("45x sqrt(25)"));

			expected = (45 * 128);
			Assert.AreEqual(expected, eval.Evaluate("(45 x 128)"));
			Assert.AreEqual(expected, eval.Evaluate("(45x128)"));

			expected = 45 * (128 + 14);
			Assert.AreEqual(expected, eval.Evaluate("45 x (128 + 14)"));
			Assert.AreEqual(expected, eval.Evaluate("45x(128+14)"));

			expected = (45 * (128 + 14));
			Assert.AreEqual(expected, eval.Evaluate("(45 x (128 + 14))"));
			Assert.AreEqual(expected, eval.Evaluate("(45x(128+14))"));
		}

		[TestMethod]
		public void TestMultiplicationX()
		{
			Assert.ThrowsException<ParseException>(() => eval.Evaluate("(45xor(26))"));
		}

		[TestMethod]
		public void TestMultiplicationNoOperator()
		{
			double expected = 3 * 5;
			Assert.AreEqual(expected, eval.Evaluate("3*(5)"));
			Assert.AreEqual(expected, eval.Evaluate("3(5)"));

			expected = (1+2)*(3+4);
			Assert.AreEqual(expected, eval.Evaluate("(1+2)*(3+4)"));
			Assert.AreEqual(expected, eval.Evaluate("(1+2)(3+4)"));
		}

		[TestMethod]
		public void TestDivision()
		{
			double expected = 128d / 45d;
			double result = eval.Evaluate("128 / 45");
			Assert.AreEqual(expected, result);

			expected = 45d / 128d;
			result = eval.Evaluate("45 / 128");
			Assert.AreEqual(expected, result);

			expected = 45d / 128d;
			result = eval.Evaluate("(45 / 128)");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestModulo()
		{
			double expected = 128 % 45;
			double result = eval.Evaluate("128 % 45");
			Assert.AreEqual(expected, result);

			expected = 45 % 128;
			result = eval.Evaluate("45 % 128");
			Assert.AreEqual(expected, result);

			expected = 45 % 128;
			result = eval.Evaluate("(45 % 128)");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestPower()
		{
			double expected = Math.Pow(128, 45);
			double result = eval.Evaluate("128 ^ 45");
			Assert.AreEqual(expected, result);

			expected = Math.Pow(45, 128);
			result = eval.Evaluate("45 ^ 128");
			Assert.AreEqual(expected, result);

			expected = Math.Pow(45, 128);
			result = eval.Evaluate("(45 ^ 128)");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestPrecedence()
		{
			Assert.AreEqual(Math.Sqrt(25) + Math.Pow(3, 4), eval.Evaluate("sqrt(25)+3^4"));
			Assert.AreEqual(3 + 4 * 2, eval.Evaluate("3 + 4 * 2"));
			Assert.AreEqual((3 + 4) * 2, eval.Evaluate("(3 + 4) * 2"));
		}
	}
}
