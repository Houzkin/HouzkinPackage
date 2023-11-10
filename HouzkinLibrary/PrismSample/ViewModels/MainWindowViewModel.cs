using Prism.Ioc;
using Prism.Mvvm;

namespace PrismSample.ViewModels {
    public class MainWindowViewModel : BindableBase {
        private string _title = "Prism Application";
        public string Title {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        public SubViewModel InstanceA { get; set; }
        public SubViewModel InstanceB { get; set; }

        public MainWindowViewModel() {
            InstanceA = DIContainer.Container.Resolve<SubViewModel>();
            InstanceB = DIContainer.Container.Resolve<SubViewModel> ();
            InstanceB.Name = "Foo";
        }
    }
    public class SubViewModel {
        public string Name { get; set; }
        public SubViewModel() {
            Name = "Hoge";
        }
    }
}
