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
}
