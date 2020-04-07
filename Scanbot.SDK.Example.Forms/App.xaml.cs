using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scanbot.SDK.Example.Forms
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var content = new MainPage();
            MainPage = new NavigationPage(content)
            {
                BarBackgroundColor = Color.FromRgb(200, 25, 60),
                BarTextColor = Color.White
            };
        }
    }
}
