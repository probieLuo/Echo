using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Echo.Converters
{
	public class IntGreaterThanZeroToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return Visibility.Collapsed;

			if (value is int i)
				return i > 0 ? Visibility.Visible : Visibility.Collapsed;

			if (int.TryParse(value.ToString(), out var n))
				return n > 0 ? Visibility.Visible : Visibility.Collapsed;

			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
