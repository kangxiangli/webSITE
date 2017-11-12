using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Offices
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
                "Offices_default",
                "Offices/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}