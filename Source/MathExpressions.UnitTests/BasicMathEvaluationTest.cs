using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MathExpressions.UnitTests;

[TestClass]
public class BasicMathEvaluationTest
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
	public void TestWhitespace()
	{
		// We can remove leading and trailing whitespace from the expression.
		Assert.AreEqual(412d + 4d, eval.Evaluate(" 412 + 4"));
		Assert.AreEqual(412d + 4d, eval.Evaluate("412 +  4   "));
		Assert.AreEqual(412d + 4d, eval.Evaluate(" 412  +  4   "));
		Assert.AreEqual(Math.Sqrt(16), eval.Evaluate(" sqrt(16)"));
		Assert.AreEqual(Math.Sqrt(16), eval.Evaluate(" sqrt ( 16 )"));
		Assert.AreEqual(Math.Sqrt(16), eval.Evaluate("  sqrt(16) "));
		// We need to cast it to a Decimal to gain that precision and then to a Double as that's the return type.
		Assert.AreEqual((double)(decimal)Math.PI, eval.Evaluate(" pi   "));
		Assert.AreEqual((double)(decimal)Math.PI, eval.Evaluate("pi  "));

		// We cannot remove whitespace from the expression.
		Assert.ThrowsException<ParseException>(() => eval.Evaluate("4 12 + 4"));
		Assert.ThrowsException<ParseException>(() => eval.Evaluate("s q r t(16)"));
		Assert.ThrowsException<ParseException>(() => eval.Evaluate("p i"));
	}


	[TestMethod]
	public void TestCommaErrors()
	{
		Assert.ThrowsException<ParseException>(() => eval.Evaluate(","));
	}


	[TestMethod]
	public void TestInfinity()
	{
		Assert.AreEqual(Double.PositiveInfinity, eval.Evaluate(" 1 / 0 "));
		Assert.AreEqual(Double.PositiveInfinity, eval.Evaluate(" -1* (-1 / 0 ) "));
		Assert.AreEqual(Double.NegativeInfinity, eval.Evaluate(" -1 / 0 "));
	}


	[TestMethod]
	public void TestAddition()
	{
		double expected = 45 + 128;
		double result = eval.Evaluate("45 + 128").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 128 + 45;
		result = eval.Evaluate("128 + 45").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 128 + 45;
		result = eval.Evaluate("(128 + 45)").GetValueOrDefault();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestSubtraction()
	{
		double expected = 128 - 45;
		double result = eval.Evaluate("128 - 45").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 45 - 128;
		result = eval.Evaluate("45 - 128").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 45 - 128;
		result = eval.Evaluate("(45 - 128)").GetValueOrDefault();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestSubtractionPrecision()
	{
		double expected = (double)(7464.36m - 7391.21m);
		double result = eval.Evaluate("7464.36 - 7391.21").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = (double)(7391.21m - 7464.36m);
		result = eval.Evaluate("7391.21 - 7464.36").GetValueOrDefault();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestMultiplication()
	{
		double expected = 128 * 45;
		double result = eval.Evaluate("128 * 45").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 45 * 128;
		result = eval.Evaluate("45 * 128").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 45 * 128;
		result = eval.Evaluate("(45 * 128)").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 45 * 128;
		result = eval.Evaluate("45 * (128)").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 45 * 128;
		result = eval.Evaluate("(45 * (128))").GetValueOrDefault();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestMultiplicationPrecision()
	{
		double expected = (double)(14.33m * 1.15m);
		double result = eval.Evaluate("14.33 * 1.15").GetValueOrDefault();
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
	public void TestMultiplyGroupThenGroup()
	{
		Assert.AreEqual(3 * 5, eval.Evaluate("(3)(5)"));
		Assert.AreEqual(3 * 15, eval.Evaluate("(3)(15)"));
		Assert.AreEqual(2 + 3 * 5, eval.Evaluate("2+(3)(5)"));

		var expected = (1 + 2) * (3 + 4);
		Assert.AreEqual(expected, eval.Evaluate("(1+2)*(3+4)"));
		Assert.AreEqual(expected, eval.Evaluate("(1+2)(3+4)"));
	}


	[TestMethod]
	public void TestMultiplyGroupThenNumber()
	{
		// Number must be >= 0
		Assert.AreEqual((3) * 5, eval.Evaluate("(3)5"));
		Assert.AreEqual((3) * 5, eval.Evaluate("(3) 5"));
		Assert.AreEqual((3) * 15, eval.Evaluate("(3)15"));
		Assert.AreEqual((3) * 15, eval.Evaluate("(3) 15"));
		Assert.AreEqual((3) * 15 + 10, eval.Evaluate("(3)15+ 10"));
	}


	[TestMethod]
	public void TestMultiplyGroupThenFunction()
	{
		Assert.AreEqual(3 * Math.Sqrt(49), eval.Evaluate("(3)sqrt(49)"));
		Assert.AreEqual(-3 * Math.Sqrt(49), eval.Evaluate("(-3)sqrt(49)"));
		Assert.AreEqual(14 * Math.Sqrt(49), eval.Evaluate("(14)sqrt(49)"));
		Assert.AreEqual(-14 * Math.Sqrt(49), eval.Evaluate("(-14)sqrt(49)"));
	}

	[TestMethod]
	public void TestMultiplyGroupThenVariable()
	{
		Assert.AreEqual((double)(3 * (decimal)Math.PI), eval.Evaluate("(3)pi"));
		Assert.AreEqual((double)(-3 * (decimal)Math.PI), eval.Evaluate("(-3)pi"));
		Assert.AreEqual((double)(14 * (decimal)Math.PI), eval.Evaluate("(14)pi"));
		Assert.AreEqual((double)(-14 * (decimal)Math.PI), eval.Evaluate("(-14)pi"));

		Assert.AreEqual((double)(3 * (decimal)Math.E), eval.Evaluate("(3)e"));
		Assert.AreEqual((double)(-3 * (decimal)Math.E), eval.Evaluate("(-3)e"));
		Assert.AreEqual((double)(14 * (decimal)Math.E), eval.Evaluate("(14)e"));
		Assert.AreEqual((double)(-14 * (decimal)Math.E), eval.Evaluate("(-14)e"));
	}

	[TestMethod]
	public void TestMultiplyFunctionThenGroup()
	{
		Assert.AreEqual(Math.Sqrt(16) * 5, eval.Evaluate("sqrt(16)(5)"));
		Assert.AreEqual(Math.Sqrt(16) * 15, eval.Evaluate("sqrt(16)(15)"));
		Assert.AreEqual(2 + Math.Sqrt(16) * 5, eval.Evaluate("2+sqrt(16)(5)"));

		Assert.AreEqual(Math.Sqrt(49) * 3, eval.Evaluate("sqrt(49)(3)"));
		Assert.AreEqual(Math.Sqrt(49) * 12, eval.Evaluate("sqrt(49)(12)"));
		Assert.AreEqual(Math.Sqrt(49) * -12, eval.Evaluate("sqrt(49)(-12)"));

		Assert.AreEqual((9 / 3) * Math.Sqrt(9) * (24 / 2), eval.Evaluate("(9 / 3)sqrt(9)(24 / 2)"));
	}

	[TestMethod]
	public void TestMultiplyFunctionThenNumber()
	{
		// Number must be >= 0
		Assert.AreEqual(Math.Sqrt(49) * 5, eval.Evaluate("sqrt(49)5"));
		Assert.AreEqual(Math.Sqrt(49) * 5, eval.Evaluate("sqrt(49) 5"));
		Assert.AreEqual(Math.Sqrt(49) * 15, eval.Evaluate("sqrt(49)15"));
		Assert.AreEqual(Math.Sqrt(49) * 15, eval.Evaluate("sqrt(49) 15"));
		Assert.AreEqual(3 - Math.Sqrt(49) * 15, eval.Evaluate("3-sqrt(49) 15"));
		Assert.AreEqual(3 + Math.Sqrt(49) * 15 + 10, eval.Evaluate("3 + sqrt(49)15+ 10"));
	}

	[TestMethod]
	public void TestMultiplyFunctionThenFunction()
	{
		Assert.AreEqual(Math.Sqrt(16) * Math.Sqrt(49), eval.Evaluate("sqrt(16)sqrt(49)"));
		Assert.AreEqual(Math.Sqrt(144) * Math.Sqrt(49), eval.Evaluate("sqrt(144)sqrt(49)"));
	}

	[TestMethod]
	public void TestMultiplyFunctionThenVariable()
	{
		Assert.AreEqual((double)((decimal)Math.Sqrt(16) * (decimal)Math.PI), eval.Evaluate("sqrt(16)pi"));
		Assert.AreEqual((double)((decimal)Math.Sqrt(144) * (decimal)Math.PI), eval.Evaluate("sqrt(144)pi"));
	}


	[TestMethod]
	public void TestMultiplyNumberThenGroup()
	{
		Assert.AreEqual(3 * 5, eval.Evaluate("3(5)"));
		Assert.AreEqual(3 * 15, eval.Evaluate("3(15)"));
		Assert.AreEqual(3 * -5, eval.Evaluate("3(-5)"));
		Assert.AreEqual(3 * -15, eval.Evaluate("3(-15)"));
	}

	[TestMethod]
	public void TestMultiplyNumberThenFunction()
	{
		Assert.AreEqual(3 * Math.Sqrt(49), eval.Evaluate("3sqrt(49)"));
		Assert.AreEqual(14 * Math.Sqrt(49), eval.Evaluate("14sqrt(49)"));
	}

	[TestMethod]
	public void TestMultiplyNumberThenVariable()
	{
		Assert.AreEqual((double)(-3 * (decimal)Math.PI), eval.Evaluate("-3pi"));
		Assert.AreEqual((double)(3 * (decimal)Math.PI), eval.Evaluate("3pi"));
		Assert.AreEqual((double)(-14 * (decimal)Math.PI), eval.Evaluate("-14pi"));

		double result = (double)(14m * (decimal)Math.PI);
		Assert.AreEqual(result, eval.Evaluate("14pi"));
		Assert.AreEqual((double)(2m * (decimal)result), eval.Evaluate("2answer"));
		Assert.AreEqual((double)(-3m * (2m * (decimal)result)), eval.Evaluate("-3answer"));

		double xyz = (double)(14m * (decimal)Math.PI);
		Assert.AreEqual(xyz, eval.Evaluate("xyz = 14pi"));
		Assert.AreEqual((double)(16m * (decimal)xyz), eval.Evaluate("16xyz"));
		Assert.AreEqual((double)(-39m * (decimal)xyz), eval.Evaluate("-39xyz"));
	}

	[TestMethod]
	public void TestMultiplyVariableThenGroup()
	{
		Assert.AreEqual((double)((decimal)Math.PI * 5), eval.Evaluate("pi(5)"));
		Assert.AreEqual((double)((decimal)Math.PI * -15), eval.Evaluate("pi(-15)"));

		double result = (double)((decimal)Math.PI * 15m);
		Assert.AreEqual(result, eval.Evaluate("pi(15)"));
		Assert.AreEqual((double)((decimal)result * 3m), eval.Evaluate("answer(3)"));
	}


	/// <summary>
	/// Legal but NOT multiplied:
	///	- group then negative number (subtraction)
	///	- variable then number (variable name) answer52 or y9
	///	- variable then negative number (subtraction) "pi-4"
	///	- function then negative number (subtraction)
	/// </summary>
	[TestMethod]
	public void TestNotMultiplication()
	{
		Assert.AreEqual((3) - 15, eval.Evaluate("(3)-15"));

		Assert.AreEqual((5) - 4, eval.Evaluate("(5)-4"));
		Assert.AreEqual((5) - 4, eval.Evaluate("answer"));
		Assert.AreEqual((5) - 4, eval.Evaluate("answer52 = answer"));
		Assert.AreEqual(((5) - 4) - 9, eval.Evaluate("answer52-9"));

		Assert.AreEqual(Math.Sqrt(49) - 5, eval.Evaluate("sqrt(49)-5"));
		Assert.AreEqual(Math.Sqrt(121) - 45, eval.Evaluate("sqrt(121)-45"));
	}

	[TestMethod]
	public void TestDivision()
	{
		// 128m/45m								= 2.8444444444444444444444444444
		// (double)(128m/45m)				= 2.8444444444444446
		// 128d/45d								= 2.8444444444444446
		// (double)(decimal)(128d/45d)	= 2.84444444444444
		double expected = (double)(128m / 45m);
		double result = eval.Evaluate("128 / 45").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = (double)(45m / 128m);
		Assert.AreEqual(expected, eval.Evaluate("45 / 128"));
		Assert.AreEqual(expected, eval.Evaluate("(45 / 128)"));
	}

	[TestMethod]
	public void TestModulo()
	{
		double expected = 128 % 45;
		double result = eval.Evaluate("128 % 45").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 45 % 128;
		result = eval.Evaluate("45 % 128").GetValueOrDefault();
		Assert.AreEqual(expected, result);

		expected = 45 % 128;
		result = eval.Evaluate("(45 % 128)").GetValueOrDefault();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestPower()
	{
		// This quantity is too large to cast to a Decimal.
		Assert.ThrowsException<OverflowException>(() => eval.Evaluate("128 ^ 45"));

		Assert.ThrowsException<OverflowException>(() => eval.Evaluate("45^18"));
		Assert.ThrowsException<OverflowException>(() => eval.Evaluate("(45 ^ 18)"));

		double expected = Math.Pow(45, 16);
		double result = eval.Evaluate("45 ^ 16").GetValueOrDefault();
		Assert.AreEqual(expected, result);
		result = eval.Evaluate("(45 ^ 16)").GetValueOrDefault();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestPrecedence()
	{
		Assert.AreEqual(Math.Sqrt(25) + Math.Pow(3, 4), eval.Evaluate("sqrt(25)+3^4"));
		Assert.AreEqual(3 + 4 * 2, eval.Evaluate("3 + 4 * 2"));
		Assert.AreEqual((3 + 4) * 2, eval.Evaluate("(3 + 4) * 2"));
		Assert.AreEqual(4 * 12 + 8, eval.Evaluate("4 * 12 + 8"));
		Assert.AreEqual(2 + 4 * 3 + 5 * Math.Pow(6, 3) - 7, eval.Evaluate("2+4*3+5*6^3-7"));
	}
}
