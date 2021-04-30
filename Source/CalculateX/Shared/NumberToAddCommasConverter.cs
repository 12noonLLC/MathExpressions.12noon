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

			return Numbers.FormatNumberWithGroupingSeparators(v);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
