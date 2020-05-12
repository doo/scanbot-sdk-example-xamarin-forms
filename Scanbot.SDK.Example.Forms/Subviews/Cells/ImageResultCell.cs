using System;
using System.Collections.Generic;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class ImageResultCell : ViewCell
    {
        public IScannedPage Source { get; private set; }

        ImageWithLabel Document { get; set; }

        ImageWithLabel Original { get; set; }

        public ImageResultCell()
        {
            Document = new ImageWithLabel();
            Original = new ImageWithLabel();

            View = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { Document, Original }
            };
        }

        protected override void OnBindingContextChanged()
        {
            Source = (IScannedPage)BindingContext;

            Document.Label.Text = "Detected Document";
            Document.Image.Source = Source.Document;

            Original.Label.Text = "Original Image";
            Original.Image.Source = Source.Original;

            base.OnBindingContextChanged();
        }
    }

    public class ImageWithLabel : StackLayout
    {
        public Label Label { get; private set; }

        public Image Image { get; private set; }

        public ImageWithLabel()
        {
            Orientation = StackOrientation.Vertical;
            Margin = new Thickness(20, 0, 20, 0);

            Label = new Label();
            Label.TextColor = Color.Gray;
            Children.Add(Label);

            Image = new Image();
            Image.Aspect = Aspect.AspectFit;
            Image.WidthRequest = 90;
            Image.HeightRequest = 90;
            Image.BackgroundColor = Color.LightGray;
            Children.Add(Image);
        }
    }
}
