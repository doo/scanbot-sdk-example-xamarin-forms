using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using Application = Xamarin.Forms.Application;
using ListView = Xamarin.Forms.ListView;

namespace Scanbot.SDK.Example.Forms
{
    public class ImageResultsPage : ContentPage
    {
        private const string PDF = "PDF", OCR = "Perform OCR", SandwichPDF = "Sandwiched PDF", TIFF = "TIFF (1-bit, B&W)";

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

            BottomBar.ButtonClicked += OnBottomBar_Clicked;
            List.ItemTapped += OnItemClick;

            (Content as AbsoluteLayout).SizeChanged += Content_SizeChanged;
        }

        private void OnBottomBar_Clicked(string buttonTitle)
        {
            switch (buttonTitle)
            {
                case BottomActionBar.ADD:
                    OnAddButtonClick();
                    break;

                case BottomActionBar.SAVE:
                    OnSaveButtonClick();
                    break;

                case BottomActionBar.DELETE_ALL:
                    OnDeleteButtonClick();
                    break;
            }
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

        async void OnAddButtonClick()
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

        async void OnSaveButtonClick()
        {
            string action = await DisplayActionSheet("Save Image as", "Cancel", null, new[] { PDF, OCR, SandwichPDF, TIFF });

            if (action == null || action.Equals("Cancel"))
            {
                return;
            }

            if (!SDKUtils.CheckLicense(this)) { return; }
            if (!SDKUtils.CheckDocuments(this, Pages.Instance.DocumentSources)) { return; }

            try
            {
                (Content as AbsoluteLayout).RaiseChild(Loader);
                switch (action)
                {
                    case PDF:
                        await GeneratePdfAsync();
                        break;
                    case OCR:
                        await PerformOcrAsync();
                        break;
                    case SandwichPDF:
                        await GenerateSandwichPdfAsync();
                        break;
                    case TIFF:
                        await GenerateTiffAsync();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                // Making the error prettier.
                var errorMessage = ex.Message.Substring(ex.Message.LastIndexOf(':')).Trim('{', '}');
                ViewUtils.Alert(this, "Error: ", $"An error occurred while saving the document: {errorMessage}");
            }
            finally
            {
                (Content as AbsoluteLayout).LowerChild(Loader);
            }
        }

        private async Task GeneratePdfAsync()
        {
            var fileUri = await SBSDK.Operations.CreatePdfAsync(Pages.Instance.DocumentSources.OfType<FileImageSource>(),
                         configuration: new PDFConfiguration
                         {
                             PageOrientation = PDFPageOrientation.Auto,
                             PageSize = PDFPageSize.A4,
                             PdfAttributes = new PDFAttributes
                             {
                                 Author = "Scanbot User",
                                 Creator = "ScanbotSDK",
                                 Title = "ScanbotSDK PDF",
                                 Subject = "Generating a normal PDF",
                                 Keywords = new[] { "x-platform", "ios", "android" },
                             }
                         });
            ViewUtils.Alert(this, "Success: ", "Wrote documents to: " + fileUri.AbsolutePath);
        }

        private async Task PerformOcrAsync()
        {
            // NOTE:
            // The default OCR engine is 'OcrConfig.ScanbotOCR' which is ML based. This mode doesn't expect the Langauges array.
            // If you wish to use the previous engine please use 'OcrConfig.Tesseract(...)'. The Languages array is mandatory in this mode.
            // Uncomment the below code to use the past legacy 'OcrConfig.Tesseract(...)' engine mode.
            // var ocrConfig = OcrConfig.Tesseract(withLanguageString: new List<string>{ "en", "de" });

            // Using the default OCR option
            var ocrConfig = OcrConfig.ScanbotOCR;

            var result = await SBSDK.Operations.PerformOcrAsync(Pages.Instance.DocumentSources.OfType<FileImageSource>(), configuration: ocrConfig);

            // You can access the results with: result.Pages
            ViewUtils.Alert(this, "OCR", result.Text);
        }

        private async Task GenerateSandwichPdfAsync()
        {
            // NOTE:
            // The default OCR engine is 'OcrConfig.ScanbotOCR' which is ML based. This mode doesn't expect the Langauges array.
            // If you wish to use the previous engine please use 'OcrConfig.Tesseract(...)'. The Languages array is mandatory in this mode.
            // Uncomment the below code to use the past legacy 'OcrConfig.Tesseract(...)' engine mode.
            // var ocrConfig = OcrConfig.Tesseract(withLanguageString: new List<string>{ "en", "de" });

            // Using the default OCR option
            var ocrConfig = OcrConfig.ScanbotOCR;

            var result = await SBSDK.Operations.CreateSandwichPdfAsync(
                Pages.Instance.DocumentSources.OfType<FileImageSource>(),
                new PDFConfiguration
                {
                    PageOrientation = PDFPageOrientation.Auto,
                    PageSize = PDFPageSize.A4,
                    PdfAttributes = new PDFAttributes
                    {
                        Author = "Scanbot User",
                        Creator = "ScanbotSDK",
                        Title = "ScanbotSDK PDF",
                        Subject = "Generating a sandwiched PDF",
                        Keywords = new[] { "x-platform", "ios", "android" },
                    }
                }, ocrConfig);

            ViewUtils.Alert(this, "PDF with OCR layer stored: ", result.AbsolutePath);
        }

        private async Task GenerateTiffAsync()
        {
            var fileUri = await SBSDK.Operations.WriteTiffAsync(
                    Pages.Instance.DocumentSources,
                    new TiffOptions { OneBitEncoded = true, Dpi = 300, Compression = TiffCompressionOptions.CompressionCcittT6 });

            ViewUtils.Alert(this, "Success: ", "Wrote documents to: " + fileUri.AbsolutePath);
        }

        private async void OnDeleteButtonClick()
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
