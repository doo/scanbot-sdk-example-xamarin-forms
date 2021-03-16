using System;
namespace Native.Renderers.Example
{
    public class ScanbotSDKConfiguration
    {
#if DEBUG
        public const bool IS_DEBUG = true;
#else
        public const bool IS_DEBUG = false;
#endif

        public const string LICENSE_KEY = "";
    }
}
