using Echo.IServices;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System.Collections.ObjectModel;
using Echo.Extensions;
using Prism.Events;

namespace Echo.ViewModels
{
	public class RecentChatListViewModel : ViewModelBase
	{
		private readonly IEventAggregator _ea;
		public RecentChatListViewModel(IRegionManager regionManager, INotificationService notificationService, IEventAggregator ea) :base(regionManager,notificationService)
		{
			_ea = ea;
			Chats = new ObservableCollection<ChatListItem>
			{
				new ChatListItem { Avatar = "https://avatars.githubusercontent.com/u/1?v=4", SendName = "Alice", RecentMsg = "Hi", UnreadMsgCount = 1 ,TargetId = "62a3e676-0fc7-41e9-bd34-aebddf4e6960" },
				new ChatListItem { Avatar = "https://avatars.githubusercontent.com/u/2?v=4", SendName = "Bob", RecentMsg = "See you", UnreadMsgCount = 0 ,TargetId = "5f8acb90-732b-4794-9f32-f4c7476d43ee" },
			};

			OpenChatCommand = new DelegateCommand<ChatListItem>(OnOpenChat, item => item != null);
		}

		private ObservableCollection<ChatListItem> _chats = new();
		public ObservableCollection<ChatListItem> Chats
		{
			get => _chats;
			set => SetProperty(ref _chats, value);
		}

		private ChatListItem? _selectedChat;
		public ChatListItem? SelectedChat
		{
			get => _selectedChat;
			set => SetProperty(ref _selectedChat, value);
		}

		public DelegateCommand<ChatListItem> OpenChatCommand { get; }

		private void OnOpenChat(ChatListItem chat)
		{
			if (chat == null) return;

			SelectedChat = chat;
			_ea?.GetEvent<Echo.Events.ChatSelectedEvent>().Publish(chat);
			_regionManager.Regions[PrismManager.ChatContentViewRegionName].RequestNavigate("ChatContentView");
		}
	}

	public class ChatListItem : BindableBase
	{
		private string _avatar = string.Empty;
		public string Avatar
		{
			get => _avatar;
			set => SetProperty(ref _avatar, value);
		}

		private string _sendName = string.Empty;
		public string SendName
		{
			get => _sendName;
			set => SetProperty(ref _sendName, value);
		}

		private string _recentMsg = string.Empty;
		public string RecentMsg
		{
			get => _recentMsg;
			set => SetProperty(ref _recentMsg, value);
		}

		private int _unreadMsgCount;
		public int UnreadMsgCount
		{
			get => _unreadMsgCount;
			set => SetProperty(ref _unreadMsgCount, value);
		}

		public string TargetId;
	}
}
