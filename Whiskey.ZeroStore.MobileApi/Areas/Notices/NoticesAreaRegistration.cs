using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Notices
{
    public class NoticesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Notices";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Notices_Api",
                "Api/Notices/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}