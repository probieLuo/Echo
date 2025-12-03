using Echo.Extensions;
using Echo.IServices;
using Echo.SignalR;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Echo.ViewModels
{
	public class ChatContentViewModel : ViewModelBase
	{
		private string _messageContent;
		public string MessageContent
		{
			get => _messageContent;
			set => SetProperty(ref _messageContent, value);
		}

		private string _currentChatObjectName;
		public string CurrentChatObjectName
		{
			get => _currentChatObjectName;
			set => SetProperty(ref _currentChatObjectName, value);
		}

		private string _currentChatObjectAvatar;
		public string CurrentChatObjectAvatar
		{
			get => _currentChatObjectAvatar;
			set => SetProperty(ref _currentChatObjectAvatar, value);
		}

		private readonly Dispatcher _uiDispatcher;
		public ObservableCollection<MessageItem> MessagesList { get; set; }
		public DelegateCommand SendMessageCommand { get; }

		private readonly IEventAggregator _ea;

	 	private readonly SignalR.SignalRClient _client;
		private string targetId;

		public ChatContentViewModel(IRegionManager regionManager, INotificationService notification, IEventAggregator ea, SignalRClient client) : base(regionManager, notification)
		{
			_ea = ea;
			_client = client;
			_uiDispatcher = Application.Current.Dispatcher;

			MessagesList = [];

			SignalRSubscribe();

			SendMessageCommand = new DelegateCommand(async () => await SendMessageAsync());
			_ea.GetEvent<Echo.Events.ChatSelectedEvent>().Subscribe(OnChatSelected, ThreadOption.UIThread);
		}

		private void OnChatSelected(ChatListItem chat)
		{
			if (chat == null) return;
			CurrentChatObjectName = chat.SendName;
			CurrentChatObjectAvatar = chat.Avatar;
			targetId = chat.TargetId;
			MessagesList.Clear();
			MessagesList.Add(MessageItem.ReceiveMsg(chat.RecentMsg, CurrentChatObjectAvatar));
		}

		private void SignalRSubscribe()
		{
			if (_client == null) return;

			_client.ReceivePrivateMessage += (senderUserId, content, sendTime, messageId) =>
			{
				_uiDispatcher.BeginInvoke(new Action(() =>
				{
					MessagesList.Add(new MessageItem
					{
						SenderAvatar = CurrentChatObjectAvatar,
						MessageContent = content,
						IsFromMe = false
					});
				}));
			};
		}

		private async Task SendMessageAsync()
		{
			if (string.IsNullOrWhiteSpace(MessageContent)) return;

			try
			{
				await _client.SendPrivateMessageAsync(targetId, MessageContent);

				MessagesList.Add(MessageItem.SendMsg(MessageContent, "https://avatars.githubusercontent.com/u/75834079?v=4"));
				MessageContent = string.Empty;
			}
			catch (Exception ex)
			{
				// 错误处理
				ShowError("发送失败", ex.Message);
			}
		}

		private async Task<string> AccessTokenProvider()
		{
			return await Task.FromResult(TokenStore.CurrentToken ?? string.Empty);
		}
	}
}
