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
	internal class UserViewModel : ViewModelBase
	{
		private readonly IEventAggregator _ea;
		public UserViewModel(IRegionManager regionManager, INotificationService notificationService, IEventAggregator ea) : base(regionManager, notificationService)
		{
			_ea = ea;
		}
	}
}
