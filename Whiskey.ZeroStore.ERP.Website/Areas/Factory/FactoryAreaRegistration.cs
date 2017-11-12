using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Factory
{
    public class FactoryAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Factory";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            var route = context.MapRoute(
                "Factory_default",
                "Factory/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
            route.DataTokens["area"] = "Factory";
        }
    }
}