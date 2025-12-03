using Echo.Common;
using Echo.Events;
using Echo.Extensions;
using Echo.IServices;
using Echo.SignalR;
using Prism.Events;
using Prism.Navigation.Regions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

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
			_signalRClient = signalRClient ?? throw new ArgumentNullException(nameof(signalRClient));
			_ea.GetEvent<LoginMessageEvent>().Subscribe(async (msg) =>
			{
				if(msg.Status && msg.MessageType == LoginMessageType.Login)
				{
					_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("MainView");
					_regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("ChatView");
					_regionManager.Regions[PrismManager.ChatListViewRegionName].RequestNavigate("RecentChatListView");
					_regionManager.Regions[PrismManager.ChatContentViewRegionName].RequestNavigate("DefaultChatContentView");

					// Start SignalR client (single instance from DI)
					try
					{
						await _signalRClient.StartAsync();
					}
					catch (Exception ex)
					{
						ShowError("SignalR 连接失败", ex.Message);
					}
				}
				else if(msg.Status && msg.MessageType == LoginMessageType.Register)
				{
					_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("MainView");
					_regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("ChatView");
					_regionManager.Regions[PrismManager.ChatListViewRegionName].RequestNavigate("RecentChatListView");
					_regionManager.Regions[PrismManager.ChatContentViewRegionName].RequestNavigate("DefaultChatContentView");
					try
					{
						await _signalRClient.StartAsync();
					}
					catch (Exception ex)
					{
						ShowError("SignalR 连接失败", ex.Message);
					}
				}
			}, ThreadOption.UIThread);
		}

		public void Configure()
		{
			_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("LoginView");
		}
	}
}
