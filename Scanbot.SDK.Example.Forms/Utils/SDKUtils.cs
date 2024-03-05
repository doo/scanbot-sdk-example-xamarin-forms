using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

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
            foreach (var field in result.Document.Fields)
            {
                builder.AppendLine($"{field.Type.Name}: {field.Value.Text} ({field.Value.Confidence:F2})");
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


        public static async Task<bool> CheckCameraPermisions()
        {
            var permissionGranted = false;
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            switch (status)
            {
                case PermissionStatus.Denied:
                    // permissions denied by the user earlier.
                    // please prompt yout own popup here to ask for permissions.
                    await MainThread.InvokeOnMainThreadAsync(PromtCameraPermissions);
                    break;
                case PermissionStatus.Disabled:
                    System.Diagnostics.Debug.WriteLine("Disabled");
                    break;

                case PermissionStatus.Granted:
                    // User allowed the permissions
                    permissionGranted = true;
                    break;

                case PermissionStatus.Restricted:
                    System.Diagnostics.Debug.WriteLine("Restricted");
                    break;

                case PermissionStatus.Unknown:
                    // Initial State
                    await Permissions.RequestAsync<Permissions.Camera>();
                    permissionGranted = await CheckCameraPermisions();
                    break;
            }

            return permissionGranted;
        }

        private static async Task PromtCameraPermissions()
        {
            var result = await App.Current.MainPage.DisplayAlert("Permission needed", "The application will need the Camera permissions for this action", accept: "Go to settings", cancel: "Cancel");
            if (result)
            {
                AppInfo.ShowSettingsUI();
            }
        }
    }
}
