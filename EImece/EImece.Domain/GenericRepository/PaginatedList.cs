﻿using System;
using System.Collections.Generic;

namespace EImece.Domain.GenericRepository
{
    /// <summary>
    /// List object to represent the paginated collection.
    /// </summary>
    /// <typeparam name="T">Type of the Entity</typeparam>
    public class PaginatedList<T> : List<T>, IPaginatedList
    {
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPageCount { get; private set; }

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

        private bool IsFirstPage { get; }
        private bool IsLastPage { get; }

        bool IPaginatedList.IsFirstPage => throw new NotImplementedException();

        bool IPaginatedList.IsLastPage => throw new NotImplementedException();

        public PaginatedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
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
    }

    public interface IPaginatedList
    {
        int TotalPageCount { get; }
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
    }
}