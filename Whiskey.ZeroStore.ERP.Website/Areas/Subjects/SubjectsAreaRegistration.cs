using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Subjects
{
    public class SubjectsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Subjects";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Subjects_default",
                "Subjects/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}