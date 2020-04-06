using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var navigator = new NavigationPage();
            navigator.Navigation.PushAsync(new MainPage());
            MainPage = navigator;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
