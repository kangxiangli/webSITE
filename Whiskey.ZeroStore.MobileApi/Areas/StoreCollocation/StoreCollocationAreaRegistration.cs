using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.StoreCollocation
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
                "StoreCollocation_Api",
                "Api/StoreCollocation/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}