using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                ViewUtils.CreateCell("BASIC DATA DETECTOR", ScanningUIClicked())
            });
            table.Root.Add(new TableSection("WORKFLOWS")
            {
                ViewUtils.CreateCell("Scan MRZ + Image", WorkflowMRZClicked()),
                ViewUtils.CreateCell("Scan MRZ + Front and Back Image", WorkFlowMRZFrontBackClicked()),
                ViewUtils.CreateCell("Scan QR Code and Document Image", WorkflowQRClicked()),
                ViewUtils.CreateCell("Scan Disability Certificate", WorkflowDCClicked()),
                ViewUtils.CreateCell("Scan Payform", WorkflowPayformClicked())
            });

            Content = Container;
        }

        IScannedPage _selectedPage;
        public IScannedPage SelectedPage
        {
            get => _selectedPage;
            set
            {
                _selectedPage = value;
                Image.Source = _selectedPage.Original;
            }
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

        private EventHandler WorkflowMRZClicked()
        {
            return async (sender, e) =>
            {
            };
        }

        private EventHandler WorkFlowMRZFrontBackClicked()
        {
            return async (sender, e) =>
            {
            };
        }

        private EventHandler WorkflowQRClicked()
        {
            return async (sender, e) =>
            {
            };
        }

        private EventHandler WorkflowDCClicked()
        {
            return async (sender, e) =>
            {
            };
        }

        private EventHandler WorkflowPayformClicked()
        {
            return async (sender, e) =>
            {
                var workflow = SBSDK.UI.CreateWorkflow();
                workflow.AddScanPayFormStep(
                    title: "PayForm Scanner",
                    message: "Please scan a SEPA PayForm",
                    resultValidationHandler: (o, args) =>
                    {
                        var result = args.Result as IWorkflowPayFormResult;
                        if (result.PayForm.RecognizedFields.Count == 0)
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

        private async Task RunWorkflow(IWorkflow workflow)
        {
            var config = new WorkflowScannerConfiguration
            {
                IgnoreBadAspectRatio = true,
                BottomBarBackgroundColor = Color.Blue,
            };
            var result = await SBSDK.UI.LaunchWorkflowScannerAsync(workflow, config);
            if (result.Status == OperationResult.Ok)
            {
                var results = result.Results;
                Console.WriteLine(results);
            }
        }

    }
}
