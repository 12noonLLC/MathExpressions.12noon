using MathExpressions.Metadata;

namespace MathExpressions.UnitConversion;

/// <summary>Units for Mass</summary>
public enum MassUnit
{
	/// <summary>Milligram unit (mg)</summary>
	[Abbreviation("mg")]
	Milligram = 0,
	/// <summary>Gram unit (g)</summary>
	[Abbreviation("g")]
	Gram = 1,
	/// <summary>Kilogram unit (kg)</summary>
	[Abbreviation("kg")]
	Kilogram = 2,
	/// <summary>Ounce unit (oz)</summary>
	[Abbreviation("oz")]
	Ounce = 3,
	/// <summary>Pound unit (lb)</summary>
	[Abbreviation("lb")]
	Pound = 4,
	/// <summary>Ton unit (ton)</summary>
	[Abbreviation("ton")]
	Ton = 5,
}

/// <summary>
/// Class representing mass conversion.
/// </summary>
public static class MassConverter
{
	// In enum order
	private static readonly decimal[] factors = new[]
	{
		0.000001m,					//milligram
		0.001m,						//gram
		1m,							//kilogram
		0.45359237m / 16m,		//ounce
		0.45359237m,				//pound
		0.45359237m * 2_000m,	//ton [short, US]      
	};

	/// <summary>
	/// Converts the specified from unit to the specified unit.
	/// </summary>
	/// <param name="fromUnit">Convert from unit.</param>
	/// <param name="toUnit">Convert to unit.</param>
	/// <param name="fromValue">Convert from value.</param>
	/// <returns>The converted value.</returns>
	public static decimal Convert(
		 MassUnit fromUnit,
		 MassUnit toUnit,
		 decimal fromValue)
	{
		if (fromUnit == toUnit)
		{
			return fromValue;
		}

		decimal fromFactor = factors[(int)fromUnit];
		decimal toFactor = factors[(int)toUnit];
		decimal result = fromFactor * fromValue / toFactor;
		return result;
	}
}
