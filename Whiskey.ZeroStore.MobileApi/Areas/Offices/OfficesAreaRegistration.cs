using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Offices
{
    public class OfficesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Offices";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Offices_default2",
                "Api/Offices/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}