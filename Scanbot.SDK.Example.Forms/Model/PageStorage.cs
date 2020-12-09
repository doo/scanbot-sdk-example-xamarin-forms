using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ScanbotSDK.Xamarin;
using ScanbotSDK.Xamarin.Forms;
using SQLite;
using Xamarin.Forms;

namespace Scanbot.SDK.Example.Forms
{
    public class PageStorage
    {
        public static PageStorage Instance = new PageStorage();

        public const string DatabaseFilename = "SBSDKPageStorage.db3";

        public const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite
            | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }

        static SQLiteAsyncConnection Database = new SQLiteAsyncConnection(DatabasePath, Flags);
        
        private PageStorage()
        {

        }

        public async Task InitializeAsync()
        {
            var result = await Database.CreateTablesAsync(CreateFlags.None, typeof(DBPage));//.ConfigureAwait(false);
            Console.WriteLine("Storage initialize: " + result);
        }

        public async Task<int> Save(IScannedPage page)
        {
            var dbPage = DBPage.From(page);
            return await Database.InsertAsync(dbPage);
        }

        public async Task<int> Update(IScannedPage page)
        {
            return await Database.UpdateAsync(DBPage.From(page));
        }

        public async Task<List<DBPage>> Load()
        {
            var pages = await Database.Table<DBPage>().ToListAsync();
            return pages;
        }

        public async Task<int> Delete(IScannedPage page)
        {
            return await Database.DeleteAsync(DBPage.From(page));
        }
    }

    /*
     * SQLite storage requires a non-abstract class with a constructor and primitive types
     */
    public class DBPage
    {
        [PrimaryKey]
        public string Id { get; set; }

        public string Document { get; set; }
        public string Original { get; set; }
        public string DocumentPreview { get; set; }
        public string OriginalPreview { get; set; }
        public string AvailablePreview { get; set; }

        public int Filter { get; set; }
        public int DetectionStatus { get; set; }

        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double X3 { get; set; }
        public double Y3 { get; set; }
        public double X4 { get; set; }
        public double Y4 { get; set; }

        public static DBPage From(IScannedPage page)
        {

            var result = new DBPage
            {
                Id = page.Id,
                Document = ImageToPath(page.Document),
                Original = ImageToPath(page.Original),
                DocumentPreview = ImageToPath(page.DocumentPreview),
                OriginalPreview = ImageToPath(page.OriginalPreview),
                AvailablePreview = ImageToPath(page.AvailablePreview),
                Filter = (int)page.Filter,
                DetectionStatus = (int)page.DetectionStatus
            };

            result.MapPolygon(page.Polygon);

            return result;
        }

        public void MapPolygon(Point[] points)
        {
            if (points.Length < 4)
            {
                return;
            }
            X1 = points[0].X;
            Y1 = points[0].Y;
            X2 = points[1].X;
            Y2 = points[1].Y;
            X3 = points[2].X;
            Y3 = points[2].Y;
            X4 = points[3].X;
            Y4 = points[3].Y;
        }

        public Point[] CreatePolygon()
        {
            var result = new List<Point>();
            result.Add(new Point(X1, Y1));
            result.Add(new Point(X2, Y2));
            result.Add(new Point(X3, Y3));
            result.Add(new Point(X4, Y4));
            return result.ToArray();
        }

        public static string ImageToPath(ImageSource source)
        {
            return ((FileImageSource)source)?.File;
        }

    }
}
