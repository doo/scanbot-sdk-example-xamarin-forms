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

    public class BottomActionBar : StackLayout
    {
        public BottomActionButton Add { get; private set; }
        public BottomActionBar()
        {
            Orientation = StackOrientation.Horizontal;

            Add = new BottomActionButton("Assets/add.png", "Add");
            Add.WidthRequest = 100;
            Add.HeightRequest = 50;
            Children.Add(Add);
        }
    }

    public class BottomActionButton : StackLayout
    {
        public Image Image { get; private set; }

        public Label Label { get; private set; }

        public BottomActionButton(string resource, string text)
        {
            BackgroundColor = App.ScanbotRed;
            Orientation = StackOrientation.Horizontal;

            var test = ImageSource.FromResource("sgd");
            var test2 = ImageSource.FromResource(resource);
            Image = new Image();
            Image.Source = ImageSource.FromResource(resource);
            Image.WidthRequest = 50;
            Image.HeightRequest = 50;
            Image.VerticalOptions = LayoutOptions.Center;
            Children.Add(Image);

            Label = new Label();
            Label.Text = text;
            Label.TextColor = Color.White;
            Label.VerticalOptions = LayoutOptions.Center;
            Children.Add(Label);
        }
    }
}
