using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.Pagination
{
    public class PaginatedList<T>
    {
        public PaginatedList(List<T> items, int count, Page page)
        {
            PageNumber = page.Number;
            TotalPages = (int)Math.Ceiling(count / (double)page.Size);
            HasNextPage = page.Number < TotalPages;
            HasPreviousPage = page.Number > 1;
            PageSize = page.Size;
            Items = items;
            TotalItems = count;
        }

        public List<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalPages { get; }
        public int TotalItems { get; }
        public bool HasNextPage { get; }
        public bool HasPreviousPage { get; }
        public PaginatedList<V> Select<V>(Func<T, V> map)
        {
            return new PaginatedList<V>(Items.Select(map).ToList(), TotalItems, new Page(PageNumber, PageSize));
        }
    }
}
