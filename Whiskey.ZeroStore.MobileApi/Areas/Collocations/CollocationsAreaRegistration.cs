using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Collocations
{
    public class CollocationsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Collocations";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Collocations_Api",
                "Api/Collocations/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}