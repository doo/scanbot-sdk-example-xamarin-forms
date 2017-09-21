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

namespace scanbotsdkexamplexamarinforms.Droid
{
    [Activity(Label = "scanbot-sdk-example-xamarin-forms.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
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
    }
}
