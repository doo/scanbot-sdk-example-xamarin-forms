using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using ScanbotSDK.Xamarin.Android.Wrapper;
using System.IO;
using Xamarin.Forms;
using Android.Util;

using scanbotsdkexamplexamarinforms.Droid.Activities;

namespace scanbotsdkexamplexamarinforms.Droid
{
    [Activity(Label = "scanbot-sdk-example-xamarin-forms.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        static string LOG_TAG = typeof(MainActivity).Name;

        public static TempImageStorage TempImageStorage = new TempImageStorage();


        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App());
        }

        public static string GetPublicExternalStorageDirectory()
        {
            var externalPublicPath = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, "scanbot-sdk-example-xamarin-forms");
            Directory.CreateDirectory(externalPublicPath);
            return externalPublicPath;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            Log.Debug(LOG_TAG, "ActivityResult received: resultCode = " + resultCode + "; requestCode = " + requestCode);

            if (requestCode == CameraViewDemoActivity.REQUEST_CODE_SCANBOT_CAMERA && resultCode == Result.Ok)
            {
                var documentImageFileUri = data.GetStringExtra(CameraViewDemoActivity.EXTRAS_ARG_DOC_IMAGE_FILE_URI);
                var originalImageFileUri = data.GetStringExtra(CameraViewDemoActivity.EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI);
                Log.Debug(LOG_TAG, "documentImageFileUri = " + documentImageFileUri);
                Log.Debug(LOG_TAG, "originalImageFileUri = " + originalImageFileUri);

                // Send a message to the Xamarin Forms layer:
                var msg = new ScanbotCameraResultMessage
                {
                    DocumentImageFileUri = documentImageFileUri,
                    OriginalImageFileUri = originalImageFileUri
                };
                MessagingCenter.Send(msg, ScanbotCameraResultMessage.ID);
            }
        }
    }
}
