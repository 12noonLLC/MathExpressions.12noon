using MathExpressions.Metadata;
using System;

namespace MathExpressions.UnitConversion;

/// <summary>Units for Time</summary>
public enum TimeUnit
{
	/// <summary>Millisecond unit (ms)</summary>
	[Abbreviation("ms")]
	Millisecond = 0,
	/// <summary>Second unit (sec)</summary>
	[Abbreviation("sec")]
	Second = 1,
	/// <summary>Minute unit (min)</summary>
	[Abbreviation("min")]
	Minute = 2,
	/// <summary>Hour unit (hr)</summary>
	[Abbreviation("hr")]
	Hour = 3,
	/// <summary>Day unit (d)</summary>
	[Abbreviation("d")]
	Day = 4,
	/// <summary>Week unit (wk)</summary>
	[Abbreviation("wk")]
	Week = 5,
}

/// <summary>
/// Class representing time conversion.
/// </summary>
public static class TimeConverter
{

	/// <summary>
	/// Converts the specified from unit to the specified unit.
	/// </summary>
	/// <param name="fromUnit">Convert from unit.</param>
	/// <param name="toUnit">Convert to unit.</param>
	/// <param name="fromValue">Convert from value.</param>
	/// <returns>The converted value.</returns>
	public static decimal Convert(
		 TimeUnit fromUnit,
		 TimeUnit toUnit,
		 decimal fromValue)
	{
		if (fromUnit == toUnit)
		{
			return fromValue;
		}

		double doubleFromValue = (double)fromValue;
		var span = fromUnit switch
		{
			TimeUnit.Millisecond	=> TimeSpan.FromMilliseconds(doubleFromValue),
			TimeUnit.Second		=> TimeSpan.FromSeconds(doubleFromValue),
			TimeUnit.Minute		=> TimeSpan.FromMinutes(doubleFromValue),
			TimeUnit.Hour			=> TimeSpan.FromHours(doubleFromValue),
			TimeUnit.Day			=> TimeSpan.FromDays(doubleFromValue),
			TimeUnit.Week			=> TimeSpan.FromDays(doubleFromValue * 7),
			_ => throw new ArgumentOutOfRangeException(nameof(fromUnit)),
		};
		return (decimal)(toUnit switch
		{
			TimeUnit.Millisecond	=> span.TotalMilliseconds,
			TimeUnit.Second		=> span.TotalSeconds,
			TimeUnit.Minute		=> span.TotalMinutes,
			TimeUnit.Hour			=> span.TotalHours,
			TimeUnit.Day			=> span.TotalDays,
			TimeUnit.Week			=> span.TotalDays / 7d,
			_ => throw new ArgumentOutOfRangeException(nameof(toUnit)),
		});
	}
}
