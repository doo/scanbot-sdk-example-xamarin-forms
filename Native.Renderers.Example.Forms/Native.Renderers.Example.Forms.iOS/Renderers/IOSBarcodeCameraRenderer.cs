
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Native.Renderers.Example.Forms.iOS.Helpers;
using Native.Renderers.Example.Forms.iOS.Renderers;
using Native.Renderers.Example.Forms.Views;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.Forms;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BarcodeCameraView), typeof(IOSBarcodeCameraRenderer))]
namespace Native.Renderers.Example.Forms.iOS.Renderers
{
    public class IOSBarcodeCameraRenderer : ViewRenderer<BarcodeCameraView, IOSBarcodeCameraView>
    {

        private IOSBarcodeCameraView cameraView;
        private UIViewController CurrentViewController
        {
            get
            {
                if (UIApplication.SharedApplication.Windows.First() is UIWindow window)
                {
                    return window.RootViewController;
                }

                return null;
            }
        }

        private BarcodeCameraView.BarcodeScannerResultHandler barcodeScannerResultHandler;

        public IOSBarcodeCameraRenderer() : base() { }

        protected override void OnElementChanged(ElementChangedEventArgs<BarcodeCameraView> e)
        {

            if (CurrentViewController == null) { return; }

            double x = e.NewElement.X;
            double y = e.NewElement.Y;
            double width = e.NewElement.WidthRequest;
            double height = e.NewElement.HeightRequest;

            cameraView = new IOSBarcodeCameraView(CurrentViewController, new CGRect(x, y, width, height));
            SetNativeControl(cameraView);

            base.OnElementChanged(e);

            if (Control == null) { return; }

            cameraView.ScannerDelegate.OnDetect = HandleBarcodeScannerResults;

            barcodeScannerResultHandler = Element.OnBarcodeScanResult;

            Element.OnResume = (sender, args) =>
            {
                cameraView.StartCamera();
            };

            Element.OnPause = (sender, args) =>
            {
                cameraView.StopCamera();
            };
        }

        private void HandleBarcodeScannerResults(SBSDKBarcodeScannerResult[] codes)
        {
            List<Barcode> barcodes = codes
                .Select((code) => new Barcode()
                {
                    Text = code.RawTextString,
                    Format = code.Type.ToFormsBarcodeFormat().Value
                })
                .ToList();

            barcodeScannerResultHandler?.Invoke(new BarcodeScanningResult()
            {
                Barcodes = barcodes
            });
        }
    }

    // Since we cannot directly inherit from SBSDKBarcodeScannerViewControllerDelegate in our ViewRenderer
    // we have created this wrapper class to allow binding to its events through the use of delegates
    public class BarcodeScannerDelegate : SBSDKBarcodeScannerViewControllerDelegate
    {
        public delegate void OnDetectHandler(SBSDKBarcodeScannerResult[] codes);
        public delegate void OnCaptureHandler(UIImage barcodeImage);
        public delegate void OnChangeViewFinderHandler(CGRect rect);
        public delegate bool OnShouldDetectHandler();

        public OnDetectHandler OnDetect;
        public OnCaptureHandler OnCapture;
        public OnChangeViewFinderHandler OnChangeViewFinder;
        public OnShouldDetectHandler OnShouldDetect;

        public override void DidCapture(SBSDKBarcodeScannerViewController controller, UIImage barcodeImage)
        {
            base.DidCapture(controller, barcodeImage);
            OnCapture?.Invoke(barcodeImage);
        }

        public override void DidChangeViewFinder(SBSDKBarcodeScannerViewController controller, CGRect rect)
        {
            base.DidChangeViewFinder(controller, rect);
            OnChangeViewFinder?.Invoke(rect);
        }

        public override void DidDetect(SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes)
        {
            base.DidDetect(controller, codes);
            OnDetect?.Invoke(codes);
        }

        public override bool ShouldDetect(SBSDKBarcodeScannerViewController controller)
        {
            bool result = base.ShouldDetect(controller);
            return OnShouldDetect?.Invoke() ?? result;
        }
    }

    public class IOSBarcodeCameraView : UIView
    {

        public SBSDKBarcodeScannerViewController ScannerViewController { get => scannerViewController; }
        public BarcodeScannerDelegate ScannerDelegate
        {
            get => scannerDelegate;

            private set
            {
                scannerViewController.Delegate = value;
                scannerDelegate = value;
            }
        }

        private BarcodeScannerDelegate scannerDelegate;
        private SBSDKBarcodeScannerViewController scannerViewController;

        private readonly UIViewController parentViewController;

        public IOSBarcodeCameraView(UIViewController parentViewController, CGRect frame) : base(frame)
        {
            this.parentViewController = parentViewController;
            SetupViews();
        }

        public void StartCamera()
        {
            scannerViewController.CameraSession.StartSession();
        }

        public void StopCamera()
        {
            scannerViewController.CameraSession.StopSession();
        }

        private void SetupViews()
        {
            scannerViewController = new SBSDKBarcodeScannerViewController(parentViewController, this);
            ScannerDelegate = new BarcodeScannerDelegate();
        }
    }
}
