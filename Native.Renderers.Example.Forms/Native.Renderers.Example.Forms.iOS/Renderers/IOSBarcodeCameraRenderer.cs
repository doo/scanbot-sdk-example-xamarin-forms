
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Native.Renderers.Example.Forms.iOS.Renderers;
using Native.Renderers.Example.Forms.Views;
using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.Forms;
using ScanbotSDK.Xamarin.Forms.iOS;
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

            cameraView.Controller.ShouldUseFinderFrame = true;
            cameraView.ScannerDelegate.OnDetect = HandleBarcodeScannerResults;

            Element.StartDetectionHandler = (args, sender) =>
            {
                cameraView.Controller.CameraSession.ResumeDetection();
            };

            Element.StopDetectionHandler = (args, sender) =>
            {
                cameraView.Controller.CameraSession.PauseDetection();
            };

            barcodeScannerResultHandler = Element.OnBarcodeScanResult;
        }

        private void HandleBarcodeScannerResults(SBSDKBarcodeScannerResult[] codes)
        {
            barcodeScannerResultHandler?.Invoke(new BarcodeScanningResult()
            {
                Barcodes = codes.ToFormsBarcodes()
            });
        }
    }

    // Since we cannot directly inherit from SBSDKBarcodeScannerViewControllerDelegate in our ViewRenderer,
    // we have created this wrapper class to allow binding to its events through the use of delegates
    public class BarcodeScannerDelegate : SBSDKBarcodeScannerViewControllerDelegate
    {
        public delegate void OnDetectHandler(SBSDKBarcodeScannerResult[] codes);
        
        public OnDetectHandler OnDetect;
        
        public override void DidDetect(SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes)
        {
            OnDetect?.Invoke(codes);
        }

        public override bool ShouldDetect(SBSDKBarcodeScannerViewController controller)
        {
            return true;
        }
    }

    public class IOSBarcodeCameraView : UIView
    {
        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }

        public IOSBarcodeCameraView(UIViewController parent, CGRect frame) : base(frame)
        {
            Controller = new SBSDKBarcodeScannerViewController(parent, this);
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;
        }
    }
}
