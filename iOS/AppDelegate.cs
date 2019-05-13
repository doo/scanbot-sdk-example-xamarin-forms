using System;
using System.IO;
using Foundation;
using ScanbotSDK.Xamarin.Forms;
using UIKit;

namespace scanbotsdkexamplexamarinforms.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        // TODO Add your Scanbot SDK license key here.
        // You can test all Scanbot SDK features and develop your app without a license. 
        // However, if you do not specify the license key when initializing the SDK, 
        // it will work in trial mode (trial period of 1 minute). 
        // To get another trial period you have to restart your app.
        //const string licenseKey = null;

        // limited trial key
        const string licenseKey =
              "CIEW07uSHTMw2XOB6FOIHutqwKn4JM" +
              "YNcJgoBzhcJwSi1iHZY8e+eSEZdHpU" +
              "5ANOko27SCzgN9Lv5hrDWVSYuwqyoO" +
              "GSDUaPZ2u7bJq4L9qoVy4oGRtznZs1" +
              "LysMnl4pdLxNiJsAAW4tVpBm0IC1c8" +
              "ECn1DBbqTuc1fEaj9imdmBJBOifDqX" +
              "UL2IguJD19+4I6XxJuatAJ+ewEb8JZ" +
              "BTDRpgqi1Npjabsg4bX6i2x3Zf30Iv" +
              "Kc/VgvmMv9FvnIpR1qJ94PwppKRgjB" +
              "f95ZZKlCZn+gErLMybb4Vv1FNGjz0v" +
              "mKmrB1B1OXs6hqSd6GcVogiId6hdLx" +
              "sxlsQFhedxHw==\nU2NhbmJvdFNESw" +
              "ppby5zY2FuYm90LmV4YW1wbGUuc2Nh" +
              "bmJvdC1zZGstZXhhbXBsZS14YW1hcm" +
              "luLWZvcm1zCjE1NjAxMjQ3OTkKMTMx" +
              "MDcxCjE=\n";

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Console.WriteLine("Scanbot SDK Example: Initializing Scanbot SDK...");

            // Initialization with a custom "StorageBaseDirectory" for demo purposes - see comments below.
            SBSDKInitializer.Initialize(app, licenseKey, new SBSDKConfiguration { EnableLogging = true, StorageBaseDirectory = GetDemoStorageBaseDirectory() });

            // Alternative initialization with the default "StorageBaseDirectory".
            //SBSDKInitializer.Initialize(app, licenseKey, new SBSDKConfiguration { EnableLogging = true });

            global::Xamarin.Forms.Forms.Init();

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        string GetDemoStorageBaseDirectory()
        {
            // For demo purposes we use a sub-folder in the Documents folder in the Data Container of this App, since the contents can be shared via iTunes.
            // For more detais about the iOS file system see:
            // - https://developer.apple.com/library/archive/documentation/FileManagement/Conceptual/FileSystemProgrammingGuide/FileSystemOverview/FileSystemOverview.html
            // - https://docs.microsoft.com/en-us/xamarin/ios/app-fundamentals/file-system
            // - https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder

            var customDocumentsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "my-custom-storage");
            Directory.CreateDirectory(customDocumentsFolder);
            return customDocumentsFolder;
        }
    }
}
