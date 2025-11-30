using Echo.Extensions;
using Echo.IServices;
using Notification.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Echo.ViewModels
{
	public class LoginViewModel: BindableBase
    {
        private readonly IRegionManager _regionManager;

		private readonly IAuthService _authService;

		public DelegateCommand LoginCommand { get; private set; }
		public DelegateCommand RegisterCommand { get; }

		public string Password { get; set; }
		public string UserName { get; set;  }
		public string Message { get; set; }
		public bool SnackbarOneIsActive { get; set; }
		public string RegisterUserName { get; set; }
		public string RegisterEmail { get; set; }
		public string RegisterPassword { get; set; }
		public string RegisterConfirmPassword { get; set; }

		public LoginViewModel(IRegionManager regionManager, IAuthService authService)
		{
            LoginCommand = new DelegateCommand(Login);
            _regionManager = regionManager;
			_authService = authService;
		}

        private async void Login()
        {
			var loginResponse =await  _authService.Login(UserName,Password);
			if (loginResponse.Status)
			{
				_regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("MainView");
			}
			else
			{
				
			}

		}
    }
}
