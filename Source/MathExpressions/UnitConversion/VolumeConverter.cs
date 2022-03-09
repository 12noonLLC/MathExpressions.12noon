using MathExpressions.Metadata;
using System.ComponentModel;

namespace MathExpressions.UnitConversion;

/// <summary>Units for Liquid Volume</summary>
public enum VolumeUnit
{
	/// <summary>Milliliter unit (ml)</summary>
	[Abbreviation("ml")]
	Milliliter = 0,
	/// <summary>Liter unit (l)</summary>
	[Abbreviation("l")]
	Liter = 1,
	/// <summary>Kiloliter unit (kl)</summary>
	[Abbreviation("kl")]
	Kiloliter = 2,
	/// <summary>Fluid ounce unit (oz)</summary>
	[Abbreviation("oz")]
	[Description("Fluid Ounce")]
	FluidOunce = 3,
	/// <summary>Cup unit (cup)</summary>
	[Abbreviation("cup")]
	Cup = 4,
	/// <summary>Pint unit (pt)</summary>
	[Abbreviation("pt")]
	Pint = 5,
	/// <summary>Quart unit (qt)</summary>
	[Abbreviation("qt")]
	Quart = 6,
	/// <summary>Gallon unit (gal)</summary>
	[Abbreviation("gal")]
	Gallon = 7,
}

/// <summary>
/// Class representing liquid volume conversion.
/// </summary>
public static class VolumeConverter
{
	// In enum order
	private static readonly decimal[] factors = new[]
	{
		0.000001m,					//milliliter
		0.001m,						//liter
		1m,							//kiloliter
		0.0037854118m / 128m,	//ounce [US, liquid]
		0.0037854118m / 16m,		//cup [US]
		0.0037854118m / 8m,		//pint [US, liquid]
		0.0037854118m / 4m,		//quart [US, liquid]
		0.0037854118m,				//gallon [US, liquid]
	};

	/// <summary>
	/// Converts the specified from unit to the specified unit.
	/// </summary>
	/// <param name="fromUnit">Convert from unit.</param>
	/// <param name="toUnit">Convert to unit.</param>
	/// <param name="fromValue">Convert from value.</param>
	/// <returns>The converted value.</returns>
	public static decimal Convert(
		 VolumeUnit fromUnit,
		 VolumeUnit toUnit,
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
