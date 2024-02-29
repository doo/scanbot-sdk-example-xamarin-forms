using System;
using Native.Renderers.Example.Forms.Common;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Native.Renderers.Example.Forms
{
    public partial class MainPage : ContentPage
    {
        private bool isDetectionOn;
        private bool IsDetectionOn
        {
            get => isDetectionOn;
            set
            {
                isDetectionOn = value;
                scanButton.Text = value ? "STOP SCANNING" : "START SCANNING";
                RefreshCamera();
            }
        }

        public MainPage()
        {
            InitializeComponent();
            SetupViews();
        }

        private void SetupViews()
        {
            cameraView.OnBarcodeScanResult = HandleBarcodeScanningResult;
            cameraView.OverlayConfiguration = new SelectionOverlayConfiguration(
                automaticSelectionEnabled: true, overlayFormat: OverlayFormat.Code,
                polygon: Color.Black, text: Color.Black, textContainer: Color.White,
                highlightedPolygonColor: Color.Red, highlightedTextColor: Color.Red, highlightedTextContainerColor: Color.Black);
            cameraView.ImageGenerationType = BarcodeImageGenerationType.CapturedImage;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            RefreshCamera();

            SetupIOSAppearance();

            scanButton.Clicked += OnScanButtonPressed;
            infoButton.Clicked += OnInfoButtonPressed;

            if (ScanbotSDKConfiguration.LICENSE_KEY.Equals(""))
            {
                ShowTrialLicenseAlert();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            scanButton.Clicked -= OnScanButtonPressed;
            infoButton.Clicked -= OnInfoButtonPressed;

            PauseCamera();
        }

        private void OnScanButtonPressed(object sender, EventArgs e)
        {
            if (!SBSDK.IsLicenseValid && !IsDetectionOn)
            {
                ShowExpiredLicenseAlert();
                return;
            }

            IsDetectionOn = !IsDetectionOn;
            cameraView.IsVisible = true;
            cameraViewImage.IsVisible = false;
        }

        private void PauseCamera()
        {
            IsDetectionOn = false;
            cameraView.Pause();
        }

        private void RefreshCamera()
        {
            if (IsDetectionOn)
            {
                cameraView.StartDetection();
                resultsLabel.Text = string.Empty;
            }
            else
            {
                cameraView.StopDetection();
            }
        }

        private void OnInfoButtonPressed(object sender, EventArgs e)
        {
            DisplayAlert("Info", SBSDK.IsLicenseValid ? "Your SDK License is valid." : "Your SDK License has expired.", "Close");
        }

        private void ShowExpiredLicenseAlert()
        {
            DisplayAlert("Error", "Your SDK license has expired", "Close");
        }

        private void ShowTrialLicenseAlert()
        {
            DisplayAlert("Welcome", "You are using the Trial SDK License. The SDK will be active for one minute.", "Close");
        }

        private void SetupIOSAppearance()
        {
            if (Device.RuntimePlatform != Device.iOS) { return; }

            var safeInsets = On<iOS>().SafeAreaInsets();
            safeInsets.Bottom = 0;
            Padding = safeInsets;
        }

        private void HandleBarcodeScanningResult(BarcodeScanningResult result)
        {
            string text = string.Empty;
            Barcode barcodeItem = null;
            foreach (Barcode barcode in result.Barcodes)
            {
                text += string.Format("{0} ({1})\n", barcode.Text, barcode.Format.ToString().ToUpper());
                barcodeItem = barcode;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                resultsLabel.Text = text;
                if (cameraView.ImageGenerationType == BarcodeImageGenerationType.CapturedImage)
                {
                    IsDetectionOn = false;
                    cameraView.IsVisible = false;
                    cameraViewImage.IsVisible = true;
                    cameraViewImage.Source = barcodeItem?.Image;
                    cameraViewImage.Aspect = Aspect.AspectFit;
                }
            });
        }
    }
}
