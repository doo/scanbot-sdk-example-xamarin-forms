using System;
using System.IO;
using Android.App;
using Android.Runtime;
using Android.Util;
using ScanbotSDK.Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms.Droid
{
    // It is strongly recommended to add the LargeHeap = true flag in your Application class.
    // Working with images, creating PDFs, etc. are memory intensive tasks. So to prevent OutOfMemoryError, consider adding this flag!
    // For more details see: http://developer.android.com/guide/topics/manifest/application-element.html#largeHeap
    [Application(LargeHeap = true)]
    public class MainApplication : Application
    {
        static string LOG_TAG = typeof(MainApplication).Name;

        // TODO Add your Scanbot SDK license key here.
        // You can test all Scanbot SDK features and develop your app without a license. 
        // However, if you do not specify the license key when initializing the SDK, 
        // it will work in trial mode (trial period of 1 minute). 
        // To get another trial period you have to restart your app.
        //const string licenseKey = null;

        // limited trial key
        const string licenseKey =
              "ToNwgEItnq6POs/fCbMlXxJ+CiEG3y" +
              "m/p4zxOnydigb3IJaJ1mcFc0M7Uytx" +
              "wfCerGJFsmtFXaLJAX/dlkCg00/hMj" +
              "859KG1WTvuI4fnxHKyolYcG6HUVn+G" +
              "n/uw3DaAN5aw3ScLTAAXHWQpubrRgy" +
              "wEHtW6LGNKlr6+Cr6g6s//3oO2ulqc" +
              "3f8LxtVRoJOLi9GHH7gysFDgjGSSHG" +
              "Nd7d99OaoRQeqJFoo5POOcKSPJnP9H" +
              "M8NNGI0P0WtQp+d6ABh61triMT4q5P" +
              "VR8rk9OSYP+El9MLEtDNxlKRpJcqfB" +
              "Cy93R/JtlMz4sUQk9M1WLB2eDyAIcw" +
              "zv9Ixj3LEs+A==\nU2NhbmJvdFNESw" +
              "ppby5zY2FuYm90LmV4YW1wbGUuc2Nh" +
              "bmJvdF9zZGtfZXhhbXBsZV94YW1hcm" +
              "luX2Zvcm1zCjE1NjAxMjQ3OTkKMTMx" +
              "MDcxCjI=\n";

        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        { }

        public override void OnCreate()
        {
            base.OnCreate();

            Log.Debug(LOG_TAG, "Initializing Scanbot SDK...");

            // Initialization with a custom, public(!) "StorageBaseDirectory" for demo purposes - see comments below!
            SBSDKInitializer.Initialize(this, licenseKey, new SBSDKConfiguration { EnableLogging = true, StorageBaseDirectory = GetDemoStorageBaseDirectory() });

            // Alternative initialization with the default "StorageBaseDirectory" which will be internal and secure (recommended).
            //SBSDKInitializer.Initialize(this, licenseKey, new SBSDKConfiguration { EnableLogging = true });
        }

        string GetDemoStorageBaseDirectory()
        {
            // !! Please note !!
            // In this demo app we overwrite the "StorageBaseDirectory" of the Scanbot SDK by a custom public(!) storage directory.
            // "GetExternalFilesDir" returns an external, public(!) storage directoy.
            // All image files as well export files (PDF, TIFF, etc) created by the Scanbot SDK in this demo app will be stored 
            // in a sub-folder of this storage directory and will be accessible for every(!) app having external storage permissions!
            // We use the "ExternalStorageDirectory" here only for demo purposes, to be able to share generated PDF and TIFF files. 
            // (also see the example code for PDF and TIFF creation).
            // If you need a secure storage for all images and export files (which is strongly recommended):
            //  - either use the default settings of the Scanbot SDK (don't overwrite the "StorageBaseDirectory" config parameter above)
            //  - or set a suitable custom internal(!) StorageBaseDirectory.
            //
            // For more detais about the Android file system see:
            // - https://developer.android.com/guide/topics/data/data-storage
            // - https://docs.microsoft.com/en-us/xamarin/android/platform/files/

            var externalPublicPath = Path.Combine(GetExternalFilesDir(null).AbsolutePath, "my-custom-storage");
            Directory.CreateDirectory(externalPublicPath);
            return externalPublicPath;
        }
    }
}
