using Echo.Extensions;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace Echo.ViewModels
{
	internal class ChatViewModel : BindableBase
    {
        private readonly IRegionManager regionManager;

        public ChatViewModel(IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            // 延迟导航：让 UI 线程先完成区域初始化
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                if (regionManager.Regions.ContainsRegionWithName(PrismManager.MainViewRegionName))
                {
                    regionManager.Regions[PrismManager.ChatListViewRegionName].RequestNavigate("RecentChatListView");
                }
                else
                {
                    Console.WriteLine($"错误：找不到区域 {PrismManager.MainViewRegionName}");
                }
            }), DispatcherPriority.ContextIdle); 
        }
    }
}
