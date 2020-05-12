using System;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class ImageDetailPage : ContentPage
    {
        public ImageDetailPage()
        {
        }


        EventHandler CropClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }
                if (!SDKUtils.CheckPage(this, Pages.Instance.SelectedPage)) { return; }

                await SBSDK.UI.LaunchCroppingScreenAsync(Pages.Instance.SelectedPage);
                Pages.Instance.UpdateImage();
            };
        }

        private EventHandler ApplyImageFilterClicked()
        {
            return async (sender, e) =>
            {
                if (!SDKUtils.CheckLicense(this)) { return; }
                if (!SDKUtils.CheckPage(this, Pages.Instance.SelectedPage)) { return; }

                var page = new FilterPage(Pages.Instance.SelectedPage);
                await Application.Current.MainPage.Navigation.PushAsync(page);
            };
        }

    }
}
