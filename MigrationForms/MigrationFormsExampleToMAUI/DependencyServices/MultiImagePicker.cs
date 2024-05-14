namespace MigrationFormsExampleToMAUI.DependencyServices;

public partial class MultiImagePicker
{
    /// <summary>
    /// Get photos paths from gallery with completion handler
    /// </summary>
    /// <returns></returns>
    public partial void PickPhotosAsync(Action<List<ImageSource>> completionHandler);
}


