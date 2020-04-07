using System;
using System.Collections.Generic;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class MainPage : ContentPage
    {
        StackLayout Container { get; set; }
        Image Image { get; set; }

        public MainPage()
        {
            Title = "SCANBOT SDK EXAMPLE";

            Container = new StackLayout();
            Container.Orientation = StackOrientation.Vertical;
            Container.BackgroundColor = Color.White;

            Image = new Image
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.LightGray,
                Aspect = Aspect.AspectFit
            };
            Image.SizeChanged += delegate
            {
                if (SelectedPage != null) {
                    // Don't allow images larger than a third of the screen
                    Image.HeightRequest = Content.Height / 3;
                }
            };
            Container.Children.Add(Image);

            var table = new TableView();
            table.BackgroundColor = Color.White;
            Container.Children.Add(table);

            table.Root = new TableRoot();
            table.Root.Add(new TableSection("SCAN DOCUMENTS")
            {
                ViewUtils.CreateCell("SCANNING UI", ScanningUIClicked()),
                ViewUtils.CreateCell("IMPORT IMAGE", ImportButtonClicked())
            });
            table.Root.Add(new TableSection("DETECT DATA")
            {
                ViewUtils.CreateCell("BASIC DATA DETECTOR", ScanningUIClicked())
            });

            Content = Container;
        }

        IScannedPage _selectedPage;
        public IScannedPage SelectedPage
        {
            get => _selectedPage;
            set
            {
                _selectedPage = value;
                Image.Source = _selectedPage.Original;
            }
        }

        public List<IScannedPage> Pages { get; set; } = new List<IScannedPage>();

        EventHandler ScanningUIClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                var configuration = new DocumentScannerConfiguration
                {
                    CameraPreviewMode = CameraPreviewMode.FitIn,
                    IgnoreBadAspectRatio = true,
                    MultiPageEnabled = true,
                    PolygonColor = Color.Red,
                    PolygonColorOK = Color.Green,
                    BottomBarBackgroundColor = Color.Blue,
                    PageCounterButtonTitle = "%d Page(s)",
                    //DocumentImageSizeLimit = new Size(2000, 3000),
                    // see further customization configs...
                };
                var result = await SBSDK.UI.LaunchDocumentScannerAsync(configuration);
                if (result.Status == OperationResult.Ok)
                {
                    Pages.Clear();
                    foreach (var page in result.Pages)
                        Pages.Add(page);
                    SelectedPage = Pages[0];
                }
            };
        }

        EventHandler ImportButtonClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }

                ImageSource source = await ImagePicker.Forms.ImagePicker.Instance.Pick();
                if (source != null)
                {
                    // Import the selected image as original image and create a Page object
                    var importedPage = await SBSDK.Operations.CreateScannedPageAsync(source);
                    // Run document detection on it
                    await importedPage.DetectDocumentAsync();
                    SelectedPage = importedPage;
                    Pages.Add(SelectedPage);
                }
            };
        }

    }
}
