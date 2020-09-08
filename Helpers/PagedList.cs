namespace Library.API.Helpers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class PagedListCollection<T> : List<T>
    {
        public int CurrentPage { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }

        public bool HasPrevious => CurrentPage > 1;

        public bool HasNext => CurrentPage < TotalPages;

        public PagedListCollection() { }

        private PagedListCollection(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            TotalCount = totalCount;
            CurrentPage = pageNumber;
            TotalPages = (int) Math.Ceiling(totalCount / (double) pageSize);
            AddRange(items);
        }

        public PagedListCollection<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedListCollection<T>(items, count, pageNumber, pageSize);
        }
    }
}