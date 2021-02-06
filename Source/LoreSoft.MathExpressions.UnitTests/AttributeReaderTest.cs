using LoreSoft.MathExpressions.Metadata;
using LoreSoft.MathExpressions.UnitConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoreSoft.MathExpressions.UnitTests.Metadata
{
	[TestClass]
	public class AttributeReaderTest
	{
		[TestMethod]
      public void GetDescription()
      {
         string result = AttributeReader.GetDescription<SpeedUnit>(SpeedUnit.MilePerHour);
         Assert.AreEqual("Mile/Hour", result);

         result = AttributeReader.GetDescription<SpeedUnit>(SpeedUnit.Knot);
         Assert.AreEqual("Knot", result);
      }

      [TestMethod]
      public void GetAbbreviation()
      {
         string result = AttributeReader.GetAbbreviation<SpeedUnit>(SpeedUnit.MilePerHour);
         Assert.AreEqual("mph", result);

         result = AttributeReader.GetAbbreviation<SpeedUnit>(SpeedUnit.Knot);
         Assert.AreEqual("knot", result);
      }
   }
}
