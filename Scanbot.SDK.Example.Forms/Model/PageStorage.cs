using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ScanbotSDK.Xamarin.Forms;
using SQLite;

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

        static readonly Lazy<SQLiteAsyncConnection> lazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(DatabasePath, Flags);
        });

        static SQLiteAsyncConnection Database => lazyInitializer.Value;
        static bool initialized = false;

        private PageStorage()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        async Task InitializeAsync()
        {
            if (!initialized)
            {
                if (!Database.TableMappings.Any(m => m.MappedType.Name == typeof(IScannedPage).Name))
                {
                    await Database.CreateTablesAsync(CreateFlags.None, typeof(IScannedPage)).ConfigureAwait(false);
                }
                initialized = true;
            }
        }

        public async Task<int> Save(IScannedPage page)
        {
            var dbPage = new DBPage { InternalPage = page };
            return await Database.InsertAsync(dbPage);
        }

        public static async Task<List<DBPage>> Load()
        {
            return await Database.Table<DBPage>().ToListAsync();
        }

        public async Task<int> Delete(DBPage page)
        {
            return await Database.DeleteAsync(page);
        }
    }

    /*
     * SQLite storage requires a non-abstract class with a constructor
     */
    public class DBPage
    {
        public IScannedPage InternalPage { get; set; }
    }

    public static class TaskExtensions
    {
        // NOTE: Async void is intentional here. This provides a way
        // to call an async method from the constructor while
        // communicating intent to fire and forget, and allow
        // handling of exceptions
        public static async void SafeFireAndForget(this Task task,
            bool returnToCallingContext,
            Action<Exception> onException = null)
        {
            try
            {
                await task.ConfigureAwait(returnToCallingContext);
            }

            // if the provided action is not null, catch and
            // pass the thrown exception
            catch (Exception ex) when (onException != null)
            {
                onException(ex);
            }
        }
    }
}
