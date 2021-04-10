using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CalculateX.UnitTests
{
	[TestClass]
	public class FormatTest
	{
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
		public void TestClear()
		{
			Assert.AreEqual("0", Numbers.FormatNumberWithCommas(0));
			Assert.AreEqual("0", Numbers.FormatNumberWithCommas(0.0));

			Assert.AreEqual("4", Numbers.FormatNumberWithCommas(4));
			Assert.AreEqual("4", Numbers.FormatNumberWithCommas(4.0));
			Assert.AreEqual("4.53", Numbers.FormatNumberWithCommas(4.53));
			Assert.AreEqual("104.53", Numbers.FormatNumberWithCommas(104.53));
			Assert.AreEqual("2,134", Numbers.FormatNumberWithCommas(2134));
			Assert.AreEqual("2,345.53", Numbers.FormatNumberWithCommas(2345.53));
			Assert.AreEqual("2,345.123456789", Numbers.FormatNumberWithCommas(2345.123456789));

			Assert.AreEqual("0.1234", Numbers.FormatNumberWithCommas(0.1234));
			Assert.AreEqual("0.123456789", Numbers.FormatNumberWithCommas(0.123456789));
			Assert.AreEqual("0.989669363564753", Numbers.FormatNumberWithCommas(0.989669363564753));
			Assert.AreEqual("5.989669363564753", Numbers.FormatNumberWithCommas(5.989669363564753));
			Assert.AreEqual("3,456,789.123456789", Numbers.FormatNumberWithCommas(3456789.123456789));

			Assert.AreEqual("989,669,363,564,753", Numbers.FormatNumberWithCommas(989669363564753));
		}
	}
}
