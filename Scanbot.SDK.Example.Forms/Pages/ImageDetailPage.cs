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
                Source = Pages.Instance.SelectedPage.Document
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
        }

        async void OnCropButtonClick(object sender, EventArgs e)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }
            if (!SDKUtils.CheckPage(this, Pages.Instance.SelectedPage)) { return; }

            var result = await SBSDK.UI.LaunchCroppingScreenAsync(Pages.Instance.SelectedPage);

            Image.Source = null;
            Image.Source = Pages.Instance.SelectedPage.Document;
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

            var image = await SBSDK.Operations.ApplyImageFilterAsync(Image.Source, filter);
            await Pages.Instance.SelectedPage.SetFilterAsync(filter);

            if (CurrentFilter == ImageFilter.None)
            {
                Image.Source = Pages.Instance.SelectedPage.Document;
            }
            else
            {
                Image.Source = image;
            }
        }

        async void OnDeleteButtonClick(object sender, EventArgs e)
        {
            Pages.Instance.RemoveCurrent();
            Image.Source = null;
            await Navigation.PopAsync();
        }

    }
}
