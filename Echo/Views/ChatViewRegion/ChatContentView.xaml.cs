using Echo.ViewModels;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Echo.Views.ChatViewRegion
{
	public partial class ChatContentView : UserControl
	{
		private INotifyCollectionChanged? _messagesSubscription;

		public ChatContentView()
		{
			InitializeComponent();
			this.Loaded += ChatContentView_Loaded;
			this.Unloaded += ChatContentView_Unloaded;
		}

		private void ChatContentView_Loaded(object? sender, RoutedEventArgs e)
		{
			TrySubscribeMessages();
		}

		private void ChatContentView_Unloaded(object? sender, RoutedEventArgs e)
		{
			UnsubscribeMessages();
		}

		private void TrySubscribeMessages()
		{
			UnsubscribeMessages();

			if (DataContext is ChatContentViewModel vm && vm.MessagesList is INotifyCollectionChanged coll)
			{
				_messagesSubscription = coll;
				_messagesSubscription.CollectionChanged += MessagesList_CollectionChanged;
			}
		}

		private void UnsubscribeMessages()
		{
			if (_messagesSubscription != null)
			{
				_messagesSubscription.CollectionChanged -= MessagesList_CollectionChanged;
				_messagesSubscription = null;
			}
		}

		private void MessagesList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			// 当新消息添加时滚动到底部（在 UI 线程）
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				Dispatcher.InvokeAsync(() =>
				{
					MessagesScrollViewer?.ScrollToEnd();
				});
			}
		}

		private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter) return;

			// Shift+Enter：保留换行行为，不拦截
			if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
			{
				// 不设置 e.Handled，让 TextBox 插入换行
				return;
			}

			// 普通 Enter：执行 ViewModel 的发送命令并拦截按键（避免插入换行）
			if (DataContext is ChatContentViewModel vm && vm.SendMessageCommand != null && vm.SendMessageCommand.CanExecute())
			{
				vm.SendMessageCommand.Execute();
				e.Handled = true;
			}
		}
	}
}
