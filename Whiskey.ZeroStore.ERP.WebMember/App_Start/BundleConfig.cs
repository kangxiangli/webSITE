using System.Web.Optimization;

namespace Whiskey.ZeroStore.ERP.WebMember
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            BundleTable.EnableOptimizations = false;

            //global CSS
            bundles.Add(new StyleBundle("~/bundles/styles_basic").Include(

                 ));

            //global CSS extention
            bundles.Add(new StyleBundle("~/bundles/styles_ext").Include(
                //"~/content/styles/bootstrap/bootstrap-theme.css",
                "~/Content/Styles/Bootstrap/bootstrap.css",
                "~/content/fonts/fontawesome/css/font-awesome.min.css",
                "~/Content/Styles/Cropper/cropper.min.css"
                 ));

            //Javascript
            bundles.Add(new ScriptBundle("~/bundles/scripts_basic").Include(
                "~/content/scripts/Jquery/jquery-1.11.1.min.js",
                //"~/Content/Scripts/Jquery/SignalR/jquery.signalR-2.2.0.min.js",
                "~/Scripts/zUI.js",
                "~/Content/Scripts/Layer/layer.js",
                "~/content/scripts/layout/fashion.js"
                ));

            //Javascript extention
            bundles.Add(new ScriptBundle("~/bundles/scripts_ext").Include(
                "~/content/scripts/bootstrap/bootstrap.min.js",
                "~/content/scripts/bootstrap/bootbox.js",
				"~/Content/Scripts/Cropper/cropper.min.js",
                "~/content/scripts/layout/globals.js",
                "~/content/scripts/layout/statistics.js"
                //"~/content/scripts/imgtooltip/imgtooltip-min.js"
                ));

            //Validate
            bundles.Add(new ScriptBundle("~/bundles/validates").Include(
                 "~/content/scripts/jquery/validate/jquery.validate.js",
                 "~/content/scripts/jquery/validate/jquery.validate.unobtrusive.js",
                 "~/content/scripts/jquery/validate/jquery.unobtrusive-ajax.js"
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
