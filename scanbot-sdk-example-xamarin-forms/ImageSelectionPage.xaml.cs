using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public partial class ImageSelectionPage : ContentPage
    {
        public static readonly BindableProperty PagesProperty = BindableProperty.Create(
            propertyName: "Pages",
            returnType: typeof(IEnumerable<IScannedPage>),
            declaringType: typeof(ImageSelectionPage),
            defaultValue: default(IEnumerable<IScannedPage>));

        public IEnumerable<IScannedPage> Pages
        {
            get { return (IEnumerable<IScannedPage>)GetValue(PagesProperty); }
            set { SetValue(PagesProperty, value); }
        }

        public ICommand RemoveCommand { get; }
        public ICommand RemoveAllCommand { get; }
        public ICommand BackCommand { get; }

        public ImageSelectionPage()
        {
            this.BindingContext = this;

            RemoveCommand = new Command(async (arg) =>
            {
                var page = (IScannedPage)arg;
                await page.RemoveAsync();
                Pages = await SBSDK.Operations.GetAllPagesAsync();
            });

            RemoveAllCommand = new Command(async () =>
            {
                await SBSDK.Operations.CleanUp();
                Pages = await SBSDK.Operations.GetAllPagesAsync();
            });

            BackCommand = new Command(() => Navigation.PopAsync());

            InitializeComponent();

            Task.Run(async () =>
            {
                Pages = await SBSDK.Operations.GetAllPagesAsync();
            });
        }

        public event EventHandler<PageEventArgs> PageSelected;

        void Handle_Tapped(object sender, EventArgs e)
        {
            var page = ((Element)sender).BindingContext as IScannedPage;

            PageSelected?.Invoke(this, new PageEventArgs
            {
                Page = page
            });
        }
    }

    public class PageEventArgs : EventArgs
    {
        public IScannedPage Page { get; set; }
    }
}
