using Echo.Events;
using MaterialDesignThemes.Wpf;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Echo.Views
{
	/// <summary>
	/// LoginView.xaml 的交互逻辑
	/// </summary>
	public partial class LoginView : UserControl
	{
		private readonly IEventAggregator _ea;
		public LoginView(IEventAggregator ea)
		{
			_ea = ea;
			InitializeComponent();

			SnackbarOne.MessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(1.5));
			_ea.GetEvent<LoginMessageEvent>().Subscribe((msg) =>
			{
				switch (msg.MessageType)
				{
					case LoginMessageType.Login:
						if (!msg.Status)
							SnackbarOne.MessageQueue?.Enqueue(msg.Msg);
						break;
					case LoginMessageType.Register:
						if (!msg.Status)
							SnackbarOne.MessageQueue?.Enqueue(msg.Msg);
						break;
				}
			});
		}
	}
}
