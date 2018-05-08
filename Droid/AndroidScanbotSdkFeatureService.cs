using Android.App;
using Android.Content;
using Xamarin.Forms;

using scanbotsdkexamplexamarinforms.Services;
using scanbotsdkexamplexamarinforms.Droid;
using scanbotsdkexamplexamarinforms.Droid.Activities;
using scanbotsdkexamplexamarinforms.Droid.Services;

using ScanbotSDK.Xamarin.Android.Wrapper;

[assembly: Dependency(typeof(AndroidScanbotSdkFeatureService))]

namespace scanbotsdkexamplexamarinforms.Droid
{
    public class AndroidScanbotSdkFeatureService : IScanbotSdkFeatureService
    {
        #region IScanbotSdkFeatureService implementation

        public void StartScanningUi()
        {
            var intent = new Intent(Forms.Context, typeof(CameraViewDemoActivity));
            var mainActivity = Forms.Context as Activity;
            mainActivity.StartActivityForResult(intent, CameraViewDemoActivity.REQUEST_CODE_SCANBOT_CAMERA);
        }

        public void StartOcrService()
        {
            var images = MainActivity.TempImageStorage.GetImages();
            if (images.Length == 0)
            {
                var msg = new AlertMessage
                {
                    Title = "Info",
                    Message = "Please snap some images via Scanning UI."
                };
                MessagingCenter.Send(msg, AlertMessage.ID);
                return;
            }

            var ocrIntent = new Intent(Forms.Context, typeof(OcrDemoService));
            Forms.Context.StartService(ocrIntent);
        }

        public bool IsLicenseValid()
        {
            return SBSDK.IsLicenseValid();
        }

        #endregion
    }
}
