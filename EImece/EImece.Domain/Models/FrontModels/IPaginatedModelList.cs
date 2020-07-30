using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Models.FrontModels
{
    public interface IPaginatedModelList { 

        int TotalPageCount { get; }
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }

        int RouteId { set; get; }
        int Sorting { set; get; }
    }
}
