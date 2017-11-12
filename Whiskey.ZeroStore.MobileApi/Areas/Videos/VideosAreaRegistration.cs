using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Videos
{
    public class VideosAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Videos";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Videos_default",
                "Api/Videos/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}