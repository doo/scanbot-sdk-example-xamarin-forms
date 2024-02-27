using Native.Renderers.Example.Forms.iOS.Utils;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.Forms;

namespace Native.Renderers.Example.Forms.iOS
{

    // Since we cannot directly inherit from SBSDKBarcodeScannerViewControllerDelegate in our ViewRenderer,
    // we have created this wrapper class to allow binding to its events through the use of delegates
    internal class BarcodeScannerDelegate : SBSDKBarcodeScannerViewControllerDelegate
    {
        public delegate void OnDetectHandler(SBSDKBarcodeScannerResult[] codes);
        public OnDetectHandler OnDetect;

        public override void DidDetectBarcodes(SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes)
        {
            OnDetect?.Invoke(codes);
        }

        public override bool ShouldDetectBarcodes(SBSDKBarcodeScannerViewController controller)
        {
            if (!SBSDK.IsLicenseValid)
            {
                ViewUtils.ShowAlert("License Expired!", "Ok");
                return false;
            }
            return true;
        }
    }

    internal class BarcodeTrackingOverlayDelegate : SBSDKBarcodeTrackingOverlayControllerDelegate
    {
        public delegate void DidTapOnBarcodeAROverlay(SBSDKBarcodeScannerResult barcode);
        public DidTapOnBarcodeAROverlay DidTapBarcodeOverlay;

        public override void DidTapOnBarcode(SBSDKBarcodeTrackingOverlayController controller, SBSDKBarcodeScannerResult barcode)
        {
            DidTapBarcodeOverlay?.Invoke(barcode);
        }
    }
}

