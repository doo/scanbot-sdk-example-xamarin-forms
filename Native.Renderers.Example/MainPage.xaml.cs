using System;
using System.Linq;
using Xamarin.Forms;

namespace Native.Renderers.Example
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            SetupViews();
        }

        private void SetupViews()
        {
            cameraView.OnBarcodeScanResult = (result) =>
            {
                string text = "Results: \n";
                foreach (string barcodeText in result.Barcodes.Select((item) => item.Text))
                {
                    text += "\n" + barcodeText;
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

            start.Clicked += OnStartClick;
            stop.Clicked += OnStopClick;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            start.Clicked -= OnStartClick;
            stop.Clicked -= OnStopClick;
        }

        private void OnStartClick(object sender, EventArgs e)
        {
            cameraView.Resume();
        }

        private void OnStopClick(object sender, EventArgs e)
        {
            cameraView.Pause();
        }
    }
}
