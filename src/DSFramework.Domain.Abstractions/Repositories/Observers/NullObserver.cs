using System;

namespace DSFramework.Domain.Abstractions.Repositories.Observers
{
    public class NullObserver : IRepositoryObserver
    {
        public static readonly IRepositoryObserver Instance = new NullObserver();

        public void OnGet(string collection, TimeSpan elapsed)
        { }

        public void OnSearch(string collection, TimeSpan elapsed, int? documentsCount)
        { }

        public void OnBulkUpdate(string collection, TimeSpan elapsed, int? documentsCount)
        { }

        public void OnBulkUpdateFailed(string collection, bool isVersionMismatch)
        { }

        public void OnGetFailed(string collection)
        { }

        public void OnSearchFailed(string collection)
        { }

        public void OnCreate(string collection, TimeSpan elapsed)
        { }

        public void OnCreateFailed(string collection, bool isVersionMismatch)
        { }

        public void OnUpdate(string collection, TimeSpan elapsed)
        { }

        public void OnUpdateFailed(string collection, bool isVersionMismatch)
        { }

        public void OnDelete(string collection, TimeSpan elapsed)
        { }

        public void OnDeleteFailed(string collection, bool isVersionMismatch)
        { }

        public void OnDeleteMany(string collection, TimeSpan elapsed, int? documentsCount)
        { }

        public void OnDeleteManyFailed(string collection, bool isVersionMismatch)
        { }

        public void OnGetAll(string collection, TimeSpan elapsed, int? documentsCount)
        { }

        public void OnGetAllFailed(string collection)
        { }

        public void OnGetMany(string collection, TimeSpan elapsed, int? documentsCount)
        { }

        public void OnGetManyFailed(string collection)
        { }
    }
}