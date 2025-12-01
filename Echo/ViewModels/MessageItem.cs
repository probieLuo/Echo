using System;
using System.Collections.Generic;
using System.Text;

namespace Echo.ViewModels
{
	public class MessageItem
	{
		public string SenderAvatar { get; set; } // 发送者头像
		public string MessageContent { get; set; } // 消息内容
		public bool IsFromMe { get; set; } // 是否来自自己
		public string Avatar { get; set; }

		public static MessageItem SendMsg(string msg,string avatar)
		{
			return new MessageItem
			{
				Avatar = avatar,
				MessageContent = msg,
				IsFromMe = true
			};
		}

		public static MessageItem ReceiveMsg(string msg, string avatar)
		{
			return new MessageItem
			{
				SenderAvatar = avatar,
				MessageContent = msg,
				IsFromMe = false
			};
		}
	}
}
