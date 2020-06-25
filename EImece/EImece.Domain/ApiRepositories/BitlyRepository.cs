using EImece.Domain.Helpers;
using EImece.Domain.Models.UrlShortenModels;
using EImece.Domain.Repositories.IRepositories;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Ninject;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EImece.Domain.ApiRepositories
{
    public class BitlyRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public IShortUrlRepository ShortUrlRepository { get; set; }

        public static string BitlyAPIAccessToken
        {
            get
            {
                string access_token = AppConfig.GetConfigString("BitlyAPI_Access_Token", "f5d605e9e5a9b5eaa92d26d432a104fa2b6eb9bb");
                return access_token;
            }
        }

        private static IRestResponse MakeGetRequest(string requestUrl)
        {
            var client = new RestClient(requestUrl);
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bearer " + BitlyAPIAccessToken);
            request.AddHeader("content-type", "application/json");
            IRestResponse response = client.Execute(request);
            return response;
        }

        public string GetGroup()
        {
            var client = new RestClient("https://api-ssl.bitly.com/v4/groups");
            var request = new RestRequest(Method.GET);
            // request.AddHeader("postman-token", "3f3e65e2-8aa7-bda3-0faa-c19a1c30cb29");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bearer " + BitlyAPIAccessToken);
            IRestResponse response = client.Execute(request);

            return response.Content;
        }

        /// <summary>
        /// http://dev.bitly.com/v4_documentation.html
        /// Use the /oauth/access_token API endpoint documented below to acquire an OAuth access token,
        /// passing the code value appended by Bitly to the previous redirect and the same redirect_uri value
        /// that was used previously. This API endpoint will return an OAuth access token, as well as the specified
        /// Bitly user's login and API key, allowing your application to utilize the Bitly API on that user's behalf.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="code"></param>
        /// <param name="redirectionUrl"></param>
        /// <returns></returns>
        public string GetOAuthAccessToken(string clientId, string clientSecret, string code, string redirectionUrl)
        {
            clientId = !String.IsNullOrEmpty(clientId) ? clientId : "cfc00f82f891ea8f3b449571cd5161f49cabc1b1";
            clientSecret = "c8688e0ed49a5bf39cbecacee841c650e14fe734";

            // Make that request to get OAuth code
            //https://bitly.com/oauth/authorize?client_id=...&state=...&redirect_uri=http://myexamplewebapp.com/oauth_page
            code = "7fdfb003365667bdb57678f263664da57bf28773";
            redirectionUrl = " http://login.seatechnologyjobs.com/";
            var client = new RestClient("https://api-ssl.bitly.com/oauth/access_token");
            var request = new RestRequest(Method.POST);
            //  request.AddHeader("postman-token", "72780a27-4dd7-b531-af52-28af12dcec33");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            string bodyParameter = String.Format("client_id={0}&client_secret={1}&code={2}&redirect_uri={3}", clientId, clientSecret, code, redirectionUrl);
            request.AddBody(bodyParameter);
            IRestResponse response = client.Execute(request);

            return response.Content;
        }

        /// <summary>
        /// Shorten a Link
        /// Convert a long url to a Bitlink
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        public BitlyShortUrl ShortenUrl(BitlyShortUrlRequest shortUrl)
        {
            string json = JsonConvert.SerializeObject(shortUrl, Formatting.Indented);

            var client = new RestClient("https://api-ssl.bitly.com/v4/shorten");
            var request = new RestRequest(Method.POST);
            // request.AddHeader("postman-token", "93e96bcd-b2b6-a486-6939-8af2fe5f2f95");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bearer " + BitlyAPIAccessToken);
            request.AddHeader("content-type", "application/json");
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var statusCode = response.StatusCode;
            if (statusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<BitlyShortUrl>(response.Content);
            }
            else
            {
                return new BitlyShortUrl() { long_url = shortUrl.long_url, link = shortUrl.long_url };
            }
        }

        // https://api-ssl.bitly.com/v4/bitlinks/{bitlink}/clicks
        /// <summary>
        /// Get Clicks for a Bitlink
        /// This will return the click counts for a specified Bitlink.This returns an array with clicks based on a date.
        /// </summary>
        /// <param name="bitLink">A Bitlink made of the domain and hash</param>
        /// <returns></returns>
        public BitlyUrlClickStats GetBitlyUrlStats(string bitLink)
        {
            var requestUrl = String.Format("https://api-ssl.bitly.com/v4/bitlinks/{0}/clicks", bitLink);
            IRestResponse response = MakeGetRequest(requestUrl);
            var statusCode = response.StatusCode;
            return JsonConvert.DeserializeObject<BitlyUrlClickStats>(response.Content);
        }

        /// <summary>
        /// Get Clicks Summary for a Bitlink
        /// This will return the click counts for a specified Bitlink. This rolls up all the data into a single field of clicks.
        /// </summary>
        /// <param name="bitLink">A Bitlink made of the domain and hash</param>
        /// <returns></returns>
        public BitlyUrlClickSummaryStats GetBitlyUrlSummaryStats(string bitLink)
        {
            var requestUrl = String.Format("https://api-ssl.bitly.com/v4/bitlinks/{0}/clicks/summary", bitLink);
            IRestResponse response = MakeGetRequest(requestUrl);
            var statusCode = response.StatusCode;
            if (statusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<BitlyUrlClickSummaryStats>(response.Content);
            }
            else
            {
                return new BitlyUrlClickSummaryStats() { };
            }
        }

        public EmailShortenUrlsResult ConvertEmailLinkstoBitlyShortLinks(string emailContent)
        {
            var sGuid = AppConfig.GetConfigString("BitlyApi_Group_Guid", "Bi2cjVeYTlv");
            var emailContentResult = ConvertEmailLinkstoBitlyShortLinks(sGuid, emailContent);
            return emailContentResult;
        }

        /// <summary>
        /// Takes email content and convert all links to bitly shorten link
        /// </summary>
        /// <param name="shortUrlGroupGuid"></param>
        /// <param name="emailContent"></param>
        /// <returns></returns>
        public EmailShortenUrlsResult ConvertEmailLinkstoBitlyShortLinks(string shortUrlGroupGuid, string emailContent)
        {
            var result = new EmailShortenUrlsResult();
            try
            {
                HtmlDocument doc = new HtmlDocument();
                Dictionary<string, string> urlDic = new Dictionary<string, string>();
                result.UrlLongAndShortUrls = urlDic;
                result.EmailContent = emailContent;
                // convert string to stream
                byte[] byteArray = Encoding.ASCII.GetBytes(emailContent);
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    // convert stream to string
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        doc.Load(reader);
                        var aHrefLinks = doc.DocumentNode.SelectNodes("//a[@href]").Distinct();
                        if (aHrefLinks != null)
                        {
                            foreach (HtmlNode link in aHrefLinks)
                            {
                                string hrefValue = "";
                                string dataBitlyUrl = "";
                                try
                                {
                                    // Get the value of the HREF attribute
                                    hrefValue = link.GetAttributeValue("href", string.Empty).ToStr();
                                    dataBitlyUrl = link.GetAttributeValue("data-bitly-url", string.Empty).ToStr().ToLower();
                                    dataBitlyUrl = String.IsNullOrEmpty(dataBitlyUrl) ? "true" : dataBitlyUrl.ToLower();

                                    if (urlDic.ContainsKey(hrefValue))
                                        continue;

                                    var shortUrl = new BitlyShortUrlRequest();
                                    shortUrl.group_guid = shortUrlGroupGuid;
                                    shortUrl.long_url = hrefValue;
                                    string bitlyShortUrl = hrefValue;
                                    if (dataBitlyUrl.ToBool())
                                    {
                                        var tt = ShortenUrl(shortUrl);
                                        bitlyShortUrl = tt.link;
                                    }

                                    //    Console.WriteLine(hrefValue);
                                    if (!String.IsNullOrEmpty(hrefValue) && !String.IsNullOrEmpty(bitlyShortUrl))
                                    {
                                        urlDic.Add(hrefValue, bitlyShortUrl);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //  Console.WriteLine(ex.Message);
                                    Logger.Error(ex, ex.Message + " " + hrefValue);
                                }
                            }
                            foreach (HtmlNode link in aHrefLinks)
                            {
                                string hrefValue = "";
                                try
                                {
                                    hrefValue = link.GetAttributeValue("href", string.Empty);
                                    if (urlDic.ContainsKey(hrefValue))
                                    {
                                        var bitlyShortUrl = urlDic[hrefValue];
                                        link.SetAttributeValue("href", bitlyShortUrl);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //   Console.WriteLine(ex.Message);
                                    Logger.Error(ex, ex.Message + " " + hrefValue);
                                }
                            }
                        }
                    }
                }

                string emailContentResult = doc.DocumentNode.OuterHtml;
                result.EmailContentBitlyLinks = emailContentResult;
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        // TODO: Under development
        public TmlnkResponse GetTmlnkResponse(string url = "", string email = "", string group = "")
        {
            //string applicationUrl = Settings.GetConfigString("ApplicationFullUrl");
            //string requestUrl = String.Format("{3}/api/shorten?url={0}&email={1}&group={2}", url.ToStr(), email.ToStr(), group.ToStr(), applicationUrl);
            //IRestResponse response = MakeGetRequest(requestUrl);
            //var statusCode = response.StatusCode;
            //return JsonConvert.DeserializeObject<TmlnkResponse>(response.Content);

            var p = ShortUrlRepository.GenerateShortUrl(url, email, group);

            var r = new TmlnkResponse();
            r.EmailEid = "";
            r.ErrorMessage = "";
            r.GroupEid = "";
            r.HasError = false;
            r.ShortUrl = p.UrlKey;
            r.UrlEid = p.UrlKey;

            return r;
        }

        public EmailShortenUrlsResult ConvertEmailLinkstoShortLinks(string emailContent, string group = "")
        {
            var result = ConvertEmailLinkstoShortLinks(emailContent, group, "//a[@href]");
            return result;
        }

        public List<String> GetHtmlLinks(string htmlContent, string nodeSelection = "//a[@href]")
        {
            var result = new List<String>();
            try
            {
                HtmlDocument doc = new HtmlDocument();
                // convert string to stream
                byte[] byteArray = Encoding.ASCII.GetBytes(htmlContent);
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    // convert stream to string
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        doc.Load(reader);
                        var aHrefLinks = doc.DocumentNode.SelectNodes(nodeSelection).Distinct();
                        if (aHrefLinks != null)
                        {
                            foreach (HtmlNode link in aHrefLinks)
                            {
                                string hrefValue = "";
                                try
                                {
                                    // Get the value of the HREF attribute
                                    hrefValue = link.GetAttributeValue("href", string.Empty).ToStr();
                                    result.Add(hrefValue);
                                }
                                catch (Exception ex)
                                {
                                    //  Console.WriteLine(ex.Message);
                                    Logger.Error(ex, ex.Message + " " + hrefValue);
                                }
                            }
                        }
                    }
                }
                result = result.Distinct().ToList();
                result = result != null ? result : new List<string>();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
            }
            return result;
        }

        private EmailShortenUrlsResult ConvertEmailLinkstoShortLinks(string emailContent, string group, string nodeSelection = "//a[@href]")
        {
            var result = new EmailShortenUrlsResult();
            try
            {
                HtmlDocument doc = new HtmlDocument();
                Dictionary<string, TmlnkResponse> TmlnkResponseDic = new Dictionary<string, TmlnkResponse>();
                result.TmlnkResponseDic = TmlnkResponseDic;
                result.EmailContent = emailContent;
                // convert string to stream
                byte[] byteArray = Encoding.ASCII.GetBytes(emailContent);
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    // convert stream to string
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        doc.Load(reader);
                        var aHrefLinks = doc.DocumentNode.SelectNodes(nodeSelection).Distinct();
                        if (aHrefLinks != null)
                        {
                            foreach (HtmlNode link in aHrefLinks)
                            {
                                string hrefValue = "";
                                string dataBitlyUrl = "";
                                try
                                {
                                    // Get the value of the HREF attribute
                                    hrefValue = link.GetAttributeValue("href", string.Empty).ToStr();
                                    dataBitlyUrl = link.GetAttributeValue("data-bitly-url", string.Empty).ToStr().ToLower();
                                    dataBitlyUrl = String.IsNullOrEmpty(dataBitlyUrl) ? "true" : dataBitlyUrl.ToLower();

                                    if (TmlnkResponseDic.ContainsKey(hrefValue))
                                        continue;

                                    TmlnkResponse tmlnkResponse = null;
                                    string email = "";
                                    if (dataBitlyUrl.ToBool())
                                    {
                                        tmlnkResponse = GetTmlnkResponse(hrefValue, email, group);
                                    }

                                    //    Console.WriteLine(hrefValue);
                                    if (tmlnkResponse != null)
                                    {
                                        TmlnkResponseDic.Add(hrefValue, tmlnkResponse);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //  Console.WriteLine(ex.Message);
                                    Logger.Error(ex, ex.Message + " " + hrefValue);
                                }
                            }
                            foreach (HtmlNode link in aHrefLinks)
                            {
                                string hrefValue = "";
                                try
                                {
                                    hrefValue = link.GetAttributeValue("href", string.Empty);
                                    if (TmlnkResponseDic.ContainsKey(hrefValue))
                                    {
                                        var tmlnkResponse = TmlnkResponseDic[hrefValue];
                                        link.SetAttributeValue("href", tmlnkResponse.ShortUrl);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //   Console.WriteLine(ex.Message);
                                    Logger.Error(ex, ex.Message + " " + hrefValue);
                                }
                            }
                        }
                    }
                }
                //insert tracking image (by group only) before closed
                var trackingImage = GetTmlnkResponse("", "", group);
                var trackingImageNode = doc.CreateElement("img");
                trackingImageNode.SetAttributeValue("src", String.Format("{0}.png", trackingImage.ShortUrl));
                trackingImageNode.SetAttributeValue("style", "display:none;");
                doc.DocumentNode.ChildNodes.Add(trackingImageNode);

                string emailContentResult = doc.DocumentNode.OuterHtml;
                result.EmailContentBitlyLinks = emailContentResult;
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public string ConvertEmailBodyForTracking(bool trackWithBitly, bool trackWithMlnk, string body, string emailTemplateName, string groupName)
        {
            if (trackWithBitly)
            {
                var bitlyEmailObj = ConvertEmailLinkstoBitlyShortLinks(body);
                if (String.IsNullOrEmpty(bitlyEmailObj.ErrorMessage))
                {
                    body = bitlyEmailObj.EmailContentBitlyLinks;
                    Logger.Info("Email Template body is changed to bitly shorten links " + emailTemplateName);
                }
                else
                {
                    Logger.Error("Email Template body NOT changed to bitly shorten links " + emailTemplateName + " ErrorMessage:" + bitlyEmailObj.ErrorMessage);
                }
            }
            else if (trackWithMlnk)
            {
                var mlnkEmailObj = ConvertEmailLinkstoShortLinks(body, groupName);
                if (String.IsNullOrEmpty(mlnkEmailObj.ErrorMessage))
                {
                    body = mlnkEmailObj.EmailContentBitlyLinks;
                    Logger.Info("Email Template body is changed to t.Mlnk  shorten links " + emailTemplateName);
                }
                else
                {
                    Logger.Error("Email Template body NOT changed to t.Mlnk shorten links " + emailTemplateName + " ErrorMessage:" + mlnkEmailObj.ErrorMessage);
                }
            }

            return body;
        }
    }
}