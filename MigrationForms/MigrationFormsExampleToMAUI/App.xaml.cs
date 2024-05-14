using MigrationFormsExampleToMAUI.Model;

namespace MigrationFormsExampleToMAUI;

public partial class App : Application
{
    public static Color ScanbotRed = Color.FromRgb(200, 25, 60);

    public App()
    {
        InitializeComponent();
        
#pragma warning disable CS4014
        // There's no requirement to await this, can just disable warning
        InitializeAsync();
#pragma warning restore CS4014

        MainPage = new AppShell();
    }
    
    async Task<bool> InitializeAsync()
    {
        await PageStorage.Instance.InitializeAsync();
        await MigrationFormsExampleToMAUI.Model.Pages.Instance.LoadFromStorage();
        return true;
    }
}