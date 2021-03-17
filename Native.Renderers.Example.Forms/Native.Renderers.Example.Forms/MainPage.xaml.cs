
using System;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;
using System.Linq;

namespace Native.Renderers.Example.Forms
{
    public partial class MainPage : ContentPage
    {

        private bool isCameraOn = false;

        public MainPage()
        {
            InitializeComponent();
            SetupViews();
        }

        private void SetupViews()
        {
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
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            scanButton.Clicked -= OnScanButtonPressed;
            infoButton.Clicked -= OnInfoButtonPressed;
        }

        private void OnScanButtonPressed(object sender, EventArgs e)
        {
            if (!SBSDK.IsLicenseValid) {
                ShowExpiredLicenseAlert();
                return;
            }

            if (isCameraOn)
            {
                scanButton.Text = "START SCANNING";
                cameraView.Pause();
            }
            else {
                scanButton.Text = "STOP SCANNING";
                cameraView.Resume();
            }

            isCameraOn = !isCameraOn;
        }

        private void OnInfoButtonPressed(object sender, EventArgs e) {
            DisplayAlert("Info", SBSDK.IsLicenseValid ? "Your SDK License is valid." : "Your SDK License has expired.", "Close");
        }

        private void ShowExpiredLicenseAlert() {
            DisplayAlert("Error", "Your SDK license has expired", "Close");
        }
    }
}
