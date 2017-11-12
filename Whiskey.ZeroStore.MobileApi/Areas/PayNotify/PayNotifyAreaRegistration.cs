using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.PayNotify
{
    public class PayNotifyAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PayNotify";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PayNotify_default",
                "PayNotify/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}