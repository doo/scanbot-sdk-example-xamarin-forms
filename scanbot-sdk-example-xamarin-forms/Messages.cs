using System;

namespace scanbotsdkexamplexamarinforms
{
    public class ScanbotCameraResultMessage
    {
        public static readonly string ID = typeof(ScanbotCameraResultMessage).Name;

        public string DocumentImageFileUri;
        public string OriginalImageFileUri;
    }

    public class AlertMessage
    {
        public static readonly string ID = typeof(AlertMessage).Name;

        public string Title;
        public string Message;
    }
}
