using System.Windows.Input;
using scanbotsdkexamplexamarinforms.Services;
using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public class MainPageViewModel
    {
        IScanbotSdkFeatureService ScanbotSdkFeatureService;

        public ICommand OpenScanningUiCommand { get; }

        public MainPageViewModel()
        {
            ScanbotSdkFeatureService = DependencyService.Get<IScanbotSdkFeatureService>();

            OpenScanningUiCommand = new Command(() =>
            {
                ScanbotSdkFeatureService.StartScanningUi();
            });

        }
    }
}
