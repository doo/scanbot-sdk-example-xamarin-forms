using System;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class BottomActionBar : StackLayout
    {
        internal const string CROP = "CROP";
        internal const string FILTER = "FILTER";
        internal const string QUALITY = "QUALITY";
        internal const string DELETE = "DELETE";
        internal const string ADD = "ADD";
        internal const string SAVE = "SAVE";
        internal const string DELETE_ALL = "DELETE ALL";

        public const int HEIGHT = 50;

        public Action<string> ButtonClicked;

        // Pseudo-universal bottom action bar for multiple pages:
        // These are initialized in Image Results Page
        public Button AddButton { get; private set; }
        public Button SaveButton { get; private set; }
        public Button DeleteAllButton { get; private set; }

        // Whereas these are initialized in Image Details Page
        public Button CropButton { get; private set; }
        public Button FilterButton { get; private set; }
        public Button CheckQuality { get; private set; }
        public Button DeleteButton { get; private set; }

        public BottomActionBar(bool isDetailPage)
        {
            BackgroundColor = App.ScanbotRed;
            Orientation = StackOrientation.Horizontal;
            HeightRequest = HEIGHT;
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.EndAndExpand;
            Padding = new Thickness(29, 0);

            if (isDetailPage)
            {
                CropButton = CreateButton(CROP);
                FilterButton = CreateButton(FILTER);
                CheckQuality = CreateButton(QUALITY);
                DeleteButton = CreateButton(DELETE, true);
            }
            else
            {
                AddButton = CreateButton(ADD);
                SaveButton = CreateButton(SAVE);
                DeleteAllButton = CreateButton(DELETE_ALL, true);
            }
        }

        Button CreateButton(string text, bool alignRight = false)
        {
            var button = new Button();
            button.Text = text;
            button.TextColor = Color.White;
            button.VerticalOptions = LayoutOptions.Center;
            button.Margin = new Thickness(0, 0, 5, 0);
            button.FontSize = 14;
            button.HeightRequest = HEIGHT;
            if (alignRight)
            {
                button.HorizontalOptions = LayoutOptions.EndAndExpand;
            }

            Children.Add(button);

            button.Clicked += Button_Click;
            return button;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            ButtonClicked?.Invoke((sender as Button).Text);
        }
    }
}
