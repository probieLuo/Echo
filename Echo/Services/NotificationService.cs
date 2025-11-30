using Echo.IServices;
using Notification.Wpf;
using System.Windows;
using System.Windows.Threading;

namespace Echo.Services
{
	public class NotificationService : INotificationService
	{
		private readonly NotificationManager _manager;
		private readonly Dispatcher _uiDispatcher;

		public NotificationService()
		{
			_uiDispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

			// 确保在 UI Dispatcher 上创建 NotificationManager
			if (_uiDispatcher.CheckAccess())
			{
				_manager = new NotificationManager();
			}
			else
			{
				NotificationManager tmp = null;
				_uiDispatcher.Invoke(() => tmp = new NotificationManager());
				_manager = tmp;
			}
		}

		public void Show(NotificationContent content)
		{
			if (_uiDispatcher.CheckAccess())
			{
				_manager.Show(content);
			}
			else
			{
				_uiDispatcher.Invoke(() => _manager.Show(content));
			}
		}
	}
}
