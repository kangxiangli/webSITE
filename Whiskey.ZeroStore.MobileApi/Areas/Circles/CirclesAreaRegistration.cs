using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Circles
{
    public class CirclesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Circles";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Api_Circles",
                "Api/Circles/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}