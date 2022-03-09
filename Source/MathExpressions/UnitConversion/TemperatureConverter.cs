using MathExpressions.Metadata;
using System;

namespace MathExpressions.UnitConversion;

/// <summary>Units for Temperature</summary>
public enum TemperatureUnit
{
	/// <summary>Degrees Celsius unit (c)</summary>
	[Abbreviation("c")]
	Celsius = 0,
	/// <summary>Degrees Fahrenheit unit (f)</summary>
	[Abbreviation("f")]
	Fahrenheit = 1,
	/// <summary>Degrees Kelvin unit (k)</summary>
	[Abbreviation("k")]
	Kelvin = 2,
}

/// <summary>
/// Class representing temperature conversion.
/// </summary>
public static class TemperatureConverter
{
	/// <summary>
	/// Converts the specified from unit to the specified unit.
	/// </summary>
	/// <param name="fromUnit">Convert from unit.</param>
	/// <param name="toUnit">Convert to unit.</param>
	/// <param name="fromValue">Convert from value.</param>
	/// <returns>The converted value.</returns>
	public static decimal Convert(
		 TemperatureUnit fromUnit,
		 TemperatureUnit toUnit,
		 decimal fromValue)
	{
		if (fromUnit == toUnit)
		{
			return fromValue;
		}

		if (fromUnit == TemperatureUnit.Celsius)
		{
			if (toUnit == TemperatureUnit.Kelvin)
			{
				return fromValue + 273.15m;
			}
			else if (toUnit == TemperatureUnit.Fahrenheit)
			{
				// F = (C * 9/5) + 32
				return (fromValue * 9m / 5m) + 32m;
			}
		}
		else if (fromUnit == TemperatureUnit.Kelvin)
		{
			if (toUnit == TemperatureUnit.Celsius)
			{
				return fromValue - 273.15m;
			}
			else if (toUnit == TemperatureUnit.Fahrenheit)
			{
				return ((fromValue - 273.15m) + 32m) * 5m / 9m;
			}
		}
		else if (fromUnit == TemperatureUnit.Fahrenheit)
		{
			if (toUnit == TemperatureUnit.Celsius)
			{
				// C = (F - 32) * 5/9
				return (fromValue - 32m) * 5m / 9m;
			}
			else if (toUnit == TemperatureUnit.Kelvin)
			{
				return ((fromValue - 32m) * 5m / 9m) + 273.15m;
			}
		}
		return Decimal.Zero;
	}
}
