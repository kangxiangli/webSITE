using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Shopkeeper
{
    public class ShopkeeperAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Shopkeeper";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Shopkeeper_Api",
                "Api/Shopkeeper/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}