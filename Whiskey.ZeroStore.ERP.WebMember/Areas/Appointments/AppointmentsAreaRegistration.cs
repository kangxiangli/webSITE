using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Appointments
{
    public class AppointmentsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Appointments";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Appointments_default",
                "Appointments/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}