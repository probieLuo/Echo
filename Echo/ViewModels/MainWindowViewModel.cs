using Echo.Common;
using Echo.Extensions;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace Echo.ViewModels
{
    public class MainWindowViewModel : BindableBase, IConfigureService
    {
        private string _title = "Echo";
		private readonly IRegionManager regionManager;

		public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel(IRegionManager regionManager)
        {
			this.regionManager = regionManager;
		}

		public void Configure()
		{
            regionManager.Regions[PrismManager.RootViewRegionName].RequestNavigate("LoginView");
        }
	}
}
