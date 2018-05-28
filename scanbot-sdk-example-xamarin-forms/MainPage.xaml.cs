using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public partial class scanbot_sdk_example_xamarin_formsPage : ContentPage
    {
        public scanbot_sdk_example_xamarin_formsPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<AlertMessage>(this, AlertMessage.ID, (msg) =>
            {
                DisplayAlert(msg.Title, msg.Message, "OK");
            });
        }
    }
}
