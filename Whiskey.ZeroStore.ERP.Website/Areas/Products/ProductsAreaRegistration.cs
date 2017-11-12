using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Products
{
    public class ProductsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Products";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
           var route= context.MapRoute(
                "Products_default",
                "Products/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
           route.DataTokens["area"] = "Products";
        }
    }
}