using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Members
{
    public class MembersAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Members";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Members_default",
                "Members/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}