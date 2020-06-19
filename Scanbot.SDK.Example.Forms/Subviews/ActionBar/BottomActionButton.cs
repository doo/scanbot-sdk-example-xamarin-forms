using System;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
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
            Label.FontSize = 14;
            Children.Add(Label);
        }
    }
}
