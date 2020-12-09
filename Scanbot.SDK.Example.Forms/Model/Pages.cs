using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScanbotSDK.Xamarin;
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

        public async Task<bool> Add(IScannedPage page, bool save = true)
        {
            List.Add(page);
            if (save)
            {
                await PageStorage.Instance.Save(page);
            }
            return true;
        }

        public async Task<bool> LoadFromStorage()
        {
            var pages = await PageStorage.Instance.Load();
            foreach (var page in pages)
            {
                var reconstructed = await SBSDK.Operations.ReconstructPage(
                    page.PageId,
                    page.CreatePolygon(),
                    (ImageFilter)page.Filter,
                    (DocumentDetectionStatus)page.DetectionStatus
                );
                await Add(reconstructed, false);
            }
            return true;
        }
    }
}
