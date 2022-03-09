using MathExpressions.Metadata;
using MathExpressions.Properties;
using MathExpressions.UnitConversion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MathExpressions;

/// <summary>
/// A class representing unit conversion expressions.
/// </summary>
public class ConvertExpression : IExpression
{
	private static readonly Dictionary<string, ConversionMap> _conversionCache;

	private readonly ConversionMap _current;
	private readonly string _expression;

	/// <summary>The format of a conversion expression.</summary>
	public const string KeyExpressionFormat2 = "[{0}->{1}]";

	/// <summary>Initializes a new instance of the <see cref="ConvertExpression"/> class.</summary>
	/// <param name="expression">The conversion expression for this instance.</param>
	public ConvertExpression(string expression)
	{
		if (!_conversionCache.ContainsKey(expression))
		{
			throw new ArgumentException(String.Format(Resources.InvalidConversionExpression1, expression), nameof(expression));
		}

		this._expression = expression;
		_current = _conversionCache[expression];
	}

	/// <summary>Gets the number of arguments this expression uses.</summary>
	/// <value>The argument count.</value>
	public int ArgumentCount => 1;

	/// <summary>Convert the numbers to the new unit.</summary>
	/// <param name="numbers">The numbers used in the conversion.</param>
	/// <returns>The result of the conversion execution.</returns>
	/// <exception cref="ArgumentNullException">When numbers is null.</exception>
	/// <exception cref="ArgumentException">When the length of numbers do not equal <see cref="ArgumentCount"/>.</exception>
	public PreciseNumber Evaluate(PreciseNumber[] operands)
	{
		((IExpression)this).Validate(operands);

		PreciseNumber fromValue = operands[0];
		if (!fromValue.HasValue)
		{
			return fromValue;
		}

		return _current.UnitType switch
		{
			UnitType.Length		=> new PreciseNumber(LengthConverter.Convert((LengthUnit)_current.FromUnit, (LengthUnit)_current.ToUnit, fromValue.Value)),
			UnitType.Mass			=> new PreciseNumber(MassConverter.Convert((MassUnit)_current.FromUnit, (MassUnit)_current.ToUnit, fromValue.Value)),
			UnitType.Speed			=> new PreciseNumber(SpeedConverter.Convert((SpeedUnit)_current.FromUnit, (SpeedUnit)_current.ToUnit, fromValue.Value)),
			UnitType.Temperature	=> new PreciseNumber(TemperatureConverter.Convert((TemperatureUnit)_current.FromUnit, (TemperatureUnit)_current.ToUnit, fromValue.Value)),
			UnitType.Time			=> new PreciseNumber(TimeConverter.Convert((TimeUnit)_current.FromUnit, (TimeUnit)_current.ToUnit, fromValue.Value)),
			UnitType.Volume		=> new PreciseNumber(VolumeConverter.Convert((VolumeUnit)_current.FromUnit, (VolumeUnit)_current.ToUnit, fromValue.Value)),
			_							=> throw new ArgumentOutOfRangeException(nameof(operands)),
		};
	}


	///<summary>
	/// Determines whether the specified expression name is for unit conversion.
	///</summary>
	///<param name="expression">The expression to check.</param>
	///<returns><c>true</c> if the specified expression is a unit conversion; otherwise, <c>false</c>.</returns>
	public static bool IsConvertExpression(string expression)
	{
		//do basic checks before creating cache
		if (String.IsNullOrEmpty(expression))
		{
			return false;
		}

		if (expression[0] != '[')
		{
			return false;
		}

		return _conversionCache.ContainsKey(expression);
	}


	static ConvertExpression()
	{
		_conversionCache = new Dictionary<string, ConversionMap>(StringComparer.OrdinalIgnoreCase);

		AddToCache<LengthUnit>(UnitType.Length);
		AddToCache<MassUnit>(UnitType.Mass);
		AddToCache<SpeedUnit>(UnitType.Speed);
		AddToCache<TemperatureUnit>(UnitType.Temperature);
		AddToCache<TimeUnit>(UnitType.Time);
		AddToCache<VolumeUnit>(UnitType.Volume);
	}

	private static void AddToCache<T>(UnitType unitType)
		 where T : struct, IComparable, IFormattable, IConvertible
	{
		Type enumType = typeof(T);
		int[] a = (int[])Enum.GetValues(enumType);

		foreach (int x in Enumerable.Range(0, a.Length))
		{
			MemberInfo parentInfo = GetMemberInfo(enumType, Enum.GetName(enumType, x));
			string parentKey = AttributeReader.GetAbbreviation(parentInfo);

			foreach (int i in Enumerable.Range(0, a.Length))
			{
				if (x == i)
				{
					continue;
				}

				MemberInfo info = GetMemberInfo(enumType, Enum.GetName(enumType, i));

				string key = String.Format(
					 CultureInfo.InvariantCulture,
					 KeyExpressionFormat2,
					 parentKey,
					 AttributeReader.GetAbbreviation(info));

				_conversionCache.Add(key, new ConversionMap(unitType, x, i));
			}
		}
	}

	private static MemberInfo GetMemberInfo(Type type, string? name)
	{
		if (name is null)
		{
			throw new ArgumentNullException(nameof(name));
		}

		MemberInfo[] info = type.GetMember(name) ?? throw new ArgumentException("Unable to load member.", nameof(name));
		return info[0];
	}

	/// <summary>
	/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
	/// </returns>
	/// <filterPriority>2</filterPriority>
	public override string ToString() => _expression;


	private class ConversionMap
	{
		public ConversionMap(UnitType unitType, int fromUnit, int toUnit)
		{
			UnitType = unitType;
			FromUnit = fromUnit;
			ToUnit = toUnit;
		}

		public UnitType UnitType { get; }
		public int FromUnit { get; }
		public int ToUnit { get; }

		public override string ToString() => $"{UnitType}, [{FromUnit}->{ToUnit}]";
	}
}
