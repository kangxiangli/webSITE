using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Galleries
{
    public class GalleriesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Galleries";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Galleries_Api",
                "Api/Galleries/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}