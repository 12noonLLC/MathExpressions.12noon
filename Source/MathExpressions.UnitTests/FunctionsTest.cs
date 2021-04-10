using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MathExpressions.UnitTests
{
	[TestClass]
	public class FunctionsTest
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
		public void TestSqrt()
		{
			Assert.AreEqual(Math.Sqrt(121), eval.Evaluate("sqrt(121)").GetValueOrDefault());
			Assert.AreEqual(Math.Sqrt(1221), eval.Evaluate("Sqrt(1221)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestCbrt()
		{
			Assert.AreEqual(Math.Cbrt(512), eval.Evaluate("cbrt(512)").GetValueOrDefault());
			Assert.AreEqual(Math.Cbrt(1221), eval.Evaluate("CBRT(1221)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestAbs()
		{
			Assert.AreEqual(Math.Abs(121), eval.Evaluate("abs(121)").GetValueOrDefault());
			Assert.AreEqual(Math.Abs(-121), eval.Evaluate("Abs(-121)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestPow()
		{
			Assert.AreEqual(Math.Pow(12, 4), eval.Evaluate("Pow(12, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Pow(-12, 4), eval.Evaluate("pow(-12, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Pow(-12, 5), eval.Evaluate("pow(-12, 5)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestMin()
		{
			Assert.AreEqual(Math.Min(12, 4), eval.Evaluate("min(12, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Min(-12, 4), eval.Evaluate("Min(-12, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Min(5, -12), eval.Evaluate("Min(5, -12)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestMax()
		{
			Assert.AreEqual(Math.Max(12, 4), eval.Evaluate("max(12, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Max(-12, 4), eval.Evaluate("Max(-12, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Max(5, -12), eval.Evaluate("Max(5, -12)").GetValueOrDefault());
		}

		// Also tests precedence of operations inside functions.
		[TestMethod]
		public void TestRound()
		{
			Assert.AreEqual(Math.Round(188d, 4), eval.Evaluate("round(188, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(5d + 188d + 2d, 4), eval.Evaluate("round(5 + 188 + 2, 4)").GetValueOrDefault());

			Assert.AreEqual(Math.Round(13d * 2d + 3d, 4), eval.Evaluate("round(13*2+3,4)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(13d + 2d * 3d, 4), eval.Evaluate("round(13+2*3,4)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(188d + 3d, 4), eval.Evaluate("round(188+3,4)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(188d + 3d, 4 + 2 * 3), eval.Evaluate("round(188+3, 4+2*3)").GetValueOrDefault());

			Assert.AreEqual(Math.Round(5d + 188d, 4), eval.Evaluate("round(5 + 188, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(3d * (188d - 5d) / 2d, 4), eval.Evaluate("round(3 * (188 - 5) / 2, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(188d + 5d, 4), eval.Evaluate("round(188 + 5, 4)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(5d + 188d + 2d, 4), eval.Evaluate("round(5 + 188 + 2, 4)").GetValueOrDefault());

			Assert.AreEqual(Math.Round(5d + Math.Pow(188d, 2), 4), eval.Evaluate("round(5 + 188 ^ 2, 4)").GetValueOrDefault());
			Assert.AreEqual(4.4994, eval.Evaluate("round(5sin(40) + cos(188) ^ 2, 4)").GetValueOrDefault());


			Assert.AreEqual(Math.Round(12.456, 2), eval.Evaluate("round(12.456, 2)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(12.455, 2), eval.Evaluate("ROUND(12.455, 2)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(12.454, 2), eval.Evaluate("ROUND(12.454, 2)").GetValueOrDefault());

			Assert.AreEqual(Math.Round(-12.456, 2), eval.Evaluate("round(-12.456, 2)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(-12.455, 2), eval.Evaluate("Round(-12.455, 2)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(-12.454, 2), eval.Evaluate("Round(-12.454, 2)").GetValueOrDefault());

			Assert.AreEqual(Math.Round(-12.1, 2), eval.Evaluate("round(-12.1, 2)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(-12.5, 2), eval.Evaluate("Round(-12.5, 2)").GetValueOrDefault());
			Assert.AreEqual(Math.Round(-12.6, 2), eval.Evaluate("ROUND(-12.6, 2)").GetValueOrDefault());

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => eval.Evaluate("Round(5.4, -12)"));

			Assert.AreEqual(Math.Round(189.12341324514355466, 4 + 3), eval.Evaluate("round(189.12341324514355466, 4 + 3)").GetValueOrDefault());

			Assert.AreEqual(Math.Round(5d + Math.Pow(188d, 2), 4), eval.Evaluate("round(5 + 188 ^ 2, 4)").GetValueOrDefault());
			Assert.AreEqual(4.4994, eval.Evaluate("round(5sin(40) + cos(188) ^ 2, 4)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestTruncate()
		{
			Assert.AreEqual(Math.Truncate(0.0), eval.Evaluate("Truncate(0)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(0.0), eval.Evaluate("Truncate(0.0)").GetValueOrDefault());

			Assert.AreEqual(Math.Truncate(0.1234), eval.Evaluate("Truncate(0.1234)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(0.123456789), eval.Evaluate("Truncate(0.123456789)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(4.0), eval.Evaluate("Truncate(4)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(4.53), eval.Evaluate("Truncate(4.53)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(189.4), eval.Evaluate("Truncate(189.4)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(104.53), eval.Evaluate("Truncate(104.53)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(2345.0), eval.Evaluate("Truncate(2345)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(2345.58), eval.Evaluate("Truncate(2345.58)").GetValueOrDefault());

			Assert.AreEqual(Math.Truncate(0.989669363564753), eval.Evaluate("Truncate(0.989669363564753)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(5.989669363564753), eval.Evaluate("Truncate(5.989669363564753)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(989669363564753.0), eval.Evaluate("Truncate(989669363564753)").GetValueOrDefault());
			Assert.AreEqual(Math.Truncate(3456789.123456789), eval.Evaluate("Truncate(3456789.123456789)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestFloor()
		{
			Assert.AreEqual(Math.Floor(12.4), eval.Evaluate("Floor(12.4)").GetValueOrDefault());
			Assert.AreEqual(Math.Floor(12.5), eval.Evaluate("Floor(12.5)").GetValueOrDefault());
			Assert.AreEqual(Math.Floor(12.6), eval.Evaluate("Floor(12.6)").GetValueOrDefault());

			Assert.AreEqual(Math.Floor(-12.4), eval.Evaluate("Floor(-12.4)").GetValueOrDefault());
			Assert.AreEqual(Math.Floor(-12.5), eval.Evaluate("Floor(-12.5)").GetValueOrDefault());
			Assert.AreEqual(Math.Floor(-12.6), eval.Evaluate("Floor(-12.6)").GetValueOrDefault());

			Assert.AreEqual(Math.Floor(-1.1), eval.Evaluate("floor(-1.1)").GetValueOrDefault());
			Assert.AreEqual(Math.Floor(-0.1), eval.Evaluate("FLOOR(-0.1)").GetValueOrDefault());
			Assert.AreEqual(Math.Floor(0.1), eval.Evaluate("floor(0.1)").GetValueOrDefault());
			Assert.AreEqual(Math.Floor(1.1), eval.Evaluate("FLOOR(1.1)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestCeiling()
		{
			Assert.AreEqual(Math.Ceiling(12.4), eval.Evaluate("Ceiling(12.4)").GetValueOrDefault());
			Assert.AreEqual(Math.Ceiling(12.5), eval.Evaluate("Ceiling(12.5)").GetValueOrDefault());
			Assert.AreEqual(Math.Ceiling(12.6), eval.Evaluate("Ceiling(12.6)").GetValueOrDefault());

			Assert.AreEqual(Math.Ceiling(-12.4), eval.Evaluate("Ceiling(-12.4)").GetValueOrDefault());
			Assert.AreEqual(Math.Ceiling(-12.5), eval.Evaluate("Ceiling(-12.5)").GetValueOrDefault());
			Assert.AreEqual(Math.Ceiling(-12.6), eval.Evaluate("Ceiling(-12.6)").GetValueOrDefault());

			Assert.AreEqual(Math.Ceiling(-1.1), eval.Evaluate("ceiling(-1.1)").GetValueOrDefault());
			Assert.AreEqual(Math.Ceiling(-0.1), eval.Evaluate("CEILING(-0.1)").GetValueOrDefault());
			Assert.AreEqual(Math.Ceiling(0.1), eval.Evaluate("ceiling(0.1)").GetValueOrDefault());
			Assert.AreEqual(Math.Ceiling(1.1), eval.Evaluate("CEILING(1.1)").GetValueOrDefault());
		}


		[TestMethod]
		public void TestCos()
		{
			Assert.AreEqual(Math.Cos(Math.PI), eval.Evaluate("cos(pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Cos(2 * Math.PI), eval.Evaluate("Cos(2pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Cos(Math.PI * 2), eval.Evaluate("Cos(pi*2)").GetValueOrDefault());

			Assert.AreEqual(Math.Cos((double)((decimal)Math.PI * (3m / 4m))), eval.Evaluate("Cos(pi*(3/4))").GetValueOrDefault());
		}

		[TestMethod]
		public void TestCosh()
		{
			Assert.AreEqual(Math.Cosh((double)(decimal)Math.PI), eval.Evaluate("cosh(pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Cosh((double)(2m * (decimal)Math.PI)), eval.Evaluate("Cosh(2pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Cosh((double)((decimal)Math.PI * 2m)), eval.Evaluate("Cosh(pi*2)").GetValueOrDefault());

			Assert.AreEqual(Math.Cosh((double)((decimal)Math.PI * (3m / 4m))), eval.Evaluate("Cosh(pi*(3/4))").GetValueOrDefault());
		}

		[TestMethod]
		public void TestSin()
		{
			Assert.AreEqual(Math.Sin((double)(decimal)Math.PI), eval.Evaluate("sin(pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Sin((double)(2m * (decimal)Math.PI)), eval.Evaluate("Sin(2pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Sin((double)((decimal)Math.PI * 2m)), eval.Evaluate("Sin(pi*2)").GetValueOrDefault());

			Assert.AreEqual(Math.Sin((double)((decimal)Math.PI * (3m / 4m))), eval.Evaluate("Sin(pi*(3/4))").GetValueOrDefault());
		}

		[TestMethod]
		public void TestSinh()
		{
			Assert.AreEqual(Math.Sinh((double)(decimal)Math.PI), eval.Evaluate("sinh(pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Sinh((double)(2m * (decimal)Math.PI)), eval.Evaluate("Sinh(2pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Sinh((double)((decimal)Math.PI * 2m)), eval.Evaluate("Sinh(pi*2)").GetValueOrDefault());

			Assert.AreEqual(Math.Sinh((double)((decimal)Math.PI * (3m / 4m))), eval.Evaluate("Sinh(pi*(3/4))").GetValueOrDefault());
		}

		[TestMethod]
		public void TestTan()
		{
			Assert.AreEqual(Math.Tan((double)(decimal)Math.PI), eval.Evaluate("tan(pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Tan((double)(2m * (decimal)Math.PI)), eval.Evaluate("Tan(2pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Tan((double)((decimal)Math.PI * 2m)), eval.Evaluate("Tan(pi*2)").GetValueOrDefault());

			Assert.AreEqual(Math.Tan((double)((decimal)Math.PI * (3m / 4m))), eval.Evaluate("Tan(pi*(3/4))").GetValueOrDefault());
		}

		[TestMethod]
		public void TestTanh()
		{
			Assert.AreEqual(Math.Tanh((double)(decimal)Math.PI), eval.Evaluate("tanh(pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Tanh((double)(2m * (decimal)Math.PI)), eval.Evaluate("Tanh(2pi)").GetValueOrDefault());
			Assert.AreEqual(Math.Tanh((double)((decimal)Math.PI * 2m)), eval.Evaluate("Tanh(pi*2)").GetValueOrDefault());

			Assert.AreEqual(Math.Tanh((double)((decimal)Math.PI * (3m / 4m))), eval.Evaluate("Tanh(pi*(3/4))").GetValueOrDefault());
		}

		[TestMethod]
		public void TestAcos()
		{
			Assert.AreEqual(Double.NaN, Math.Acos((double)(decimal)Math.PI));
			Assert.AreEqual(Double.NaN, eval.Evaluate("acos(pi)").GetValueOrDefault());

			Assert.AreEqual(Math.Acos(0.5), eval.Evaluate("Acos(0.5)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestAsin()
		{
			Assert.AreEqual(Double.NaN, Math.Asin((double)(decimal)Math.PI));
			Assert.AreEqual(Double.NaN, eval.Evaluate("asin(pi)").GetValueOrDefault());

			Assert.AreEqual(Math.Asin(0.5), eval.Evaluate("Asin(0.5)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestAtan()
		{
			Assert.AreEqual(Math.Atan((double)(decimal)Math.PI), eval.Evaluate("atan(pi)").GetValueOrDefault());

			Assert.AreEqual(Math.Atan(0.5), eval.Evaluate("Atan(0.5)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestAtanh()
		{
			Assert.AreEqual(Double.NaN, Math.Atanh((double)(decimal)Math.PI));
			Assert.AreEqual(Double.NaN, eval.Evaluate("Atanh(pi)").GetValueOrDefault());

			Assert.AreEqual(Math.Atanh(0.5), eval.Evaluate("Atanh(0.5)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestAtan2()
		{
			Assert.ThrowsException<ParseException>(() => eval.Evaluate("atan2(5)"));
			Assert.ThrowsException<ParseException>(() => eval.Evaluate("Atan2(0.5)"));

			Assert.AreEqual(Math.Atan2((double)(decimal)Math.PI, 99), eval.Evaluate("atan2(pi, 99)").GetValueOrDefault());

			Assert.AreEqual(Math.Atan2(0.5, 4), eval.Evaluate("Atan2(0.5, 4)").GetValueOrDefault());
		}


		[TestMethod]
		public void TestExp()
		{
			Assert.AreEqual(Math.Exp(12), eval.Evaluate("exp(12)").GetValueOrDefault());
			Assert.AreEqual(Math.Exp(-12), eval.Evaluate("Exp(-12)").GetValueOrDefault());
		}

		[TestMethod]
		public void TestLog()
		{
			Assert.AreEqual(Math.Log(12), eval.Evaluate("Log(12)").GetValueOrDefault());
			Assert.AreEqual(Math.Log(-12), eval.Evaluate("Log(-12)").GetValueOrDefault());

			Assert.AreEqual(12, Math.Log(Math.Exp(12)));
			Assert.AreEqual(12, eval.Evaluate("Log(exp(12))").GetValueOrDefault());

			Assert.AreEqual(-3, Math.Log(Math.Exp(-3)));
			Assert.AreEqual(-3, eval.Evaluate("Log(Exp(-3))").GetValueOrDefault());
		}

		[TestMethod]
		public void TestLog10()
		{
			Assert.AreEqual(Math.Log10(12), eval.Evaluate("Log10(12)").GetValueOrDefault());
			Assert.AreEqual(Math.Log10(-12), eval.Evaluate("Log10(-12)").GetValueOrDefault());

			Assert.AreEqual(12, Math.Log10(Math.Pow(10, 12)));
			Assert.AreEqual(12, eval.Evaluate("Log10(pow(10, 12))").GetValueOrDefault());

			Assert.AreEqual(-3, Math.Log10(Math.Pow(10, -3)));
			Assert.AreEqual(-3, eval.Evaluate("Log10(Pow(10, -3))").GetValueOrDefault());
		}

		[TestMethod]
		public void TestLog2()
		{
			Assert.AreEqual(Math.Log2(12), eval.Evaluate("Log2(12)").GetValueOrDefault());
			Assert.AreEqual(Math.Log2(-12), eval.Evaluate("Log2(-12)").GetValueOrDefault());

			Assert.AreEqual(12, Math.Log2(Math.Pow(2, 12)));
			Assert.AreEqual(12, eval.Evaluate("Log2(pow(2, 12))").GetValueOrDefault());

			Assert.AreEqual(-3, Math.Log2(Math.Pow(2, -3)));
			Assert.AreEqual(-3, eval.Evaluate("Log2(Pow(2, -3))").GetValueOrDefault());
		}
	}
}
