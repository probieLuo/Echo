using Echo.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Echo.ViewModels
{
	public class LoginViewModel: BindableBase
    {
        private readonly IRegionManager regionManager;

        public DelegateCommand LoginCommand { get; private set; }
        public LoginViewModel(IRegionManager regionManager)
		{
            LoginCommand = new DelegateCommand(Login);
            this.regionManager = regionManager;
            
        }

        private void Login()
        {
            regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("MainView");
        }
    }
}
