using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Comments
{
    public class CommentsAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Comments";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Comments_Api",
                "Api/Comments/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}