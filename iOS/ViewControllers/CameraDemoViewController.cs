using UIKit;
using Foundation;

using ScanbotSDK.iOS;

namespace scanbotsdkexamplexamarinforms.iOS.ViewControllers
{
    public abstract class CameraDemoDelegate
    {
        public abstract void DidCaptureDocumentImage(UIImage documentImage);
        public abstract void DidCaptureOriginalImage(UIImage originalImage);
    }

    public class CameraDemoViewController : UIViewController
    {
        protected SBSDKScannerViewController scannerViewController;

        protected bool viewAppeared;

        public CameraDemoDelegate cameraDelegate;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the SBSDKScannerViewController.
            // We want it to be embedded into self.
            // As we do not want automatic image storage we pass nil here for the image storage.
            scannerViewController = new SBSDKScannerViewController(this, null);

            // =================================================================
            //
            //
            // UI customizations can be implemented via delegate methods from "SBSDKScannerViewControllerDelegate".
            //
            // Please check the API docs of our native Scanbot SDK for iOS, since all those methods and properties are also available as Scanbot Xamarin bindings:
            //
            // SBSDKScannerViewController: https://scanbotsdk.github.io/documentation/ios/html/interface_s_b_s_d_k_scanner_view_controller.html
            //
            // SBSDKScannerViewControllerDelegate: https://scanbotsdk.github.io/documentation/ios/html/protocol_s_b_s_d_k_scanner_view_controller_delegate_01-p.html
            //
            // Please see some example implementations of "SBSDKScannerViewControllerDelegate" methods below (e.g. "scannerController:viewForDetectionStatus:", etc).
            //
            //
            // =================================================================

            // Set the delegate to self.
            scannerViewController.WeakDelegate = this;

            // We want unscaled images in full size:
            scannerViewController.ImageScale = 1.0f;

            // The minimum score in percent (0 - 100) of the perspective distortion to accept a detected document. 
            // Default is 75.0. Set lower values to accept more perspective distortion. Warning: Lower values result in more blurred document images.
            scannerViewController.AcceptedAngleScore = 70;

            // The minimum size in percent (0 - 100) of the screen size to accept a detected document. It is sufficient that height or width match the score. 
            // Default is 80.0. Warning: Lower values result in low resolution document images.
            scannerViewController.AcceptedSizeScore = 80;

            // Sensitivity factor for automatic capturing. Must be in the range [0.0...1.0]. Invalid values are threated as 1.0. 
            // Defaults to 0.66 (1 sec).s A value of 1.0 triggers automatic capturing immediately, a value of 0.0 delays the automatic by 3 seconds.
            scannerViewController.AutoCaptureSensitivity = 0.7f;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            viewAppeared = false;
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            viewAppeared = true;
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.AllButUpsideDown;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            // White statusbar
            return UIStatusBarStyle.LightContent;
        }


        // =====================================================================
        // 
        // Implementation of some delegate methods from "SBSDKScannerViewControllerDelegate":
        //
        #region SBSDKScannerViewControllerDelegate

        [Export("scannerControllerShouldAnalyseVideoFrame:")]
        public bool ScannerControllerShouldAnalyseVideoFrame(SBSDKScannerViewController controller)
        {
            // We want to only process video frames when self is visible on screen and front most view controller
            return viewAppeared && PresentedViewController == null;
        }

        [Export("scannerController:didCaptureDocumentImage:")]
        public void ScannerControllerDidCaptureDocumentImage(SBSDKScannerViewController controller, UIImage documentImage)
        {
            // Here we get the perspective corrected and cropped document image after the shutter was (auto)released.
            if (cameraDelegate != null)
            {
                cameraDelegate.DidCaptureDocumentImage(documentImage);
            }

            DismissViewController(true, null);
        }

        [Export("scannerController:didCaptureImage:")]
        public void ScannerControllerDidCaptureImage(SBSDKScannerViewController controller, UIImage image)
        {
            // Here we get the full image from the camera. We could run another manual detection here or use the latest
            // detected polygon from the video stream to process the image with.

            if (cameraDelegate != null)
            {
                cameraDelegate.DidCaptureOriginalImage(image);
            }
        }

        [Export("scannerController:didDetectPolygon:withStatus:")]
        public void ScannerControllerDidDetectPolygonWithStatus(SBSDKScannerViewController controller, SBSDKPolygon polygon, SBSDKDocumentDetectionStatus status)
        {
            // Everytime the document detector finishes detection it calls this delegate method.
        }

        [Export("scannerController:viewForDetectionStatus:")]
        public UIView ScannerControllerViewForDetectionStatus(SBSDKScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            // Alternative method to "scannerController:localizedTextForDetectionStatus:".
            // Here you can return a custom view that you want to use to visualize the latest detection status.

            var label = new SBSDKDetectionStatusLabel();
            label.BackgroundColor = UIColor.Red;
            label.TextColor = UIColor.White;

            switch (status)
            {
                case SBSDKDocumentDetectionStatus.Ok:
                    label.Text = "Don't move.\nCapturing...";
                    label.BackgroundColor = UIColor.Green;
                    break;
                case SBSDKDocumentDetectionStatus.OK_SmallSize:
                    label.Text = "Move closer";
                    break;
                case SBSDKDocumentDetectionStatus.OK_BadAngles:
                    label.Text = "Perspective";
                    break;
                case SBSDKDocumentDetectionStatus.Error_NothingDetected:
                    label.Text = "No Document";
                    break;
                case SBSDKDocumentDetectionStatus.Error_Noise:
                    label.Text = "Background too noisy";
                    break;
                case SBSDKDocumentDetectionStatus.Error_Brightness:
                    label.Text = "Poor light";
                    break;
                case SBSDKDocumentDetectionStatus.OK_BadAspectRatio:
                    label.Text = "Wrong aspect ratio.\n Rotate your device";
                    break;
                default:
                    return null;
            }

            label.SizeToFit();
            return label;
        }

        [Export("scannerController:polygonColorForDetectionStatus:")]
        public UIColor ScannerControllerPolygonColorForDetectionStatus(SBSDKScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            // If the detector has found an acceptable polygon we show it with green color
            if (status == SBSDKDocumentDetectionStatus.Ok)
            {
                return UIColor.Green;
            }

            return UIColor.Red;
        }

        [Export("scannerController:localizedTextForDetectionStatus:")]
        public string ScannerControllerLocalizedTextForDetectionStatus(SBSDKScannerViewController controller, SBSDKDocumentDetectionStatus status)
        {
            // Alternative method to "scannerController:viewForDetectionStatus:"
            // He you can return just the localized text for the status label depending on the detection status.
            return null;
        }

        #endregion

    }
}
