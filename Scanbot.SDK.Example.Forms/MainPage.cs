using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.ShareFile;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class MainPage : ContentPage
    {
        StackLayout Container { get; set; }
        Image Image { get; set; }

        public MainPage()
        {
            Title = "SCANBOT SDK EXAMPLE";

            Container = new StackLayout();
            Container.Orientation = StackOrientation.Vertical;
            Container.BackgroundColor = Color.White;

            Image = new Image
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.LightGray,
                Aspect = Aspect.AspectFit
            };
            Image.SizeChanged += delegate
            {
                if (SelectedPage != null) {
                    // Don't allow images larger than a third of the screen
                    Image.HeightRequest = Content.Height / 3;
                }
            };
            Container.Children.Add(Image);

            var table = new TableView();
            table.BackgroundColor = Color.White;
            Container.Children.Add(table);

            table.Root = new TableRoot();
            table.Root.Add(new TableSection("SCAN DOCUMENTS")
            {
                ViewUtils.CreateCell("SCANNING UI", ScanningUIClicked()),
                ViewUtils.CreateCell("IMPORT IMAGE", ImportButtonClicked())
            });
            table.Root.Add(new TableSection("DETECT DATA")
            {
                ViewUtils.CreateCell("Cropping UI", CropClicked()),
                ViewUtils.CreateCell("Perform OCR", PerformOCRClicked()),
                ViewUtils.CreateCell("Apply Image Filter", ApplyImageFilterClicked()),
                ViewUtils.CreateCell("Create PDF", CreatePDFClicked()),
                ViewUtils.CreateCell("Create TIFF (1-bit black&white)", CreateTIFFClicked()),
                ViewUtils.CreateCell("Cleanup", CleanupClick()),
                ViewUtils.CreateCell("Barcode Scanner", BarcodeScannerClicked()),
                ViewUtils.CreateCell("MRZ Scanner", MRZScannerClicked()),
                ViewUtils.CreateCell("EHIC Scanner", EHICScannerClicked()),
            });
            table.Root.Add(new TableSection("WORKFLOWS")
            {
                ViewUtils.CreateCell("Scan MRZ + Image", WorkflowMRZClicked()),
                ViewUtils.CreateCell("Scan QR Code and Document Image", WorkflowQRClicked()),
                ViewUtils.CreateCell("Scan Disability Certificate", WorkflowDCClicked()),
                ViewUtils.CreateCell("Scan Payform", WorkflowPayformClicked())
            });

            Content = Container;
        }

        private EventHandler CropClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }
                if (!SDKUtils.CheckPage(this, SelectedPage)) { return; }

                await SBSDK.UI.LaunchCroppingScreenAsync(SelectedPage);
                UpdateImage();
            };
        }

        private EventHandler PerformOCRClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }
                if (!SDKUtils.CheckPage(this, SelectedPage)) { return; }

                // or specify more languages like { "en", "de", ... }
                var languages = new[] { "en" };
                var result = await SBSDK.Operations.PerformOcrAsync(DocumentSources, languages);
                ViewUtils.Alert(this, "OCR Results", result.Text);
            };
        }

        private EventHandler ApplyImageFilterClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }
                if (!SDKUtils.CheckPage(this, SelectedPage)) { return; }

                var page = new FilterPage(SelectedPage);
                await Application.Current.MainPage.Navigation.PushAsync(page);
            };
        }

        private EventHandler CreatePDFClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                if (DocumentSources == null || DocumentSources.Count() == 0)
                {
                    ViewUtils.Alert(this, "Oops!", "Please import or scan a document first");
                    return;
                }

                var fileUri = await SBSDK.Operations
                .CreatePdfAsync(DocumentSources, PDFPageSize.FixedA4);

                // Please note that on Android sharing works only with public accessible files.
                // Files from internal, secure storage folders cannot be shared.
                // (also see the SDK initialization with external (public) storage)
                CrossShareFile.Current.ShareLocalFile(fileUri.AbsolutePath);
            };
        }

        private EventHandler CreateTIFFClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                var fileUri = await SBSDK.Operations
                .WriteTiffAsync(DocumentSources, new TiffOptions { OneBitEncoded = true });

                // Please note that on Android sharing works only with public accessible files.
                // Files from internal, secure storage folders cannot be shared.
                // (also see the SDK initialization with external (public) storage)
                CrossShareFile.Current.ShareLocalFile(fileUri.AbsolutePath);
            };
        }

        private EventHandler CleanupClick()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                await SBSDK.Operations.CleanUp();
                Pages.Clear();
                SelectedPage = null;

                var message = "Cleanup done. All scanned images " +
                "and generated files (PDF, TIFF, etc) have been removed.";
                ViewUtils.Alert(this, "Cleanup complete!", message);
            };
        }

        private EventHandler BarcodeScannerClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                var config = new BarcodeScannerConfiguration();
                var result = await SBSDK.UI.LaunchBarcodeScannerAsync(config);
                if (result.Status == OperationResult.Ok)
                {
                    if (result.Barcodes.Count == 0)
                    {
                        ViewUtils.Alert(this, "Oops!", "No barcodes found, please try again");
                        return;
                    }

                    var barcode = result.Barcodes[0];

                    var message = SDKUtils.ParseBarcodes(result.Barcodes);
                    ViewUtils.Alert(this, "Barcode Scanner result", message);
                }
            };
        }

        private EventHandler MRZScannerClicked()
        {
            return async (sender, e) =>
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
            };
        }

        private EventHandler EHICScannerClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                var configuration = new HealthInsuranceCardConfiguration { };
                var result = await SBSDK.UI.LaunchHealthInsuranceCardScannerAsync(configuration);
                if (result.Status == OperationResult.Ok)
                {
                    var message = SDKUtils.ParseEHICResult(result);
                    ViewUtils.Alert(this, "MRZ Scanner result", message);
                }
            };
        }

        IEnumerable<ImageSource> DocumentSources
        {
            get => Pages.Select(p => p.Document).Where(image => image != null);
        }
            

        IScannedPage _selectedPage;
        public IScannedPage SelectedPage
        {
            get => _selectedPage;
            set
            {
                _selectedPage = value;
                Image.Source = _selectedPage.Document;
            }
        }

        void UpdateImage()
        {
            Image.Source = SelectedPage.Document;
        }

        public List<IScannedPage> Pages { get; set; } = new List<IScannedPage>();

        EventHandler ScanningUIClicked()
        {
            return async (sender, e) =>
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
                    Pages.Clear();
                    foreach (var page in result.Pages)
                        Pages.Add(page);
                    SelectedPage = Pages[0];
                }
            };
        }

        EventHandler ImportButtonClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                ImageSource source = await ImagePicker.Forms.ImagePicker.Instance.Pick();
                if (source != null)
                {
                    // Import the selected image as original image and create a Page object
                    var importedPage = await SBSDK.Operations.CreateScannedPageAsync(source);
                    // Run document detection on it
                    await importedPage.DetectDocumentAsync();
                    SelectedPage = importedPage;
                    Pages.Add(SelectedPage);
                }
            };
        }

        EventHandler WorkflowMRZClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                var workflow = SBSDK.UI.CreateWorkflow();
                var ratios = new[] {
                    new PageAspectRatio(85.0, 54.0), // ID card
                    new PageAspectRatio(125.0, 88.0) // Passport
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
            };
        }

        EventHandler WorkflowQRClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                var workflow = SBSDK.UI.CreateWorkflow();
                workflow.AddScanBarcodeStep(
                    "Scan Step 1/2", "Please scan a QR code.",
                    new[] { BarcodeFormat.QrCode }, new Size(1.0, 1.0)
                );

                workflow.AddScanDocumentPageStep(
                    "Scan Step 2/2",
                    "Please scan a document.");

                await RunWorkflow(workflow);
            };
        }

        EventHandler WorkflowDCClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                var workflow = SBSDK.UI.CreateWorkflow();
                var ratios = new[] {
                    // DC form A5 portrait (e.g. white sheet, AUB Muster 1b/E (1/2018))
                    new PageAspectRatio(148.0, 210.0),
                    // DC form A6 landscape (e.g. yellow sheet, AUB Muster 1b (1.2018))
                    new PageAspectRatio(148.0, 105.0)
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
            };
        }

        EventHandler WorkflowPayformClicked()
        {
            return async (sender, e) =>
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
            };
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

                // Find the first captured page available and set it as the SelectedPage
                var page = FindPage(results);
                if (page != null)
                {
                    SelectedPage = page;
                }
            }
        }

        IScannedPage FindPage(IWorkflowStepResult[] results)
        {
            foreach (var result in results)
            {
                // Not all StepResults contain a captured page, try to find the one that has it
                if (result.CapturedPage != null)
                {
                    return result.CapturedPage;
                }
            }
            return null;
        }

    }
}
