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

[assembly: ExportRenderer(typeof(BarcodeCameraView), typeof(IOSBarcodeCameraRenderer))]
namespace Native.Renderers.Example.Forms.iOS.Renderers
{
    class IOSBarcodeCameraRenderer : ViewRenderer<BarcodeCameraView, IOSBarcodeCameraView>
    {
        private IOSBarcodeCameraView cameraView;

        protected override void OnElementChanged(ElementChangedEventArgs<BarcodeCameraView> e)
        {
            if (UIApplication.SharedApplication.KeyWindow?.RootViewController == null) { return; }

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

            if (!cameraView.IsInitialised)
            {
                cameraView.Initialize(Element);
                cameraView.SetBarcodeConfigurations();
            }
        }
    }

    class IOSBarcodeCameraView : UIView
    {
        public SBSDKBarcodeScannerViewController Controller { get; private set; }
        public BarcodeScannerDelegate ScannerDelegate { get; private set; }
        internal bool IsInitialised = false;
        private BarcodeCameraView element;
        public IOSBarcodeCameraView(CGRect frame) : base(frame) { }

        public void Initialize(BarcodeCameraView element)
        {
            this.element = element;
            IsInitialised = true;
            Controller = new SBSDKBarcodeScannerViewController();
            ScannerDelegate = new BarcodeScannerDelegate();
            Controller.Delegate = ScannerDelegate;

            AddBarcodeScannerToParentViewController();
        }

        private void AddBarcodeScannerToParentViewController()
        {
            Controller.View.Frame = Bounds;

#if IOS16_0_OR_GREATER
            // On iOS 16+ and macOS 13+ the SBSDKBarcodeScannerViewController has to be added to a parent ViewController, otherwise the transport controls won't be displayed.
            var viewController = GetParentPageViewControllerInElementsTree(element?.ParentView) ?? UIApplication.SharedApplication.KeyWindow?.RootViewController;

            // If we don't find the viewController, assume it's not Shell and still continue, the transport controls will still be displayed
            if (viewController?.View is null)
            {
                // Zero out the safe area insets of the SBSDKBarcodeScannerViewController
                UIEdgeInsets insets = viewController.View.SafeAreaInsets;
                Controller.AdditionalSafeAreaInsets =
                    new UIEdgeInsets(insets.Top * -1, insets.Left, insets.Bottom * -1, insets.Right);


                // Add the View from the SBSDKBarcodeScannerViewController to the parent ViewController
                viewController.AddChildViewController(Controller);
                viewController.View.AddSubview(Controller.View);
            }
#endif
            AddSubview(Controller.View);
        }

        private UIViewController GetParentPageViewControllerInElementsTree( Element virtualViewParentElement)
        {
            // Traverse up the element tree to find the nearest parent page
            Element currentElement = virtualViewParentElement;
            while (currentElement != null && !(currentElement is Page))
            {
                currentElement = currentElement.Parent;
            }

            // If a Page is found, get its corresponding ViewController
            return (currentElement as Page)?.GetRenderer()?.ViewController;
        }

        internal void SetBarcodeConfigurations()
        {
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
