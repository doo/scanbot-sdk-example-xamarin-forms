using System;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class ViewUtils
    {
        public static ViewCell CreateCell(string title, EventHandler action)
        {
            var cell = new ViewCell
            {
                View = new Label
                {
                    Text = title,
                    VerticalTextAlignment = TextAlignment.Center,
                    Margin = new Thickness(20, 0, 0, 0),
                    FontSize = 12
                }
            };

            cell.Tapped += action;

            return cell;
        }
    }
}
