using System;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class ImageDetailPage : ContentPage
    {
        public Image Image { get; private set; }

        public BottomActionBar BottomBar { get; private set; }

        public ImageFilter CurrentFilter { get; set; }

        public ImageDetailPage()
        {
            Image = new Image
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.LightGray,
                Aspect = Aspect.AspectFit,
            };
            Image.SizeChanged += delegate
            {
                // Don't allow images larger than 2/3 of the screen
                Image.HeightRequest = Content.Height / 3 * 2;
            };
            
            BottomBar = new BottomActionBar(true);

            Content = new StackLayout
            {
                Children = { Image, BottomBar }
            };

            BottomBar.AddClickEvent(BottomBar.CropButton, OnCropButtonClick);
            BottomBar.AddClickEvent(BottomBar.FilterButton, OnFilterButtonClick);
            BottomBar.AddClickEvent(BottomBar.DeleteButton, OnDeleteButtonClick);

            LoadImage();
        }

        async void LoadImage()
        {
            // If encryption is enabled, load the decrypted document.
            // Else accessible via Document or DocumentPreview
            Image.Source = await Pages.Instance.SelectedPage.DecryptedDocument();
        }

        async void OnCropButtonClick(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }
            if (!SDKUtils.CheckPage(this, Pages.Instance.SelectedPage)) { return; }

            var result = await SBSDK.UI.LaunchCroppingScreenAsync(Pages.Instance.SelectedPage);
            await Pages.Instance.UpdateSelection();

            Image.Source = null;
            LoadImage();
        }

        async void OnFilterButtonClick(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }
            if (!SDKUtils.CheckPage(this, Pages.Instance.SelectedPage)) { return; }

            var buttons = Enum.GetNames(typeof(ImageFilter));
            var action = await DisplayActionSheet("Filter", "Cancel", null, buttons);

            ImageFilter filter;
            Enum.TryParse(action, out filter);
            CurrentFilter = filter;

            await Pages.Instance.UpdateFilterForSelection(filter);
            LoadImage();
        }

        async void OnDeleteButtonClick(object sender, EventArgs e)
        {
            await Pages.Instance.RemoveSelection();
            Image.Source = null;
            await Navigation.PopAsync();
        }

    }
}
