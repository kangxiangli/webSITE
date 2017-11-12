using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Properties
{
    public class PropertiesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Properties";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Properties_Api",
                "Api/Properties/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
