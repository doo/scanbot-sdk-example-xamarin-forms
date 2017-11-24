using scanbotsdkexamplexamarinforms.Services;
using Xamarin.Forms;
using scanbotsdkexamplexamarinforms.iOS;
using scanbotsdkexamplexamarinforms.iOS.ViewControllers;
using UIKit;
using ScanbotSDK.Xamarin.iOS.Wrapper;
using System;
using Foundation;
using ScanbotSDK.iOS;
using System.Threading.Tasks;
using System.IO;

[assembly: Dependency(typeof(IOSScanbotSdkFeatureService))]

namespace scanbotsdkexamplexamarinforms.iOS
{
    public class IOSScanbotSdkFeatureService : IScanbotSdkFeatureService
    {

        #region IScanbotSdkFeatureService implementation

        public void StartScanningUi()
        {
            var cameraViewController = new CameraDemoViewController();
            cameraViewController.cameraDelegate = cameraHandler;
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(cameraViewController, true, null);
        }

        public void StartOcrService()
        {
            var images = TempImageStorage.GetImages();
            if (images.Length == 0)
            {
                DebugLog("No images provided. Please snap some images via Camera UI.");
                return;
            }

            string[] ocrLangugages = { "de", "en" };

            var docsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var demoOutputFolder = Path.Combine(docsFolder, "scanbot-sdk-example-xamarin-forms");
            Directory.CreateDirectory(demoOutputFolder);
            var targetFile = Path.Combine(demoOutputFolder, new NSUuid().AsString().ToLower() + ".pdf");
            var pdfOutputFileURL = NSUrl.FromFilename(targetFile);

            Task.Run(() =>
            {
                try
                {
                    var ocrText = PerformOCR(images, ocrLangugages, pdfOutputFileURL);
                    DebugLog("Recognized OCR text: " + ocrText);
                    DebugLog("Sandwiched PDF file created: " + pdfOutputFileURL);
                }
                catch (Exception e)
                {
                    DebugLog("Error performing OCR: " + e.StackTrace);
                }
            });
        }

        #endregion



        CameraDemoDelegateHandler cameraHandler = new CameraDemoDelegateHandler();

        static TempImageStorage TempImageStorage = new TempImageStorage();

        class CameraDemoDelegateHandler : CameraDemoDelegate
        {
            public override void DidCaptureDocumentImage(UIImage documentImage)
            {
                TempImageStorage.AddImage(documentImage);
            }

            public override void DidCaptureOriginalImage(UIImage originalImage)
            {
                // TODO
            }
        }


        string PerformOCR(NSUrl[] images, string[] languages, NSUrl pdfOutputFileURL = null)
        {
            DebugLog("Performing OCR on images");
            SBSDKImageStorage storage = new SBSDKImageStorage();
            foreach (NSUrl url in images)
            {
                storage.AddImage(ImageUtils.LoadImage(url));
            }
            NSIndexSet indexSet = NSIndexSet.FromNSRange(NSMakeRange(0, images.Length));

            // transform languages array to one string e.g. ["en", "de"] => "en+de"
            string languageString = string.Join("+", languages);
            DebugLog("Language string: " + languageString);

            NSError error = null;

            SBSDKOCRResult result = SBSDKOpticalTextRecognizer.RecognizeText(storage, indexSet, languageString, pdfOutputFileURL, out error);

            if (error != null)
            {
                DebugLog("Error in OCR: " + error.LocalizedDescription);
                throw new NSErrorException(error);
            }

            return result.RecognizedText;
        }

        NSRange NSMakeRange(int location, int length)
        {
            NSRange newRange = new NSRange();
            newRange.Length = length;
            newRange.Location = location;
            return newRange;
        }

        void DebugLog(string msg)
        {
            Console.WriteLine("Scanbot SDK Example: " + msg);
        }

    }
}
