using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Authorities
{
    public class AuthoritiesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Authorities";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Authorities_default",
                "Authorities/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}