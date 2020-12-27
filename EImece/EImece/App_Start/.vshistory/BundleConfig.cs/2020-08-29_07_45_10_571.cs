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
                   //   "~/Content/bootstrap-theme.css.map",
                   "~/Content/site.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/eimeceTheme").Include(
                 "~/Content/mstore/css/theme.min.css",
                  "~/Content/mstore/css/vendor.min.css"
                 ));

            bundles.Add(new ScriptBundle("~/bundles/mstore").Include(
                    "~/Content/mstore/js/vendor.min.js",
                    "~/Content/mstore/js/theme.min.js"));

            bundles.Add(new StyleBundle("~/Content/admincss").Include(
                      "~/Content/Gridmvc.css",
                      "~/Content/deleteStyle.css",
                      "~/Content/checkBoxStyle.css",
                      "~/Content/adminSite.css"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/adminScripts").Include(
                  "~/Scripts/adminEimece.js",
                  "~/Scripts/gridmvc.js",
                  "~/MVCGridHandler.axd/script.js" ));
            


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

            bundles.Add(new StyleBundle("~/Content/jQuery-File-Upload").Include(
                    "~/Content/jQuery.FileUpload/css/jquery.fileupload.css",
                   "~/Content/jQuery.FileUpload/css/jquery.fileupload-ui.css",
                   "~/Content/blueimp-gallery2/css/blueimp-gallery.css",
                     "~/Content/blueimp-gallery2/css/blueimp-gallery-video.css",
                       "~/Content/blueimp-gallery2/css/blueimp-gallery-indicator.css"
                   ));

            bundles.Add(new ScriptBundle("~/bundles/jQuery-File-Upload").Include(
                     //<!-- The Templates plugin is included to render the upload/download listings -->
                     "~/Scripts/jQuery.FileUpload/vendor/jquery.ui.widget.js",
                       "~/Scripts/jQuery.FileUpload/tmpl.min.js",
                    //<!-- The Load Image plugin is included for the preview images and image resizing functionality -->
                    "~/Scripts/jQuery.FileUpload/load-image.all.min.js",
                    //<!-- The Canvas to Blob plugin is included for image resizing functionality -->
                    "~/Scripts/jQuery.FileUpload/canvas-to-blob.min.js",
                    //"~/Scripts/file-upload/jquery.blueimp-gallery.min.js",
                    //<!-- The Iframe Transport is required for browsers without support for XHR file uploads -->
                    "~/Scripts/jQuery.FileUpload/jquery.iframe-transport.js",
                    //<!-- The basic File Upload plugin -->
                    "~/Scripts/jQuery.FileUpload/jquery.fileupload.js",
                    //<!-- The File Upload processing plugin -->
                    "~/Scripts/jQuery.FileUpload/jquery.fileupload-process.js",
                    //<!-- The File Upload image preview & resize plugin -->
                    "~/Scripts/jQuery.FileUpload/jquery.fileupload-image.js",
                    //<!-- The File Upload audio preview plugin -->
                    "~/Scripts/jQuery.FileUpload/jquery.fileupload-audio.js",
                    //<!-- The File Upload video preview plugin -->
                    "~/Scripts/jQuery.FileUpload/jquery.fileupload-video.js",
                    //<!-- The File Upload validation plugin -->
                    "~/Scripts/jQuery.FileUpload/jquery.fileupload-validate.js",
                    //!-- The File Upload user interface plugin -->
                    "~/Scripts/jQuery.FileUpload/jquery.fileupload-ui.js",
                    //Blueimp Gallery 2
                    "~/Scripts/blueimp-gallery2/js/blueimp-gallery.js",
                    "~/Scripts/blueimp-gallery2/js/blueimp-gallery-video.js",
                    "~/Scripts/blueimp-gallery2/js/blueimp-gallery-indicator.js",
                    "~/Scripts/blueimp-gallery2/js/jquery.blueimp-gallery.js"

                    ));

            bundles.Add(new ScriptBundle("~/bundles/Blueimp-Gallerry2").Include(//Blueimp Gallery 2
                    "~/Scripts/blueimp-gallery2/js/blueimp-gallery.js",
                    "~/Scripts/blueimp-gallery2/js/blueimp-gallery-video.js",
                    "~/Scripts/blueimp-gallery2/js/blueimp-gallery-indicator.js",
                    "~/Scripts/blueimp-gallery2/js/jquery.blueimp-gallery.js"));

           
            bundles.Add(new ScriptBundle("~/bundles/eimeceScripts").Include( 
                    "~/Scripts/jquery-3.1.1.min.js",
                    "~/Scripts/eimece.js",
                    "~/Scripts/cookie-consent.js",
                    "~/Scripts/mustache.min.js"));

        }
    }
}