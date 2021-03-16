
using Native.Renderers.Example;
using IO.Scanbot.Sdk.Camera;
using Xamarin.Forms;
using Android.Content;
using Xamarin.Forms.Platform.Android;
using IO.Scanbot.Sdk.Contourdetector;
using IO.Scanbot.Sdk.Barcode;
using AndroidX.Core.Content;
using Android;
using Android.Content.PM;
using AndroidX.Core.App;
using IO.Scanbot.Sdk.Barcode.Entity;
using System.Linq;
using Android.Widget;
using Native.Renderers.Example.Droid;

[assembly: ExportRenderer(typeof(BarcodeCameraView), typeof(AndroidBarcodeCameraRenderer))]
namespace Native.Renderers.Example.Droid
{
    public class AndroidBarcodeCameraRenderer : ViewRenderer<BarcodeCameraView, ScanbotCameraView>, ICameraOpenCallback
    {
        static string LOG_TAG = typeof(AndroidBarcodeCameraRenderer).Name;

        public BarcodeCameraView.BarcodeScannerResultHandler HandleScanResult;
        protected ScanbotCameraView cameraView;
        protected DocumentAutoSnappingController autoSnappingController;
        protected BarcodeDetectorFrameHandler barcodeDetectorFrameHandler;

        private readonly int REQUEST_PERMISSION_CODE = 200;

        public AndroidBarcodeCameraRenderer(Context context) : base(context)
        {
            cameraView = new ScanbotCameraView(context);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<BarcodeCameraView> e)
        {
            SetNativeControl(cameraView);

            base.OnElementChanged(e);

            // We use this delegate to receive the Scan Result on the Core side
            HandleScanResult = Element.OnBarcodeScanResult;

            // OnResume and OnPause must be set in order for the Camera View to work properly
            Element.OnResume = (sender, e) =>
            {
                cameraView.OnResume();
                CheckPermissions();
            };

            Element.OnPause = (sender, e) =>
            {
                cameraView.OnPause();
            };

            if (Control != null)
            {
                // In this example we demonstrate how to lock the orientation of the UI (Activity)
                // as well as the orientation of the taken picture to portrait.
                cameraView.LockToPortrait(true);

                // Here we create the BarcodeDetectorFrameHandler which will take care of detecting
                // barcodes in your video frames
                var detector = new IO.Scanbot.Sdk.ScanbotSDK(Context.GetActivity()).BarcodeDetector();
                barcodeDetectorFrameHandler = BarcodeDetectorFrameHandler.Attach(cameraView, detector);

                if (barcodeDetectorFrameHandler is BarcodeDetectorFrameHandler handler)
                {
                    handler.SetDetectionInterval(1000);
                    handler.AddResultHandler(new BarcodeDetectorResultHandler((result) => HandleFrameHandlerResult(result)));
                    handler.SaveCameraPreviewFrame(true);

                    var barcodeAutoSnappingController = BarcodeAutoSnappingController.Attach(cameraView, handler);
                    barcodeAutoSnappingController.SetSensitivity(1f);
                }

                cameraView.SetCameraOpenCallback(this);
            }
        }

        public void OnCameraOpened()
        {
            cameraView.PostDelayed(() =>
            {
                // Disable auto-focus sound:
                cameraView.SetAutoFocusSound(false);

                // Uncomment to disable shutter sound (supported since Android 4.2+):
                // Please note that some devices may not allow disabling the camera shutter sound. 
                // If the shutter sound state cannot be set to the desired value, this method will be ignored.
                // cameraView.SetShutterSound(false);

                // Enable ContinuousFocus mode:
                cameraView.ContinuousFocus();
            }, 500);
        }

        private bool HandleSuccess(BarcodeScanningResult result)
        {
            if (result == null) { return false; }

            ScanbotSDK.Xamarin.Forms.BarcodeScanningResult outResult = new ScanbotSDK.Xamarin.Forms.BarcodeScanningResult
            {
                Barcodes = result.BarcodeItems.Select((item) =>
                {
                    var barcode = new ScanbotSDK.Xamarin.Forms.Barcode
                    {
                        Text = item.Text
                    };
                    return barcode;
                }).ToList()
            };

            HandleScanResult?.Invoke(outResult);
            return true;
        }

        bool HandleFrameHandlerResult(FrameHandlerResult result)
        {
            if (result is FrameHandlerResult.Success success)
            {
                if (success.Value is BarcodeScanningResult barcodeResult)
                {
                    HandleSuccess(barcodeResult);
                }
            }
            else
            {
                cameraView.Post(() => {
                    Toast.MakeText(Context.GetActivity(), "License has expired!", ToastLength.Long).Show();
                    cameraView.RemoveFrameHandler(barcodeDetectorFrameHandler);
                });
            }

            return false;
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
}

class BarcodeDetectorResultHandler : BarcodeDetectorFrameHandler.BarcodeDetectorResultHandler
{
    public delegate bool HandleResultFunction(FrameHandlerResult result);
    private HandleResultFunction handleResultFunc;

    public BarcodeDetectorResultHandler(HandleResultFunction handleResultFunc)
    {
        this.handleResultFunc = handleResultFunc;
    }

    public override bool Handle(FrameHandlerResult result)
    {
        handleResultFunc(result);
        return false;
    }
}