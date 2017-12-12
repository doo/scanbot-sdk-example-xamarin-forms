using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Widget;
using Android.Views;

// native SDK namespace
using Net.Doo.Snap.Camera;
using Net.Doo.Snap.Lib.Detector;
using Net.Doo.Snap.UI;

// Wrapper namespace
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Android.Wrapper;


namespace scanbotsdkexamplexamarinforms.Droid.Activities
{
    [Activity(Theme = "@style/Theme.AppCompat")]
    public class CameraViewDemoActivity : AppCompatActivity, IPictureCallback, ContourDetectorFrameHandler.IResultHandler, ICameraOpenCallback
    {
        static string LOG_TAG = typeof(CameraViewDemoActivity).Name;

        public const int REQUEST_CODE_SCANBOT_CAMERA = 42001;
        public const string EXTRAS_ARG_DOC_IMAGE_FILE_URI = "documentImageFileUri";
        public const string EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI = "originalImageFileUri";

        protected ScanbotCameraView cameraView;
        protected AutoSnappingController autoSnappingController;
        protected bool flashEnabled;
        protected TextView userGuidanceTextView;
        protected ProgressBar imageProcessingProgress;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SupportRequestWindowFeature(WindowCompat.FeatureActionBarOverlay);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CameraViewDemo);

            SupportActionBar.Hide();

            cameraView = FindViewById<ScanbotCameraView>(Resource.Id.scanbotCameraView);

            // Uncomment to disable AutoFocus by manually touching the camera view:
            //cameraView.SetAutoFocusOnTouch(false);

            userGuidanceTextView = FindViewById<TextView>(Resource.Id.userGuidanceTextView);

            imageProcessingProgress = FindViewById<ProgressBar>(Resource.Id.imageProcessingProgress);

            ContourDetectorFrameHandler contourDetectorFrameHandler = ContourDetectorFrameHandler.Attach(cameraView);
            PolygonView polygonView = FindViewById<PolygonView>(Resource.Id.scanbotPolygonView);
            contourDetectorFrameHandler.AddResultHandler(polygonView);
            contourDetectorFrameHandler.AddResultHandler(this);

            // Please note: https://github.com/doo/Scanbot-SDK-Examples/wiki/Detecting-and-drawing-contours#contour-detection-parameters
            contourDetectorFrameHandler.SetAcceptedAngleScore(55);
            contourDetectorFrameHandler.SetAcceptedSizeScore(65);

            autoSnappingController = AutoSnappingController.Attach(cameraView, contourDetectorFrameHandler);

            cameraView.AddPictureCallback(this);
            cameraView.SetCameraOpenCallback(this);

            FindViewById(Resource.Id.scanbotSnapButton).Click += delegate
            {
                cameraView.TakePicture(false);
            };

            FindViewById(Resource.Id.scanbotFlashButton).Click += delegate
            {
                cameraView.UseFlash(!flashEnabled);
                flashEnabled = !flashEnabled;
            };
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
                /*
                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1) {
                    cameraView.SetShutterSound(false);
                }
                */

                // Enable ContinuousFocus mode:
                cameraView.ContinuousFocus();
            }, 500);
        }

        protected override void OnResume()
        {
            base.OnResume();
            cameraView.OnResume();
        }

        protected override void OnPause()
        {
            base.OnPause();
            cameraView.OnPause();
        }

        public bool HandleResult(ContourDetectorFrameHandler.DetectedFrame result)
        {
            // Here you are continiously notified about contour detection results.
            // For example, you can set a localized text for user guidance depending on the detection status.

            var color = Color.Red;
            var guideText = "";

            if (result.DetectionResult == DetectionResult.Ok)
            {
                guideText = "Don't move.\nCapturing...";
                color = Color.Green;
            }
            else if (result.DetectionResult == DetectionResult.OkButTooSmall)
            {
                guideText = "Move closer";
            }
            else if (result.DetectionResult == DetectionResult.OkButBadAngles)
            {
                guideText = "Perspective";
            }
            else if (result.DetectionResult == DetectionResult.OkButBadAspectRatio)
            {
                guideText = "Wrong aspect ratio.\n Rotate your device";
            }
            else if (result.DetectionResult == DetectionResult.ErrorNothingDetected)
            {
                guideText = "No Document";
            }
            else if (result.DetectionResult == DetectionResult.ErrorTooNoisy)
            {
                guideText = "Background too noisy";
            }
            else if (result.DetectionResult == DetectionResult.ErrorTooDark)
            {
                guideText = "Poor light";
            }

            // The HandleResult callback is coming from a worker thread. Use main UI thread to update UI:
            userGuidanceTextView.Post(() =>
            {
                userGuidanceTextView.Text = guideText;
                userGuidanceTextView.SetTextColor(Color.White);
                userGuidanceTextView.SetBackgroundColor(color);
            });

            return false;
        }

        public void OnPictureTaken(byte[] image, int imageOrientation)
        {
            // Here we get the full image from the camera and apply document detection on it.
            // Implement a suitable async(!) detection and image handling here.
            // This is just a demo showing detected image as downscaled preview image.

            // Show progress spinner:
            RunOnUiThread(() => {
                imageProcessingProgress.Visibility = ViewStates.Visible;
                userGuidanceTextView.Visibility = ViewStates.Gone;
            });

            // decode bytes as Bitmap
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InSampleSize = 2; // use 1 for original size (if you want no downscale)
                                      // in this demo we downscale the image to 1/2
            Bitmap originalBitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length, options);

            // rotate original image if required:
            if (imageOrientation > 0)
            {
                Matrix matrix = new Matrix();
                matrix.SetRotate(imageOrientation, originalBitmap.Width / 2f, originalBitmap.Height / 2f);
                originalBitmap = Bitmap.CreateBitmap(originalBitmap, 0, 0, originalBitmap.Width, originalBitmap.Height, matrix, false);
            }

            // Store the original image as file:
            var originalImgUri = MainActivity.TempImageStorage.AddImage(originalBitmap);

            Android.Net.Uri documentImgUri = null;
            // Run document detection on original image:
            var detectionResult = SBSDK.DocumentDetection(originalBitmap);
            if (detectionResult.Status.IsOk())
            {
                var documentImage = detectionResult.Image as Bitmap;
                // Store the document image as file:
                documentImgUri = MainActivity.TempImageStorage.AddImage(documentImage);
            }
            else
            {
                // No document detected! Use original image as document image, so user can try to apply manual cropping.
                documentImgUri = originalImgUri;
            }

            var extras = new Bundle();
            extras.PutString(EXTRAS_ARG_DOC_IMAGE_FILE_URI, documentImgUri.ToString());
            extras.PutString(EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI, originalImgUri.ToString());
            var intent = new Intent();
            intent.PutExtras(extras);
            SetResult(Result.Ok, intent);

            Finish();
            return;

            /* If you want to continue scanning:
            RunOnUiThread(() => {
                // continue camera preview
                cameraView.StartPreview();
                cameraView.ContinuousFocus();
            });
            */
        }

    }
}
