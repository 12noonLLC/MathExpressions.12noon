using MathExpressions.UnitConversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathExpressions.UnitTests;

[TestClass]
public class UnitConversion
{
	[TestMethod, TestCategory("LengthConverter")]
	public void LengthConverter_Convert()
	{
		decimal result = LengthConverter.Convert(LengthUnit.Feet, LengthUnit.Inch, 1);
		Assert.AreEqual(12m, result);

		result = LengthConverter.Convert(LengthUnit.Inch, LengthUnit.Feet, 12);
		Assert.AreEqual(decimal.One, result);

		result = LengthConverter.Convert(LengthUnit.Mile, LengthUnit.Kilometer, 10);
		Assert.AreEqual(16.09344m, result);

		result = LengthConverter.Convert(LengthUnit.Meter, LengthUnit.Feet, 10);
		//Assert.AreEqual(32.808398950131235d, result);
		Assert.AreEqual(32.808398950131233595800524934m, result);
	}


	[TestMethod, TestCategory("MassConverter")]
	public void MassConverter_Convert()
	{
		decimal result = MassConverter.Convert(MassUnit.Ounce, MassUnit.Pound, 12);
		Assert.AreEqual(0.75m, result);

		result = MassConverter.Convert(MassUnit.Pound, MassUnit.Ounce, 0.75m);
		Assert.AreEqual(12m, result);

		result = MassConverter.Convert(MassUnit.Kilogram, MassUnit.Pound, 1);
		Assert.AreEqual(2.2046226218487758072297380135m, result);

		result = MassConverter.Convert(MassUnit.Ton, MassUnit.Pound, 1);
		Assert.AreEqual(2_000m, result);
	}


	[TestMethod, TestCategory("SpeedConverter")]
	public void SpeedConverter_Convert()
	{
		decimal result = SpeedConverter.Convert(SpeedUnit.MeterPerSecond, SpeedUnit.KilometerPerHour, 60);
		Assert.AreEqual(216m, result);

		result = SpeedConverter.Convert(SpeedUnit.MilePerHour, SpeedUnit.KilometerPerHour, 60);
		// 96.560639999999992d
		Assert.AreEqual(96.56064m, result);
	}


	[TestMethod, TestCategory("SpeedConverter")]
	public void SpeedConverter_Evaluate()
	{
		MathEvaluator eval = new();

		// Note: When compared to the result of Evaluate(), they must be cast to Double and then to Decimal for the correct precision.
		PreciseNumber result = new ConvertExpression("[kph->m/s]").Evaluate(new PreciseNumber[] { new PreciseNumber(100m) });
		Assert.IsTrue(result.HasValue);
		Assert.AreEqual(100m * 1_000m / (60m * 60m), result.Value);
		Assert.AreEqual((double)(100m * 1_000m / (60m * 60m)), eval.Evaluate("100[kph->m/s]"));

		result = new ConvertExpression("[mph->ft/s]").Evaluate(new PreciseNumber[] { new PreciseNumber(100m) });
		Assert.IsTrue(result.HasValue);
		Assert.AreEqual(100m * 5_280m / (60m * 60m), result.Value);
		Assert.AreEqual(100d * 5_280d / (60d * 60d), eval.Evaluate("100[mph->ft/s]"));
		Assert.AreEqual((double)(100m * 5_280m / (60m * 60m)), eval.Evaluate("100[mph->ft/s]"));

		result = new ConvertExpression("[mph->kph]").Evaluate(new PreciseNumber[] { new PreciseNumber(50m) });
		Assert.IsTrue(result.HasValue);
		Assert.AreEqual(50m * (0.3048m * 5_280m) / 1_000m, result.Value);
		Assert.AreEqual((double)(50m * (0.3048m * 5_280m) / 1_000m), eval.Evaluate("50[mph->kph]"));
	}


	[TestMethod, TestCategory("TemperatureConverter")]
	public void TemperatureConverter_Convert()
	{
		decimal result = TemperatureConverter.Convert(TemperatureUnit.Celsius, TemperatureUnit.Fahrenheit, -40);
		Assert.AreEqual(-40m, result);

		result = TemperatureConverter.Convert(TemperatureUnit.Celsius, TemperatureUnit.Fahrenheit, 0);
		Assert.AreEqual(32m, result);

		result = TemperatureConverter.Convert(TemperatureUnit.Fahrenheit, TemperatureUnit.Celsius, 212);
		Assert.AreEqual(100m, result);

		result = TemperatureConverter.Convert(TemperatureUnit.Fahrenheit, TemperatureUnit.Kelvin, 212);
		Assert.AreEqual(373.15m, result);
	}


	[TestMethod, TestCategory("TimeConverter")]
	public void TimeConverter_Convert()
	{
		decimal result = TimeConverter.Convert(TimeUnit.Hour, TimeUnit.Day, 24);
		Assert.AreEqual(1, result);

		result = TimeConverter.Convert(TimeUnit.Minute, TimeUnit.Hour, 60);
		Assert.AreEqual(1, result);

		result = TimeConverter.Convert(TimeUnit.Day, TimeUnit.Week, 7);
		Assert.AreEqual(1, result);
	}


	[TestMethod, TestCategory("VolumeConverter")]
	public void VolumeConverter_Convert()
	{
		decimal result = VolumeConverter.Convert(VolumeUnit.Gallon, VolumeUnit.Quart, 1);
		Assert.AreEqual(4, result);
	}
}
