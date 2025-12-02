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

		public MainViewModel(IRegionManager regionManager)
        {
			this.regionManager = regionManager;

			SignoutCommand = new DelegateCommand(OnSignout);

		}

		private void OnSignout()
		{
			regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("LoginView");
		}
	}
}
