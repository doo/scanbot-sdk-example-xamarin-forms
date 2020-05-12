using System;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
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
            List.ItemsSource = Pages.Instance.List;
            List.RowHeight = 120;
            List.BackgroundColor = Color.White;

            BottomBar = new BottomActionBar();
            
            Content = new StackLayout
            {
                Children = { List, BottomBar }
            };

            AddClickEvent(BottomBar.AddButton, OnAddButtonClick);
            AddClickEvent(BottomBar.SaveButton, OnSaveButtonClick);
            AddClickEvent(BottomBar.DeleteButton, OnDeleteButtonClick);
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

            if (action.Equals(parameters[0]))
            {
                if (!SDKUtils.CheckLicense(this)) { return; }
                if (!SDKUtils.CheckDocuments(this, Pages.Instance.DocumentSources)) { return; }

                var fileUri = await SBSDK.Operations
                .CreatePdfAsync(Pages.Instance.DocumentSources, PDFPageSize.FixedA4);

                ViewUtils.Alert(this, "Success: ", "Wrote documents to: " + fileUri.AbsolutePath);
            }
            else if (action.Equals(parameters[1]))
            {
                // TODO
            }
            else if (action.Equals(parameters[2]))
            {
                if (!SDKUtils.CheckLicense(this)) { return; }
                if (!SDKUtils.CheckDocuments(this, Pages.Instance.DocumentSources)) { return; }

                var fileUri = await SBSDK.Operations
                .WriteTiffAsync(Pages.Instance.DocumentSources, new TiffOptions { OneBitEncoded = true });

                ViewUtils.Alert(this, "Success: ", "Wrote documents to: " + fileUri.AbsolutePath);
            }
        }

        private void OnDeleteButtonClick(object sender, EventArgs e)
        {
            Pages.Instance.List.Clear();
            List.ItemsSource = Pages.Instance.List;
        }


        void AddClickEvent(BottomActionButton button, EventHandler action)
        {
            var recognizer = new TapGestureRecognizer();
            recognizer.Tapped += action;

            button.GestureRecognizers.Add(recognizer);
        }

        // TODO NOT HERE
        EventHandler PerformOCRClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }
                if (!SDKUtils.CheckPage(this, Pages.Instance.SelectedPage)) { return; }

                // or specify more languages like { "en", "de", ... }
                var languages = new[] { "en" };
                var result = await SBSDK.Operations.PerformOcrAsync(Pages.Instance.DocumentSources, languages);
                ViewUtils.Alert(this, "OCR Results", result.Text);
            };
        }

    }
}
