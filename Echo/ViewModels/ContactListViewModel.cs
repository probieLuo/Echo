using Echo.Extensions;
using Echo.IServices;
using Echo.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Echo.ViewModels
{
	public class ContactListViewModel : ViewModelBase
	{
		private readonly IEventAggregator _ea;
		private readonly IUserService _userService;
		public ContactListViewModel(IRegionManager regionManager, INotificationService notificationService, IEventAggregator ea, IUserService userService) : base(regionManager, notificationService)
		{
			_ea = ea;
			_userService = userService;

			ContactList = new ObservableCollection<ContactItem>();

			OpenChatCommand = new DelegateCommand(OnOpenChat);
			OnLoadedCommand = new DelegateCommand(async () => await LoadContactsAsync());
		}

		public ObservableCollection<ContactItem> ContactList { get; set; }

		private ContactItem? _selectedContact;
		public ContactItem? SelectedContact
		{
			get => _selectedContact;
			set => SetProperty(ref _selectedContact, value);
		}

		public DelegateCommand OpenChatCommand { get; }
		public DelegateCommand OnLoadedCommand { get; }

		private async Task LoadContactsAsync()
		{
			try
			{
				var resp = await _userService.GetUsers();
				if (resp == null || !resp.Status || resp.Result == null)
				{
					ShowError("加载联系人失败", resp?.Message ?? "未知错误");
					return;
				}

				ContactList.Clear();
				foreach (var u in resp.Result)
				{
					ContactList.Add(new ContactItem
					{
						Name = u.Username,
						UserId = u.UserId,
						Avatar = string.Empty,
					});
				}
			}
			catch (Exception ex)
			{
				ShowError("加载联系人异常", ex.Message);
			}
		}

		private void OnOpenChat()
		{
			ChatListItem data = new ChatListItem
			{
				Avatar = SelectedContact.Avatar,
				SendName = SelectedContact.Name,
				TargetId = SelectedContact.UserId,
			};
			_regionManager.Regions[PrismManager.ChatContentViewRegionName].RequestNavigate("ChatContentView");
			_ea?.GetEvent<Echo.Events.ChatSelectedEvent>().Publish(data);
		}
	}

	public class ContactItem
	{
		public string Avatar { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string UserId { get; set; } = string.Empty;	
	}
}
