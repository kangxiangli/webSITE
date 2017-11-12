using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses
{
    public class WarehousesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Warehouses";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
        //    context.MapRoute(
        //        "Warehouses_default",
        //        "Warehouses/{controller}/{action}/{id}",
        //        new { action = "Index", id = UrlParameter.Optional }
        //    );
            context.MapRoute(
               "Warehouses_default",
               "Warehouses/{controller}/{action}/{id}",
               new { action = "Index", id = UrlParameter.Optional }
           );
        }
    }
}