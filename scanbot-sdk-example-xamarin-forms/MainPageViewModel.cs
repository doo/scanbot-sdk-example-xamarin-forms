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
                var result = await SBSDK.Instance.LaunchDocumentScannerAsync();
                if (result != null && result.Length > 0)
                {
                    page = result[0];
                    DocumentImageSource = page.Document;
                }
            });
            OpenCroppingScreenCommand = new Command(async () => 
            {
                if (page == null)
                {
                    return;
                }
                var result = await SBSDK.Instance.LaunchCroppingScreenAsync(page);
                if (result != null)
                {
                    page = result;
                    DocumentImageSource = page.Document;
                }
            });
            PickPhotoCommand = new Command(async () =>
            {
                var image = await DependencyService.Get<IImagePicker>().PickImageAsync();
                if (image != null)
                {
                    page = await SBSDK.Instance.CreateScannedPageAsync(image);
                    DocumentImageSource = page.Original;
                }
            });

            StartOcrServiceCommand = new Command(async () =>
            {
                if (DocumentImageSource == null)
                {
                    return;
                }
                var result = await SBSDK.Instance.PerformOCRAsync(new[] { DocumentImageSource }, new[] { "en", "de" });
                MessagingCenter.Send(new AlertMessage { Message = result, Title = "OCR" }, AlertMessage.ID);
            });

            OpenBarcodeScannerCommand = new Command(async () =>
            {
                var result = await SBSDK.Instance.LaunchBarcodeScannerAsync();
                if (result != null)
                {
                    MessagingCenter.Send(new AlertMessage { Message = result.Text, Title = result.Format.ToString() }, AlertMessage.ID);
                }
            });

            OpenMrzScannerCommand = new Command(async () =>
            {
                var result = await SBSDK.Instance.LaunchMrzScannerAsync();
                if (result != null)
                {
                    var sb = new StringBuilder();
                    foreach (var field in result)
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
                    var document = SBSDK.Instance.ApplyImageFilterAsync(page.Document, ImageFilter.Binarized);
                    await page.SetDocumentAsync(await document);
                    DocumentImageSource = page.Document;
                }
            });
        }

        void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
