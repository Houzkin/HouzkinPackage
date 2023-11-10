using Prism.Ioc;
using PrismSample.ViewModels;
using PrismSample.Views;
using System.Windows;

namespace PrismSample {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        protected override Window CreateShell() {
            DIContainer.Container = this.Container;
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry) {
            containerRegistry.RegisterSingleton<SubViewModel>();
        }
    }
    public static class DIContainer {
        public static IContainerProvider Container { get; set; }
    }
}
