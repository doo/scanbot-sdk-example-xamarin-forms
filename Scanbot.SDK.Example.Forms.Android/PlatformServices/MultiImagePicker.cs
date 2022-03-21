using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using NativeMedia;
using Scanbot.SDK.Example.Forms.Droid;

[assembly: Dependency(typeof(MultiImagePicker))]
namespace Scanbot.SDK.Example.Forms.Droid
{
    /// <summary>
    /// Android Multiple Image picker class implementation of Dependency Service IMultiImagePicker interface
    /// </summary>
    public class MultiImagePicker : IMultiImagePicker
    {
        /// <summary>
        /// Picks multiple photos from the Photos Application and invokes the completion handler after selection
        /// </summary>
        /// <param name="completionHandler"><completion handler callback on image selection/param>
        public async void PickPhotosAsync(Action<List<ImageSource>> completionHandler)
        {
            var result = await PickMultipleMediaFiles();
            completionHandler?.Invoke(result);
        }

        /// <summary>
        /// Picks multiple images from the gallery
        /// </summary>
        /// <returns></returns>
        private async Task<List<ImageSource>> PickMultipleMediaFiles()
        {
            var imagesSources = new List<ImageSource>();
            var cts = new CancellationTokenSource();
            IMediaFile[] files = null;
            try
            {
                var request = new MediaPickRequest(10, MediaFileType.Image)
                {
                    Title = "Select"
                };

                cts.CancelAfter(TimeSpan.FromMinutes(5));

                var results = await MediaGallery.PickAsync(request, cts.Token);
                files = results?.Files?.ToArray();

                if (files == null)
                    return null;

                foreach (var file in files)
                {
                    var stream = await file.OpenReadAsync();
                    imagesSources.Add(ImageSource.FromStream(() => stream));
                }
            }
            catch (OperationCanceledException)
            {
                // handling a cancellation request
            }
            catch (Exception)
            {
                // handling other exceptions
            }
            finally
            {
                cts.Dispose();
            }

            return imagesSources;
        }
    }
}
