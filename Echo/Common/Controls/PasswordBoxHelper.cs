using System.Windows;
using System.Windows.Controls;

namespace Echo.Common.Controls
{
    public static class PasswordBoxHelper
    {
        private static bool _isUpdatingPassword = false;

        public static string GetPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject obj, string value)
        {
            obj.SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnPasswordChanged));

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox && !_isUpdatingPassword)
            {
                _isUpdatingPassword = true;
                passwordBox.Password = e.NewValue.ToString();
                _isUpdatingPassword = false;
            }
        }

        public static bool GetIsPasswordBindingEnable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPasswordBindingEnableProperty);
        }

        public static void SetIsPasswordBindingEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPasswordBindingEnableProperty, value);
        }

        public static readonly DependencyProperty IsPasswordBindingEnableProperty =
            DependencyProperty.RegisterAttached("IsPasswordBindingEnable", typeof(bool), typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnIsPasswordBindingEnableChanged));

        private static void OnIsPasswordBindingEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }
                else
                {
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && !_isUpdatingPassword)
            {
                _isUpdatingPassword = true;
                SetPassword(passwordBox, passwordBox.Password);
                _isUpdatingPassword = false;
            }
        }
    }
}