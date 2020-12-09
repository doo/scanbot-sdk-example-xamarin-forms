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

        public async Task<int> RemoveSelection()
        {
            var result = await PageStorage.Instance.Delete(SelectedPage);
            List.Remove(SelectedPage);
            SelectedPage = null;
            return result;
        }

        public async Task<int> UpdateFilterForSelection(ImageFilter filter)
        {
            await SelectedPage.SetFilterAsync(filter);
            return await UpdateSelection();
        }

        public async Task<int> UpdateSelection()
        {
            return await PageStorage.Instance.Update(SelectedPage);
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
                    page.Id,
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
