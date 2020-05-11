using System;
using System.Collections.Generic;
using System.Linq;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class Pages
    {
        public static readonly Pages Instance = new Pages();

        public List<IScannedPage> List { get; set; } = new List<IScannedPage>();

        public Image Image { get; set; }

        public IEnumerable<ImageSource> DocumentSources
        {
            get => List.Select(p => p.Document).Where(image => image != null);
        }

        IScannedPage _selectedPage;
        public IScannedPage SelectedPage
        {
            get => _selectedPage;
            set
            {
                _selectedPage = value;
                Image.Source = _selectedPage.Document;
            }
        }

        public void UpdateImage()
        {
            Image.Source = SelectedPage.Document;
        }


        private Pages() { }

    }
}
