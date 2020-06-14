using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Models.UrlShortenModels
{
    public class EmailShortenUrlsResult
    {
        public string EmailContent { get; set; }
        public string ErrorMessage { get; set; }
        public string EmailContentBitlyLinks { get; set; }
        public Dictionary<string, string> UrlLongAndShortUrls { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, TmlnkResponse> TmlnkResponseDic { get; set; } = new Dictionary<string, TmlnkResponse>();
    }

    public class BitlyShortUrlRequest
    {
        public string group_guid { get; set; }
        public string long_url { get; set; }
    }

    public class References
    {
        public string group { get; set; }
    }

    public class BitlyShortUrl
    {
        public DateTime created_at { get; set; }
        public string id { get; set; }
        public string link { get; set; }
        public List<object> custom_bitlinks { get; set; }
        public string long_url { get; set; }
        public bool archived { get; set; }
        public List<object> tags { get; set; }
        public List<object> deeplinks { get; set; }
        public References references { get; set; }
    }

    public partial class BitlyUrlClickStats
    {
        [JsonProperty("unit_reference")]
        public string UnitReference { get; set; }

        [JsonProperty("link_clicks")]
        public LinkClick[] LinkClicks { get; set; }

        [JsonProperty("units")]
        public long Units { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }
    }

    public partial class LinkClick
    {
        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("clicks")]
        public long Clicks { get; set; }
    }

    public partial class BitlyUrlClickSummaryStats
    {
        [JsonIgnore]
        public string FullUrl { get; set; }

        [JsonIgnore]
        public string BitlyShortenUrl { get; set; }

        [JsonProperty("unit_reference")]
        public string UnitReference { get; set; }

        [JsonProperty("total_clicks")]
        public long TotalClicks { get; set; }

        [JsonProperty("units")]
        public long Units { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }
    }

    public partial class TmlnkResponse
    {
        [JsonProperty("ShortUrl")]
        public string ShortUrl { get; set; }

        [JsonProperty("EmailEid")]
        public object EmailEid { get; set; }

        [JsonProperty("GroupEid")]
        public string GroupEid { get; set; }

        [JsonProperty("UrlEid")]
        public string UrlEid { get; set; }

        [JsonProperty("ErrorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("HasError")]
        public bool HasError { get; set; }
    }
}