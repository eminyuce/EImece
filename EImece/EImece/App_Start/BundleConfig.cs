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
            // Storefront Register / Customers Index still Render this bundle. Same 1.14.2 file as admin.
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                  "~/Scripts/admin-vendor/jquery-ui-1.14.2.js"));

            // Admin-only stack (jQuery 4 / Bootstrap 5). Storefront uses siteJquery / Crizal vendor bundles.
            bundles.Add(new ScriptBundle("~/bundles/adminJquery").Include(
                        "~/Scripts/admin-vendor/jquery-4.0.0.js"));

            bundles.Add(new ScriptBundle("~/bundles/adminJqueryMigrate").Include(
                        "~/Scripts/admin-vendor/jquery-migrate-4.0.2.js"));

            bundles.Add(new ScriptBundle("~/bundles/adminJqueryUi").Include(
                        "~/Scripts/admin-vendor/jquery-ui-1.14.2.js"));

            bundles.Add(new StyleBundle("~/Content/adminJqueryUiCss").Include(
                        "~/Content/admin-vendor/jquery-ui/jquery-ui.css"));

            // Admin Bootstrap 5.3.8 (already minified; ScriptBundle AjaxMin cannot parse BS5 ES6).
            bundles.Add(new Bundle("~/bundles/adminBootstrap").Include(
                        "~/Scripts/admin-vendor/bootstrap.bundle.min.js"));

            bundles.Add(new Bundle("~/Content/adminBootstrapCss").Include(
                        "~/Content/admin-vendor/bootstrap/bootstrap.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Storefront site-wide Bootstrap 5.3.8 + jQuery 4 (shared vendor files with admin).
            bundles.Add(new ScriptBundle("~/bundles/siteJquery").Include(
                        "~/Scripts/admin-vendor/jquery-4.0.0.js"));
            bundles.Add(new ScriptBundle("~/bundles/siteJqueryMigrate").Include(
                        "~/Scripts/admin-vendor/jquery-migrate-4.0.2.js"));
            bundles.Add(new Bundle("~/bundles/siteBootstrap").Include(
                        "~/Scripts/admin-vendor/bootstrap.bundle.min.js"));
            bundles.Add(new Bundle("~/Content/siteBootstrapCss").Include(
                        "~/Content/admin-vendor/bootstrap/bootstrap.min.css",
                        "~/Content/pageThemes.css"));


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
                  "~/Scripts/admin-bs5-jquery-bridge.js",
                  "~/Scripts/rich-text-editor.js",
                  "~/Scripts/adminEimece.js",
                  "~/Scripts/adminRequiredLabels.js",
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

            bundles.Add(new ScriptBundle("~/bundles/eimeceScripts").Include(
                    "~/Scripts/eimece.js",
                    "~/Scripts/cookie-consent.js",
                    "~/Scripts/mustache.min.js"));

            EImece.App_Start.DesignConfig.RegisterDesignBundles(bundles);
        }
    }
}