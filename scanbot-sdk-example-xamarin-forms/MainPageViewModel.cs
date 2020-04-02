using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Plugin.ShareFile;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace scanbotsdkexamplexamarinforms
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand OpenWorkflowsCommand { get; }
        public ICommand OpenScanningUiCommand { get; }
        public ICommand OpenCroppingScreenCommand { get; }
        public ICommand ImportImageCommand { get; }
        public ICommand PerformOcrCommand { get; }
        public ICommand OpenBarcodeScannerCommand { get; }
        public ICommand OpenMrzScannerCommand { get; }
        public ICommand OpenEHICScannerCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand HandlePreviewTapped { get; }
        public ICommand CreatePdfCommand { get; }
        public ICommand WriteBinarizedTiffCommand { get; }
        public ICommand CleanupCommand { get; }

        IScannedPage _selectedPage;
        public IScannedPage SelectedPage
        {
            get => _selectedPage;
            private set
            {
                _selectedPage = value;
                NotifyPropertyChanged("SelectedPage");
            }
        }

        ObservableCollection<IScannedPage> _pages = new ObservableCollection<IScannedPage>();
        public ObservableCollection<IScannedPage> Pages
        {
            get => _pages;
            private set
            {
                _pages = value;
                NotifyPropertyChanged("Pages");
            }
        }

        public MainPageViewModel()
        {
            OpenWorkflowsCommand = new Command(async () =>
            {
                await Application.Current.MainPage.Navigation.PushAsync(new WorkflowsPage());
            });

            OpenScanningUiCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense()) { return; }

                var configuration = new DocumentScannerConfiguration
                {
                    CameraPreviewMode = CameraPreviewMode.FitIn,
                    IgnoreBadAspectRatio = true,
                    MultiPageEnabled = true,
                    PolygonColor = Color.Red,
                    PolygonColorOK = Color.Green,
                    BottomBarBackgroundColor = Color.Blue,
                    PageCounterButtonTitle = "%d Page(s)",
                    //DocumentImageSizeLimit = new Size(2000, 3000),
                    // see further customization configs...
                };
                var result = await SBSDK.UI.LaunchDocumentScannerAsync(configuration);
                if (result.Status == OperationResult.Ok)
                {
                    Pages.Clear();
                    foreach (var page in result.Pages)
                        Pages.Add(page);
                    SelectedPage = Pages[0];
                }
            });

            OpenCroppingScreenCommand = new Command(async () => 
            {
                if (!CheckScanbotSDKLicense() || !CheckPageSelected()) { return; }

                var page = SelectedPage;
                await SBSDK.UI.LaunchCroppingScreenAsync(page);
            });

            ImportImageCommand = new Command(async () =>
            {
                var image = await DependencyService.Get<IImagePicker>().PickImageAsync();
                if (image != null)
                {
                    if (!CheckScanbotSDKLicense()) { return; }

                    // import the selected image as original image and create a Page object first:
                    var importedPage = await SBSDK.Operations.CreateScannedPageAsync(image);
                    // then run document detection on it:
                    await importedPage.DetectDocumentAsync();
                    SelectedPage = importedPage;
                    Pages.Add(SelectedPage);
                }
            });

            PerformOcrCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense() || !CheckHasPages()) { return; }

                var languages = new[] { "en" }; // or specify more languages like { "en", "de", ... }
                var result = await SBSDK.Operations.PerformOcrAsync(DocumentSources, languages);
                MessagingCenter.Send(new AlertMessage { Message = result.Text, Title = "OCR Result" }, AlertMessage.ID);
            });

            OpenBarcodeScannerCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense()) { return; }

                var config = new BarcodeScannerConfiguration();
                var result = await SBSDK.UI.LaunchBarcodeScannerAsync(config);
                if (result.Status == OperationResult.Ok)
                {
                    MessagingCenter.Send(new AlertMessage { Message = result.Text, Title = result.Format.ToString() }, AlertMessage.ID);
                }
            });

            OpenMrzScannerCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense()) { return; }

                MrzScannerConfiguration configuration = new MrzScannerConfiguration
                {
                    FinderWidthRelativeToDeviceWidth = 0.95,
                    FinderHeightRelativeToDeviceWidth = 0.2,
                };

                var result = await SBSDK.UI.LaunchMrzScannerAsync(configuration);
                if (result.Status == OperationResult.Ok)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"DocumentType: {result.DocumentType}");
                    foreach (var field in result.Fields)
                    {
                        sb.AppendLine($"{field.Name}: {field.Value} ({field.Confidence:F2})");
                    }
                    MessagingCenter.Send(new AlertMessage { Message = sb.ToString(), Title = "MRZ Result" }, AlertMessage.ID);
                }
            });

            OpenEHICScannerCommand = new Command(async () =>
            {
                var configuration = new HealthInsuranceCardConfiguration { };
                var result = await SBSDK.UI.LaunchHealthInsuranceCardScannerAsync(configuration);
                if (result.Status == OperationResult.Ok)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"DocumentType: European Health insurance card");
                    foreach (var field in result.Fields)
                    {
                        sb.AppendLine($"{field.Type}: {field.Value} ({field.Confidence:F2})");
                    }
                    MessagingCenter.Send(new AlertMessage
                    { Message = sb.ToString(), Title = "EHIC Result" }, AlertMessage.ID);
                }
            });

            ApplyFilterCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense() || !CheckDocumentSelected()) { return; }

                await Application.Current.MainPage.Navigation.PushAsync(new FilterPage { CurrentPage = SelectedPage });
            });

            CleanupCommand = new Command(async () =>
            {
                await SBSDK.Operations.CleanUp();
                Pages.Clear();
                SelectedPage = null;
                MessagingCenter.Send(new AlertMessage { Message = "Cleanup done. All scanned images and generated files (PDF, TIFF, etc) have been removed.", Title = "Info" }, AlertMessage.ID);
            });

            HandlePreviewTapped = new Command(arg =>
            {
                SelectedPage = arg as IScannedPage;
            });

            CreatePdfCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense() || !CheckHasPages()) { return; }

                var fileUri = await SBSDK.Operations.CreatePdfAsync(DocumentSources, PDFPageSize.FixedA4);

                // Please note that on Android sharing works only with public accessible files.
                // Files from internal, secure storage folders cannot be shared.
                // (also see the SDK initialization with external (public) storage)
                CrossShareFile.Current.ShareLocalFile(fileUri.AbsolutePath);
            });

            WriteBinarizedTiffCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense() || !CheckHasPages()) { return; }

                var fileUri = await SBSDK.Operations.WriteTiffAsync(DocumentSources, new TiffOptions { OneBitEncoded = true });

                // Please note that on Android sharing works only with public accessible files.
                // Files from internal, secure storage folders cannot be shared.
                // (also see the SDK initialization with external (public) storage)
                CrossShareFile.Current.ShareLocalFile(fileUri.AbsolutePath);
            });
        }

        IEnumerable<ImageSource> DocumentSources => Pages.Select(p => p.Document).Where(image => image != null);

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
                Message = "Scanbot SDK trial license has expired."
            };
            MessagingCenter.Send(msg, AlertMessage.ID);
            return false;
        }

        bool CheckPageSelected()
        {
            if (SelectedPage != null)
            {
                return true;
            }

            var msg = new AlertMessage
            {
                Title = "Info",
                Message = "Please scan and select a page image first."
            };
            MessagingCenter.Send(msg, AlertMessage.ID);
            return false;
        }

        bool CheckDocumentSelected()
        {
            if (SelectedPage != null && SelectedPage.Document != null)
            {
                return true;
            }

            var msg = new AlertMessage
            {
                Title = "Info",
                Message = "Please select a page with a detected document."
            };
            MessagingCenter.Send(msg, AlertMessage.ID);
            return false;
        }

        bool CheckHasPages()
        {
            if (Pages?.Count > 0)
            {
                return true;
            }

            var msg = new AlertMessage
            {
                Title = "Info",
                Message = "Please snap a document image via Document Scanner or import an image from the Photo Library."
            };
            MessagingCenter.Send(msg, AlertMessage.ID);
            return false;
        }

    }
}
