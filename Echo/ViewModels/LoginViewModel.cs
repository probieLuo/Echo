using Echo.Events;
using Echo.Extensions;
using Echo.IServices;
using Echo.SignalR;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Navigation.Regions;
using System;
using System.Threading.Tasks;

namespace Echo.ViewModels
{
	public class LoginViewModel : ViewModelBase
	{
		private readonly IAuthService _authService;
		private readonly IEventAggregator _ea;
		private readonly IContainerProvider _containerProvider;
		public DelegateCommand LoginCommand { get; private set; }
		public DelegateCommand RegisterCommand { get; }

		public string Password { get; set; }
		public string UserName { get; set; }
		public string Message { get; set; }
		public bool SnackbarOneIsActive { get; set; }
		public string RegisterUserName { get; set; }
		public string RegisterEmail { get; set; }
		public string RegisterPassword { get; set; }
		public string RegisterConfirmPassword { get; set; }

		private bool _isBusy;

		public bool IsBusy
		{
			get => _isBusy;
			set
			{
				if (_isBusy == value) return;
				_isBusy = value;
				RaiseCommandsCanExecuteChanged();
			}
		}

		public LoginViewModel(IRegionManager regionManager, IAuthService authService, INotificationService notification, IEventAggregator ea, IContainerProvider containerProvider) : base(regionManager, notification)
		{
			_authService = authService;
			_ea = ea;
			_containerProvider = containerProvider;

			LoginCommand = new DelegateCommand(async () => await LoginAsync(), () => !IsBusy);
			RegisterCommand = new DelegateCommand(async () => await RegisterAsync(), () => !IsBusy);
		}

		private void RaiseCommandsCanExecuteChanged()
		{
			LoginCommand?.RaiseCanExecuteChanged();
			RegisterCommand?.RaiseCanExecuteChanged();
		}

		private async Task RegisterAsync()
		{
			if (!IsValidRegister())
				return;

			try
			{
				IsBusy = true;
				var registerResponse = await _authService.Register(RegisterUserName, RegisterEmail, RegisterPassword);

				if (registerResponse.Status)
				{
					_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Register, Status = true, Msg = "注册成功" });
					ShowSuccess("注册成功", registerResponse.Message);
				}
				else
				{
					_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Register, Status = false, Msg = registerResponse.Message });
					ShowInformation("注册失败", registerResponse.Message);
				}
			}
			catch (Exception ex)
			{
				// Don't include sensitive data
				_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Register, Status = false, Msg = "网络或服务器错误" });
				ShowError("错误", ex.Message);
			}
			finally
			{
				IsBusy = false;
			}
		}

		private async Task LoginAsync()
		{
			try
			{
				IsBusy = true;

#if DEBUG
				var loginResponse1 = await _authService.Login("admin", "123456");
				var result1 = loginResponse1.Result;
				if (result1 != null)
				{
					TokenStore.CurrentToken = result1.Token;
					TokenStore.CurrentUserId = result1.UserId;
				}

				_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Login, Status = true, Msg = "登录成功" });
				ShowSuccess("登录成功", loginResponse1.Message);
				return;
#endif

				var loginResponse = await _authService.Login(UserName, Password);

				if (loginResponse.Status)
				{
					// 保存 Token 与 UserId 以供 SignalR 使用
					var result = loginResponse.Result;
					if (result != null)
					{
						TokenStore.CurrentToken = result.Token;
						TokenStore.CurrentUserId = result.UserId;
					}

					_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Login, Status = true, Msg = "登录成功" });
					ShowSuccess("登录成功", loginResponse.Message);
				}
				else
				{
					_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Login, Status = false, Msg = loginResponse.Message });
					ShowInformation("登录失败", loginResponse.Message);
				}
			}
			catch (Exception ex)
			{
				_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Login, Status = false, Msg = "网络或服务器错误" });
				ShowError("错误", ex.Message);
			}
			finally
			{
				IsBusy = false;
			}
		}

		private bool IsValidRegister()
		{
			if (string.IsNullOrEmpty(RegisterUserName))
			{
				_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Register, Status = false, Msg = "用户名不能为空！" });
				return false;
			}
			if (string.IsNullOrEmpty(RegisterPassword))
			{
				_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Register, Status = false, Msg = "密码不能为空！" });
				return false;
			}
			if (string.IsNullOrEmpty(RegisterConfirmPassword))
			{
				_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Register, Status = false, Msg = "确认密码不能为空！" });
				return false;
			}
			if (string.IsNullOrEmpty(RegisterEmail))
			{
				_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Register, Status = false, Msg = "邮箱不能为空！" });
				return false;
			}
			if (RegisterConfirmPassword != RegisterPassword)
			{
				_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Register, Status = false, Msg = "两次输入密码不一致！" });
				return false;
			}
			if (RegisterPassword.Length < 6)
			{
				_ea.GetEvent<LoginMessageEvent>().Publish(new LoginMessage { MessageType = LoginMessageType.Register, Status = false, Msg = "密码设置至少6位！" });
				return false;
			}
			return true;
		}
	}
}