using MathExpressions.Metadata;
using System.ComponentModel;

namespace MathExpressions.UnitConversion
{
	/// <summary>Units for Speed</summary>
	public enum SpeedUnit
	{
		/// <summary>Meter/Second unit (m/s)</summary>
		[Abbreviation("m/s")]
		[Description("Meter/Second")]
		MeterPerSecond = 0,
		/// <summary>Kilometer/Hour unit (kph)</summary>
		[Abbreviation("kph")]
		[Description("Kilometer/Hour")]
		KilometerPerHour = 1,
		/// <summary>Foot/Second unit (ft/s)</summary>
		[Abbreviation("ft/s")]
		[Description("Foot/Second")]
		FootPerSecond = 2,
		/// <summary>Mile/Hour unit (mph)</summary>
		[Abbreviation("mph")]
		[Description("Mile/Hour")]
		MilePerHour = 3,
		/// <summary>Knot unit (knot)</summary>
		[Abbreviation("knot")]
		Knot = 4,
		/// <summary>Mach unit (mach)</summary>
		[Abbreviation("mach")]
		Mach = 5,
	}

	/// <summary>
	/// Class representing speed conversion.
	/// </summary>
	public static class SpeedConverter
	{
		/// <summary>
		/// For precision, we need to keep the numerator and denominator
		/// separate, so that we can multiple first and then divide.
		/// </summary>
		// In enum order
		private static readonly decimal[] factorNumerators = new[]
		{
			1m,									//meter/second
			1_000m,								//kilometer/hour
			0.3048m,								//foot/second
			(0.3048m * 5_280m),				//mile/hour (mph)
			1_852m,								//knot
			340.29m,								//mach
		};
		private static readonly decimal[] factorDenominators = new[]
		{
			1m,									//meter/second
			3_600m,								//kilometer/hour
			1m,									//foot/second
			3_600m,								//mile/hour (mph)
			3_600m,								//knot
			1m,								//mach
		};

		/// <summary>
		/// Converts the specified from unit to the specified unit.
		/// </summary>
		/// <param name="fromUnit">Convert from unit.</param>
		/// <param name="toUnit">Convert to unit.</param>
		/// <param name="fromValue">Convert from value.</param>
		/// <returns>The converted value.</returns>
		public static decimal Convert(
			 SpeedUnit fromUnit,
			 SpeedUnit toUnit,
			 decimal fromValue)
		{
			if (fromUnit == toUnit)
			{
				return fromValue;
			}

			decimal fromFactorNumerator = factorNumerators[(int)fromUnit];
			decimal fromFactorDenominator = factorDenominators[(int)fromUnit];
			decimal toFactorNumerator = factorNumerators[(int)toUnit];
			decimal toFactorDenominator = factorDenominators[(int)toUnit];
			//decimal result = fromFactor * fromValue / toFactor;
			decimal result = fromFactorNumerator * fromValue * toFactorDenominator / fromFactorDenominator / toFactorNumerator;
			return result;
		}
	}
}
