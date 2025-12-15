using Notification.Wpf;

namespace Echo.IServices
{
	public interface INotificationService
	{
		void Show(NotificationContent content);
	}
}