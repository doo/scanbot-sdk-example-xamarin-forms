using System;
using System.Linq;
using CoreGraphics;
using HomeKit;
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

            Element.OnResumeHandler = (sender, e1) =>
            {
                cameraView.Controller.UnfreezeCamera();
                cameraView.ScannerDelegate.isScanning = true;
            };

            Element.StartDetectionHandler = (sender, e2) =>
            {
                cameraView.Controller.UnfreezeCamera();
                cameraView.ScannerDelegate.isScanning = true;
            };

            Element.OnPauseHandler = (sender, e3) =>
            {
                cameraView.Controller.FreezeCamera();
                cameraView.ScannerDelegate.isScanning = false;
            };

            Element.StopDetectionHandler = (sender, e4) =>
            {
                cameraView.Controller.FreezeCamera();
                cameraView.ScannerDelegate.isScanning = false;
            };
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            if (Control == null) { return; }

            if (cameraView?.initialised == false && CurrentViewController.ChildViewControllers.First() is PageRenderer pageRendererVc)
            {
                cameraView.Initialize(pageRendererVc);
                cameraView.SetBarcodeConfigurations(Element);
            }
        }
    }

    // Since we cannot directly inherit from SBSDKBarcodeScannerViewControllerDelegate in our ViewRenderer,
    // we have created this wrapper class to allow binding to its events through the use of delegates
    class BarcodeScannerDelegate : SBSDKBarcodeScannerViewControllerDelegate
    {
        internal bool isScanning = true;
        public delegate void OnDetectHandler(SBSDKBarcodeScannerResult[] codes);
        public OnDetectHandler OnDetect;

        public override void DidDetectBarcodes(SBSDKBarcodeScannerViewController controller, SBSDKBarcodeScannerResult[] codes)
        {
            if (controller.BarcodeImageGenerationType == SBSDKBarcodeImageGenerationType.CapturedImage)
            {
                isScanning = false; // it will restrict further scans and stop scanning when the image is captured.
            }
            OnDetect?.Invoke(codes);
        }

        public override bool ShouldDetectBarcodes(SBSDKBarcodeScannerViewController controller)
        {
            return isScanning;
        }
    }

    class IOSBarcodeCameraView : UIView
    {
        public bool initialised = false;
        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        internal BarcodeScannerDelegate ScannerDelegate;
        private BarcodeCameraView element;
        public IOSBarcodeCameraView(CGRect frame) : base(frame) { }

        public void Initialize(UIViewController parentViewController)
        {
            initialised = true;
            Controller = new SBSDKBarcodeScannerViewController(parentViewController, this);
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;
        }

        internal void SetBarcodeConfigurations(BarcodeCameraView element)
        {
            this.element = element;
            Controller.AcceptedBarcodeTypes = SBSDKBarcodeType.AllTypes;
            Controller.BarcodeImageGenerationType = element.ImageGenerationType.ToNative();
            ScannerDelegate.OnDetect = HandleBarcodeScannerResults;
            SetSelectionOverlayConfiguration(element.OverlayConfiguration);
        }

        private void SetSelectionOverlayConfiguration(SelectionOverlayConfiguration configuration)
        {
            if (configuration != null && configuration.Enabled)
            {
                var overlayConfiguration = new SBSDKBarcodeTrackingOverlayConfiguration();

                var polygonStyle = new SBSDKBarcodeTrackedViewPolygonStyle();
                polygonStyle.PolygonColor = configuration.PolygonColor.ToUIColor();
                polygonStyle.PolygonSelectedColor = configuration.HighlightedPolygonColor?.ToUIColor();

                // use below properties if you want to set background color to the polygon. As of now they are set to clear
                // eg: to show translucent color over barcode. 
                polygonStyle.PolygonBackgroundColor = UIColor.Clear;
                polygonStyle.PolygonBackgroundSelectedColor = UIColor.Clear;

                var textStyle = new SBSDKBarcodeTrackedViewTextStyle();
                textStyle.TrackingOverlayTextFormat = configuration.OverlayTextFormat.ToNative();
                textStyle.TextColor = configuration.TextColor.ToUIColor();
                textStyle.SelectedTextColor = configuration.HighlightedTextColor?.ToUIColor();
                textStyle.TextBackgroundColor = configuration.TextContainerColor.ToUIColor();
                textStyle.TextBackgroundSelectedColor = configuration.HighlightedTextContainerColor?.ToUIColor();

                overlayConfiguration.IsAutomaticSelectionEnabled = configuration.AutomaticSelectionEnabled;
                overlayConfiguration.TextStyle = textStyle;
                overlayConfiguration.PolygonStyle = polygonStyle;

                Controller.IsTrackingOverlayEnabled = configuration.Enabled;
                Controller.TrackingOverlayController.Configuration = overlayConfiguration;
                Controller.TrackingOverlayController.Delegate = new BarcodeTrackingOverlayDelegate
                {
                    DidTapBarcodeOverlay = HandleDidTapOnBarcodeOverlay
                };
            }
        }

        private void HandleBarcodeScannerResults(SBSDKBarcodeScannerResult[] codes)
        {
            var returnResults = true;
            if (Controller.IsTrackingOverlayEnabled)
            {
                // return results if tracking overlay is set to automatic selection true
                returnResults = Controller.TrackingOverlayController?.Configuration?.IsAutomaticSelectionEnabled ?? false;
            }

            if (returnResults)
            {
                this.element?.OnBarcodeScanResult.Invoke(new BarcodeScanningResult()
                {
                    Barcodes = codes.ToFormsBarcodes()
                });
            }
        }

        private void HandleDidTapOnBarcodeOverlay(SBSDKBarcodeScannerResult barcode)
        {
            this.element?.OnBarcodeScanResult.Invoke(new BarcodeScanningResult()
            {
                Barcodes = new System.Collections.Generic.List<Barcode> { barcode.ToFormsBarcode() }
            });
        }
    }

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
