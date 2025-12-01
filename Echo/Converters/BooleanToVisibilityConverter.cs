using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Echo.Converters
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// 检查输入值是否为布尔类型
			if (value is bool boolValue)
			{
				// 如果参数为 "True" 或 "False"，则根据参数值反转布尔值
				if (parameter != null && parameter.ToString().Equals("True", StringComparison.OrdinalIgnoreCase))
				{
					return boolValue ? Visibility.Visible : Visibility.Collapsed;
				}
				else if (parameter != null && parameter.ToString().Equals("False", StringComparison.OrdinalIgnoreCase))
				{
					return boolValue ? Visibility.Collapsed : Visibility.Visible;
				}

				// 默认情况下，true 显示，false 隐藏
				return boolValue ? Visibility.Visible : Visibility.Collapsed;
			}

			// 如果输入值不是布尔类型，返回默认值
			return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// 将 Visibility 转换回布尔值
			if (value is Visibility visibility)
			{
				return visibility == Visibility.Visible;
			}

			// 如果输入值不是 Visibility 类型，返回默认值
			return false;
		}
	}
}
