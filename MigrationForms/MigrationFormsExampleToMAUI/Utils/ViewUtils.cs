namespace MigrationFormsExampleToMAUI.Utils;

public class ViewUtils
{
    public static async void Alert(ContentPage context, string title, string message)
    {
        await context.DisplayAlert(title, message, "Close");
    }

    public static ViewCell CreateCell(string title, EventHandler action, Color? color = null)
    {
        if (color == null)
        {
            color = Colors.Black;
        }

        var cell = new ViewCell
        {
            View = new Label
            {
                Text = title,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                FontSize = 14,
                TextColor = (Color)color
            }
        };

        cell.Tapped += action;

        return cell;
    }

    public static ViewCell CreateCopyrightCell()
    {
        var cell = new ViewCell
        {
            View = new Label
            {
                Text = StringUtils.CopyrightLabel,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                Padding = new Thickness(0, 25, 0, 25),
                TextColor = Colors.Gray,
                FontSize = 12
            }
        };

        return cell;
    }
}