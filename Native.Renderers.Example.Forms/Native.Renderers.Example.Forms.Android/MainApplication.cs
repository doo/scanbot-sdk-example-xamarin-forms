using System;
using Android.App;
using Android.Runtime;
using Android.Util;
using Native.Renderers.Example.Forms.Common;
using ScanbotSDK.Xamarin.Android;
using ScanbotSDK.Xamarin.Forms;

namespace Native.Renderers.Example.Forms.Droid
{
#if DEBUG
    [Application(Debuggable = true, LargeHeap = true, Theme = "@style/MainTheme")]
#else
    [Application(Debuggable = false, LargeHeap = true, Theme = "@style/MainTheme")]
#endif
    public class MainApplication : Application
    {

        static readonly string LOG_TAG = typeof(MainApplication).Name;

        // Use a custom temp storage directory for demo purposes.
        public static TempImageStorage TempImageStorage;

        // Initializers
        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }
        public MainApplication() { }

        // Application Lifecycle
        public override void OnCreate()
        {
            base.OnCreate();

            TempImageStorage = new TempImageStorage();

            Log.Debug(LOG_TAG, "Initializing Scanbot SDK...");
            var config = new ScanbotSDK.Xamarin.Forms.SBSDKConfiguration { EnableLogging = ScanbotSDKConfiguration.IS_DEBUG };
            SBSDKInitializer.Initialize(this, ScanbotSDKConfiguration.LICENSE_KEY, config);

            // In this example we always cleanup the demo temp storage directory on app start
            TempImageStorage.CleanUp();
        }
    }
}
