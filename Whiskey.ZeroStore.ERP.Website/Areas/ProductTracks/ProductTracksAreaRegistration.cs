using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.ProductTracks
{
    public class ProductTracksAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ProductTracks";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ProductTracks_default",
                "ProductTracks/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}