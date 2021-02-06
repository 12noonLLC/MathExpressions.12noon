using LoreSoft.MathExpressions;
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
	}
}
