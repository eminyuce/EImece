using System.Web.Optimization;

namespace EImece
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = true;
            bundles.IgnoreList.Clear();
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                  "~/Scripts/jquery-ui-{version}.js"));

            // Admin-only stack. Storefront keeps ~/bundles/jquery and ~/bundles/jqueryui until storefront cutover.
            bundles.Add(new ScriptBundle("~/bundles/adminJquery").Include(
                        "~/Scripts/admin-vendor/jquery-4.0.0.js"));

            bundles.Add(new ScriptBundle("~/bundles/adminJqueryMigrate").Include(
                        "~/Scripts/admin-vendor/jquery-migrate-4.0.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/adminJqueryUi").Include(
                        "~/Scripts/admin-vendor/jquery-ui-1.14.2.js"));

            bundles.Add(new StyleBundle("~/Content/adminJqueryUiCss").Include(
                        "~/Content/admin-vendor/jquery-ui/jquery-ui.css"));

            // Phase 1: Bootstrap 3.3.7 JS with jQuery 4 version-check patch. Phase 3 swaps this to bootstrap.bundle.js 5.3.8.
            bundles.Add(new ScriptBundle("~/bundles/adminBootstrap").Include(
                        "~/Scripts/admin-vendor/bootstrap-3.3.7.js"));

            bundles.Add(new StyleBundle("~/Content/adminBootstrapCss").Include(
                        "~/Content/admin-vendor/bootstrap/bootstrap.css"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-theme.css",
                   "~/Content/site.css"
                      ));

            // Core theme + tiny vendor CSS. Skin duplicates removed from layout (see perf-overrides.css).
            bundles.Add(new StyleBundle("~/Content/eimeceTheme").Include(
                 "~/Content/mstore/css/theme.min.css",
                  "~/Content/mstore/css/vendor.min.css",
                  "~/Content/mstore/css/perf-overrides.css",
                  "~/Content/pageThemes.css"
                 ));

            // vendor.min.js already includes jQuery 3.3.1 + Bootstrap + Owl + Feather + Fancybox.
            bundles.Add(new ScriptBundle("~/bundles/mstore").Include(
                    "~/Content/mstore/js/vendor.min.js",
                    "~/Content/mstore/js/theme.min.js"));

            bundles.Add(new StyleBundle("~/Content/admincss").Include(
                      "~/Content/griddly.css",
                      "~/Content/adminGriddlyCompat.css",
                      "~/Content/deleteStyle.css",
                      "~/Content/checkBoxStyle.css",
                      "~/Content/adminSite.css",
                      "~/Content/adminShell.css",
                      "~/Content/adminReports.css",
                      "~/Content/adminGridModern.css"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/adminScripts").Include(
                  "~/Scripts/rich-text-editor.js",
                  "~/Scripts/adminEimece.js",
                  "~/Scripts/adminGridModern.js",
                  "~/Scripts/griddly.js"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                                "~/Content/themes/base/accordion.css",
                                "~/Content/themes/base/all.css",
                                "~/Content/themes/base/autocomplete.css",
                                "~/Content/themes/base/base.css",
                                "~/Content/themes/base/button.css",
                                "~/Content/themes/base/core.css",
                                "~/Content/themes/base/datepicker.css",
                                "~/Content/themes/base/dialog.css",
                                "~/Content/themes/base/draggable.css",
                                "~/Content/themes/base/menu.css",
                                "~/Content/themes/base/progressbar.css",
                                "~/Content/themes/base/resizable.css",
                                "~/Content/themes/base/selectable.css",
                                "~/Content/themes/base/selectmenu.css",
                                "~/Content/themes/base/slider.css",
                                "~/Content/themes/base/sortable.css",
                                "~/Content/themes/base/spinner.css",
                                "~/Content/themes/base/tabs.css",
                                "~/Content/themes/base/theme.css",
                                "~/Content/themes/base/tooltip.css"));

            bundles.Add(new StyleBundle("~/Content/filepondcss").Include(
                    "~/Content/filepond/filepond.min.css",
                    "~/Content/filepond/filepond-plugin-image-preview.min.css"
                    ));

            bundles.Add(new ScriptBundle("~/bundles/filepond").Include(
                    "~/Scripts/filepond/filepond-plugin-image-preview.min.js",
                    "~/Scripts/filepond/filepond-plugin-file-validate-type.min.js",
                    "~/Scripts/filepond/filepond-plugin-file-validate-size.min.js",
                    "~/Scripts/filepond/filepond-plugin-image-exif-orientation.min.js",
                    "~/Scripts/filepond/filepond-plugin-image-validate-size.min.js",
                    "~/Scripts/filepond/filepond.min.js",
                    "~/Scripts/filepond/filepond.jquery.js",
                    "~/Scripts/admin/filepond-progress-tracker.js"
                    ));

            bundles.Add(new StyleBundle("~/Content/blueimp-gallery").Include(
                    "~/Content/blueimp-gallery2/css/blueimp-gallery.css",
                    "~/Content/blueimp-gallery2/css/blueimp-gallery-video.css",
                    "~/Content/blueimp-gallery2/css/blueimp-gallery-indicator.css"
                    ));

            bundles.Add(new ScriptBundle("~/bundles/Blueimp-Gallerry2").Include(//Blueimp Gallery 2
                    "~/Scripts/blueimp-gallery2/js/blueimp-gallery.js",
                    "~/Scripts/blueimp-gallery2/js/blueimp-gallery-video.js",
                    "~/Scripts/blueimp-gallery2/js/blueimp-gallery-indicator.js",
                    "~/Scripts/blueimp-gallery2/js/jquery.blueimp-gallery.js"));

            // Do NOT re-include jQuery here — mstore vendor.min.js already ships jQuery 3.3.1 (~85KB saved).
            bundles.Add(new ScriptBundle("~/bundles/eimeceScripts").Include(
                    "~/Scripts/eimece.js",
                    "~/Scripts/cookie-consent.js",
                    "~/Scripts/mustache.min.js"));

            EImece.App_Start.DesignConfig.RegisterDesignBundles(bundles);
        }
    }
}