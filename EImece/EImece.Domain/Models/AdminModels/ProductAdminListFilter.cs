using System;

namespace EImece.Domain.Models.AdminModels
{
    /// <summary>
    /// Optional filters for the admin products list (beyond category / brand / search).
    /// Null bools and empty state mean "all" (no filter applied).
    /// </summary>
    public class ProductAdminListFilter
    {
        public string State { get; set; }

        public bool? IsActive { get; set; }

        public bool? MainPage { get; set; }

        public bool? IsCampaign { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// When false (site price display disabled), price range filters are ignored.
        /// </summary>
        public bool ApplyPriceFilter { get; set; }

        public bool HasAnyFilter
        {
            get
            {
                return !string.IsNullOrWhiteSpace(State)
                    || IsActive.HasValue
                    || MainPage.HasValue
                    || IsCampaign.HasValue
                    || (ApplyPriceFilter && (MinPrice.HasValue || MaxPrice.HasValue));
            }
        }

        public string ToQueryString()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(State))
            {
                parts.Add("state=" + Uri.EscapeDataString(State.Trim()));
            }
            if (IsActive == true)
            {
                parts.Add("isActive=true");
            }
            if (MainPage == true)
            {
                parts.Add("mainPage=true");
            }
            if (IsCampaign == true)
            {
                parts.Add("isCampaign=true");
            }
            if (ApplyPriceFilter)
            {
                if (MinPrice.HasValue)
                {
                    parts.Add("minPrice=" + MinPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
                if (MaxPrice.HasValue)
                {
                    parts.Add("maxPrice=" + MaxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            return parts.Count == 0 ? string.Empty : string.Join("&", parts);
        }
    }
}
