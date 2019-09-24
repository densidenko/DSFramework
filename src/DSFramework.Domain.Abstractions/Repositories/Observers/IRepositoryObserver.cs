using System;

namespace DSFramework.Domain.Abstractions.Repositories.Observers
{
    public interface IRepositoryObserver
    {
        void OnGet(string collection, TimeSpan elapsed);
        void OnSearch(string collection, TimeSpan elapsed, int? documentsCount);
        void OnBulkUpdate(string collection, TimeSpan elapsed, int? documentsCount);
        void OnBulkUpdateFailed(string collection, bool isVersionMismatch);
        void OnGetFailed(string collection);
        void OnSearchFailed(string collection);
        void OnCreate(string collection, TimeSpan elapsed);
        void OnCreateFailed(string collection, bool isVersionMismatch);
        void OnUpdate(string collection, TimeSpan elapsed);
        void OnUpdateFailed(string collection, bool isVersionMismatch);
        void OnDelete(string collection, TimeSpan elapsed);
        void OnDeleteFailed(string collection, bool isVersionMismatch);
        void OnDeleteMany(string collection, TimeSpan elapsed, int? documentsCount);
        void OnDeleteManyFailed(string collection, bool isVersionMismatch);
        void OnGetAll(string collection, TimeSpan elapsed, int? documentsCount);
        void OnGetAllFailed(string collection);
        void OnGetMany(string collection, TimeSpan elapsed, int? documentsCount);
        void OnGetManyFailed(string collection);
    }
}