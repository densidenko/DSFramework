using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DS.Domain.Abstractions.Specifications;

namespace DS.Domain.Abstractions.Repositories
{
    public interface IRepositoryBase<in TKey, TAggregate>
    {
        TAggregate Get(TKey key);
        Task<TAggregate> GetAsync(TKey key);

        TAggregate Update(TAggregate entity);
        Task<TAggregate> UpdateAsync(TAggregate entity);
        void BulkUpsert(TAggregate[] entities);
        Task BulkUpsertAsync(TAggregate[] entities);

        TAggregate Delete(TKey key);
        Task<TAggregate> DeleteAsync(TKey key);
        TAggregate[] DeleteMany(TKey[] keys);
        Task<TAggregate[]> DeleteManyAsync(TKey[] keys);
        TAggregate[] DeleteMany(IDomainSpecification<TAggregate> specification);
        Task<TAggregate[]> DeleteManyAsync(IDomainSpecification<TAggregate> specification);

        TAggregate[] Search(IDomainSpecification<TAggregate> specification);
        SearchResult<TAggregate> Search(IDomainSpecification<TAggregate> specification, SearchOptions searchOptions);
        long Count(IDomainSpecification<TAggregate> specification);
        long Count(IDomainSpecification<TAggregate> specification, SearchOptions searchOptions);
        Task<TAggregate[]> SearchAsync(IDomainSpecification<TAggregate> specification, SearchOptions searchOptions = null);
        Task<long> CountAsync(IDomainSpecification<TAggregate> specification);
        TAggregate[] GetAll();
        Task<TAggregate[]> GetAllAsync();
        TAggregate[] GetMany(params TKey[] keys);
        Task<TAggregate[]> GetManyAsync(params TKey[] keys);
    }

    public class SearchResult<TAggregateRoot>
    {
        public SearchResult(TAggregateRoot[] items, bool hasMore)
        {
            Items = items;
            HasMore = hasMore;
        }

        public TAggregateRoot[] Items { get; }
        public bool HasMore { get; }
    }

    public class SearchOptions
    {
        public SearchOptions(int limit)
        {
            Limit = limit;
        }

        public SearchOptions()
        {
            Limit = Int32.MaxValue;
        }

        public static SearchOptions NoLimits = new SearchOptions();
        public int Limit { get; }
        public SortOptions SortOptions { get; set; }
    }

    public class SortOptions
    {
        public bool SortByTextScore { get; set; }

        private List<KeyValuePair<string, bool>> _fields = new List<KeyValuePair<string, bool>>();

        public SortOptions SortBy(string field, bool asc = true)
        {
            _fields.Add(new KeyValuePair<string, bool>(field, asc));
            return this;
        }

        public bool IsEmpty()
        {
            return !SortByTextScore && !_fields.Any();
        }

        public IEnumerable<KeyValuePair<string, bool>> GetFields()
        {
            return _fields.ToArray();
        }
    }
}