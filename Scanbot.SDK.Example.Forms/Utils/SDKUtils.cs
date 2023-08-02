using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool CheckPage(ContentPage context, IScannedPage page)
        {
            var result = page != null;
            if (!result)
            {
                ViewUtils.Alert(context, "Oops!", "Please select a page");
            }
            return result;
        }

        public static bool CheckDocuments(ContentPage context, IEnumerable<ImageSource> documents)
        {
            var result = documents != null && documents.Count() > 0;
            if (!result)
            {
                ViewUtils.Alert(context, "Oops!", "Please import or scan a document first");
            }
            return result;
        }

        public static string ParseBarcodes(List<Barcode> barcodes)
        {
            var builder = new StringBuilder();

            foreach (var code in barcodes)
            {
                builder.AppendLine($"{code.Format}: {code.Text}");
            }

            return builder.ToString();
        }

        public static string ParseMRZResult(MrzScannerResult result)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"DocumentType: {result.DocumentType}");
            foreach (var field in result.Fields)
            {
                builder.AppendLine($"{field.Name}: {field.Value} ({field.Confidence:F2})");
            }
            return builder.ToString();
        }

        public static string ParseEHICResult(HealthInsuranceCardScannerResult result)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"DocumentType: European Health insurance card");
            foreach (var field in result.Fields)
            {
                builder.AppendLine($"{field.Type}: {field.Value} ({field.Confidence:F2})");
            }
            return builder.ToString();
        }

        public static string ParseGDRResult(GenericDocumentRecognizerResult result)
        {
            var firstDocument = result.Documents.First();
            return string.Join("\n", firstDocument.Fields
                .Where((f) => f != null && f.Type != null && f.Type.Name != null && f.Value != null && f.Value.Text != null)
                .Select((f) => string.Format("{0}: {1}", f.Type.Name, f.Value.Text))
            );
        }

        public static string ParseCheckResult(CheckRecognizerResult result)
        {
            return string.Join("\n", result.Document.Fields
                .Where((f) => f != null && f.Type != null && f.Type.Name != null && f.Value != null && f.Value.Text != null)
                .Select((f) => string.Format("{0}: {1}", f.Type.Name, f.Value.Text))
            );
        }

        public static string ParseTextDataScannerResult(TextDataScannerResult result)
        {
            return string.Format("{0} (confidence: {1})", result.Text, result.Confidence);
        }
    }
}
