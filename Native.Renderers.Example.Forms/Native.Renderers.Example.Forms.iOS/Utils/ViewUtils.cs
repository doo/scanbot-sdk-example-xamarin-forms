using UIKit;

namespace Native.Renderers.Example.Forms.iOS.Utils
{
    public static class ViewUtils
    {
        /// <summary>
        /// RootViewContoller from the view hierarchy.
        /// </summary>
        internal static UIViewController RooViewController => (UIApplication.SharedApplication?.Delegate as AppDelegate)?.Window?.RootViewController;

        // ------------------------------------------------------------------------------------------------------------------------
        // Displays a popup message with message and a single button.s
        // ------------------------------------------------------------------------------------------------------------------------
        internal static void ShowAlert(string message, string buttonTitle)
        {
            var alert = UIAlertController.Create("Alert", message, UIAlertControllerStyle.Alert);
            var action = UIAlertAction.Create(buttonTitle ?? "Ok", UIAlertActionStyle.Cancel, (obj) => { });
            alert.AddAction(action);
            RooViewController?.PresentViewController(alert, true, null);
        }
    }
}

