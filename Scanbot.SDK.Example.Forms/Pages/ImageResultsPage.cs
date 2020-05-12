using System;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class ImageResultsPage : ContentPage
    {
        public ListView List { get; private set; }

        public BottomActionBar BottomBar { get; private set; }

        public ImageResultsPage()
        {
            Title = "Image Results";
            List = new ListView();
            List.ItemTemplate = new DataTemplate(typeof(ImageResultCell));
            List.ItemsSource = Pages.Instance.List;
            List.RowHeight = 120;
            List.BackgroundColor = Color.White;

            BottomBar = new BottomActionBar();
            
            Content = new StackLayout
            {
                Children = { List, BottomBar }
            };
        }

    }
}
