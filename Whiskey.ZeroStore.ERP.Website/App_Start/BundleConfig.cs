using System.Web;
using System.Web.Optimization;

namespace Whiskey.ZeroStore.ERP.Website
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {

            BundleTable.EnableOptimizations = false;

            //global CSS
            bundles.Add(new StyleBundle("~/bundles/global_styles").Include(
                "~/content/styles/bootstrap/bootstrap-theme.css",
                "~/content/fonts/fontawesome/css/font-awesome.min.css",
                "~/Content/Styles/Bootstrap/BootstrapSelect/bootstrap-select-1.12.2.min.css",
                "~/Content/Styles/Layout/notification.css"
                 ));
            //default Css//默认主题的css，其它主题时 不用
            bundles.Add(new StyleBundle("~/bundles/default_styles").Include(
                "~/content/styles/bootstrap/bootstrap.css",
                "~/content/styles/layout/globals.css",
                "~/content/styles/layout/themes.css",
                "~/content/styles/layout/utility.css"
                 ));
            //Javascript
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                "~/content/scripts/Jquery/jquery-1.11.1.min.js",
                "~/content/scripts/bootstrap/bootstrap.js",
                "~/content/scripts/bootstrap/bootbox.js",
                "~/content/scripts/layout/whiskey.js",
                "~/content/scripts/layout/globals.js",
                "~/Content/Scripts/lodash/lodash.min.js",
                "~/content/scripts/layout/statistics.js",
                "~/content/scripts/imgtooltip/imgtooltip-min.js",
                "~/Content/Scripts/Jquery/SignalR/jquery.signalR-2.2.0.min.js",
                "~/Content/Scripts/Bootstrap/BootstrapSelect/bootstrap-select-1.12.2.min.js",
                "~/wwwroot/built/StoreSelect.js",
                "~/wwwroot/lib/vue-2.3.3.js",
                "~/wwwroot/lib/vue-filter-date.js",
                "~/wwwroot/src/components/switcher.js"
                ));


            //Validate
            bundles.Add(new ScriptBundle("~/bundles/validates").Include(
                "~/content/scripts/jquery/validate/jquery.validate.js",
                "~/content/scripts/jquery/validate/jquery.validate.unobtrusive.js",
                "~/content/scripts/jquery/validate/jquery.unobtrusive-ajax.js"
                ));


            //Waterfall
            bundles.Add(new ScriptBundle("~/bundles/waterfall").Include(
                "~/content/Scripts/jquery/jquery.waterfall.js",
                "~/content/Scripts/jquery/jquery.sliphover.js",
                "~/content/Scripts/Handlebars/handlebars-v3.0.0.js"
                ));

            //tags
            bundles.Add(new ScriptBundle("~/bundles/tags").Include(
                "~/content/scripts/bootstrap/bootstrap-tagsinput.js"
                ));


            //uploads
            bundles.Add(new StyleBundle("~/bundles/upload/styles").Include(
                "~/content/styles/jquery/ui/jquery.fileupload.css"
                ));
            bundles.Add(new ScriptBundle("~/bundles/upload/scripts").Include(
                "~/content/scripts/jquery/fileupload/jquery.ui.widget.js",
                "~/content/scripts/jquery/fileupload/jquery.iframe-transport.js",
                "~/content/scripts/jquery/fileupload/jquery.fileupload.js"
                ));
           

        }
    }
}
