
using Native.Renderers.Example.Forms.Views;
using Android.Content;
using Xamarin.Forms.Platform.Android;
using AndroidX.Core.Content;
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using IO.Scanbot.Sdk.Barcode.Entity;
using Android.Widget;
using Xamarin.Forms;
using Native.Renderers.Example.Forms.Droid.Renderers;
using ScanbotSDK.Xamarin.Forms.Android;
using Android.Views;
using IO.Scanbot.Sdk.UI.Camera;
using IO.Scanbot.Sdk.Barcode.UI;
using System.Collections.Generic;
using AndroidBarcode = IO.Scanbot.Sdk.Barcode.Entity.BarcodeItem;
using ScanbotSDK.Xamarin.Forms;

/*
    This is the Android Custom Renderer that will provide the actual implementation for BarcodeCameraView.
    We use the 'ExportRenderer' assembly directive to specify that we want to attach AndroidBarcodeCameraRenderer to
    BarcodeCameraView.

    Syntax:

    [assembly: ExportRenderer(typeof([FORMS_VIEW_CLASS]), typeof([CUSTOM_RENDERER_CLASS]))]

    ---
 */
[assembly: ExportRenderer(typeof(BarcodeCameraView), typeof(AndroidBarcodeCameraRenderer))]
namespace Native.Renderers.Example.Forms.Droid.Renderers
{
    /*
       By extending 'ViewRenderer' we specify that we want our custom renderer to target 'BarcodeCameraView' and
       override it with our native view, which is a 'FrameLayout' in this case (see layout/barcode_camera_view.xml)
    */
    class AndroidBarcodeCameraRenderer : ViewRenderer<BarcodeCameraView, FrameLayout>
    {
        bool flashEnabled;
        protected FrameLayout cameraLayout;
        protected BarcodeScannerView cameraView;
        private readonly int REQUEST_PERMISSION_CODE = 200;
        BarcodeResultDelegate resultHandler;

        public AndroidBarcodeCameraRenderer(Context context) : base(context)
        {
            SetupViews(context);
        }

        private void SetupViews(Context context) {

            // We instantiate our views from the layout XML
            cameraLayout = (FrameLayout)LayoutInflater
                .FromContext(context)
                .Inflate(Resource.Layout.barcode_camera_view, null, false);

            // Here we retrieve the Camera View...
            cameraView = cameraLayout.FindViewById<BarcodeScannerView>(Resource.Id.barcode_camera);
        }

        /*
            This is probably the most important method that belongs to a ViewRenderer.
            You must override this in order to actually implement the renderer.
            OnElementChanged is called whenever the View or one of its properties have changed;
            this includes the initialization as well, therefore we initialize our native control here.
         */
        protected override void OnElementChanged(ElementChangedEventArgs<BarcodeCameraView> e)
        {

            // The SetNativeControl method should be used to instantiate the native control,
            // and this method will also assign the control reference to the Control property
            SetNativeControl(cameraLayout);

            base.OnElementChanged(e);

            if (Control != null)
            {
                // The Element object is the instance of BarcodeCameraView as defined in the Forms
                // core project. We've defined some delegates there, and we'll bind to them here so that
                // these native calls will be executed whenever those methods will be called.
                Element.OnResumeHandler = (sender, e) =>
                {
                    cameraView.ViewController.OnResume();
                };

                Element.OnPauseHandler = (sender, e) =>
                {
                    cameraView.ViewController.OnPause();
                };

                Element.StartDetectionHandler = (sender, e) =>
                {
                    cameraView.ViewController.OnResume();
                    CheckPermissions();
                };

                Element.StopDetectionHandler = (sender, e) =>
                {
                    cameraView.ViewController.OnPause();
                };

                // Here we create the BarcodeDetectorFrameHandler which will take care of detecting
                // barcodes in your video frames
                var detector = new IO.Scanbot.Sdk.ScanbotSDK(Context.GetActivity()).CreateBarcodeDetector();
                detector.ModifyConfig(new Function1Impl<BarcodeScannerConfigBuilder>((response) => {
                    response.SetSaveCameraPreviewFrame(false);
                }));

                cameraView.InitCamera(new CameraUiSettings(false));
                // result delegate
                resultHandler = new BarcodeResultDelegate();
                resultHandler.Success += OnBarcodeResult;

                // scanner delegates
                var scannerViewCallback = new BarcodeScannerViewCallback();
                scannerViewCallback.CameraOpen = OnCameraOpened;
                scannerViewCallback.SelectionOverlayBarcodeClicked += OnSelectionOverlayBarcodeClicked;

                BarcodeScannerViewWrapper.InitDetectionBehavior(cameraView, detector, resultHandler, scannerViewCallback);
                SetSelectionOverlayConfiguration();
            }
        }

        private void OnSelectionOverlayBarcodeClicked(object sender, AndroidBarcode e)
        {
            var items = new List<AndroidBarcode> { e };
            var args = new BarcodeEventArgs(new IO.Scanbot.Sdk.Barcode.Entity.BarcodeScanningResult(items, new Java.Util.Date().Time), null);
            OnBarcodeResult(this, args);
        }

        public void OnCameraOpened()
        {
            cameraView.PostDelayed(delegate
            {
                cameraView.ViewController.UseFlash(flashEnabled);
                cameraView.ViewController.ContinuousFocus();
            }, 300);
        }


        private void SetSelectionOverlayConfiguration()
        {
            if (Element?.OverlayConfiguration?.Enabled == true)
            {
                cameraView.SelectionOverlayController.SetEnabled(Element.OverlayConfiguration.Enabled);
                cameraView.SelectionOverlayController.SetTextFormat(Element.OverlayConfiguration.OverlayTextFormat.ToAndroid());
                cameraView.SelectionOverlayController.SetPolygonColor(Element.OverlayConfiguration.PolygonColor.ToAndroid());
                cameraView.SelectionOverlayController.SetTextColor(Element.OverlayConfiguration.TextColor.ToAndroid());
                cameraView.SelectionOverlayController.SetTextContainerColor(Element.OverlayConfiguration.TextContainerColor.ToAndroid());

                if (Element.OverlayConfiguration.HighlightedPolygonColor != null)
                {
                    cameraView.SelectionOverlayController.SetPolygonHighlightedColor(Element.OverlayConfiguration.HighlightedPolygonColor.Value.ToAndroid());
                }

                if (Element.OverlayConfiguration.HighlightedTextColor != null)
                {
                    cameraView.SelectionOverlayController.SetTextHighlightedColor(Element.OverlayConfiguration.HighlightedTextColor.Value.ToAndroid());
                }

                if (Element.OverlayConfiguration.HighlightedTextContainerColor != null)
                {
                    cameraView.SelectionOverlayController.SetTextContainerHighlightedColor(Element.OverlayConfiguration.HighlightedTextContainerColor.Value.ToAndroid());
                }

                // if selection overlay is set than we get barcode results from it's own OnSelectionOverlayBarcodeClicked handler. So we don't need below result handler.
                if (resultHandler != null)
                {
                    resultHandler.Success -= OnBarcodeResult;
                }
            }
        }

        private void OnBarcodeResult(object sender, BarcodeEventArgs e)
        {
            if (e.Result != null)
            {
                ScanbotSDK.Xamarin.Forms.BarcodeScanningResult outResult = new ScanbotSDK.Xamarin.Forms.BarcodeScanningResult
                {
                    Barcodes = e.Result.BarcodeItems.ToFormsBarcodeList(),
                    Image = e.Result.PreviewFrame.ToImageSource()
                };

                Element.OnBarcodeScanResult?.Invoke(outResult);
            }

            if (e.Error != null)
            {
                cameraView.Post(() => Toast.MakeText(Context.GetActivity(), "License has expired!", ToastLength.Long).Show());
            }
        }

        private void CheckPermissions()
        {
            if (Context == null || Context.GetActivity() == null)
            {
                return;
            }

            var activity = Context.GetActivity();

            if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.Camera) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(activity, new string[] { Manifest.Permission.Camera }, REQUEST_PERMISSION_CODE);
            }
        }
    }

    public static class Extension
    {
        // --------------------------------
        // Overlay Text Format
        // --------------------------------
        public static BarcodeOverlayTextFormat ToAndroid(this OverlayFormat format)
        {
            switch (format)
            {
                case OverlayFormat.None:
                    return BarcodeOverlayTextFormat.None;
                case OverlayFormat.Code:
                    return BarcodeOverlayTextFormat.Code;
                default:
                    return BarcodeOverlayTextFormat.CodeAndType;
            }
        }
    }
}
