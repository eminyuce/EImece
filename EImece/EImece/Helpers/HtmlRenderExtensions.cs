using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace EImece.Helpers
{
    public static class HtmlRenderExtensions
    {
        /// <summary>
        /// Delegate script/resource/etc injection until the end of the page
        /// </summary>
        private class DelayedInjectionBlock : IDisposable
        {
            private const string CACHE_KEY = "DCCF8C78-2E36-4567-B0CF-FE052ACCE309";
            private const string UNIQUE_IDENTIFIER_KEY = CACHE_KEY;
            private const string EMPTY_IDENTIFIER = "";

            public static Queue<string> GetQueue(HtmlHelper helper, string identifier = null)
            {
                return _GetOrSet(helper, new Queue<string>(), identifier ?? EMPTY_IDENTIFIER);
            }

            private static T _GetOrSet<T>(HtmlHelper helper, T defaultValue, string identifier = EMPTY_IDENTIFIER) where T : class
            {
                var storage = GetStorage(helper);
                return (T)(storage.ContainsKey(identifier) ? storage[identifier] : (storage[identifier] = defaultValue));
            }

            public static Dictionary<string, object> GetStorage(HtmlHelper helper)
            {
                var storage = helper.ViewContext.HttpContext.Items[CACHE_KEY] as Dictionary<string, object>;
                if (storage == null) helper.ViewContext.HttpContext.Items[CACHE_KEY] = (storage = new Dictionary<string, object>());
                return storage;
            }

            private readonly HtmlHelper helper;
            private readonly string identifier;
            private readonly string isOnlyOne;

            public DelayedInjectionBlock(HtmlHelper helper, string identifier = null, string isOnlyOne = null)
            {
                this.helper = helper;
                ((WebViewPage)this.helper.ViewDataContainer).OutputStack.Push(new StringWriter());
                this.identifier = identifier ?? EMPTY_IDENTIFIER;
                this.isOnlyOne = isOnlyOne;
            }

            public void Dispose()
            {
                var content = ((WebViewPage)this.helper.ViewDataContainer).OutputStack;
                var renderedContent = content.Count == 0 ? string.Empty : content.Pop().ToString();

                var queue = GetQueue(this.helper, this.identifier);
                var existingIdentifiers = _GetOrSet(this.helper, new Dictionary<string, int>(), UNIQUE_IDENTIFIER_KEY);

                if (null == this.isOnlyOne || !existingIdentifiers.ContainsKey(this.isOnlyOne))
                {
                    queue.Enqueue(renderedContent);
                    if (null != this.isOnlyOne) existingIdentifiers[this.isOnlyOne] = queue.Count;
                }
            }
        }

        public static IDisposable Delayed(this HtmlHelper helper, string injectionBlockId = null, string isOnlyOne = null)
        {
            return new DelayedInjectionBlock(helper, injectionBlockId, isOnlyOne);
        }

        public static MvcHtmlString RenderDelayed(this HtmlHelper helper, string injectionBlockId = null, bool removeAfterRendering = true, bool onlyUnique = false)
        {
            var stack = DelayedInjectionBlock.GetQueue(helper, injectionBlockId);

            if (removeAfterRendering)
            {
                var strings = new List<string>();
#if DEBUG
                strings.Add(string.Format("<!-- delayed-block: {0} -->", injectionBlockId));
#endif

                while (stack.Count > 0)
                {
                    strings.Add(stack.Dequeue());
                }

                if (onlyUnique)
                {
                    strings = strings.Select(s => s.Trim()).Distinct().ToList();
                }

                var res = string.Join(Environment.NewLine, strings);
                return MvcHtmlString.Create(res);
            }

            if (onlyUnique)
            {
                stack = (Queue<string>)stack.Select(s => s.Trim()).Distinct();
            }

            return MvcHtmlString.Create(
#if DEBUG
string.Format("<!-- delayed-block: {0} -->", injectionBlockId) +
#endif
 string.Join(Environment.NewLine, stack));
        }
    }
}
