using System;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.Forms;

namespace Native.Renderers.Example.Forms.iOS.Helpers
{
    public static class SBSDKBarcodeType_Extension
    {
        public static BarcodeFormat? ToFormsBarcodeFormat(this SBSDKBarcodeType type)
        {
            if (type == SBSDKBarcodeType.Aztec)
            {
                return BarcodeFormat.Aztec;
            }
            else if (type == SBSDKBarcodeType.CodaBar)
            {
                return BarcodeFormat.Codabar;
            }
            else if (type == SBSDKBarcodeType.Code128)
            {
                return BarcodeFormat.Code128;
            }
            else if (type == SBSDKBarcodeType.Code39)
            {
                return BarcodeFormat.Code39;
            }
            else if (type == SBSDKBarcodeType.Code93)
            {
                return BarcodeFormat.Code93;
            }
            else if (type == SBSDKBarcodeType.DataMatrix)
            {
                return BarcodeFormat.DataMatrix;
            }
            else if (type == SBSDKBarcodeType.EAN13)
            {
                return BarcodeFormat.Ean13;
            }
            else if (type == SBSDKBarcodeType.EAN8)
            {
                return BarcodeFormat.Ean8;
            }
            else if (type == SBSDKBarcodeType.ITF)
            {
                return BarcodeFormat.Itf;
            }
            else if (type == SBSDKBarcodeType.PDF417)
            {
                return BarcodeFormat.Pdf417;
            }
            else if (type == SBSDKBarcodeType.QRCode)
            {
                return BarcodeFormat.QrCode;
            }
            else if (type == SBSDKBarcodeType.RSS14)
            {
                return BarcodeFormat.Rss14;
            }
            else if (type == SBSDKBarcodeType.RSSExpanded)
            {
                return BarcodeFormat.RssExpanded;
            }
            else if (type == SBSDKBarcodeType.UPCA)
            {
                return BarcodeFormat.UpcA;
            }
            else if (type == SBSDKBarcodeType.UPCE)
            {
                return BarcodeFormat.UpcE;
            }
            else if (type == SBSDKBarcodeType.MSIPlessey)
            {
                return BarcodeFormat.MSIPlessey;
            }

            return null;
        }
    }
}
