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
        public ListView List { get; private set; }
        
        public BottomActionBar BottomBar { get; private set; }

        public ImageResultsPage()
        {
            Title = "Image Results";
            List = new ListView();
            List.ItemTemplate = new DataTemplate(typeof(ImageResultCell));
            
            List.RowHeight = 120;
            List.BackgroundColor = Color.White;

            BottomBar = new BottomActionBar(false);
            
            Content = new StackLayout
            {
                Children = { List, BottomBar }
            };

            BottomBar.AddClickEvent(BottomBar.AddButton, OnAddButtonClick);
            BottomBar.AddClickEvent(BottomBar.SaveButton, OnSaveButtonClick);
            BottomBar.AddClickEvent(BottomBar.DeleteAllButton, OnDeleteButtonClick);

            List.ItemTapped += OnItemClick;
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
                PageCounterButtonTitle = "%d Page(s)"
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
            var parameters = new string[] {"PDF", "PDF with OCR", "TIFF (1-bit, B&W" };
            string action = await DisplayActionSheet("Save Image as", "Cancel", null, parameters);

            if (action == null || action.Equals("Cancel"))
            {
                return;
            }

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
                string filename = Path.Combine(path, Guid.NewGuid() + ".txt");
                var languages = new[] { "en" };
                var result = await SBSDK.Operations.PerformOcrAsync(Pages.Instance.DocumentSources, languages, filename);
                // Or do something else with the results
                ViewUtils.Alert(this, "Wrote OCR Documents to: ", filename);
            }
            else if (action.Equals(parameters[2]))
            {
                var fileUri = await SBSDK.Operations
                .WriteTiffAsync(Pages.Instance.DocumentSources, new TiffOptions { OneBitEncoded = true });
                ViewUtils.Alert(this, "Success: ", "Wrote documents to: " + fileUri.AbsolutePath);
            }
        }

        private void OnDeleteButtonClick(object sender, EventArgs e)
        {
            Pages.Instance.List.Clear();
            ReloadData();
        }

        void ReloadData()
        {
            List.ItemsSource = null;
            List.ItemsSource = Pages.Instance.List;
        }
    }
}
