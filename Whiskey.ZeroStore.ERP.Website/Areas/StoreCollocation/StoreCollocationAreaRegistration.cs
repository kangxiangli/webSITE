using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.StoreCollocation
{
    public class StoreCollocationAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "StoreCollocation";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "StoreCollocation_default",
                "StoreCollocation/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}