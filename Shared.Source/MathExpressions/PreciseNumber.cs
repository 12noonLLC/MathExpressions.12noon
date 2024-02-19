using System;
using System.Diagnostics;

namespace MathExpressions;

[DebuggerDisplay("{AsDouble,nq}")]
public class PreciseNumber
{
	public bool HasValue => Double.IsFinite(AsDouble);
	public decimal Value { get; private set; }
	public double AsDouble { get; private set; }


	public PreciseNumber(decimal newValue)
	{
		Value = newValue;
		AsDouble = (double)newValue;
	}

	public PreciseNumber(double newValue)
	{
		Value = Double.IsFinite(newValue) ? (decimal)newValue : Decimal.Zero;
		AsDouble = newValue;
	}
}
