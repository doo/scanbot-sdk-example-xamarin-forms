using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Preferences;
using Android.Util;
using Java.Util;
using Net.Doo.Snap.Blob;
using Net.Doo.Snap.Entity;
using Net.Doo.Snap.Persistence;
using Net.Doo.Snap.Persistence.Cleanup;
using Net.Doo.Snap.Process;
using Net.Doo.Snap.Process.Draft;
using Net.Doo.Snap.Util;
using AndroidNetUri = Android.Net.Uri;

namespace scanbotsdkexamplexamarinforms.Droid.Services
{
    [Service]
    public class OcrDemoService : IntentService
    {
        //public static string EXTRAS_ARG_IMAGES = "inputDocumentImages";
        //public static string EXTRAS_ARG_OCR_LANGUAGES = "ocrLanguages";
        //public static string EXTRAS_ARG_PDF_OUTPUT_FILE_URI = "pdfOutputFileUri";

        static string LOG_TAG = typeof(OcrDemoService).Name;

        DocumentProcessor documentProcessor;
        PageFactory pageFactory;
        IDocumentDraftExtractor documentDraftExtractor;
        ITextRecognition textRecognition;
        Cleaner cleaner;
        BlobManager blobManager;
        BlobFactory blobFactory;

        // Alternatively receive languages as string list from intent in the OnHandleIntent(..) method
        // and convert to enum: Language.LanguageByIso("en"), Language.LanguageByIso("de"), etc
        static List<Language> ocrLanguages = new List<Language>();

        static OcrDemoService()
        {
            // set required OCR languages ...
            ocrLanguages.Add(Language.Eng); // english
            ocrLanguages.Add(Language.Deu); // german
        }


        public override void OnCreate()
        {
            base.OnCreate();

            InitScanbotSDKDependencies();
        }

        protected override void OnHandleIntent(Intent intent)
        {
            // Alternatively receive images from intent: intent.GetStringArrayListExtra(EXTRAS_ARG_IMAGES)
            var images = MainActivity.TempImageStorage.GetImages();
            if (images.Length == 0)
            {
                ErrorLog("No images provided.");
                return;
            }

            // Alternatively receive pdfOutputFileUri from intent
            var externalPath = MainActivity.GetPublicExternalStorageDirectory();
            var targetFile = System.IO.Path.Combine(externalPath, UUID.RandomUUID() + ".pdf");
            var pdfOutputFileUri = AndroidNetUri.FromFile(new Java.IO.File(targetFile));

            Task.Run(() => {
                
                try 
                {
                    // Please note: when the OCR blob sources are configured as HTTP download URLs the fetch process will be async!
                    // Fetching from local Assets is sync.
                    FetchOcrBlobFiles();

                    var ocrText = PerformOCR(images, pdfOutputFileUri);
                    DebugLog("Recognized OCR text: " + ocrText);
                    DebugLog("Sandwiched PDF file created: " + pdfOutputFileUri);
                }
                catch (Exception e)
                {
                    ErrorLog("Error performing OCR", e);
                }
            });

        }


        void InitScanbotSDKDependencies()
        {
            var scanbotSDK = new Net.Doo.Snap.ScanbotSDK(this);
            documentProcessor = scanbotSDK.DocumentProcessor();
            pageFactory = scanbotSDK.PageFactory();
            documentDraftExtractor = scanbotSDK.DocumentDraftExtractor();
            textRecognition = scanbotSDK.TextRecognition();
            cleaner = scanbotSDK.Cleaner();
            blobManager = scanbotSDK.BlobManager();
            blobFactory = scanbotSDK.BlobFactory();
        }


        List<Blob> OcrBlobs()
        {
            // Create a collection of required OCR blobs:
            var blobs = new List<Blob>();

            // Language detector blobs of the Scanbot SDK. (see "language_classifier_blob_path" in AndroidManifest.xml!)
            foreach (var b in blobFactory.LanguageDetectorBlobs())
            {
                blobs.Add(b);
            }

            // OCR blobs of languages (see "ocr_blobs_path" in AndroidManifest.xml!)
            foreach (var lng in ocrLanguages)
            {
                foreach (var b in blobFactory.OcrLanguageBlobs(lng))
                {
                    blobs.Add(b);
                }
            }

            return blobs;
        }


        void FetchOcrBlobFiles()
        {
            // Fetch OCR blob files from the sources defined in AndroidManifest.xml
            foreach (var blob in OcrBlobs())
            {
                if (!blobManager.IsBlobAvailable(blob))
                {
                    DebugLog("Fetching OCR blob file: " + blob);
                    blobManager.Fetch(blob, false);
                }
            }
        }


        string PerformOCR(AndroidNetUri[] images, AndroidNetUri pdfOutputFileUri = null)
        {
            DebugLog("Performing OCR...");

            var pages = new List<Page>();
            foreach (AndroidNetUri imageUri in images)
            {
                var path = FileChooserUtils.GetPath(this, imageUri);
                var imageFile = new Java.IO.File(path);
                DebugLog("Creating a page of image file: " + imageFile);
                var page = pageFactory.BuildPage(imageFile);
                pages.Add(page);
            }

            if (pdfOutputFileUri == null)
            {
                // Perform OCR only for plain text result:
                var ocrResultWithTextOnly = textRecognition.WithoutPDF(ocrLanguages, pages).Recognize();
                return ocrResultWithTextOnly.RecognizedText;
            }

            // Perform OCR for PDF file with OCR information (sandwiched PDF):
            var document = new Document();
            document.Name = "document.pdf";
            document.OcrStatus = OcrStatus.Pending;
            document.Id = Java.Util.UUID.RandomUUID().ToString();
            var fullOcrResult = textRecognition.WithPDF(ocrLanguages, document, pages).Recognize();

            // move sandwiched PDF result file into requested target:
            Java.IO.File tempPdfFile = null;
            try
            {
                ISharedPreferences preferences = PreferenceManager.GetDefaultSharedPreferences(this);
                DocumentStoreStrategy documentStoreStrategy = new DocumentStoreStrategy(this, preferences);
                tempPdfFile = documentStoreStrategy.GetDocumentFile(fullOcrResult.SandwichedPdfDocument.Id, fullOcrResult.SandwichedPdfDocument.Name);
                DebugLog("Got temp PDF file from SDK: " + tempPdfFile);
                if (tempPdfFile != null && tempPdfFile.Exists())
                {
                    DebugLog("Copying temp file to target output file: " + pdfOutputFileUri);
                    File.Copy(tempPdfFile.AbsolutePath, new Java.IO.File(pdfOutputFileUri.Path).AbsolutePath);
                }
                else
                {
                    ErrorLog("Could not get sandwiched PDF document file from SDK!");
                }
            }
            finally
            {
                if (tempPdfFile != null && tempPdfFile.Exists())
                {
                    DebugLog("Deleting temp file: " + tempPdfFile);
                    tempPdfFile.Delete();
                }
            }

            return fullOcrResult.RecognizedText;
        } 


        void DebugLog(string msg)
        {
            Log.Debug(LOG_TAG, msg);
        }

        void ErrorLog(string msg)
        {
            Log.Error(LOG_TAG, msg);
        }

        void ErrorLog(string msg, Exception ex)
        {
            Log.Error(LOG_TAG, Java.Lang.Throwable.FromException(ex), msg);
        }
    }
}
