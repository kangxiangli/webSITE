using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.PunchClock
{
    public class PunchClockAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PunchClock";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "PunchClock_default",
                "PunchClock/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}