using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSFramework.Domain.Abstractions.Specifications;

namespace DSFramework.Domain.Abstractions.Repositories
{
    public interface IRepositoryBase<in TKey, TAggregate>
    {
        void BulkUpsert(TAggregate[] entities);
        Task BulkUpsertAsync(TAggregate[] entities);
        long Count(IDomainSpecification<TAggregate> specification);
        long Count(IDomainSpecification<TAggregate> specification, SearchOptions searchOptions);
        Task<long> CountAsync(IDomainSpecification<TAggregate> specification);

        TAggregate Delete(TKey key);
        Task<TAggregate> DeleteAsync(TKey key);
        TAggregate[] DeleteMany(TKey[] keys);
        TAggregate[] DeleteMany(IDomainSpecification<TAggregate> specification);
        Task<TAggregate[]> DeleteManyAsync(TKey[] keys);
        Task<TAggregate[]> DeleteManyAsync(IDomainSpecification<TAggregate> specification);
        TAggregate Get(TKey key);
        TAggregate[] GetAll();
        Task<TAggregate[]> GetAllAsync();
        Task<TAggregate> GetAsync(TKey key);
        TAggregate[] GetMany(params TKey[] keys);
        Task<TAggregate[]> GetManyAsync(params TKey[] keys);

        TAggregate[] Search(IDomainSpecification<TAggregate> specification);
        SearchResult<TAggregate> Search(IDomainSpecification<TAggregate> specification, SearchOptions searchOptions);
        Task<TAggregate[]> SearchAsync(IDomainSpecification<TAggregate> specification, SearchOptions searchOptions = null);

        TAggregate Update(TAggregate entity);
        Task<TAggregate> UpdateAsync(TAggregate entity);
    }

    public class SearchResult<TAggregateRoot>
    {
        public TAggregateRoot[] Items { get; }
        public bool HasMore { get; }

        public SearchResult(TAggregateRoot[] items, bool hasMore)
        {
            Items = items;
            HasMore = hasMore;
        }
    }

    public class SearchOptions
    {
        public static SearchOptions NoLimits = new SearchOptions();
        public int Limit { get; }
        public SortOptions SortOptions { get; set; }

        public SearchOptions(int limit)
        {
            Limit = limit;
        }

        public SearchOptions()
        {
            Limit = int.MaxValue;
        }
    }

    public class SortOptions
    {
        private readonly List<KeyValuePair<string, bool>> _fields = new List<KeyValuePair<string, bool>>();
        public bool SortByTextScore { get; set; }

        public SortOptions SortBy(string field, bool asc = true)
        {
            _fields.Add(new KeyValuePair<string, bool>(field, asc));
            return this;
        }

        public bool IsEmpty() => !SortByTextScore && !_fields.Any();

        public IEnumerable<KeyValuePair<string, bool>> GetFields() => _fields.ToArray();
    }
}