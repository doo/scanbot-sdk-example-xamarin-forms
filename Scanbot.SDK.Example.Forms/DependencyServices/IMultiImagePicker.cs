using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    /// <summary>
    /// Interface for picking photo from Gallery.
    /// </summary>
    public interface IMultiImagePicker
    {
        /// <summary>
        /// Get photos paths from gallery with completion handler
        /// </summary>
        /// <returns></returns>
        void PickPhotosAsync(Action<List<ImageSource>> completionHandler);
    }
}
