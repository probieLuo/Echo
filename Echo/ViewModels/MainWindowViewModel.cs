using Echo.Common;
using Echo.Events;
using Echo.Extensions;
using Echo.IServices;
using Echo.SignalR;
using Prism.Events;
using Prism.Navigation.Regions;
using System;
using System.Windows;

namespace Echo.ViewModels
{
	public class MainWindowViewModel : ViewModelBase, IConfigureService
	{
		private readonly IEventAggregator _ea;
		private readonly SignalRClient _signalRClient;

		public string Title { get; set; } = "Echo";
		public MainWindowViewModel(IRegionManager regionManager, INotificationService notification, IEventAggregator ea, SignalRClient signalRClient) : base(regionManager, notification)
		{
			_ea = ea;
			_signalRClient = signalRClient;

			_ea.GetEvent<LoginMessageEvent>().Subscribe((msg) =>
			{
				if(msg.Status && msg.MessageType == LoginMessageType.Login)
				{
					_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("MainView");
					Application.Current.MainWindow.WindowState = WindowState.Maximized;

					// 登录成功后异步启动 SignalR（不阻塞 UI）
					_ = _signalRClient.StartAsync();
				}
				else if(msg.Status && msg.MessageType == LoginMessageType.Register)
				{
					_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("MainView");
					Application.Current.MainWindow.WindowState = WindowState.Maximized;
				}
			},ThreadOption.UIThread);
		}

		public void Configure()
		{
			_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("LoginView");
		}
	}
}
