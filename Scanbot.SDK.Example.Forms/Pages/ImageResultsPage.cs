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
            BackgroundColor = App.ScanbotRed;
            HorizontalOptions = LayoutOptions.FillAndExpand;

            AddButton(new BottomActionButton("add.png", "ADD"));
            AddButton(new BottomActionButton("save.png", "SAVE"));
            AddButton(new BottomActionButton("delete.png", "DELETE ALL"), true);
        }

        void AddButton(BottomActionButton button, bool alignRight = false)
        {
            button.HeightRequest = 50;
            if (alignRight)
            {
                button.HorizontalOptions = LayoutOptions.EndAndExpand;
            }
            
            Children.Add(button);
        }
    }

    public class BottomActionButton : StackLayout
    {
        public Image Image { get; private set; }

        public Label Label { get; private set; }

        public BottomActionButton(string resource, string text)
        {
            Orientation = StackOrientation.Horizontal;

            Image = new Image();
            Image.Source = resource;
            Image.WidthRequest = 26;
            Image.HeightRequest = 26;
            Image.Margin = new Thickness(3, 12, 3, 12);
            Image.VerticalOptions = LayoutOptions.Center;
            Children.Add(Image);

            Label = new Label();
            Label.Text = text;
            Label.TextColor = Color.White;
            Label.VerticalOptions = LayoutOptions.Center;
            Label.Margin = new Thickness(0, 0, 5, 0);
            Children.Add(Label);
        }
    }
}
