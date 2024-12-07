using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using MigrationFormsExampleToMAUI.DependencyServices;

namespace MigrationFormsExampleToMAUI;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        // Media Gallery init implementation
        NativeMedia.Platform.Init(this, savedInstanceState);
        
        ActivityCompat.RequestPermissions(this, new string[] {
            Manifest.Permission.Camera,
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage
        }, 0);
    }
    
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        if (NativeMedia.Platform.CheckCanProcessResult(requestCode, resultCode, data))
            NativeMedia.Platform.OnActivityResult(requestCode, resultCode, data);

        base.OnActivityResult(requestCode, resultCode, data);
    }
}