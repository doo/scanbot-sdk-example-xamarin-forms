using scanbotsdkexamplexamarinforms.Services;
using Xamarin.Forms;
using scanbotsdkexamplexamarinforms.iOS;
using scanbotsdkexamplexamarinforms.iOS.ViewControllers;
using UIKit;

[assembly: Dependency(typeof(IOSScanbotSdkFeatureService))]

namespace scanbotsdkexamplexamarinforms.iOS
{
    public class IOSScanbotSdkFeatureService : IScanbotSdkFeatureService
    {
        #region IScanbotSdkFeatureService implementation

        public void StartScanningUi()
        {
            var cameraViewController = new CameraDemoViewController();
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(cameraViewController, true, null);
        }

        public void StartOcrService()
        {
            // TODO not implemented yet
        }

        #endregion
    }
}
