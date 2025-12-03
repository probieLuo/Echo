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

            var signalRClient = new SignalRClient("https://localhost:7099/chatHub", async () => await Task.FromResult(SignalR.TokenStore.CurrentToken ?? string.Empty));
            containerRegistry.GetContainer().RegisterInstance(signalRClient);

			containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
            containerRegistry.RegisterForNavigation<LoginView, LoginViewModel>();
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
            containerRegistry.RegisterForNavigation<ChatView, ChatViewModel>();
			containerRegistry.RegisterForNavigation<ChatContentView, ChatContentViewModel>();
			containerRegistry.RegisterForNavigation<RecentChatListView, RecentChatListViewModel>();
			containerRegistry.RegisterForNavigation<ContactListView, ContactListViewModel>();
			containerRegistry.RegisterForNavigation<DefaultChatContentView, DefaultChatContentViewModel>();
		}

        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose SignalRClient singleton if registered
            try
            {
                try
                {
                    var client = Container.Resolve(typeof(SignalRClient)) as SignalRClient;
                    if (client != null)
                    {
                        client.DisposeAsync().AsTask().GetAwaiter().GetResult();
                    }
                }
                catch
                {
                    // ignore resolution errors
                }
            }
            catch (Exception)
            {
                // swallow exceptions during shutdown
            }

            base.OnExit(e);
        }

	}
}
