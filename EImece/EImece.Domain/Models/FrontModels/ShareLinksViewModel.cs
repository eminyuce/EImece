using System.Collections.Generic;

namespace EImece.Domain.Models.FrontModels
{
    public class ShareLinksViewModel
    {
        public Dictionary<string, string> SocialMediaLinks { get; set; }

        public string ShareLabel { get; set; }

        public bool Compact { get; set; }

        public string ShareUrl
        {
            get
            {
                if (SocialMediaLinks == null)
                {
                    return string.Empty;
                }

                string url;
                if (SocialMediaLinks.TryGetValue(Constants.SharePageUrl, out url) && !string.IsNullOrWhiteSpace(url))
                {
                    return url;
                }

                return string.Empty;
            }
        }

        public bool HasContent
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ShareUrl)
                    || (SocialMediaLinks != null && SocialMediaLinks.Count > 0);
            }
        }

        public static ShareLinksViewModel Create(Dictionary<string, string> socialMediaLinks, string shareLabel, bool compact = false)
        {
            return new ShareLinksViewModel
            {
                SocialMediaLinks = socialMediaLinks ?? new Dictionary<string, string>(),
                ShareLabel = shareLabel,
                Compact = compact
            };
        }

        public bool ContainsLink(string key)
        {
            return SocialMediaLinks != null
                && SocialMediaLinks.ContainsKey(key)
                && !string.IsNullOrEmpty(SocialMediaLinks[key]);
        }
    }
}
