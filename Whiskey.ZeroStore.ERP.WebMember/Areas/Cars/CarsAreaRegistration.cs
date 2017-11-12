using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Cars
{
    public class CarsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Cars";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Cars_default",
                "Cars/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}