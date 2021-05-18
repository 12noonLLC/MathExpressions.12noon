using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MathExpressions.UnitTests
{
	[TestClass]
	public class VariablesTest
	{
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
		public void TestInitialize()
		{
			Assert.AreEqual(46d, eval.Evaluate("y = 46"));
			Assert.AreEqual(46d, eval.Variables["y"]);
			Assert.AreEqual(-45.9, eval.Evaluate("today = -45.9"));
			Assert.AreEqual(-45.9, eval.Variables["today"]);

			eval.Variables.Initialize();

			Assert.AreEqual(Math.PI, eval.Variables["PI"]);
			Assert.AreEqual(Math.E, eval.Variables["e"]);
			Assert.AreEqual(0d, eval.Variables[MathEvaluator.AnswerVariable]);

			Assert.ThrowsException<KeyNotFoundException>(() => eval.Variables["y"]);
			Assert.ThrowsException<KeyNotFoundException>(() => eval.Variables["today"]);
		}


		[TestMethod]
		public void TestAnswer()
		{
			Assert.AreEqual("answer", MathEvaluator.AnswerVariable);

			Assert.AreEqual((3 + 4) * 2, eval.Evaluate("(3 + 4) * 2"));
			Assert.AreEqual(eval.Answer, eval.Evaluate("(3 + 4) * 2"));
			Assert.AreEqual(eval.Evaluate(MathEvaluator.AnswerVariable), eval.Evaluate("(3 + 4) * 2"));

			Assert.AreEqual(3 + 4 * 4, eval.Evaluate("3 + 4 ^ 2"));
			Assert.AreEqual((3 + 4 * 4) * Math.Abs(5 - 8) - 7, eval.Evaluate("answer * abs(5-8) - 7"));
		}

		[TestMethod]
		public void TestConstants()
		{
			Assert.AreEqual((double)(decimal)Math.PI, eval.Evaluate("pi"));
			Assert.AreEqual((double)(decimal)Math.E, eval.Evaluate("e"));
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
		public void TestVariableNamesUnderscore()
		{
			Assert.IsTrue(VariableDictionary.IsValidVariableNameCharacter('A'));
			Assert.IsTrue(VariableDictionary.IsValidVariableNameCharacter('z'));
			Assert.IsTrue(VariableDictionary.IsValidVariableNameCharacter('0'));
			Assert.IsTrue(VariableDictionary.IsValidVariableNameCharacter('9'));
			Assert.IsTrue(VariableDictionary.IsValidVariableNameCharacter('_'));

			Assert.IsFalse(VariableDictionary.IsValidVariableNameCharacter('.'));
			Assert.IsFalse(VariableDictionary.IsValidVariableNameCharacter('%'));
			Assert.IsFalse(VariableDictionary.IsValidVariableNameCharacter('!'));

			Assert.IsTrue(eval.Variables.IsValidVariableName("a"));
			Assert.IsTrue(eval.Variables.IsValidVariableName("a_"));
			Assert.IsTrue(eval.Variables.IsValidVariableName("a1"));
			Assert.IsTrue(eval.Variables.IsValidVariableName("a1_"));
			Assert.IsTrue(eval.Variables.IsValidVariableName("abc"));
			Assert.IsTrue(eval.Variables.IsValidVariableName("abc_"));
			Assert.IsTrue(eval.Variables.IsValidVariableName("abc123"));
			Assert.IsTrue(eval.Variables.IsValidVariableName("abc123_"));
			Assert.IsTrue(eval.Variables.IsValidVariableName("abc_123"));
			Assert.IsTrue(eval.Variables.IsValidVariableName("abc_123_"));

			Assert.IsFalse(eval.Variables.IsValidVariableName("1"));
			Assert.IsFalse(eval.Variables.IsValidVariableName("1a"));
			Assert.IsFalse(eval.Variables.IsValidVariableName("_a"));
			Assert.IsFalse(eval.Variables.IsValidVariableName("a."));
			Assert.IsFalse(eval.Variables.IsValidVariableName("a#"));

			var x1 = 11d;
			var x2 = 22d;
			Assert.AreEqual(x1, eval.Evaluate("x_1=11"));
			Assert.AreEqual(x2, eval.Evaluate("x2_=22"));
			Assert.ThrowsException<ParseException>(() => eval.Evaluate("_x3=33"), "Variables must start with letter.");
			var answer = x1 * x2;
			Assert.AreEqual(answer, eval.Evaluate("x_1 * x2_"));
			Assert.AreEqual(answer + x1 - x2, eval.Evaluate("answer+x_1 -x2_"));
		}

		[TestMethod]
		public void TestVariableNameError()
		{
			VariableDictionary dct = new(new MathEvaluator());
			Assert.ThrowsException<ArgumentException>(() => dct.Add("bad-name", 1));
			Assert.ThrowsException<ArgumentException>(() => dct["bad-name2"] = 2);
		}

		[TestMethod]
		public void TestVariableNameConflicts()
		{
			Assert.ThrowsException<ParseException>(() => eval.Evaluate("sqrt=3"));
			Assert.ThrowsException<ParseException>(() => eval.Evaluate("x = sqrt+3"));

			Assert.AreEqual(Math.Sqrt(49), eval.Evaluate("sqrt(49)"));
			var s = Math.Sqrt(49);
			Assert.AreEqual(s, eval.Evaluate("s = sqrt(49)"));
		}

		[TestMethod]
		public void TestVariableDelete()
		{
			Assert.AreEqual(800d, eval.Evaluate("x = 800"));

			Assert.IsNull(eval.Evaluate(" x  =  "));

			Assert.ThrowsException<ParseException>(() => eval.Evaluate("  x "));
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
