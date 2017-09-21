using System.Collections.Generic;
using System.Windows.Input;
using scanbotsdkexamplexamarinforms.Services;
using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public class MainPageViewModel
    {
        IScanbotSdkFeatureService ScanbotSdkFeatureService;

        public ICommand OpenScanningUiCommand { get; }
        public ICommand StartOcrServiceCommand { get; }

        public MainPageViewModel()
        {
            ScanbotSdkFeatureService = DependencyService.Get<IScanbotSdkFeatureService>();

            OpenScanningUiCommand = new Command(() =>
            {
                ScanbotSdkFeatureService.StartScanningUi();
            });

            StartOcrServiceCommand = new Command(() =>
            {
                ScanbotSdkFeatureService.StartOcrService();
            });

        }
    }
}
