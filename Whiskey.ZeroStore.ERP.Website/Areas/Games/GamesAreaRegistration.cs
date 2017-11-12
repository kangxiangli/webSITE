using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Games
{
    public class GamesAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Games";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Games_default",
                "Games/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}