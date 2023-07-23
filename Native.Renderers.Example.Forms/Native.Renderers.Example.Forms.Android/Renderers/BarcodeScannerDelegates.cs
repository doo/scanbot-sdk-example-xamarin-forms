using System;
using IO.Scanbot.Sdk.Barcode;
using IO.Scanbot.Sdk.Barcode.Entity;
using IO.Scanbot.Sdk.Barcode.UI;
using IO.Scanbot.Sdk.Camera;
using ScanbotSDK.Xamarin.Android;

namespace Native.Renderers.Example.Forms.Droid.Renderers
{

    public class PictureTakenEventArgs : EventArgs
    {
        public byte[] Image { get; private set; }
        public int Orientation { get; private set; }

        public PictureTakenEventArgs(byte[] image, int orientation)
        {
            Image = image;
            Orientation = orientation;
        }
    }

    class BarcodeEventArgs : EventArgs
    {
        public BarcodeScanningResult Result { get; private set; }

        public BarcodeEventArgs(Java.Lang.Object value)
        {
            Result = (BarcodeScanningResult)value;
        }
    }

    // Here we define a custom BarcodeDetectorResultHandler. Whenever a result is ready, the frame handler
    // will call the Handle method on this object. To make this more flexible, we allow to
    // specify a delegate through the constructor.
    class BarcodeResultDelegate : BarcodeDetectorResultHandlerWrapper
    {
        public EventHandler<BarcodeEventArgs> Success;

        public override bool HandleResult(BarcodeScanningResult result, IO.Scanbot.Sdk.SdkLicenseError error)
        {
            if (!SBSDK.IsLicenseValid())
            {
                return false;
            }
            Success?.Invoke(this, new BarcodeEventArgs(result));
            return false;
        }
    }

    public class BarcodeScannerViewCallback : Java.Lang.Object, IBarcodeScannerViewCallback
    {
        public EventHandler<PictureTakenEventArgs> PictureTaken;
        public Action CameraOpen;
        public EventHandler<BarcodeItem> SelectionOverlayBarcodeClicked;

        public void OnSelectionOverlayBarcodeClicked(BarcodeItem barcodeItem)
        {
            SelectionOverlayBarcodeClicked?.Invoke(null, barcodeItem);
        }

        public void OnCameraOpen()
        {
            CameraOpen?.Invoke();
        }

        public void OnPictureTaken(byte[] image, CaptureInfo captureInfo)
        {
            if (!SBSDK.IsLicenseValid())
            {
                return;
            }
            PictureTaken?.Invoke(this, new PictureTakenEventArgs(image, captureInfo.ImageOrientation));
        }
    }

    public class PictureResultDelegate : PictureCallback
    {
        public EventHandler<PictureTakenEventArgs> PictureTaken;

        public override void OnPictureTaken(byte[] image, CaptureInfo captureInfo)
        {
            if (!SBSDK.IsLicenseValid())
            {
                return;
            }
            PictureTaken?.Invoke(this, new PictureTakenEventArgs(image, captureInfo.ImageOrientation));
        }
    }
}

