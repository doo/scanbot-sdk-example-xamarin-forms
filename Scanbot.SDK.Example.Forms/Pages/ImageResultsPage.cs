using System;
using System.IO;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class ImageResultsPage : ContentPage
    {
        public StackLayout Stack { get; private set; }

        public ListView List { get; private set; }
        
        public BottomActionBar BottomBar { get; private set; }

        public ActivityIndicator Loader { get; set; }

        public ImageResultsPage()
        {
            Title = "Image Results";
            List = new ListView();
            List.ItemTemplate = new DataTemplate(typeof(ImageResultCell));
            
            List.RowHeight = 120;
            List.BackgroundColor = Color.White;

            BottomBar = new BottomActionBar(false);

            Loader = new ActivityIndicator
            {
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                HeightRequest = Application.Current.MainPage.Height / 3 * 2,
                WidthRequest = Application.Current.MainPage.Width,
                Color = App.ScanbotRed,
                IsRunning = true,
                IsEnabled = true,
                Scale = (DeviceInfo.Platform == DevicePlatform.iOS) ? 2 : 0.3
            };

            Stack = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Spacing = 0,
                Children = { List, BottomBar }
            };
            
            Content = new AbsoluteLayout
            {
                Children = { Loader, Stack }
            };

            BottomBar.AddClickEvent(BottomBar.AddButton, OnAddButtonClick);
            BottomBar.AddClickEvent(BottomBar.SaveButton, OnSaveButtonClick);
            BottomBar.AddClickEvent(BottomBar.DeleteAllButton, OnDeleteButtonClick);

            List.ItemTapped += OnItemClick;

            (Content as AbsoluteLayout).SizeChanged += Content_SizeChanged;
        }

        private void Content_SizeChanged(object sender, EventArgs e)
        {
            // Content (AbsoluteLayout) understands its actual size,
            // but child (StackLayout) for some reason thinks its size equals Device DP
            // Fix it after parent has figured out its actual size
            var container = (AbsoluteLayout)sender;
            Stack.HeightRequest = container.Height;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ReloadData();
        }

        private void OnItemClick(object sender, ItemTappedEventArgs e)
        {
            Pages.Instance.SelectedPage = (IScannedPage)e.Item;
            Navigation.PushAsync(new ImageDetailPage());
        }

        async void OnAddButtonClick(object sender, EventArgs e)
        {
            var configuration = new DocumentScannerConfiguration
            {
                CameraPreviewMode = CameraPreviewMode.FitIn,
                IgnoreBadAspectRatio = true,
                MultiPageEnabled = true,
                PolygonColor = Color.Red,
                PolygonColorOK = Color.Green,
                BottomBarBackgroundColor = Color.Blue,
                PageCounterButtonTitle = "%d Page(s)",
                
            };
            var result = await SBSDK.UI.LaunchDocumentScannerAsync(configuration);
            if (result.Status == OperationResult.Ok)
            {
                foreach (var page in result.Pages)
                {
                    Pages.Instance.List.Add(page);
                    
                }
            }
        }

        async void OnSaveButtonClick(object sender, EventArgs e)
        {
            var parameters = new string[] {"PDF", "PDF with OCR", "TIFF (1-bit, B&W)" };
            string action = await DisplayActionSheet("Save Image as", "Cancel", null, parameters);

            if (action == null || action.Equals("Cancel"))
            {
                return;
            }

            (Content as AbsoluteLayout).RaiseChild(Loader);
            if (!SDKUtils.CheckLicense(this)) { return; }
            if (!SDKUtils.CheckDocuments(this, Pages.Instance.DocumentSources)) { return; }

            if (action.Equals(parameters[0]))
            {
                var fileUri = await SBSDK.Operations
                .CreatePdfAsync(Pages.Instance.DocumentSources, PDFPageSize.FixedA4);
                ViewUtils.Alert(this, "Success: ", "Wrote documents to: " + fileUri.AbsolutePath);
            }
            else if (action.Equals(parameters[1]))
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string pdfFilePath = Path.Combine(path, Guid.NewGuid() + ".pdf");
                var languages = new[] { "en" };
                var result = await SBSDK.Operations.PerformOcrAsync(Pages.Instance.DocumentSources, languages, pdfFilePath);
                // Or do something else with the results: result.Pages...
                ViewUtils.Alert(this, "PDF with OCR layer stored: ", pdfFilePath);
            }
            else if (action.Equals(parameters[2]))
            {
                var fileUri = await SBSDK.Operations.WriteTiffAsync(
                    Pages.Instance.DocumentSources,
                    new TiffOptions { OneBitEncoded = true, Dpi = 300, Compression = TiffCompressionOptions.CompressionCcittT6 }
                );
                ViewUtils.Alert(this, "Success: ", "Wrote documents to: " + fileUri.AbsolutePath);
            }

            (Content as AbsoluteLayout).LowerChild(Loader);

        }

        private async void OnDeleteButtonClick(object sender, EventArgs e)
        {
            var message = "Do you really want to delete all image data?";
            var result = await this.DisplayAlert("Attention!", message, "Yes", "No");
            if (result)
            {
                await Pages.Instance.Clear();
                await SBSDK.Operations.CleanUp();
                ReloadData();
            }
            
        }

        void ReloadData()
        {
            List.ItemsSource = null;
            List.ItemsSource = Pages.Instance.List;
        }
    }
}
