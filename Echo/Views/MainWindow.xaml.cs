using Echo.Events;
using Prism.Events;
using System.Windows;

namespace Echo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private readonly IEventAggregator _ea;

		public MainWindow(IEventAggregator ea)
		{
			_ea = ea;
			InitializeComponent();
			_ea.GetEvent<LoginMessageEvent>().Subscribe((msg) =>
			{
				switch (msg.MessageType)
				{
					case LoginMessageType.Login:
						if (msg.Status)
						{
							this.MinWidth = 600;
							this.MinHeight = 450;
							this.WindowState = WindowState.Maximized;
						}
						break;
					case LoginMessageType.Register:
						if (msg.Status)
						{
							this.MinWidth = 1200;
							this.MinHeight = 900;
							this.WindowState = WindowState.Maximized;
						}
						break;
				}
			});
		}
    }
}
