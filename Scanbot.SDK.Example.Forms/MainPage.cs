using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScanbotSDK.Xamarin;
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
                ViewUtils.CreateCell("Import Image & Detect Barcodes", ImportandDetectBarcodesClicked),
                ViewUtils.CreateCell("Set Barcode Formats Filter", SetBarcodeFormatsFilterClicked),
            });
            table.Root.Add(new TableSection("DATA DETECTORS")
            {
                ViewUtils.CreateCell("MRZ Scanner", MRZScannerClicked),
                ViewUtils.CreateCell("EHIC Scanner", EHICScannerClicked),
            });
            table.Root.Add(new TableSection("WORKFLOWS")
            {
                ViewUtils.CreateCell("Scan MRZ + Image", WorkflowMRZClicked),
                ViewUtils.CreateCell("Scan QR Code and Document Image", WorkflowQRClicked),
                ViewUtils.CreateCell("Scan Disability Certificate", WorkflowDCClicked),
                ViewUtils.CreateCell("Scan Payform", WorkflowPayformClicked)
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
            //config.BarcodeFormats = new List<BarcodeFormat> { BarcodeFormat.UpcA };
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
                FinderWidthRelativeToDeviceWidth = 0.95,
                FinderHeightRelativeToDeviceWidth = 0.2,
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

        async void WorkflowMRZClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var workflow = SBSDK.UI.CreateWorkflow();
            var ratios = new[] {
                    new AspectRatio(85.0, 54.0), // ID card
                    new AspectRatio(125.0, 88.0) // Passport
                };

            workflow.AddScanMachineReadableZoneStep(
                title: "Scan ID card or passport",
                message: "Please align your ID card or passport in the frame.",
                requiredAspectRatios: ratios,
                resultValidationHandler: (o, args) =>
                {
                    var result = args.Result as IWorkflowMachineReadableZoneResult;
                    if (result.MachineReadableZone == null
                    || result.MachineReadableZone.CheckDigitsCount == 0)
                    {
                        var message = "Recognition was not successful. " +
                        "Please try again and scan the side with MRZ area.";
                        args.SetError(message, ValidationErrorShowMode.Alert);
                        return;
                    }
                        // run some additional validations here
                        //result.MachineReadableZone.Fields...
                    }
            );

            await RunWorkflow(workflow);
        }

        async void WorkflowQRClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var workflow = SBSDK.UI.CreateWorkflow();
            workflow.AddScanBarcodeStep(
                "Scan Step 1/2", "Please scan a QR code.",
                new[] { BarcodeFormat.QrCode }, new AspectRatio(1.0, 1.0)
            );

            workflow.AddScanDocumentPageStep(
                "Scan Step 2/2",
                "Please scan a document.");

            await RunWorkflow(workflow);
        }

        async void WorkflowDCClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var workflow = SBSDK.UI.CreateWorkflow();
            var ratios = new[] {
                    // DC form A5 portrait (e.g. white sheet, AUB Muster 1b/E (1/2018))
                    new AspectRatio(148.0, 210.0),
                    // DC form A6 landscape (e.g. yellow sheet, AUB Muster 1b (1.2018))
                    new AspectRatio(148.0, 105.0)
                };

            workflow.AddScanDisabilityCertificateStep(
                title: "Scan Disability Certificate",
                message: "Please align the DC form in the frame.",
                requiredAspectRatios: ratios,
                resultValidationHandler: (o, args) =>
                {
                    var result = args.Result as IWorkflowDisabilityCertificateResult;
                    if (!result.DisabilityCertificate.RecognitionSuccessful)
                    {
                        string message = "Could not extract data. Please try again.";
                        args.SetError(message, ValidationErrorShowMode.Alert);
                        return;
                    }
                        // run some additional validations here
                        //result.DisabilityCertificate.Dates....
                        //result.DisabilityCertificate.Checkboxes...
                    }
            );
            await RunWorkflow(workflow);
        }

        async void WorkflowPayformClicked(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }

            var workflow = SBSDK.UI.CreateWorkflow();
            workflow.AddScanPayFormStep(
                title: "PayForm Scanner",
                message: "Please scan a SEPA PayForm",
                resultValidationHandler: (o, args) =>
                {
                    var result = args.Result as IWorkflowPayFormResult;
                    if (result.PayForm == null || result.PayForm.RecognizedFields.Count == 0)
                    {
                        args.SetError("Recognition was not successful. " +
                            "Please try again.", ValidationErrorShowMode.Alert);
                        return;
                    }
                        // run some additional validations here
                        //result.PayForm.RecognizedFields...
                    }
            );
            await RunWorkflow(workflow);
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

        async Task RunWorkflow(IWorkflow workflow)
        {
            var config = new WorkflowScannerConfiguration
            {
                IgnoreBadAspectRatio = true,
            };
            var result = await SBSDK.UI.LaunchWorkflowScannerAsync(workflow, config);
            if (result.Status == OperationResult.Ok)
            {
                var results = result.Results;

                ViewUtils.Alert(this, "Result:", SDKUtils.ParseWorkflowResults(results));

                foreach (var item in results)
                {
                    // Not all StepResults contain a captured page, try to find the one that has it
                    if (item.CapturedPage != null)
                    {
                        Pages.Instance.List.Add(item.CapturedPage);
                    }
                }
            }
        }

    }
}
