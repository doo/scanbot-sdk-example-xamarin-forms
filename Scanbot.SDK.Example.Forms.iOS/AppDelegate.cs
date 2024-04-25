﻿using System;
using System.IO;

using Foundation;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
using UIKit;

namespace Scanbot.SDK.Example.Forms.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        // TODO Add the Scanbot SDK license key here.
        // Please note: The Scanbot SDK will run without a license key for one minute per session!
        // After the trial period is over all Scanbot SDK functions as well as the UI components
        // will stop working or may be terminated. You can get an unrestricted
        // "no-strings-attached" 30 day trial license key for free.
        // Please submit the trial license form (https://scanbot.io/en/sdk/demo/trial)
        // on our website by using the app identifier
        // "io.scanbot.example.sdk.xamarin.forms" of this example app.
        const string LICENSE_KEY = null;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            ImagePicker.Forms.iOS.DependencyManager.Register();

            Console.WriteLine("Scanbot SDK Example: Initializing Scanbot SDK...");
            var configuration = new SBSDKConfiguration
            {
                EnableLogging = true,
                StorageBaseDirectory = GetDemoStorageBaseDirectory(),
                DetectorType = DocumentDetectorType.MLBased,
                // Uncomment the below to test our encyption functionality.
                //Encryption = new SBSDKEncryption
                //{
                //    Mode = EncryptionMode.AES256,
                //    Password = "S0m3W3irDL0ngPa$$w0rdino!!!!"
                //}
                // Note: all the images and files exported through the SDK will
                // not be openable from external applications, since they will be
                // encrypted.
            };

            SBSDKInitializer.Initialize(app, LICENSE_KEY, configuration);
            App.IsEncryptionEnabled = configuration.Encryption != null;

            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        string GetDemoStorageBaseDirectory()
        {
            // For demo purposes we use a sub-folder in the Documents folder in the
            // Data Container of this App, since the contents can be shared via iTunes.
            // For more detais about the iOS file system see:
            // - https://developer.apple.com/library/archive/documentation/FileManagement/Conceptual/FileSystemProgrammingGuide/FileSystemOverview/FileSystemOverview.html
            // - https://docs.microsoft.com/en-us/xamarin/ios/app-fundamentals/file-system
            // - https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder

            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var customDocumentsFolder = Path.Combine(documents, "my-custom-storage");
            Directory.CreateDirectory(customDocumentsFolder);

            return customDocumentsFolder;
        }
    }
}
