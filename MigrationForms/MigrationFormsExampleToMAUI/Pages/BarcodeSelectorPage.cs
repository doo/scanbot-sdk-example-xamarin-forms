using MigrationFormsExampleToMAUI.Model;
using MigrationFormsExampleToMAUI.Subviews.Cells;

namespace MigrationFormsExampleToMAUI.Pages;

public class BarcodeSelectorPage : ContentPage
{
    public BarcodeSelectorPage()
    {
        Title = "ACCEPTED BARCODES";
        var list = new ListView();
        list.ItemTemplate = new DataTemplate(typeof(BarcodeFormatCell));
        list.ItemsSource = BarcodeTypes.Instance.List;
        list.RowHeight = 50;
        list.BackgroundColor = Colors.White;

        Content = list;
    }
}