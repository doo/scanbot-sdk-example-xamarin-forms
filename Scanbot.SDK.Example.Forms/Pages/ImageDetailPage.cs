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

            BottomBar.ButtonClicked += OnBottomBar_Clicked;

            LoadImage();
        }

        private void OnBottomBar_Clicked(string buttonTitle)
        {
            if (!SDKUtils.CheckLicense(this)) { return; }
            if (!SDKUtils.CheckPage(this, Pages.Instance.SelectedPage)) { return; }

            switch (buttonTitle)
            {
                case BottomActionBar.CROP:
                    OnCropButtonClick();
                    break;

                case BottomActionBar.FILTER:
                    OnFilterButtonClick();
                    break;

                case BottomActionBar.QUALITY:
                    OnCheckQualityClick();
                    break;

                case BottomActionBar.DELETE:
                    OnDeleteButtonClick();
                    break;
            }
        }

        async void LoadImage()
        {
            // If encryption is enabled, load the decrypted document.
            // Else accessible via Document or DocumentPreview
            if (App.IsEncryptionEnabled)
            {
                Image.Source = await Pages.Instance.SelectedPage.DecryptedDocument();
            }
            else
            {
                Image.Source = Pages.Instance.SelectedPage.Document;
            }
        }

        async void OnCropButtonClick()
        {
            await SBSDK.UI.LaunchCroppingScreenAsync(Pages.Instance.SelectedPage);
            await Pages.Instance.UpdateSelection();

            Image.Source = null;
            LoadImage();
        }

        async void OnFilterButtonClick()
        {
            var buttons = Enum.GetNames(typeof(ImageFilter));
            var action = await DisplayActionSheet("Filter", "Cancel", null, buttons);

            ImageFilter filter;
            Enum.TryParse(action, out filter);
            CurrentFilter = filter;

            await Pages.Instance.UpdateFilterForSelection(filter);
            LoadImage();
        }

        async void OnDeleteButtonClick()
        {
            await Pages.Instance.RemoveSelection();
            Image.Source = null;
            await Navigation.PopAsync();
        }

        private async void OnCheckQualityClick()
        {
            ImageSource imageSource;
            // Please use the DecryptedDocunent() to retrieve the document when encryption is enabled.
            if (App.IsEncryptionEnabled)
            {
                imageSource = await Pages.Instance.SelectedPage.DecryptedDocument();
            }
            else
            {
                imageSource = Pages.Instance.SelectedPage.Document;
            }
           
            var quality = await SBSDK.Operations.DetectDocumentQualityAsync(imageSource);
            await DisplayAlert("Alert", "The Document Quality is: " + quality.ToString(), "Ok");
        }
    }
}
