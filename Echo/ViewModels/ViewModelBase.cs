using Echo.IServices;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace Echo.ViewModels
{
	public class ViewModelBase: BindableBase
	{
		protected readonly IRegionManager _regionManager;
		protected readonly INotificationService _notification;

		public ViewModelBase(IRegionManager manager, INotificationService notification)
		{
			_regionManager = manager;
			_notification = notification;
		}
		protected void Show(string title, string message)
		{
			_notification.Show(new Notification.Wpf.NotificationContent
			{
				Title = title,
				Message = message,
				Type = Notification.Wpf.NotificationType.None
			});
		}

		protected void ShowInformation(string title, string message)
		{
			_notification.Show(new Notification.Wpf.NotificationContent
			{
				Title = title,
				Message = message,
				Type = Notification.Wpf.NotificationType.Information
			});
		}
		protected void ShowSuccess(string title, string message)
		{
			_notification.Show(new Notification.Wpf.NotificationContent
			{
				Title = title,
				Message = message,
				Type = Notification.Wpf.NotificationType.Success
			});
		}

		protected void ShowWarning(string title, string message)
		{
			_notification.Show(new Notification.Wpf.NotificationContent
			{
				Title = title,
				Message = message,
				Type = Notification.Wpf.NotificationType.Warning
			});
		}

		protected void ShowError(string title, string message)
		{
			_notification.Show(new Notification.Wpf.NotificationContent
			{
				Title = title,
				Message = message,
				Type = Notification.Wpf.NotificationType.Error
			});
		}

		protected void ShowNotification(string title, string message)
		{
			_notification.Show(new Notification.Wpf.NotificationContent
			{
				Title = title,
				Message = message,
				Type = Notification.Wpf.NotificationType.Notification
			});
		}
	}
}
