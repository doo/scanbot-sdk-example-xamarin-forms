
## Native Renderers Example App for Xamarin.Forms
This example app demonstrates how to integrate Xamarin Scanbot SDK native features in a **Xamarin.Forms** project (Android & iOS), through the use of Custom Renderers.

## Barcode Scanner Example
The Barcode Scanner Example demonstrates how a custom renderer can be used in order to implement native features in your Xamarin Form project. In this example we have used a Custom Renderer to access the native `ScanbotCameraView` on each platform (Android & iOS).

### How does it work?
The core project defines a View called `BarcodeCameraView`, which is just a regular custom view; you can either create it programmatically or define it in your Page's XAML.

The native Xamarin projects export a `ViewRenderer` that overrides and takes care of the actual implementation of BarcodeCameraView.

*eg.*
```
[assembly: ExportRenderer(typeof(BarcodeCameraView), typeof(AndroidBarcodeCameraRenderer))]
namespace Native.Renderers.Example.Forms.Droid.Renderers
{
    public class AndroidBarcodeCameraRenderer : ViewRenderer<BarcodeCameraView, ScanbotCameraView>
    ...
```

Therefore, every feature available in the native SDK now becomes available to use.

From the ViewRenderer scope you can also reference the active instance of the target view, which in this case is `BarcodeCameraView`; as a matter of fact, this instance is available as a property named  `Element`.  

This is particularly important, since you can exploit this instance to create a communication layer between your shared view and your ViewRenderer (`BarcodeCameraView` and `AndroidBarcodeCameraRenderer` in this case).

In order to do that, you can define public properties on your shared view, which will then be accessible from your ViewRenderer.

Since these properties could also be *delegates* or *EventHandler(s)*, you can bind those two instances tightly and have large control over the whole implementation and interaction between the two of them.

In this project you will find different examples; here's one of them:

**BarcodeCameraView.cs**
*Here we define a public EventHandler property, that will be invoked whenever .Resume() is called on BarcodeCameraView*
```
// This event is defined from our native control through the Custom Renderer.
// We call this from our Page whenever we want to start/restart our camera view,
// in accord to the view lifecycle as well (eg. OnAppearing)

public EventHandler<EventArgs> OnResume;
public void Resume()
{
    OnResume?.Invoke(this, EventArgs.Empty);
}
```
**AndroidBarcodeCameraRenderer.cs**
Here we access the EventHandler property from our  ViewRenderer and we implement it by assigning a function to it.
```
Element.OnResume = (sender, e) =>
{
    // Starts/Restarts the Camera View
    cameraView.OnResume();
    CheckPermissions();
};
```
**MainPage.cs**
Finally, we can call .Resume() on our camera view. In this case we call it from the *OnAppearing()* method of our Page.
```
protected override void OnAppearing()
{
    base.OnAppearing();
    if (IsCameraOn)
    {
        cameraView.Resume();
    }
}
```
We have successfully created a bridge between the shared Page in the Core project and the native platform specific View on Android.

## Requirements
[Microsoft Visual Studio](https://www.visualstudio.com) with [Xamarin Platform](https://www.xamarin.com)
(For iOS Visual Studio for Mac **7.4+**)

## Please note

The Scanbot SDK will run without a license for one minute per session!

After the trial period has expired all Scanbot SDK functions as well as the UI components (like the Document Scanner UI) will stop working or may be terminated.
You have to restart the app to get another trial period.

To get an unrestricted, "no-strings-attached" 30-day trial license, please submit the [Trial License Form](https://scanbot.io/en/sdk/demo/trial) on our website.
