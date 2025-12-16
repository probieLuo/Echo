using Echo.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Echo.ViewModels
{
	public class MainViewModel : BindableBase
	{
		private readonly IRegionManager regionManager;

		public ObservableCollection<MenuItem> MenuItems { get; set; }
		private MenuItem _selectedItem;

		public MenuItem SelectedItem
		{
			get => _selectedItem;
			set
			{
				if (SetProperty(ref _selectedItem, value))
				{
					NavigateToSelectedItem();
				}
			}
		}

		private static IEnumerable<MenuItem> GenerateDemoItems()
		{
			yield return new MenuItem() { Name = "PythonEditor", ViewName = "PythonEditorView", Order = 2 };
			yield return new MenuItem() { Name = "Chat", ViewName = "ChatView", Order = 1 };
			yield return new MenuItem() { Name="Home",ViewName="HomeView", Order=0};
		}

		public DelegateCommand SignoutCommand { get; set; }
		public DelegateCommand UserCommand { get; set; }
		public DelegateCommand HomeCommand { get; set; }

		public MainViewModel(IRegionManager regionManager)
		{
			this.regionManager = regionManager;

			MenuItems = [.. GenerateDemoItems().OrderBy(i => i.Order),];

			SignoutCommand = new DelegateCommand(OnSignout);
			UserCommand = new DelegateCommand(OnUser);
			HomeCommand = new DelegateCommand(OnHome);
		}

		private void OnHome()
		{
			SelectedItem = MenuItems.FirstOrDefault(i => i.Name == "Home");
			regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("HomeView");
		}

		private void OnUser()
		{
			SelectedItem = MenuItems.FirstOrDefault(i => i.Name == "ChatUser");
			regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate("UserView");
		}

		private void OnSignout()
		{
			SelectedItem = null;
			regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("LoginView");
		}

		private void NavigateToSelectedItem()
		{
			if (SelectedItem == null) return;

			try
			{
				regionManager.Regions[PrismManager.MainViewRegionName].RequestNavigate(_selectedItem.ViewName);
				if (_selectedItem.Name == "Chat")
				{
					regionManager.Regions[PrismManager.ChatListViewRegionName].RequestNavigate("RecentChatListView");
					regionManager.Regions[PrismManager.ChatContentViewRegionName].RequestNavigate("DefaultChatContentView");
				}
			}
			catch
			{

			}
		}
	}
}