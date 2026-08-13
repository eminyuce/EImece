using EImece.Domain.Helpers.RazorCustomRssTemplate;
using EImece.Domain.Models.AdminModels;
using EImece.Domain.Models.FrontModels;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;

namespace EImece.Domain.Helpers.EmailHelper
{
    /// <summary>
    /// Application-wide Razor rendering engine. Compiling a Razor template produces a dynamic
    /// assembly that, on .NET Framework, is loaded into the AppDomain and can never be unloaded.
    /// Creating a new <see cref="IRazorEngineService"/> per render (the previous behaviour) therefore
    /// leaked an assembly and burned CPU on every call. This abstraction owns a single, thread-safe
    /// engine that compiles each distinct template once (keyed by content hash) and reuses the
    /// cached compilation thereafter.
    /// </summary>
    public interface IRazorTemplateEngine
    {
        /// <summary>Renders a template against a <see cref="RazorEngineModel"/>.</summary>
        RazorRenderResult GetRenderOutput(string template, RazorEngineModel model = null);

        /// <summary>Renders a template against a strongly-typed <see cref="RazorTemplateModel"/>.</summary>
        RazorRenderResult GetRenderOutputByModel<T>(string template, T model) where T : RazorTemplateModel;

        /// <summary>Renders a template against a dynamic model (mail template test emails).</summary>
        RazorRenderResult GetRenderOutputDynamic(string template, object model);
    }

    /// <summary>
    /// Thread-safe singleton implementation. <see cref="IRazorEngineService"/> is safe for concurrent
    /// use, so a single instance is shared across all requests. Registered <c>InSingletonScope</c>.
    /// </summary>
    public sealed class RazorTemplateEngine : IRazorTemplateEngine, IDisposable
    {
        private readonly IRazorEngineService _engine;

        public RazorTemplateEngine()
        {
            var configuration = new TemplateServiceConfiguration
            {
                // FIX: Debug=false disables debug source emission and enables the optimized,
                // cacheable code path. Debug=true (the old value) also kept generated files around.
                Debug = false
            };
            configuration.Namespaces.Add("EImece.Domain.Helpers");
            configuration.Namespaces.Add("EImece.Domain.Entities");
            configuration.Namespaces.Add("EImece.Domain.Models.FrontModels");
            configuration.Namespaces.Add("EImece.Domain.Helpers.RazorCustomRssTemplate");
            configuration.Namespaces.Add("System.Xml");
            configuration.Namespaces.Add("System.Web.Mvc");
            configuration.Namespaces.Add("System.Text");
            configuration.Namespaces.Add("System.Web.Mvc.Html");
            configuration.Namespaces.Add("System.Xml.Linq");
            configuration.Namespaces.Add("System.Linq");
            configuration.Namespaces.Add("Resources");
            configuration.Namespaces.Add("System.ServiceModel.Syndication");
            configuration.BaseTemplateType = typeof(VBCustomTemplateBase<>);

            // FIX: created ONCE for the lifetime of the application instead of per render.
            _engine = RazorEngineService.Create(configuration);
        }

        public RazorRenderResult GetRenderOutput(string template, RazorEngineModel model = null)
        {
            return Render(template, typeof(RazorEngineModel), model ?? new RazorEngineModel(), "rem");
        }

        public RazorRenderResult GetRenderOutputByModel<T>(string template, T model) where T : RazorTemplateModel
        {
            return Render(template, typeof(T), model, "typed_" + typeof(T).FullName);
        }

        public RazorRenderResult GetRenderOutputDynamic(string template, object model)
        {
            return Render(template, null, model ?? new DynamicMailTemplateModel(), "maildyn");
        }

        private RazorRenderResult Render(string template, Type modelType, object model, string keyPrefix)
        {
            var result = new RazorRenderResult();
            if (String.IsNullOrEmpty(template))
            {
                return result;
            }

            try
            {
                result.Source = template;

                // Content-hash key: identical templates share one compiled assembly forever.
                var key = keyPrefix + "_" + GeneralHelper.GetHashString(template);

                result.Result = _engine.IsTemplateCached(key, modelType)
                    ? _engine.Run(key, modelType, model)
                    : _engine.RunCompile(template, key, modelType, model);
            }
            catch (TemplateCompilationException ex)
            {
                result.templateCompilationException = ex;
            }
            catch (Exception ex)
            {
                result.GeneralError = ex;
            }

            return result;
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }
    }
}
