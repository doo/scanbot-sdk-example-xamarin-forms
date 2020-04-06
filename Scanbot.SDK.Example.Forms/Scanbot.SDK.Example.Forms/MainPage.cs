using System;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class MainPage : ContentPage
    {
        StackLayout Container { get; set; }

        public MainPage()
        {
            Title = "SCANBOT SDK EXAMPLE";

            Container = new StackLayout();
            Container.Orientation = StackOrientation.Vertical;
            Container.BackgroundColor = Color.White;

            var table = new TableView();
            table.BackgroundColor = Color.White;
            Container.Children.Add(table);

            table.Root = new TableRoot();
            table.Root.Add(new TableSection("Document Scanner")
            {
                ViewUtils.CreateCell("SCANNING UI", ScanningUIClicked())
            });
            table.Root.Add(new TableSection("Data Detectors")
            {
                ViewUtils.CreateCell("BASIC DATA DETECTOR", ScanningUIClicked())
            });

            Content = Container;
        }

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

    }
}
