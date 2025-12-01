using Echo.Extensions;
using Echo.IServices;
using Echo.SignalR;
using Prism.Commands;
using Prism.Navigation.Regions;
using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Echo.ViewModels
{
	public class ChatViewModel : ViewModelBase
	{
		public string MessageContent { get; set; }
		public ObservableCollection<MessageItem> MessagesList { get; set; }
		public string CurrentChatObjectName { get; set; }

		public DelegateCommand SendMessageCommand { get; }
		private readonly SignalRClient _signalRClient;

		public ChatViewModel(IRegionManager regionManager, INotificationService notification, SignalRClient signalRClient) : base(regionManager, notification)
		{
			_signalRClient = signalRClient;

			// 延迟导航：让 UI 线程先完成区域初始化
			Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
			{
				regionManager.Regions[PrismManager.ChatListViewRegionName].RequestNavigate("RecentChatListView");
			}), DispatcherPriority.ContextIdle);

			MessagesList = new ObservableCollection<MessageItem>()
			{
				new MessageItem(){
					SenderAvatar = "https://probieluo.github.io/assets/OIP%20(8).jpg",
					MessageContent = "Hello, how are you?",
					IsFromMe = false
				},
				new MessageItem(){
					SenderAvatar = "https://probieluo.github.io/assets/OIP%20(8).jpg",
					MessageContent = "Let's catch up sometime.",
					IsFromMe = false
				},
				new MessageItem(){
					SenderAvatar = "https://probieluo.github.io/assets/OIP%20(8).jpg",
					MessageContent = "Let's catch up sometime.\nLet's catch up sometime.\nLet's catch up sometime.\nLet's catch up sometime.\nLet's catch up sometime.\nLet's catch up sometime.\n",
					IsFromMe = false
				},
			};

			SignalRSubscribe();

			SendMessageCommand = new DelegateCommand(SendMessage);
		}

		private void SignalRSubscribe()
		{
			if (_signalRClient == null) return;

			_signalRClient.ReceivePrivateMessage += (senderUserId, content, sendTime, messageId) =>
			{
				// 将收到的消息添加到 MessagesList（在 UI 线程）
				Dispatcher.CurrentDispatcher.Invoke(() =>
				{
					MessagesList.Add(new MessageItem
					{
						SenderAvatar = "https://probieluo.github.io/assets/OIP%20(8).jpg",
						MessageContent = content,
						IsFromMe = false
					});
				});
			};
		}

		private async void SendMessage()
		{
			if (string.IsNullOrWhiteSpace(MessageContent)) return;

			// 发送到服务器
			try
			{
				// 这里示例将发送给用户 id 为 "target"，你可以修改为实际选择的聊天对象 id
				await _signalRClient.SendPrivateMessageAsync("e5789bdc-fc16-447a-98f4-5783706e1ebd", MessageContent);
				
				// 本地显示为已发送消息
				MessagesList.Add(MessageItem.SendMsg(MessageContent, "https://avatars.githubusercontent.com/u/75834079?v=4"));
				MessageContent = string.Empty;
			}
			catch (Exception ex)
			{
				// 错误处理
				ShowError("发送失败", ex.Message);
			}
		}
	}
}