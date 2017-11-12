using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.DataStat
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
                "DataStat",
                "API/DataStat/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}