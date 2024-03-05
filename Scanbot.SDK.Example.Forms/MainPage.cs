using System;
using System.Collections.Generic;
using System.Linq;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class MainPage : ContentPage
    {
        StackLayout Container { get; set; }

        public MainPage()
        {
            BackgroundColor = Color.White;

            Title = "SCANBOT SDK EXAMPLE";

            Container = new StackLayout();
            Container.Orientation = StackOrientation.Vertical;
            Container.BackgroundColor = Color.White;

            var table = new TableView();
            table.BackgroundColor = Color.White;
            Container.Children.Add(table);

            table.Root = new TableRoot();

            table.Root.Add(new TableSection("DOCUMENT SCANNER")
            {
                ViewUtils.CreateCell("Scan Document", ScanningUIClicked),
                ViewUtils.CreateCell("Import image & Detect Document", ImportButtonClicked),
                ViewUtils.CreateCell("View Image Results", ViewImageResultsClicked),
            });
            table.Root.Add(new TableSection("BARCODE DETECTOR")
            {
                ViewUtils.CreateCell("Scan QR- & Barcodes", BarcodeScannerClicked),
                ViewUtils.CreateCell("Scan Multiple QR- & Barcodes", BatchBarcodeScannerClicked),
                ViewUtils.CreateCell("Import Image & Detect Barcodes", ImportandDetectBarcodesClicked),
                 ViewUtils.CreateCell("Import images & Detect Barcodes", ImportImagesAndDetectBarcodesTapped),
                ViewUtils.CreateCell("Set Barcode Formats Filter", SetBarcodeFormatsFilterClicked),
            });
            table.Root.Add(new TableSection("DATA DETECTORS")
            {
                ViewUtils.CreateCell("MRZ Scanner", MRZScannerClicked),
                ViewUtils.CreateCell("EHIC Scanner", EHICScannerClicked),
                ViewUtils.CreateCell("Generic Document Recognizer", GenericDocumentRecognizerClicked),
                ViewUtils.CreateCell("Check Recognizer", CheckRecognizerClicked),
                ViewUtils.CreateCell("Text Data Scanner", TextDataScannerClicked),
                ViewUtils.CreateCell("Vin Data Scanner", VinDataScannerClicked),
            });
            table.Root.Add(new TableSection("MISCELLANEOUS")
            {
                ViewUtils.CreateCell("View License Info", ViewLicenseInfoClicked),
                ViewUtils.CreateCell("Learn more about Scanbot SDK", LearnMoreClicked, App.ScanbotRed),
                ViewUtils.CreateCopyrightCell()
            });

            Content = Container;
        }

        /**
         * DOCUMENT SCANNER
         */
        async void ScanningUIClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var openCamera = await SDKUtils.CheckCameraPermisions();

            if (!openCamera)
                return;

            var configuration = new DocumentScannerConfiguration
            {
                CameraPreviewMode = CameraPreviewMode.FitIn,
                IgnoreBadAspectRatio = true,
                MultiPageEnabled = true,
                PolygonColor = Color.Red,
                PolygonColorOK = Color.Green,
                BottomBarBackgroundColor = Color.Blue,
                PageCounterButtonTitle = "%d Page(s)",
                //DocumentImageSizeLimit = new Size(2000, 3000),
                // see further customization configs...
            };
            var result = await SBSDK.UI.LaunchDocumentScannerAsync(configuration);
            if (result.Status == OperationResult.Ok)
            {
                foreach (var page in result.Pages)
                {
                    await Pages.Instance.Add(page);

                    // If encryption is enabled, load the decrypted document.
                    // Else accessible via page.Document
                    var blur = await SBSDK.Operations.EstimateBlurriness(await page.DecryptedDocument());
                    //var blur = await SBSDK.Operations.EstimateBlurriness(page.Document);
                    Console.WriteLine("Estimated blurriness for detected document: " + blur);
                }

                await Navigation.PushAsync(new ImageResultsPage());
            }
        }

        async void ImportButtonClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            ImageSource source = await ImagePicker.Forms.ImagePicker.Instance.Pick();
            if (source != null)
            {
                // Import the selected image as original image and create a Page object
                var importedPage = await SBSDK.Operations.CreateScannedPageAsync(source);

                // Run document detection on it
                await importedPage.DetectDocumentAsync();
                await Pages.Instance.Add(importedPage);
                await Navigation.PushAsync(new ImageResultsPage());
            }
        }

        void ViewImageResultsClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }
            Navigation.PushAsync(new ImageResultsPage());
        }

        /**
         * BARCODE DETECTOR
         */
        async void BarcodeScannerClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var config = new BarcodeScannerConfiguration();
            config.BarcodeFormats = BarcodeTypes.Instance.AcceptedTypes;
            config.ConfirmationDialogConfiguration = GetConfirmationDialog();
            config.OverlayConfiguration = GetSelectionOverlayConfig();
            var result = await SBSDK.UI.LaunchBarcodeScannerAsync(config);
            if (result.Status == OperationResult.Ok)
            {
                if (result.Barcodes.Count == 0)
                {
                    ViewUtils.Alert(this, "Oops!", "No barcodes found, please try again");
                    return;
                }

                var source = result.Image;
                var barcodes = result.Barcodes;

                await Navigation.PushAsync(new BarcodeResultsPage(source, barcodes));
            }
        }

        async void BatchBarcodeScannerClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }
            var config = new BatchBarcodeScannerConfiguration();
            config.BarcodeFormats = BarcodeTypes.Instance.AcceptedTypes;
            config.OverlayConfiguration = GetSelectionOverlayConfig();
            var result = await SBSDK.UI.LaunchBatchBarcodeScannerAsync(config);
            if (result.Status == OperationResult.Ok)
            {
                if (result.Barcodes.Count == 0)
                {
                    ViewUtils.Alert(this, "Oops!", "No barcodes found, please try again");
                    return;
                }

                await Navigation.PushAsync(new BarcodeResultsPage(null, result.Barcodes));
            }
        }

        async void ImportandDetectBarcodesClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }
            ImageSource source = await Scanbot.ImagePicker.Forms.ImagePicker.Instance.Pick();

            if (source != null)
            {
                var barcodes = await SBSDK.Operations.DetectBarcodesFrom(source);
                await Navigation.PushAsync(new BarcodeResultsPage(source, barcodes));
            }
        }

        void SetBarcodeFormatsFilterClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }
            Navigation.PushAsync(new BarcodeSelectorPage());
        }

        /**
         * WORKFLOWS
         */
        async void MRZScannerClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            MrzScannerConfiguration configuration = new MrzScannerConfiguration
            {
                FinderWidthRelativeToDeviceWidth = 5,
                FinderHeightRelativeToDeviceWidth = 1,
            };

            var result = await SBSDK.UI.LaunchMrzScannerAsync(configuration);
            if (result.Status == OperationResult.Ok)
            {
                var message = SDKUtils.ParseMRZResult(result);
                ViewUtils.Alert(this, "MRZ Scanner result", message);
            }
        }

        async void EHICScannerClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var configuration = new HealthInsuranceCardConfiguration { };
            var result = await SBSDK.UI.LaunchHealthInsuranceCardScannerAsync(configuration);
            if (result.Status == OperationResult.Ok)
            {
                var message = SDKUtils.ParseEHICResult(result);
                ViewUtils.Alert(this, "MRZ Scanner result", message);
            }
        }

        async void GenericDocumentRecognizerClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var configuration = new GenericDocumentRecognizerConfiguration
            {
                DocumentType = GenericDocumentType.DeIdCard
            };
            var result = await SBSDK.UI.LaunchGenericDocumentRecognizerAsync(configuration);
            if (result.Status == OperationResult.Ok)
            {
                var message = SDKUtils.ParseGDRResult(result);
                ViewUtils.Alert(this, "GDR Result", message);
            }
        }

        async void CheckRecognizerClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var configuration = new CheckRecognizerConfiguration
            {
                AcceptedCheckStandards = new List<CheckStandard>() {
                    CheckStandard.USA,
                    CheckStandard.AUS,
                    CheckStandard.IND,
                    CheckStandard.FRA,
                    CheckStandard.KWT,
                }
            };

            var result = await SBSDK.UI.LaunchCheckRecognizerAsync(configuration);
            if (result.Status == OperationResult.Ok)
            {
                var message = SDKUtils.ParseCheckResult(result);
                ViewUtils.Alert(this, "Check Result", message);
            }

        }

        async void TextDataScannerClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var step = new TextDataScannerStep("Scan your text", "", 1.0f, new AspectRatio(3, 1));
            var configuration = new TextDataScannerConfiguration(step)
            {
                CancelButtonTitle = "Cancel",
                FlashEnabled = false,
            };

            var result = await SBSDK.UI.LaunchTextDataScannerAsync(configuration);
            if (result.Status == OperationResult.Ok)
            {
                var message = SDKUtils.ParseTextDataScannerResult(result);
                ViewUtils.Alert(this, "Text Data Scanner Result", message);
            }
        }

        void ViewLicenseInfoClicked(object sender, EventArgs e)
        {
            bool valid = SBSDK.Operations.IsLicenseValid;
            var message = "Scanbot SDK License is valid";
            if (!valid)
            {
                message = "Scanbot SDK License is expired";
            }
            ViewUtils.Alert(this, "License info", message);
        }

        async void LearnMoreClicked(object sender, EventArgs e)
        {
            var uri = new Uri("https://scanbot.io/sdk");
            await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }

        /// <summary>
        /// Import images and detect barcodes from all the images.
        /// Navigates all the barcode result list to the next page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        void ImportImagesAndDetectBarcodesTapped(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }
            DependencyService.Get<IMultiImagePicker>().PickPhotosAsync(completionHandler: async (imageSources) =>
            {
                List<Barcode> barcodes = null;
                bool canNavigate = false;
                var filteredImageSource = imageSources.Where(source => !source.IsEmpty)?.ToList() ?? new List<ImageSource>();
                if (filteredImageSource.Count > 0)
                {
                    canNavigate = true;
                    barcodes = await SBSDK.Operations.DetectBarcodesFrom(filteredImageSource);
                }

                if (imageSources == null || imageSources.Any(source => source.IsEmpty))
                {
                    await DisplayAlert("Alert", "Unable to pick at least 1 of the images.", "Ok");
                }

                if (canNavigate)
                {
                    await Navigation.PushAsync(new BarcodeResultsPage(barcodes));
                }
            });
        }

        private SelectionOverlayConfiguration GetSelectionOverlayConfig()
        {
            var config = new SelectionOverlayConfiguration(false, OverlayFormat.Code,
                Color.Yellow, Color.Yellow, Color.Black,
                Color.Red, Color.Red, Color.Black);
            return config;
        }

        private async void VinDataScannerClicked(object sender, EventArgs e)
        {
            var configuration = new VINScannerConfiguration();
            configuration.FinderAspectRatio = new AspectRatio(7, 1);
            configuration.GuidanceText = "Please place the number inside finder area.";
            var result = await SBSDK.UI.LaunchVINScannerAsync(configuration);
            if (result?.Status == OperationResult.Ok && result?.ValidationSuccessful == true)
            {
                var message = result.Text;
                ViewUtils.Alert(this, "VIN Scanner result", message);
            }
        }

         /// <summary>
        /// Init the Confirmation Dialog.
        /// </summary>
        /// <returns></returns>
        private BarcodeConfirmationDialogConfiguration GetConfirmationDialog()
        {
            return new BarcodeConfirmationDialogConfiguration
            {
                Title = "Confirmation Dialog",
                Message = "Your barcode is scanned.",
                ConfirmButtonTitle = "Confirm",
                RetryButtonTitle = "Retry",
                DialogTextFormat = BarcodeDialogFormat.TypeAndCode,
                ResultWithConfirmation = true,
            };
        }
    }
}
