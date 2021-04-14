using System;
using System.Globalization;
using System.Windows.Data;

namespace Shared
{
	/// <summary>
	/// 
	/// </summary>
	[ValueConversion(typeof(double), typeof(string))]
	internal class NumberToAddCommasConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			double v = (double)value;

			return FormatNumberWithGroupSeparators(v);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}



		/// <summary>
		/// Return the passed number with group separators.
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		internal static string FormatNumberWithGroupSeparators(double d)
		{
			// Format the integer portion with group separators.
			double m = Math.Truncate(d);
			string first = $"{m:N0}";

			// Format the decimal portion and remove the leading zero.
			// If there is no decimal separator, clear the decimal portion.
			string second = d.ToString(CultureInfo.CurrentUICulture);
			string separator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
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
