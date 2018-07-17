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

        public ICommand OpenScanningUiCommand { get; }
        public ICommand OpenCroppingScreenCommand { get; }
        public ICommand DetectDocumentAndCropCommand { get; }
        public ICommand PickPhotoCommand { get; }
        public ICommand ShowExistingScansCommand { get; }
        public ICommand StartOcrServiceCommand { get; }
        public ICommand OpenBarcodeScannerCommand { get; }
        public ICommand OpenMrzScannerCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand HandlePreviewTapped { get; }
        public ICommand ClearDocumentSelectionCommand { get; }
        public ICommand CreatePdfCommand { get; }
        public ICommand WriteBinarizedTiffCommand { get; }

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
            OpenScanningUiCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense()) { return; }

                var configuration = new DocumentScannerConfiguration
                {
                    CameraPreviewMode = CameraPreviewMode.FitIn,
                    // Customize colors, text resources, etc ...
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

            DetectDocumentAndCropCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense() || !CheckPageSelected()) { return; }

                await SelectedPage.DetectDocumentAsync();
            });

            PickPhotoCommand = new Command(async () =>
            {
                var image = await DependencyService.Get<IImagePicker>().PickImageAsync();
                if (image != null)
                {
                    SelectedPage = await SBSDK.Operations.CreateScannedPageAsync(image);
                    Pages.Add(SelectedPage);
                }
            });

            StartOcrServiceCommand = new Command(async () =>
            {
                if (!CheckScanbotSDKLicense() || !CheckDocumentSelected()) { return; }
                var result = await SBSDK.Operations.PerformOcrAsync(new[] { SelectedPage.Document }, new[] { "en", "de" });
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

                MrzScannerConfiguration configuration = new MrzScannerConfiguration
                {
                    FinderWidthRelativeToDeviceWidth = 0.95,
                    FinderHeightRelativeToDeviceWidth = 0.2,
                };

                var result = await SBSDK.UI.LaunchMrzScannerAsync(configuration);
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
                if (!CheckScanbotSDKLicense() || !CheckDocumentSelected()) { return; }

                await Application.Current.MainPage.Navigation.PushAsync(new FilterPage { CurrentPage = SelectedPage });
            });

            ShowExistingScansCommand = new Command(async () =>
            {
                var selectionPage = new ImageSelectionPage();
                selectionPage.Disappearing += (sender, e) =>
                {
                    for (int i = Pages.Count - 1; i >= 0; --i)
                    {
                        if (Pages[i].Original == null)
                        {
                            Pages.RemoveAt(i);
                        }
                    }
                    if (SelectedPage != null && SelectedPage.Original == null)
                    {
                        SelectedPage = null;
                    }
                };
                selectionPage.PageSelected += (sender, e) =>
                {
                    Pages.Add(e.Page);
                    Application.Current.MainPage.Navigation.PopAsync();
                };
                await Application.Current.MainPage.Navigation.PushAsync(selectionPage);
            });

            HandlePreviewTapped = new Command(arg =>
            {
                SelectedPage = arg as IScannedPage;
            });

            ClearDocumentSelectionCommand = new Command(() =>
            {
                Pages.Clear();
                SelectedPage = null;
            });

            CreatePdfCommand = new Command(async () =>
            {
                var fileUri = await SBSDK.Operations.CreatePdfAsync(DocumentSources);
                if (Device.RuntimePlatform == Device.iOS)
                {
                    CrossShareFile.Current.ShareLocalFile(fileUri.AbsolutePath);
                }
                else
                {
                    MessagingCenter.Send(new AlertMessage { Message = $"File written to {fileUri.AbsolutePath}", Title = "PDF" }, AlertMessage.ID);
                }
            });

            WriteBinarizedTiffCommand = new Command(async () =>
            {
                var fileUri = await SBSDK.Operations.WriteTiffAsync(DocumentSources, new TiffOptions { OneBitEncoded = true });
                if (Device.RuntimePlatform == Device.iOS)
                {
                    CrossShareFile.Current.ShareLocalFile(fileUri.AbsolutePath);
                }
                else
                {
                    MessagingCenter.Send(new AlertMessage { Message = $"File written to {fileUri.AbsolutePath}", Title = "TIFF" }, AlertMessage.ID);
                }
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
                Message = "Selected a page first."
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
                Message = "Selected a page with a detected document."
            };
            MessagingCenter.Send(msg, AlertMessage.ID);
            return false;
        }
    }
}
