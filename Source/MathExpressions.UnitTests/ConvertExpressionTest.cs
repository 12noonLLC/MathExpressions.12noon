using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathExpressions.UnitTests;

[TestClass]
public class ConvertExpressionTest
{
	//public TestContext TestContext { get; set; }

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
	public void ConvertSyntax()
	{
		MathEvaluator eval = new();
		Assert.ThrowsException<ParseException>(() => eval.Evaluate("6789[ft<-mi]"));
		Assert.ThrowsException<ParseException>(() => eval.Evaluate("6789[ft-mi]"));
	}

	[TestMethod]
	public void IsConvertExpression()
	{
		bool result = ConvertExpression.IsConvertExpression("blah");
		Assert.IsFalse(result);

		result = ConvertExpression.IsConvertExpression("[m->ft]");
		Assert.IsTrue(result);

		result = ConvertExpression.IsConvertExpression("[ms->ft]");
		Assert.IsFalse(result);
	}

	[TestMethod]
	public void Convert()
	{
		ConvertExpression e = new("[in->ft]");
		Assert.IsNotNull(e);

		PreciseNumber feet = e.Evaluate(new PreciseNumber[] { new PreciseNumber(12m) });
		Assert.IsTrue(feet.HasValue);
		Assert.AreEqual(1, feet.Value);
	}

	[TestMethod]
	public void ConvertRounding()
	{
		ConvertExpression e = new("[in->ft]");
		Assert.IsNotNull(e);

		PreciseNumber feet = e.Evaluate(new PreciseNumber[] { new PreciseNumber(120m) });
		Assert.IsTrue(feet.HasValue);
		Assert.AreEqual(10, feet.Value);
	}
}
