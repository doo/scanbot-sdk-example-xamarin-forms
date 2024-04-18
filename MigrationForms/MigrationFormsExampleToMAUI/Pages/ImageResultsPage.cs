using MigrationFormsExampleToMAUI.Subviews.ActionBar;
using MigrationFormsExampleToMAUI.Subviews.Cells;
using MigrationFormsExampleToMAUI.Utils;
using ScanbotSDK.MAUI;
using ScanbotSDK.MAUI.Constants;
using ScanbotSDK.MAUI.Models;
using ScanbotSDK.MAUI.Services;

namespace MigrationFormsExampleToMAUI.Pages;

public class ImageResultsPage: ContentPage
{
    public StackLayout Stack { get; private set; }

    public ListView List { get; private set; }
    
    public BottomActionBar BottomBar { get; private set; }

    public ActivityIndicator Loader { get; set; }

    public ImageResultsPage()
    {
        Title = "Image Results";

        List = new ListView
        {
            ItemTemplate = new DataTemplate(typeof(ImageResultCell)),
            RowHeight = 120,
            BackgroundColor = Colors.White
        };

        BottomBar = new BottomActionBar(false);
        BottomBar.AddClickEvent(BottomBar.AddButton, OnAddButtonClick);
        BottomBar.AddClickEvent(BottomBar.SaveButton, OnSaveButtonClick);
        BottomBar.AddClickEvent(BottomBar.DeleteAllButton, OnDeleteButtonClick);

        Loader = new ActivityIndicator
        {
            VerticalOptions = LayoutOptions.CenterAndExpand,
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            Color = App.ScanbotRed,
            IsRunning = true,
            IsEnabled = true
        };

        Stack = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            Spacing = 0,
            Children = { List, BottomBar }
        };

        Content = new ScrollView
        {
            Content = Stack
        };

        List.ItemTapped += OnItemClick;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ReloadData();
    }

    private void OnItemClick(object sender, ItemTappedEventArgs e)
    {
        Model.Pages.Instance.SelectedPage = (IScannedPage)e.Item;
        Navigation.PushAsync(new ImageDetailPage());
    }

    async void OnAddButtonClick(object sender, EventArgs e)
    {
        var configuration = new DocumentScannerConfiguration
        {
            CameraPreviewMode = CameraPreviewMode.FitIn,
            IgnoreBadAspectRatio = true,
            MultiPageEnabled = true,
            PolygonColor = Colors.Red,
            PolygonColorOK = Colors.Green,
            BottomBarBackgroundColor = Colors.Blue,
            PageCounterButtonTitle = "%d Page(s)",
            
        };
        var result = await ScanbotSDK.MAUI.ScanbotSDK.ReadyToUseUIService.LaunchDocumentScannerAsync(configuration);
        if (result.Status == OperationResult.Ok)
        {
            foreach (var page in result.Pages)
            {
                Model.Pages.Instance.List.Add(page);
            }
        }
    }

    async void OnSaveButtonClick(object sender, EventArgs e)
    {
        var parameters = new[] {"PDF", "PDF with OCR", "TIFF (1-bit, B&W)" };
        string action = await DisplayActionSheet("Save Image as", "Cancel", null, parameters);

        if (action == null || action.Equals("Cancel"))
        {
            return;
        }
        
        if (!SDKUtils.CheckLicense(this)) { return; }
        if (!SDKUtils.CheckDocuments(this, Model.Pages.Instance.DocumentSources)) { return; }

        if (action.Equals(parameters[0]))
        {
            var fileUri = await ScanbotSDK.MAUI.ScanbotSDK.SDKService
            .CreatePdfAsync(Model.Pages.Instance.DocumentSources.OfType<FileImageSource>(), PDFPageSize.A4);
            ViewUtils.Alert(this, "Success: ", "Wrote documents to: " + fileUri.AbsolutePath);
        }
        else if (action.Equals(parameters[1]))
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string pdfFilePath = Path.Combine(path, Guid.NewGuid() + ".pdf");

            var ocrConfig = ScanbotSDK.MAUI.ScanbotSDK.SDKService.DefaultOcrConfig;
            // Uncomment below code to use the old OCR approach. Use [OCRMode.Legacy] and set the required [InstalledLanguages] property.
            //var languages = new List<string> { "en", "de" };
            //var ocrConfig = new OcrConfigs
            //{
            //    InstalledLanguages = languages,
            //    OcrMode = OCRMode.Legacy,
            //    LanguageDataPath = ocrConfig.LanguageDataPath
            //};

            var result = await ScanbotSDK.MAUI.ScanbotSDK.SDKService.PerformOcrAsync(
                Model.Pages.Instance.DocumentSources.OfType<FileImageSource>(), 
                ocrConfig, 
                pdfFilePath);
            // Or do something else with the result: result.Pages...
            ViewUtils.Alert(this, "PDF with OCR layer stored: ", pdfFilePath);
        }
        else if (action.Equals(parameters[2]))
        {
            var fileUri = await ScanbotSDK.MAUI.ScanbotSDK.SDKService.WriteTiffAsync(
                 Model.Pages.Instance.DocumentSources.OfType<FileImageSource>(),
                new TiffOptions { OneBitEncoded = true, Dpi = 300, Compression = TiffCompressionOptions.CompressionCcittT6 }
            );
            ViewUtils.Alert(this, "Success: ", "Wrote documents to: " + fileUri.AbsolutePath);
        }
    }

    private async void OnDeleteButtonClick(object sender, EventArgs e)
    {
        var message = "Do you really want to delete all image data?";
        var result = await DisplayAlert("Attention!", message, "Yes", "No");
        if (result)
        {
            await Model.Pages.Instance.Clear();
            await ScanbotSDK.MAUI.ScanbotSDK.SDKService.CleanUp();
            ReloadData();
        }
    }

    void ReloadData()
    {
        List.ItemsSource = null;
        List.ItemsSource = Model.Pages.Instance.List;
    }
}
