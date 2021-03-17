
using System;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;
using System.Linq;
using Native.Renderers.Example.Forms.Common;

namespace Native.Renderers.Example.Forms
{
    public partial class MainPage : ContentPage
    {

        private bool isCameraOn;
        private bool IsCameraOn {
            get => isCameraOn;
            set {
                isCameraOn = value;
                scanButton.Text = value ? "STOP SCANNING" : "START SCANNING";
            }
        }

        public MainPage()
        {
            InitializeComponent();
            SetupViews();
        }

        private void SetupViews()
        {

            // Determines whether the camera should be ON or OFF at start
            IsCameraOn = false;

            // Here we determine what should happen when the Scanner returns a valid result
            cameraView.OnBarcodeScanResult = (result) =>
            {
                string text = "";
                foreach (string barcodeText in result.Barcodes.Select((item) => item.Text))
                {
                    text += barcodeText + "\n";
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    resultsLabel.Text = text;
                });
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            scanButton.Clicked += OnScanButtonPressed;
            infoButton.Clicked += OnInfoButtonPressed;

            // If the camera is ON we tell it to START, since the Page will soon be visible
            if (IsCameraOn)
            {
                cameraView.Resume();
            }

            if (ScanbotSDKConfiguration.LICENSE_KEY.Equals("")) {
                ShowTrialLicenseAlert();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            scanButton.Clicked -= OnScanButtonPressed;
            infoButton.Clicked -= OnInfoButtonPressed;

            // If the camera is ON we tell it to STOP, since the Page will soon not be visible anymore
            if (IsCameraOn) {
                cameraView.Pause();
            }
        }

        private void OnScanButtonPressed(object sender, EventArgs e)
        {
            if (!SBSDK.IsLicenseValid) {
                ShowExpiredLicenseAlert();
                return;
            }

            if (IsCameraOn)
            {
                cameraView.Pause();
            }
            else {
                cameraView.Resume();
            }

            IsCameraOn = !IsCameraOn;
        }

        private void OnInfoButtonPressed(object sender, EventArgs e) {
            DisplayAlert("Info", SBSDK.IsLicenseValid ? "Your SDK License is valid." : "Your SDK License has expired.", "Close");
        }

        private void ShowExpiredLicenseAlert() {
            DisplayAlert("Error", "Your SDK license has expired", "Close");
        }

        private void ShowTrialLicenseAlert() {
            DisplayAlert("Welcome", "You are using the Trial SDK License. The SDK will be active for one minute.", "Close");
        }
    }
}
