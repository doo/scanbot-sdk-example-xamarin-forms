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
    class IOSBarcodeCameraRenderer : ViewRenderer<BarcodeCameraView, IOSBarcodeCameraView>
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

            cameraView = new IOSBarcodeCameraView(new CGRect(x, y, width, height));
            SetNativeControl(cameraView);

            base.OnElementChanged(e);
        }

        private void HandleBarcodeScannerResults(SBSDKBarcodeScannerResult[] codes)
        {
            barcodeScannerResultHandler?.Invoke(new BarcodeScanningResult()
            {
                Barcodes = codes.ToFormsBarcodes()
            });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (Control == null) { return; }

            if (CurrentViewController.ChildViewControllers.First() is PageRenderer pageRendererVc)
            {
                cameraView.Initialize(pageRendererVc);
                cameraView.ScannerDelegate.OnDetect = HandleBarcodeScannerResults;
                barcodeScannerResultHandler = Element.OnBarcodeScanResult;
            }
        }
    }

    // Since we cannot directly inherit from SBSDKBarcodeScannerViewControllerDelegate in our ViewRenderer,
    // we have created this wrapper class to allow binding to its events through the use of delegates
    class BarcodeScannerDelegate : SBSDKBarcodeScannerViewControllerDelegate
    {
        public delegate void OnDetectHandler(SBSDKBarcodeScannerResult[] codes);
        public OnDetectHandler OnDetect;

        public override void DidDetect(SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes, UIImage image)
        {
            OnDetect?.Invoke(codes);
        }

        public override bool ShouldDetectBarcodes(SBSDKBarcodeScannerViewController controller)
        {
            return true;
        }
    }

    class IOSBarcodeCameraView : UIView
    {
        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }

        public IOSBarcodeCameraView(CGRect frame) : base(frame) {}

        public void Initialize(UIViewController parentViewController) {
            Controller = new SBSDKBarcodeScannerViewController(parentViewController, this);
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;
        }
    }
}
