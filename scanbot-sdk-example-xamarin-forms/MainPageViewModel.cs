using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenScanningUiCommand { get; }
        public ICommand OpenCroppingScreenCommand { get; }
        public ICommand PickPhotoCommand { get; }
        public ICommand StartOcrServiceCommand { get; }
        public ICommand OpenBarcodeScannerCommand { get; }
        public ICommand OpenMrzScannerCommand { get; }
        public ICommand ApplyFilterCommand { get; }

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

        IScannedPage page;

        public MainPageViewModel()
        {
            OpenScanningUiCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense()) { return; }
                var result = await SBSDK.UI.LaunchDocumentScannerAsync();
                if (result.Status == OperationResult.Ok)
                {
                    page = result.Pages[0];
                    DocumentImageSource = page.Document;
                }
            });
            OpenCroppingScreenCommand = new Command(async () => 
            {
                if (!CheckScanbotSDKLicense()) { return; }
                if (page == null)
                {
                    return;
                }
                var result = await SBSDK.UI.LaunchCroppingScreenAsync(page);
                if (result.Status == OperationResult.Ok)
                {
                    page = result.Page;
                    DocumentImageSource = page.Document;
                }
            });
            PickPhotoCommand = new Command(async () =>
            {
                var image = await DependencyService.Get<IImagePicker>().PickImageAsync();
                if (image != null)
                {
                    page = await SBSDK.Operations.CreateScannedPageAsync(image);
                    DocumentImageSource = page.Original;
                }
            });

            StartOcrServiceCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense()) { return; }
                if (DocumentImageSource == null)
                {
                    return;
                }
                var result = await SBSDK.Operations.PerformOcrAsync(new[] { DocumentImageSource }, new[] { "en", "de" });
                MessagingCenter.Send(new AlertMessage { Message = result, Title = "OCR" }, AlertMessage.ID);
            });

            OpenBarcodeScannerCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense()) { return; }
                var result = await SBSDK.UI.LaunchBarcodeScannerAsync();
                if (result.Status == OperationResult.Ok)
                {
                    MessagingCenter.Send(new AlertMessage { Message = result.Text, Title = result.Format.ToString() }, AlertMessage.ID);
                }
            });

            OpenMrzScannerCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense()) { return; }
                var result = await SBSDK.UI.LaunchMrzScannerAsync();
                if (result.Status == OperationResult.Ok)
                {
                    var sb = new StringBuilder();
                    foreach (var field in result.Fields)
                    {
                        sb.AppendLine($"{field.Name}: {field.Value} ({field.Confidence:F2})");
                    }
                    MessagingCenter.Send(new AlertMessage { Message = sb.ToString(), Title = "MRZ" }, AlertMessage.ID);
                }
            });

            ApplyFilterCommand = new Command(async () =>
            {
                if (page == null)
                {
                    return;
                }

                if (page.Document != null)
                {
                    var document = await SBSDK.Operations.ApplyImageFilterAsync(page.Document, ImageFilter.Binarized);
                    DocumentImageSource = await page.SetDocumentAsync(document);
                }
            });
        }

        void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        bool CheckScanbotSDKLicense()
        {
            if (SBSDK.Operations.IsLicenseValid)
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
