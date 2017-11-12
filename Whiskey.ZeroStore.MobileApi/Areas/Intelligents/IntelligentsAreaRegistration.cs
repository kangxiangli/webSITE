using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Intelligents
{
    public class IntelligentsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Intelligents";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Intelligents_Api",
                "Api/Intelligents/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}