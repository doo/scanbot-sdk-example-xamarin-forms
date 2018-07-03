using System;
using System.Collections.Generic;
using System.Windows.Input;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public partial class FilterPage : ContentPage
    {
		public static readonly BindableProperty CurrentPageProperty = BindableProperty.Create("CurrentPage", typeof(IScannedPage), typeof(FilterPage),
                                                                                              propertyChanged: HandleBindingPropertyChangedDelegate);

        public IScannedPage CurrentPage
        {
            get => (IScannedPage)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        public static readonly BindableProperty PreviewImageProperty = BindableProperty.Create(
            propertyName: "PreviewImage",
            returnType: typeof(ImageSource),
            declaringType: typeof(FilterPage),
            defaultValue: default(ImageSource));

        public ImageSource PreviewImage
        {
            get { return (ImageSource)GetValue(PreviewImageProperty); }
            set { SetValue(PreviewImageProperty, value); }
        }

        public static readonly BindableProperty CurrentFilterProperty = BindableProperty.Create(
            propertyName: "CurrentFilter",
            returnType: typeof(ImageFilter),
            declaringType: typeof(FilterPage),
            defaultValue: default(ImageFilter));

        public ImageFilter CurrentFilter
        {
            get { return (ImageFilter)GetValue(CurrentFilterProperty); }
            set { SetValue(CurrentFilterProperty, value); }
        }

        public static readonly BindableProperty WorkingProperty = BindableProperty.Create(
            propertyName: "Working",
            returnType: typeof(bool),
            declaringType: typeof(FilterPage),
            defaultValue: false);

        public bool Working
        {
            get { return (bool)GetValue(WorkingProperty); }
            set { SetValue(WorkingProperty, value); }
        }

        public ICommand DoneCommand { get; }
        public ICommand BackCommand { get; }

        public FilterPage()
        {
            BindingContext = this;

            BackCommand = new Command(() => Navigation.PopAsync());
            DoneCommand = new Command(async () =>
            {
                using (BeginWorking())
                {
                    await CurrentPage.SetFilterAsync(CurrentFilter);
                }
                await Navigation.PopAsync();
            });

            InitializeComponent();
        }

        static void HandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
        {
            var page = newValue as IScannedPage;
            var self = (FilterPage)bindable;
            self.PreviewImage = page?.DocumentPreview;
            self.CurrentFilter = page?.Filter ?? ImageFilter.None;
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            var action = await DisplayActionSheet("Filter", "Cancel", null, Enum.GetNames(typeof(ImageFilter)));
            using (BeginWorking())
            {
                ImageFilter filter;
                Enum.TryParse(action, out filter);
                CurrentFilter = filter;
                PreviewImage = await CurrentPage.GetFilteredDocumentPreviewAsync(filter);
            }
        }

        WorkingBlock BeginWorking() => new WorkingBlock(this);

        struct WorkingBlock : IDisposable
        {
            FilterPage page;
            public WorkingBlock(FilterPage page)
            {
                this.page = page;
                page.Working = true;
            }

            public void Dispose()
            {
                page.Working = false;
            }
        }
    }
}
