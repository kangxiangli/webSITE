using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Coupons
{
    public class CouponsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Coupons";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Coupons_Api",
                "Api/Coupons/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}