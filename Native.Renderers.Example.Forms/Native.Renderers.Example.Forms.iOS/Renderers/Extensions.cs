using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.Forms;

namespace Native.Renderers.Example.Forms.iOS.Renderers
{
    public static class Extension
    {
        public static SBSDKBarcodeOverlayFormat ToNative(this OverlayFormat overlayTextFormat)
        {
            switch (overlayTextFormat)
            {
                case OverlayFormat.None:
                    return SBSDKBarcodeOverlayFormat.None;
                case OverlayFormat.Code:
                    return SBSDKBarcodeOverlayFormat.Code;
                default:
                    return SBSDKBarcodeOverlayFormat.CodeAndType;
            }
        }

        public static SBSDKBarcodeImageGenerationType ToNative(this BarcodeImageGenerationType imageGenerationType)
        {
            switch (imageGenerationType)
            {
                case BarcodeImageGenerationType.None:
                    return SBSDKBarcodeImageGenerationType.None;
                case BarcodeImageGenerationType.CapturedImage:
                    return SBSDKBarcodeImageGenerationType.CapturedImage;
                case BarcodeImageGenerationType.FromVideoFrame:
                    return SBSDKBarcodeImageGenerationType.FromVideoFrame;
                default:
                    return SBSDKBarcodeImageGenerationType.None;
            }
        }
    }
}