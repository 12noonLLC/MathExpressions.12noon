using System;
using System.Globalization;

namespace Shared
{
	internal static class Numbers
	{
		/// <summary>
		/// Return the passed number with grouping separators.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		internal static string FormatNumberWithGroupingSeparators(double d)
		{
			// Format the integer portion with grouping separators.
			double m = Math.Truncate(d);
			string first = $"{m:N0}";

			// Format the decimal portion and remove the leading zero.
			// If there is no decimal separator, clear the decimal portion.
			string second = d.ToString(CultureInfo.CurrentCulture);
			string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			int ixSeparator = second.IndexOf(separator);
			if (ixSeparator == -1)
			{
				return first;
			}

			second = second[ixSeparator..];
			return first + second;
		}
	}
}
