namespace EImece.Web.Models;

/// <summary>
/// View model for the custom Admin data grid (Grid.Mvc / MVCGrid replacement).
/// </summary>
public sealed class AdminListViewModel
{
    public string Title { get; set; } = "Admin";
    public string? Search { get; set; }
    public string ControllerName { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public IReadOnlyList<string> Columns { get; set; } = Array.Empty<string>();
    public IReadOnlyList<IReadOnlyList<string?>> Rows { get; set; } = Array.Empty<IReadOnlyList<string?>>();
    public string? Notice { get; set; }

    public bool ShowCreateLink { get; set; } = true;
    public bool ShowEditButton { get; set; } = true;
    public bool ShowDeleteButton { get; set; } = true;
    public bool ShowExportLink { get; set; }
    public string CreateAction { get; set; } = "SaveOrEdit";
    public string EditAction { get; set; } = "SaveOrEdit";
    public string ExportAction { get; set; } = "ExportExcelAsync";

    /// <summary>Optional Ajax soft-delete endpoint name under Admin/Ajax (e.g. DeleteProductGridItem).</summary>
    public string? AjaxDeleteAction { get; set; }

    /// <summary>
    /// Grid name for adminEimece.js (data-gridname). Derived from AjaxDeleteAction when unset
    /// (DeleteBrandGridItem → BrandGrid).
    /// </summary>
    public string? GridName { get; set; }

    public string? ResolvedGridName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(GridName)) return GridName;
            if (string.IsNullOrWhiteSpace(AjaxDeleteAction)) return null;
            const string prefix = "Delete";
            const string suffix = "Item";
            if (AjaxDeleteAction.StartsWith(prefix, StringComparison.Ordinal)
                && AjaxDeleteAction.EndsWith(suffix, StringComparison.Ordinal)
                && AjaxDeleteAction.Length > prefix.Length + suffix.Length)
            {
                return AjaxDeleteAction[prefix.Length..^suffix.Length];
            }
            return null;
        }
    }

    public bool EnableBulkOps => !string.IsNullOrWhiteSpace(ResolvedGridName);

    /// <summary>When set, last column before actions is treated as IsActive (Evet/Hayır) with a toggle hint.</summary>
    public bool ShowActiveBadge { get; set; } = true;

    // Paging / sorting (server-side)
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public string? Sort { get; set; }
    public string SortDir { get; set; } = "desc";

    public static readonly int[] PageSizeOptions = [10, 25, 50, 100, 200];
}

public sealed class ReportResultViewModel
{
    public string Title { get; set; } = "Report";
    public string ActionName { get; set; } = string.Empty;
    public System.Data.DataTable? Table { get; set; }
    public System.Data.DataSet? DataSet { get; set; }
    public string? Error { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
