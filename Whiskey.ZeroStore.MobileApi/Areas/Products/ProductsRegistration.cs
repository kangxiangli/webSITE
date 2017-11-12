using System.Web.Mvc;
namespace Whiskey.ZeroStore.MobileApi.Areas.Products
{
    public class ProductsRegistration : AreaRegistration 
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
            context.MapRoute(
                "Products_API",
                "API/Products/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}