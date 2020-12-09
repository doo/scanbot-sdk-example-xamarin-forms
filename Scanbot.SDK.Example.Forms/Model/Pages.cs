using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class Pages
    {
        public static readonly Pages Instance = new Pages();

        public List<IScannedPage> List { get; set; } = new List<IScannedPage>();

        public IEnumerable<ImageSource> DocumentSources
        {
            get => List.Select(p => p.Document).Where(image => image != null);
        }

        public IScannedPage SelectedPage { get; set; }


        private Pages() { }

        internal void RemoveCurrent()
        {
            List.Remove(SelectedPage);
            SelectedPage = null;
        }

        internal async Task<bool> Add(IScannedPage page)
        {
            List.Add(page);
            await PageStorage.Instance.Save(page);
            return true;
        }
    }
}
