using System;
namespace Scanbot.SDK.Example.Forms
{
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

}
