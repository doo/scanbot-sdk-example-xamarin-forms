using MigrationFormsExampleToMAUI.Utils;
using ScanbotSDK.MAUI.Constants;
using ScanbotSDK.MAUI.Services;

namespace MigrationFormsExampleToMAUI;

public class FilterPage: ContentPage
{
    public StackLayout Container { get; set; }
    public Image Image { get; set; }
    public Button FilterButton { get; set; }

    public ImageFilter CurrentFilter { get; set; }

    public IScannedPage CurrentPage { get; set; }

    public FilterPage(IScannedPage current)
    {
        CurrentPage = current;

        Container = new StackLayout();
        Container.Orientation = StackOrientation.Vertical;
        Container.BackgroundColor = Colors.White;

        Image = new Image
        {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            BackgroundColor = Colors.LightGray,
            Aspect = Aspect.AspectFit
        };
        Image.SizeChanged += delegate
        {
            // Don't allow images larger than half of the screen
            Image.HeightRequest = Content.Height / 2;
        };
        Container.Children.Add(Image);

        FilterButton = new Button
        {
            Text = "None",
            HorizontalOptions = LayoutOptions.FillAndExpand,
            HeightRequest = 50,
            Margin = new Thickness(10, 10, 10, 10)
        };
        FilterButton.Pressed += FilterButtonClicked;
        Container.Children.Add(FilterButton);

        Image.Source = CurrentPage.Document;

        Content = Container;
    }

    async void FilterButtonClicked(object sender, EventArgs e)
    {
        if (!SDKUtils.CheckLicense(this)) { return; }

        var action = await DisplayActionSheet(
            "Filter", "Cancel", null, Enum.GetNames(typeof(ImageFilter))
        );

        ImageFilter filter;
        Enum.TryParse(action, out filter);
        CurrentFilter = filter;

        Image.Source = await ScanbotSDK.MAUI.ScanbotSDK.SDKService.ApplyImageFilterAsync(Image.Source, filter);
    }
}
