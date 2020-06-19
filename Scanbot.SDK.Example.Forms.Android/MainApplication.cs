using System;
using System.IO;
using Android.App;
using Android.Runtime;
using Android.Util;
using ScanbotSDK.Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms.Droid
{
    // It is strongly recommended to add the LargeHeap = true flag in your Application class.
    // Working with images, creating PDFs, etc. are memory intensive tasks.
    // So to prevent OutOfMemoryError, consider adding this flag!
    // For additional details see:
    // http://developer.android.com/guide/topics/manifest/application-element.html#largeHeap
    [Application(LargeHeap = true)]
    public class MainApplication : Application
    {
        static string LOG_TAG = typeof(MainApplication).Name;

        // TODO Add the Scanbot SDK license key here.
        // Please note: The Scanbot SDK will run without a license key for one minute per session!
        // After the trial period is over all Scanbot SDK functions
        // as well as the UI components will stop working or may be terminated.
        // You can get an unrestricted "no-strings-attached" 30 day trial license key for free.
        // Please submit the trial license form (https://scanbot.io/en/sdk/demo/trial) on our website
        // by using the app identifier "io.scanbot.example.sdk.xamarin.forms" of this example app.
        const string LICENSE_KEY = null;


        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public override void OnCreate()
        {
            base.OnCreate();

            Log.Debug(LOG_TAG, "Initializing Scanbot SDK...");

            // Initialization with a custom, public(!) "StorageBaseDirectory"
            // for demo purposes - see comments below!
            var configuration = new SBSDKConfiguration
            {
                EnableLogging = true,
                // If no StorageBaseDirectory is specified, the default will be used
                StorageBaseDirectory = GetDemoStorageBaseDirectory()
            };
            SBSDKInitializer.Initialize(this, LICENSE_KEY, configuration);
        }

        string GetDemoStorageBaseDirectory()
        {
            /** !!Please note!!

             * In this demo app we overwrite the "StorageBaseDirectory"
             * of the Scanbot SDK by a custom public (!) storage directory.
             * "GetExternalFilesDir" returns an external, public (!) storage directoy.
             * All image files as well export files (PDF, TIFF, etc) created
             * by the Scanbot SDK in this demo app will be stored in a sub-folder
             * of this storage directory and will be accessible
             * for every(!) app having external storage permissions!

             * We use the "ExternalStorageDirectory" here only for demo purposes,
             * to be able to share generated PDF and TIFF files.
             * (also see the example code for PDF and TIFF creation).

             * If you need a secure storage for all images
             * and export files (which is strongly recommended):
                - Use the default settings of the Scanbot SDK (don't overwrite
                  the "StorageBaseDirectory" config parameter above)
                - Set a suitable custom internal (!) StorageBaseDirectory.

             * For more detais about the Android file system see:
                - https://developer.android.com/guide/topics/data/data-storage
                - https://docs.microsoft.com/en-us/xamarin/android/platform/files/
            */
            var directory = GetExternalFilesDir(null).AbsolutePath;
            var externalPublicPath = Path.Combine(directory, "my-custom-storage");
            Directory.CreateDirectory(externalPublicPath);
            return externalPublicPath;
        }
    }
}
