using System;
using System.Collections.Generic;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class BarcodeSelectorPage : ContentPage
    {
        public BarcodeSelectorPage()
        {
            Title = "ACCEPTED BARCODES";
            var list = new ListView();
            list.ItemTemplate = new DataTemplate(typeof(BarcodeFormatCell));
            list.ItemsSource = BarcodeTypes.Instance.List;
            list.RowHeight = 50;
            list.BackgroundColor = Color.White;

            Content = list;
        }
    }

    public class BarcodeFormatCell : ViewCell
    {
        public KeyValuePair<BarcodeFormat, bool> Source { get; private set; }

        public Label Label { get; set; }

        public Switch Switch { get; set; }

        public BarcodeFormatCell()
        {
            Label = new Label()
            {
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                TextColor = Color.Black
            };

            Switch = new Switch
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            View = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
                Margin = new Thickness(0, 0, 10, 0),
                Children = { Label, Switch }
            };

            Switch.Toggled += delegate
            {
                BarcodeTypes.Instance.Update(Source.Key, Switch.IsToggled);
            };
        }

        protected override void OnBindingContextChanged()
        {
            Source = (KeyValuePair<BarcodeFormat, bool>)BindingContext;
            Label.Text = Source.Key.ToString();
            Switch.IsToggled = Source.Value;

            base.OnBindingContextChanged();
        }
    }
}
