using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scanbot.SDK.Example.Forms
{
    public partial class App : Application
    {
        public static Color ScanbotRed = Color.FromRgb(200, 25, 60);

        public App()
        {
            InitializeComponent();
            InitializeAsync();

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
