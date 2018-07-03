using System;
using Foundation;
using ScanbotSDK.Xamarin.Forms;
using UIKit;

namespace scanbotsdkexamplexamarinforms.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        // Add your Scanbot SDK license key here.
        // You can test all Scanbot SDK features and develop your app without a license. 
        // However, if you do not specify the license key when initializing the SDK, 
        // it will work in trial mode (trial period of 1 minute). 
        // To get another trial period you have to restart your app.
        const string licenseKey = null;
        
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Console.WriteLine("Scanbot SDK Example: Initializing Scanbot SDK...");
            SBSDKInitializer.Initialize(app, licenseKey, new SBSDKConfiguration { EnableLogging = true });

            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
