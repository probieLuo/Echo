using Echo.IServices;
using Prism.Navigation.Regions;

namespace Echo.ViewModels
{
	public class ChatViewModel : ViewModelBase
	{
		public ChatViewModel(IRegionManager regionManager, INotificationService notification) : base(regionManager, notification)
		{
		}
	}
}