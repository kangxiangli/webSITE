using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.DataStat
{
    public class DataStatAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "DataStat";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "DataStat_default",
                "DataStat/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}