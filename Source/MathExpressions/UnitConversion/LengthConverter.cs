using MathExpressions.Metadata;

namespace MathExpressions.UnitConversion
{
	/// <summary>Units for Length</summary>
	public enum LengthUnit
	{
		/// <summary>Millimeter unit (mm)</summary>
		[Abbreviation("mm")]
		Millimeter = 0,
		/// <summary>Centimeter unit (cm)</summary>
		[Abbreviation("cm")]
		Centimeter = 1,
		/// <summary>Meter unit (m)</summary>
		[Abbreviation("m")]
		Meter = 2,
		/// <summary>Kilometer unit (km)</summary>
		[Abbreviation("km")]
		Kilometer = 3,
		/// <summary>Inch unit (in)</summary>
		[Abbreviation("in")]
		Inch = 4,
		/// <summary>Feet unit (ft)</summary>
		[Abbreviation("ft")]
		Feet = 5,
		/// <summary>Yard unit (yd)</summary>
		[Abbreviation("yd")]
		Yard = 6,
		/// <summary>Mile unit (mile)</summary>
		[Abbreviation("mile")]
		Mile = 7,
	}

	/// <summary>
	/// Class representing length conversion.
	/// </summary>
	public static class LengthConverter
	{
		// In enum order
		private static readonly decimal[] Factors = new[]
		{
			0.001m,				//millimeter
			0.01m,				//centimeter
			1m,					//meter
			1_000m,				//kilometer
			0.3048m / 12m,		//inch
			0.3048m,				//feet
			0.9144m,				//yard
			0.3048m * 5_280m,	//mile
		};


		/// <summary>
		/// Converts the specified from unit to the specified unit.
		/// </summary>
		/// <param name="fromUnit">Convert from unit.</param>
		/// <param name="toUnit">Convert to unit.</param>
		/// <param name="fromValue">Convert from value.</param>
		/// <returns>The converted value.</returns>
		public static decimal Convert(
			 LengthUnit fromUnit,
			 LengthUnit toUnit,
			 decimal fromValue)
		{
			if (fromUnit == toUnit)
			{
				return fromValue;
			}

			decimal fromFactor = Factors[(int)fromUnit];
			decimal toFactor = Factors[(int)toUnit];
			decimal result = fromFactor * fromValue / toFactor;
			return result;
		}
	}
}
