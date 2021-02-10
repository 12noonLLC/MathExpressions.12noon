using MathExpressions.UnitConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathExpressions.UnitTests
{
	[TestClass]
	public class UnitConversion
	{
      [TestMethod, TestCategory("LengthConverter")]
      public void LengthConverter_Convert()
      {
         double result = LengthConverter.Convert(LengthUnit.Feet, LengthUnit.Inch, 1);
         Assert.AreEqual(12, result);

         result = LengthConverter.Convert(LengthUnit.Inch, LengthUnit.Feet, 12);
         Assert.AreEqual(1, result);

         result = LengthConverter.Convert(LengthUnit.Mile, LengthUnit.Kilometer, 10);
         Assert.AreEqual(16.09344, result);

         result = LengthConverter.Convert(LengthUnit.Meter, LengthUnit.Feet, 10);
         Assert.AreEqual(32.808398950131235, result);
      }


      [TestMethod, TestCategory("MassConverter")]
      public void MassConverter_Convert()
      {
         double result = MassConverter.Convert(MassUnit.Ounce, MassUnit.Pound, 12);
         Assert.AreEqual(0.75, result);

         result = MassConverter.Convert(MassUnit.Pound, MassUnit.Ounce, 0.75);
         Assert.AreEqual(12, result);

         result = MassConverter.Convert(MassUnit.Kilogram, MassUnit.Pound, 1);
         Assert.AreEqual(2.2046226218487757d, result);

         result = MassConverter.Convert(MassUnit.Ton, MassUnit.Pound, 1);
         Assert.AreEqual(2000d, result);
      }


      [TestMethod, TestCategory("SpeedConverter")]
      public void Example()
      {
         double result = SpeedConverter.Convert(SpeedUnit.MeterPerSecond, SpeedUnit.KilometerPerHour, 60);
         Assert.AreEqual(216d, result);

         result = SpeedConverter.Convert(SpeedUnit.MilePerHour, SpeedUnit.KilometerPerHour, 60);
         Assert.AreEqual(96.560639999999992d, result);
      }


      [TestMethod, TestCategory("TemperatureConverter")]
      public void TemperatureConverter_Convert()
      {
         double result = TemperatureConverter.Convert(TemperatureUnit.Celsius, TemperatureUnit.Fahrenheit, -40d);
         Assert.AreEqual(-40, result);

         result = TemperatureConverter.Convert(TemperatureUnit.Celsius, TemperatureUnit.Fahrenheit, 0);
         Assert.AreEqual(32, result);

         result = TemperatureConverter.Convert(TemperatureUnit.Fahrenheit, TemperatureUnit.Celsius, 212);
         Assert.AreEqual(100, result);

         result = TemperatureConverter.Convert(TemperatureUnit.Fahrenheit, TemperatureUnit.Kelvin, 212);
         Assert.AreEqual(373.15, result);
      }


      [TestMethod, TestCategory("TimeConverter")]
      public void TimeConverter_Convert()
      {
         double result = TimeConverter.Convert(TimeUnit.Hour, TimeUnit.Day, 24);
         Assert.AreEqual(1, result);

         result = TimeConverter.Convert(TimeUnit.Minute, TimeUnit.Hour, 60);
         Assert.AreEqual(1, result);

         result = TimeConverter.Convert(TimeUnit.Day, TimeUnit.Week, 7);
         Assert.AreEqual(1, result);
      }


      [TestMethod, TestCategory("VolumeConverter")]
      public void VolumeConverter_Convert()
      {
         double result = VolumeConverter.Convert(VolumeUnit.Gallon, VolumeUnit.Quart, 1);
         Assert.AreEqual(4, result);
      }
   }
}
