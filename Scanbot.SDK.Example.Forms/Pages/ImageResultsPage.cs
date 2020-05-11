using System;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class ImageResultsPage : ContentPage
    {
        public ImageResultsPage()
        {
            Title = "Image Results";
            var list = new ListView();
            list.ItemTemplate = new DataTemplate(typeof(ImageResultCell));
            list.ItemsSource = Pages.Instance.List;
            list.RowHeight = 120;
            list.BackgroundColor = Color.White;
            Content = list;
        }
    }
}
