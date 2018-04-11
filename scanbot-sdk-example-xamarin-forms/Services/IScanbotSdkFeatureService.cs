using System;
using System.Collections.Generic;

namespace scanbotsdkexamplexamarinforms.Services
{
    public interface IScanbotSdkFeatureService
    {
        void StartScanningUi();

        void StartOcrService();

        bool IsLicenseValid();

        // TODO
        // void StartCroppingUi();
        // ...
    }
}
