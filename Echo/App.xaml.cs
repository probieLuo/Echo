using DryIoc;
using Echo.Common;
using Echo.IServices;
using Echo.Services;
using Echo.ViewModels;
using Echo.Views;
using Echo.Views.ChatViewRegion;
using Example;
using Notification.Wpf;
using Prism.Container.DryIoc;
using Prism.Ioc;
using System;
using System.Threading.Tasks;
using System.Windows;
using Echo.SignalR;

namespace Echo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }
        protected override void OnInitialized()
        {
            var service = App.Current.MainWindow.DataContext as IConfigureService;
            if (service != null)
            {
                service.Configure();
            }
            base.OnInitialized();

        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
			containerRegistry.GetContainer()
				.Register<HttpRestClient>(made: Parameters.Of.Type<string>(serviceKey: "webUrl"));
			containerRegistry.GetContainer().RegisterInstance(@"http://localhost:5221/", serviceKey: "webUrl");
			containerRegistry.RegisterSingleton<Echo.IServices.INotificationService, Echo.Services.NotificationService>();

			containerRegistry.Register<IAuthService, AuthService>();

			// 注册 SignalRClient 为单例工厂，确保 AccessTokenProvider 在连接时读取最新 TokenStore
			var container = containerRegistry.GetContainer();
			string webUrl = container.Resolve<string>(serviceKey: "webUrl");
			string hubUrl = new Uri(new Uri(webUrl), "chatHub").ToString(); // e.g. http://localhost:5221/chatHub

			containerRegistry.RegisterSingleton<SignalRClient>(() => new SignalRClient(hubUrl, () => Task.FromResult(TokenStore.CurrentToken ?? string.Empty)));

			containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
            containerRegistry.RegisterForNavigation<LoginView, LoginViewModel>();
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
            containerRegistry.RegisterForNavigation<ChatView, ChatViewModel>();
            containerRegistry.RegisterForNavigation<RecentChatListView, RecentChatListViewModel>();
        }

	}
}
