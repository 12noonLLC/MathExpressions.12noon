using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LoreSoft.MathExpressions.UnitTests
{
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
			ConvertExpression e = new ConvertExpression("[in->ft]");
			Assert.IsNotNull(e);

			double feet = e.Convert(new double[] { 12d });
			Assert.AreEqual(1, feet);
		}
	}
}
