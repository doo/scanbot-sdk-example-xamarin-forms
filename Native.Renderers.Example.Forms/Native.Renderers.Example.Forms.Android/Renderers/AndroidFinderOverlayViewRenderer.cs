using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Native.Renderers.Example.Forms.Droid.Renderers;
using Native.Renderers.Example.Forms.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FinderOverlayView), typeof(AndroidFinderOverlayViewRenderer))]
namespace Native.Renderers.Example.Forms.Droid.Renderers
{
    public class AndroidFinderOverlayViewRenderer : ViewRenderer<FinderOverlayView, IO.Scanbot.Sdk.UI.Camera.FinderOverlayView>
    {
        private IO.Scanbot.Sdk.UI.Camera.FinderOverlayView finderOverlayView;

        public AndroidFinderOverlayViewRenderer(Context context) : base(context) {
            finderOverlayView = (IO.Scanbot.Sdk.UI.Camera.FinderOverlayView) LayoutInflater
                .From(Context)
                .Inflate(Resource.Layout.finder_overlay_view, null, false);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FinderOverlayView> e)
        {

            SetNativeControl(finderOverlayView);

            base.OnElementChanged(e);

            if (Control == null) { return; }

            // Here you can set the required Aspect Ratios for your Overlay Finder View (2:1 in this example)
            Control.RequiredAspectRatios = new List<IO.Scanbot.Sdk.UI.Camera.FinderAspectRatio>
            {
                new IO.Scanbot.Sdk.UI.Camera.FinderAspectRatio(2, 1)
            };

        }
    }
}
