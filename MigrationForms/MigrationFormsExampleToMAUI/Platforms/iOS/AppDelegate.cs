using Foundation;
using MigrationFormsExampleToMAUI.DependencyServices;

namespace MigrationFormsExampleToMAUI;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}