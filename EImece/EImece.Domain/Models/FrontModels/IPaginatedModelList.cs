namespace EImece.Domain.Models.FrontModels
{
    public interface IPaginatedModelList
    {
        int TotalPageCount { get; }
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        string Filter { set; get; }
        string Search { set; get; }
        int ? RouteId { set; get; }
        int ? Sorting { set; get; }
        int ? MinPrice { set; get; }
        int ? MaxPrice { set; get; }
    }
}