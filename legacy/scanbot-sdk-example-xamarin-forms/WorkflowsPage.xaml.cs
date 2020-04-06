using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public partial class WorkflowsPage : ContentPage
    {
        public class ResultPresenter
        {
            private readonly IWorkflowStepResult result;

            public ResultPresenter(IWorkflowStepResult result)
            {
                this.result = result;
            }

            public ImageSource Image
            {
                get
                {
                    var page = result.CapturedPage ?? result.VideoFramePage;
                    return page?.DocumentPreview ?? page?.OriginalPreview;
                }
            }

            public string Description
            {
                get
                {
                    var sb = new StringBuilder();
                    if (result is IWorkflowBarcodeResult barcodeResult)
                    {
                        foreach (var barcode in barcodeResult.Barcodes)
                        {
                            sb.AppendFormat("{0}\n{1}\n", barcode.Format, barcode.Text);
                        }
                    }
                    if (result is IWorkflowMachineReadableZoneResult mrzResult)
                    {
                        if (mrzResult.MachineReadableZone != null && mrzResult.MachineReadableZone.Fields != null)
                        {
                            foreach (var field in mrzResult.MachineReadableZone.Fields)
                            {
                                sb.AppendFormat("{0}: {1}\n", field.Name, field.Value);
                            }
                            sb.AppendFormat("Valid check digits: {0}/{1}\n", mrzResult.MachineReadableZone.ValidCheckDigitsCount, mrzResult.MachineReadableZone.CheckDigitsCount);
                        }
                        else
                        {
                            sb.AppendLine("MRZ not recognized");
                        }
                    }
                    if (result is IWorkflowPayFormResult payformResult)
                    {
                        if (payformResult.PayForm != null && payformResult.PayForm.RecognizedFields != null)
                        {
                            foreach (var field in payformResult.PayForm.RecognizedFields)
                            {
                                sb.AppendFormat("{0}: {1}\n", field.Token.Type, field.Value);
                            }
                        }
                        else
                        {
                            sb.AppendLine("Payform not recognized");
                        }
                    }
                    if (result is IWorkflowDisabilityCertificateResult dcResult)
                    {
                        if (dcResult.DisabilityCertificate.RecognitionSuccessful)
                        {
                            foreach (var cb in dcResult.DisabilityCertificate.Checkboxes)
                            {
                                sb.AppendFormat("{0}: {1}\n", cb.Type, cb.IsChecked ? "Yes" : "No");
                            }
                            foreach (var date in dcResult.DisabilityCertificate.Dates)
                            {
                                sb.AppendFormat("{0}: {1}\n", date.Type, date.DateString);
                            }
                        }
                        else
                        {
                            sb.AppendLine("DC not recognized");
                        }
                    }
                    return sb.ToString();
                }
            }
        }

        public static readonly BindableProperty ResultsProperty = BindableProperty.Create(
            propertyName: "Results",
            returnType: typeof(IEnumerable<ResultPresenter>),
            declaringType: typeof(WorkflowsPage),
            defaultValue: null);

        public IEnumerable<ResultPresenter> Results
        {
            get { return (IEnumerable<ResultPresenter>)GetValue(ResultsProperty); }
            set { SetValue(ResultsProperty, value); }
        }

        public WorkflowsPage()
        {
            this.BindingContext = this;
            InitializeComponent();
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
                Results = result.Results?.Select(r => new ResultPresenter(r)).ToArray();
            }
        }

        async void HandleScanQrAndDocumentClicked(object sender, EventArgs e)
        {
            var workflow = SBSDK.UI.CreateWorkflow();
            workflow.AddScanBarcodeStep(
                "Scan Step 1/2",
                "Please scan a QR code.",
                new[] { BarcodeFormat.QrCode },
                new Size(1.0, 1.0)
            );

            workflow.AddScanDocumentPageStep(
                "Scan Step 2/2",
                "Please scan a document.");

            await RunWorkflow(workflow);
        }

        async void HandleScanMrzClicked(object sender, EventArgs e)
        {
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
                    if (result.MachineReadableZone == null || result.MachineReadableZone.CheckDigitsCount == 0)
                    {
                        args.SetError("Recognition was not successful. Please try again and scan the side with MRZ area.", ValidationErrorShowMode.Alert);
                        return;
                    }
                    // run some additional validations here
                    //result.MachineReadableZone.Fields...
                }
            );

            await RunWorkflow(workflow);
        }

        async void HandleScanMrzFrontBackClicked(object sender, EventArgs e)
        {
            var workflow = SBSDK.UI.CreateWorkflow();
            var ratios = new[] { new PageAspectRatio(85.0, 54.0) }; // ID card

            workflow.AddScanDocumentPageStep(
                title: "Step 1/2",
                message: "Please scan the front of your ID card.",
                requiredAspectRatios: ratios
            );

            workflow.AddScanMachineReadableZoneStep(
                title: "Step 2/2",
                message: "Please scan the back of your ID card.", 
                requiredAspectRatios: ratios,
                resultValidationHandler: (o, args) =>
                {
                    var result = args.Result as IWorkflowMachineReadableZoneResult;
                    if (result.MachineReadableZone == null || result.MachineReadableZone.CheckDigitsCount == 0)
                    {
                        args.SetError("This does not seem to be the correct side. Please scan the back with MRZ.", ValidationErrorShowMode.Alert);
                        return;
                    }
                    // run some additional validations here
                    //result.MachineReadableZone.Fields...
                }
            );
            await RunWorkflow(workflow);
        }

        async void HandleScanDisabilityCertificateClicked(object sender, EventArgs e)
        {
            var workflow = SBSDK.UI.CreateWorkflow();
            var ratios = new[] {
                new PageAspectRatio(148.0, 210.0), // DC form A5 portrait (e.g. white sheet, AUB Muster 1b/E (1/2018))
                new PageAspectRatio(148.0, 105.0)  // DC form A6 landscape (e.g. yellow sheet, AUB Muster 1b (1.2018))
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
                        args.SetError("Could not extract data. Please try again.", ValidationErrorShowMode.Alert);
                        return;
                    }
                    // run some additional validations here
                    //result.DisabilityCertificate.Dates....
                    //result.DisabilityCertificate.Checkboxes...
                }
            );
            await RunWorkflow(workflow);
        }

        async void HandleScanPayFormClicked(object sender, EventArgs e)
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
                        args.SetError("Recognition was not successful. Please try again.", ValidationErrorShowMode.Alert);
                        return;
                    }
                    // run some additional validations here
                    //result.PayForm.RecognizedFields...
                }
            );
            await RunWorkflow(workflow);
        }
    }
}
