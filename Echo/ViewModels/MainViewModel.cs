using Echo.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Windows.Threading;

namespace Echo.ViewModels
{
    public class MainViewModel : BindableBase
    {
		private readonly IRegionManager regionManager;

		public DelegateCommand SignoutCommand {  get; set; }
		public DelegateCommand UserCommand { get; set; }
		public DelegateCommand HomeCommand { get; set; }
		

		public MainViewModel(IRegionManager regionManager)
        {
			this.regionManager = regionManager;

			SignoutCommand = new DelegateCommand(OnSignout);
			UserCommand = new DelegateCommand(OnUser);
			HomeCommand = new DelegateCommand(OnHome);
		}

		private void OnHome()
		{
			regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("ChatView");
		}

		private void OnUser()
		{
			regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("UserView");
		}

		private void OnSignout()
		{
			regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("LoginView");
		}
	}
}
