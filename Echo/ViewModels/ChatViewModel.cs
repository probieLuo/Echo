using Echo.Extensions;
using Echo.IServices;
using Prism.Commands;
using Prism.Navigation.Regions;
using System;

namespace Echo.ViewModels
{
	public class ChatViewModel : ViewModelBase
	{
		public DelegateCommand OpenRecentChatListCommand { get; }
		public DelegateCommand OpenContactListViewCommand { get; }
		
		public ChatViewModel(IRegionManager regionManager, INotificationService notification) : base(regionManager, notification)
		{
			OpenRecentChatListCommand = new DelegateCommand(OpenRecentChatList);
			OpenContactListViewCommand = new DelegateCommand(OpenContactListView);
		}

		private void OpenContactListView()
		{
			_regionManager.Regions[PrismManager.ChatListViewRegionName].RequestNavigate("ContactListView");
		}

		private void OpenRecentChatList()
		{
			_regionManager.Regions[PrismManager.ChatListViewRegionName].RequestNavigate("RecentChatListView");
		}
	}
}