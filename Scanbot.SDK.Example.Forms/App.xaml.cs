using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scanbot.SDK.Example.Forms
{
    public partial class App : Application
    {
        public static Color ScanbotRed = Color.FromRgb(200, 25, 60);

        public static bool IsEncryptionEnabled = false;

        public App()
        {
            InitializeComponent();
#pragma warning disable CS4014
            // There's no requirement to await this, can just disable warning
            InitializeAsync();
#pragma warning restore CS4014 

            var content = new MainPage();
            MainPage = new NavigationPage(content)
            {
                BarBackgroundColor = ScanbotRed,
                BarTextColor = Color.White
            };
        }

        async Task<bool> InitializeAsync()
        {
            await PageStorage.Instance.InitializeAsync();
            await Pages.Instance.LoadFromStorage();
            return true;
        }
    }
}
