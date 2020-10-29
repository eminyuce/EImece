using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Models.FrontModels
{
    public class PaginatedModelList<T> : List<T>, IPaginatedModelList
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPageCount { get; set; }
        public string Search { set; get; }
        public string Filter { set; get; }
        public int? Sorting { set; get; }
        public string RouteId { set; get; }
        public int? MinPrice { set; get; }
        public int? MaxPrice { set; get; }

        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPageCount);
            }
        }

        public PaginatedModelList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            // Check: Do we need to check if pageSize > totalCount.
            // Check: Do we need to check if int parameters < 0.

            AddRange(source);
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPageCount = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public IEnumerable<T> GetPagingResult()
        {
            if (PageIndex == 0)
            {
                PageIndex = 1;
            }
            return this.Skip((PageIndex - 1) * PageSize).Take(PageSize);
        }
    }
}