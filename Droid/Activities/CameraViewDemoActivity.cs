using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Widget;

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

        public static string EXTRAS_ARG_DOC_IMAGE_FILE_URI = "documentImageFileUri";
        public static string EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI = "originalImageFileUri";

        protected ScanbotCameraView cameraView;
        protected AutoSnappingController autoSnappingController;
        protected bool flashEnabled;
        protected ImageView resultImageView;
        protected TextView userGuidanceTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SupportRequestWindowFeature(WindowCompat.FeatureActionBarOverlay);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CameraViewDemo);

            SupportActionBar.Hide();

            cameraView = FindViewById<ScanbotCameraView>(Resource.Id.scanbotCameraView);
            cameraView.LockToPortrait();

            resultImageView = FindViewById<ImageView>(Resource.Id.scanbotResultImageView);

            userGuidanceTextView = FindViewById<TextView>(Resource.Id.userGuidanceTextView);

            ContourDetectorFrameHandler contourDetectorFrameHandler = ContourDetectorFrameHandler.Attach(cameraView);
            PolygonView polygonView = FindViewById<PolygonView>(Resource.Id.scanbotPolygonView);
            contourDetectorFrameHandler.AddResultHandler(polygonView);
            contourDetectorFrameHandler.AddResultHandler(this);
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
            cameraView.Post(() =>
            {
                cameraView.ContinuousFocus();
            });
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
                guideText = "Don't move";
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
                userGuidanceTextView.SetTextColor(color);
            });

            return false;
        }

        public void OnPictureTaken(byte[] image, int imageOrientation)
        {
            // Here we get the full image from the camera.
            // Implement a suitable async(!) detection and image handling here.
            // This is just a demo showing detected image as downscaled preview image.

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

            // Run document detection on original image:
            var detectionResult = SBSDK.DocumentDetection(originalBitmap);
            if (detectionResult.Status.IsOk())
            {
                var documentImage = detectionResult.Image as Bitmap;
                var documentImgUri = MainActivity.TempImageStorage.AddImage(documentImage);
                var originalImgUri = MainActivity.TempImageStorage.AddImage(originalBitmap);

                Bundle extras = new Bundle();
                extras.PutString(EXTRAS_ARG_DOC_IMAGE_FILE_URI, documentImgUri.ToString());
                extras.PutString(EXTRAS_ARG_ORIGINAL_IMAGE_FILE_URI, originalImgUri.ToString());
                Intent intent = new Intent();
                intent.PutExtras(extras);
                SetResult(Result.Ok, intent);

                Finish();
                return;
            }

            // continue camera preview
            cameraView.StartPreview();
            cameraView.ContinuousFocus();
        }

    }
}
