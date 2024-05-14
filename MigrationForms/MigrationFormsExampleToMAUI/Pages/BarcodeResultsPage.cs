using ScanbotSDK.MAUI.Models;

namespace MigrationFormsExampleToMAUI.Pages;

public class BarcodeResultsPage: ContentPage
{
    const int ROWHEIGHT = 60;

    public ListView List { get; set; }

    public Image SnappedImage { get; set; }

    public List<Barcode> Barcodes { get; set; }

    public ActivityIndicator Loader { get; set; }

    public BarcodeResultsPage(List<Barcode> barcodes)
    {
        BackgroundColor = Colors.White;
        SetTitle();

        Barcodes = barcodes;
        InitializeList();
        Content = List;
    }

    public BarcodeResultsPage(ImageSource source, List<Barcode> barcodes = null)
    {
        SetTitle();
        BackgroundColor = Colors.White;

        var Container = new StackLayout();
        Container.Orientation = StackOrientation.Vertical;
        Container.BackgroundColor = Colors.White;
        
        Content = Container;

        SnappedImage = new Image
        {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            BackgroundColor = Colors.LightGray,
            Aspect = Aspect.AspectFit
        };
        SnappedImage.SizeChanged += delegate
        {
            if (source == null || source.IsEmpty)
            {
                // If there is no snapped image, do not show empty container
                SnappedImage.HeightRequest = 0;
                return;
            }
            // Don't allow images larger than a third of the screen
            SnappedImage.HeightRequest = Content.Height / 3;
        };
        Loader = new ActivityIndicator
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            IsEnabled = true,
            IsRunning = true,
        };

        Container.Children.Add(SnappedImage);
        Container.Children.Add(Loader);

        SnappedImage.Source = source;

        if (barcodes == null)
        {
            //var copy = Utils.Copy(source);
            DetectBarcodes(source, delegate
            {
                InitializeList();
                Container.Children.Remove(Loader);
                Container.Children.Add(List);
            });
        }
        else
        {
            Barcodes = barcodes;
            InitializeList();
            Container.Children.Remove(Loader);
            Container.Children.Add(List);
        }

    }

    async void DetectBarcodes(ImageSource source, Action callback = null)
    {
        Barcodes = await ScanbotSDK.MAUI.ScanbotSDK.DetectionService.DetectBarcodesFrom(source);
        callback();
    }

    void SetTitle()
    {
        Title = "DETECTED BARCODES";
    }

    void InitializeList()
    {
        List = new ListView();
        List.BackgroundColor = Colors.White;
        List.ItemTemplate = new DataTemplate(typeof(BarcodeCell));
        List.HasUnevenRows = true;
        List.ItemsSource = Barcodes;
    }

    public class BarcodeCell : ViewCell
    {
        public Barcode Source { get; private set; }

        public Image ImageView { get; set; }

        public Label TypeLabel { get; set; }

        public Label ContentLabel { get; set; }

        public BarcodeCell()
        {
            ImageView = new Image
            {
                Aspect = Aspect.AspectFit,
                HeightRequest = ROWHEIGHT,
                WidthRequest = ROWHEIGHT,
                BackgroundColor = Color.FromRgb(250, 250, 250)
            };

            TypeLabel = new Label
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                TextColor = Colors.Black
            };

            ContentLabel = new Label
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                TextColor = Colors.DarkGray
            };

            StackLayout labelContainer = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.StartAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = { TypeLabel, ContentLabel }
            };

            View = new StackLayout()
            {
                BackgroundColor = Colors.White,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
                Padding = new Thickness(0, 0, 10, 0),
                Children = { ImageView, labelContainer }
            };
        }

        protected override void OnBindingContextChanged()
        {
            Source = (Barcode)BindingContext;

            ImageView.Source = Source.Image;

            TypeLabel.Text = Source.Format.ToString();
            ContentLabel.Text = Source.Text;

            base.OnBindingContextChanged();
        }
    }
}

