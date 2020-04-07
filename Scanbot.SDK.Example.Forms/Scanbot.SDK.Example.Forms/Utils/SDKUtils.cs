using System;
using System.Text;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class SDKUtils
    {
        public static bool CheckLicense(ContentPage context)
        {
            if (!SBSDK.Operations.IsLicenseValid)
            {
                ViewUtils.Alert(context, "Oops!", "License expired or invalid");
            }
            return SBSDK.Operations.IsLicenseValid;
        }

        public static string ParseWorkflowResult(IWorkflowStepResult result)
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
                if (mrzResult.MachineReadableZone != null
                    && mrzResult.MachineReadableZone.Fields != null)
                {
                    foreach (var field in mrzResult.MachineReadableZone.Fields)
                    {
                        sb.AppendFormat("{0}: {1}\n", field.Name, field.Value);
                    }
                    sb.AppendFormat("Valid check digits: {0}/{1}\n",
                        mrzResult.MachineReadableZone.ValidCheckDigitsCount,
                        mrzResult.MachineReadableZone.CheckDigitsCount);
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

        public static string ParseWorkflowResults(IWorkflowStepResult[] results)
        {
            var builder = new StringBuilder();

            foreach(var result in results)
            {
                builder.Append(ParseWorkflowResult(result));
            }
            return builder.ToString();
        }
    }
}
