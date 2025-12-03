using Echo.Extensions;
using Echo.IServices;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Echo.ViewModels
{
	public class ContactListViewModel : ViewModelBase
	{
		private readonly IEventAggregator _ea;
		public ContactListViewModel(IRegionManager regionManager, INotificationService notificationService, IEventAggregator ea) : base(regionManager, notificationService)
		{
			_ea = ea;

			ContactList = new ObservableCollection<ContactItem>
			{
				new ContactItem { Avatar = "https://probieluo.github.io/assets/OIP%20(8).jpg", Name = "admin" },
				new ContactItem { Avatar = "https://avatars.githubusercontent.com/u/75834079?v=4", Name = "probie" },
			};

			OpenChatCommand = new DelegateCommand<ContactItem>(OnOpenChat, item => item != null);
		}

		public ObservableCollection<ContactItem> ContactList { get; set; }

		private ContactItem? _selectedContact;
		public ContactItem? SelectedContact
		{
			get => _selectedContact;
			set => SetProperty(ref _selectedContact, value);
		}

		public DelegateCommand<ContactItem> OpenChatCommand { get; }

		private void OnOpenChat(ContactItem chat)
		{
			if (chat == null) return;

			SelectedContact = chat;
		}
	}

	public class ContactItem
	{
		public string Avatar { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
	}
}
