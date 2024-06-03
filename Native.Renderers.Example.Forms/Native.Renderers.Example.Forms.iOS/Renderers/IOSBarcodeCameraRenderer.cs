using CoreGraphics;

using System.Linq;

using Native.Renderers.Example.Forms.iOS.Renderers;
using Native.Renderers.Example.Forms.Views;

using ScanbotSDK.iOS;
using ScanbotSDK.Xamarin.Forms;
using ScanbotSDK.Xamarin.Forms.iOS;

using UIKit;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using GameController;

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
                cameraView.EnableFinder();
            };

            Element.StartDetectionHandler = (sender, e2) =>
            {
                cameraView.Controller.UnfreezeCamera();
                cameraView.ScannerDelegate.isScanning = true;
                cameraView.EnableFinder();
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

    class IOSBarcodeCameraView : UIView
    {
        public bool initialised = false;
        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }
        internal bool IsInitialised = false;
        private BarcodeCameraView element;
        public IOSBarcodeCameraView(CGRect frame) : base(frame) { }

        public void Initialize(UIViewController parentViewController)
        {
            initialised = true;
            Controller = new SBSDKBarcodeScannerViewController(parentViewController, this);
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;
            EnableFinder();

            // Enable Pinch to zoom.
            Controller.ZoomConfiguration = new SBSDKBaseScannerZoomConfiguration();
            Controller.ZoomConfiguration.PinchToZoomEnabled = true;
        }

        internal void SetBarcodeConfigurations(BarcodeCameraView element)
        {
            this.element = element;
            Controller.AcceptedBarcodeTypes = SBSDKBarcodeType.AllTypes;
            Controller.BarcodeImageGenerationType = element.ImageGenerationType.ToNative();
            ScannerDelegate.OnDetect = HandleBarcodeScannerResults;
            SetSelectionOverlayConfiguration(element.OverlayConfiguration);
        }

        /// <summary>
        /// Enable Finder and customise the Finder size.
        /// </summary>
        internal void EnableFinder()
        {
            var finderConfig = new SBSDKBaseScannerViewFinderConfiguration();
            finderConfig.ViewFinderEnabled = true;

            // Uncomment below code to customise the Finder size.

            //var finderWidth = Frame.Width / 4; // one fourth FinderWidth
            //var remainingWidth = Frame.Width - finderWidth; // one fourth FinderWidth
            //var horizontalInsets = remainingWidth / 2; // horizontal inset for finder

            //var finderHeight = Frame.Height / 4; // one fourth FinderHeight
            //var remainingHeight = Frame.Height - finderHeight; // one fourth FinderHeight
            //var verticalInsets = remainingHeight / 2; // vertical inset for finder

            //finderConfig.AspectRatio = new SBSDKAspectRatio(1, 1);
            //finderConfig.MinimumInset = new UIEdgeInsets(verticalInsets, horizontalInsets, verticalInsets, horizontalInsets);

            finderConfig.BackgroundColor = UIColor.Black.ColorWithAlpha(0.4f);
            finderConfig.LineColor = UIColor.Black;
            finderConfig.LineWidth = 2;

            Controller.ViewFinderConfiguration = finderConfig;
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
                overlayConfiguration.IsSelectable = true;
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
}
