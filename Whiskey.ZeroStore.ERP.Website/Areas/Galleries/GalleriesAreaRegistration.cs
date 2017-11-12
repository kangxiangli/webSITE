using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Galleries
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
                "Galleries_default",
                "Galleries/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}