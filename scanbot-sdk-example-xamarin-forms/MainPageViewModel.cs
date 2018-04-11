using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using scanbotsdkexamplexamarinforms.Services;
using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        IScanbotSdkFeatureService ScanbotSdkFeatureService;

        public ICommand OpenScanningUiCommand { get; }
        public ICommand StartOcrServiceCommand { get; }

        Uri documentImageFileUri, originalImageFileUri;

        ImageSource _documentImageSource;
        public ImageSource DocumentImageSource
        { 
            get
            {
                return _documentImageSource;
            }
            private set 
            {
                _documentImageSource = value;
                NotifyPropertyChanged("DocumentImageSource");
            }
        }


        public MainPageViewModel()
        {
            ScanbotSdkFeatureService = DependencyService.Get<IScanbotSdkFeatureService>();

            MessagingCenter.Subscribe<ScanbotCameraResultMessage>(this, ScanbotCameraResultMessage.ID, (msg) => {
                System.Diagnostics.Debug.WriteLine("documentImageFileUri: " + msg.DocumentImageFileUri);
                System.Diagnostics.Debug.WriteLine("originalImageFileUri: " + msg.OriginalImageFileUri);

                documentImageFileUri = new Uri(msg.DocumentImageFileUri);
                originalImageFileUri = new Uri(msg.OriginalImageFileUri);

                DocumentImageSource = ImageSource.FromFile(documentImageFileUri.LocalPath);
            });

            OpenScanningUiCommand = new Command(() =>
            {
                if (!CheckScanbotSDKLicense()) { return; }

                ScanbotSdkFeatureService.StartScanningUi();
            });

            StartOcrServiceCommand = new Command(() =>
            {
                if (!CheckScanbotSDKLicense()) { return; }

                ScanbotSdkFeatureService.StartOcrService();
            });

        }

        void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        bool CheckScanbotSDKLicense()
        {
            if (ScanbotSdkFeatureService.IsLicenseValid())
            {
                return true;
            }

            var msg = new AlertMessage
            {
                Title = "Info",
                Message = "Scanbot SDK (trial) license has expired!"
            };
            MessagingCenter.Send(msg, AlertMessage.ID);
            return false;
        }

    }
}
