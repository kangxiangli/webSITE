using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Coupons
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
                "Coupons_default",
                "Coupons/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}