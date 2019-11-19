using System.Collections.Generic;

namespace DSFramework.Data.Contracts.Models
{
    public class PagedList<T> where T : class
    {
        public List<T> Data { get; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }

        public PagedList()
        {
            Data = new List<T>();
        }
    }
}