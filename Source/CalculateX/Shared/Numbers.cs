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


		/// <summary>
		/// Remove currency symbol and grouping separators from passed string.
		/// If it is not parseable as a number, it returns the original string.
		/// </summary>
		/// <param name="pasteText">String to remove currency symbol and grouping separators if possible</param>
		/// <returns>Passed string without currency symbol and grouping separators</returns>
		internal static string RemoveCurrencySymbolAndGroupingSeparators(string pasteText)
		{
			if (Double.TryParse(pasteText, NumberStyles.Currency, CultureInfo.CurrentCulture, out double value))
			{
				pasteText = value.ToString();
			}

			return pasteText;
		}
	}
}
