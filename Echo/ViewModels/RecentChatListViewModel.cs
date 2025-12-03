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
				new ChatListItem { Avatar = "https://probieluo.github.io/assets/OIP%20(8).jpg", SendName = "admin", RecentMsg = "Hi", UnreadMsgCount = 1 ,TargetId = "9c9e0e12-d605-4963-b6be-28313fc71d32" },
				new ChatListItem { Avatar = "https://avatars.githubusercontent.com/u/75834079?v=4", SendName = "probie", RecentMsg = "See you", UnreadMsgCount = 0 ,TargetId = "782a848d-1346-42bc-80e0-4aa91e2b4b35" },
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
			_regionManager.Regions[PrismManager.ChatContentViewRegionName].RequestNavigate("ChatContentView");
			_ea?.GetEvent<Echo.Events.ChatSelectedEvent>().Publish(chat);
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
