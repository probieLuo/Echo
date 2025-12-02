using Echo.Common;
using Echo.Events;
using Echo.Extensions;
using Echo.IServices;
using Echo.SignalR;
using Prism.Events;
using Prism.Navigation.Regions;
using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Echo.ViewModels
{
	public class MainWindowViewModel : ViewModelBase, IConfigureService
	{
		private readonly IEventAggregator _ea;

		public string Title { get; set; } = "Echo";

		//private double _width;
		//public double Width
		//{
		//	get => _width;
		//	set => SetProperty(ref _width, value);
		//}

		//private double _height;
		//public double Height
		//{
		//	get => _height;
		//	set => SetProperty(ref _height, value);
		//}

		public MainWindowViewModel(IRegionManager regionManager, INotificationService notification, IEventAggregator ea) : base(regionManager, notification)
		{
			_ea = ea;
			_ea.GetEvent<LoginMessageEvent>().Subscribe((msg) =>
			{
				if(msg.Status && msg.MessageType == LoginMessageType.Login)
				{
					_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("MainView");
					_regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("ChatView");
					_regionManager.Regions[PrismManager.ChatListViewRegionName].RequestNavigate("RecentChatListView");
					_regionManager.Regions[PrismManager.ChatContentViewRegionName].RequestNavigate("DefaultChatContentView");

					//Application.Current.MainWindow.WindowState = WindowState.Maximized;
				}
				else if(msg.Status && msg.MessageType == LoginMessageType.Register)
				{
					_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("MainView");
					_regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("ChatView");
					_regionManager.Regions[PrismManager.ChatListViewRegionName].RequestNavigate("RecentChatListView");
					_regionManager.Regions[PrismManager.ChatContentViewRegionName].RequestNavigate("DefaultChatContentView");
					//Application.Current.MainWindow.WindowState = WindowState.Maximized;
				}
			},ThreadOption.UIThread);
		}

		public void Configure()
		{
			_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("LoginView");
		}
	}
}
