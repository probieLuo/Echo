using Echo.Common;
using Echo.ViewModels;
using Echo.Views;
using Echo.Views.ChatViewRegion;
using Prism.Ioc;
using System.Windows;

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
            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
            containerRegistry.RegisterForNavigation<LoginView, LoginViewModel>();
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
            containerRegistry.RegisterForNavigation<ChatView, ChatViewModel>();
            containerRegistry.RegisterForNavigation<RecentChatListView, RecentChatListViewModel>();
        }
    }
}
