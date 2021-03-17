
using System;
using ScanbotSDK.Xamarin.Forms;
using Xamarin.Forms;

namespace Native.Renderers.Example.Forms.Views
{
    /*
        This is the View that will be referenced in the XAML to integrate our native Camera View.
        The class itself does not provide any specific implementation; the actual implementation
        is achieved through the use of Custom Renderers.
        Renderers are platform specific and they are implemented in the relative native projects.
        Take a look at AndroidBarcodeCameraRenderer.cs to see how the implementation is done.
     */
    public class BarcodeCameraView : View
    {
        // This is the delegate that will be used from our native controller to
        // notify us that the scanner has returned a valid result.
        // We can set this from our Page class to implement a custom behavior.
        public delegate void BarcodeScannerResultHandler(BarcodeScanningResult result);
        public BarcodeScannerResultHandler OnBarcodeScanResult;

        // This event is defined from our native control through the Custom Renderer.
        // We call this from our Page whenever we want to start/restart our camera view,
        // in accord to the view lifecycle as well (eg. OnAppearing)
        public EventHandler<EventArgs> OnResume;
        public void Resume()
        {
            OnResume?.Invoke(this, EventArgs.Empty);
        }

        // This event is defined from our native control through the Custom Renderer.
        // We call this from our Page whenever we want to stop/pause our camera view,
        // in accord to the view lifecycle as well (eg. OnDisappearing)
        public EventHandler<EventArgs> OnPause;
        public void Pause()
        {
            OnPause?.Invoke(this, EventArgs.Empty);
        }

        public BarcodeCameraView()
        {
            OnBarcodeScanResult = HandleBarcodeScanResult;
        }

        private void HandleBarcodeScanResult(BarcodeScanningResult result)
        {
            // If we don't implement the delegate from our Page class, this method
            // will be called instead as a fallback mechanism.
        }
    }
}
