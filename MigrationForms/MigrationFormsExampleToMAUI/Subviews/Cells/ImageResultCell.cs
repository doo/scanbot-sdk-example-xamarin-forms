using ScanbotSDK.MAUI.Services;

namespace MigrationFormsExampleToMAUI.Subviews.Cells;

public class ImageResultCell : ViewCell
{
    public IScannedPage Source { get; private set; }

    Image Document { get; set; }

    public ImageResultCell()
    {
        Document = new Image();
        Document.Margin = new Thickness(10, 10, 10, 10);

        View = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            Children = { Document  }
        };
    }

    protected override async void OnBindingContextChanged()
    {
        if (BindingContext == null)
        {
            return;
        }
        Source = (IScannedPage)BindingContext;
        // If encryption is enabled, load the decrypted document.
        // Else accessible via page.Document
        Document.Source = await Source.DecryptedDocumentPreview();
        //Document.Source = Source.DocumentPreview;
        base.OnBindingContextChanged();
    }

}