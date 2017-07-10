using Android.Content;
using Xamarin.Forms;
using scanbotsdkexamplexamarinforms.Services;
using scanbotsdkexamplexamarinforms.Droid;
using scanbotsdkexamplexamarinforms.Droid.Activities;

[assembly: Dependency(typeof(AndroidScanbotSdkFeatureService))]

namespace scanbotsdkexamplexamarinforms.Droid
{
    public class AndroidScanbotSdkFeatureService : IScanbotSdkFeatureService
    {
        #region IScanbotSdkFeatureService implementation

        public void StartScanningUi()
        {
            Intent intent = new Intent(Forms.Context, typeof(CameraViewDemoActivity));
            Forms.Context.StartActivity(intent);
        }

        #endregion
    }
}
