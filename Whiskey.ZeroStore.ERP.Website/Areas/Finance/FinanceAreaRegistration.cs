using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Finance
{
    public class FinanceAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Finance";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            var route = context.MapRoute(
                 "Finance_default",
                 "Finance/{controller}/{action}/{id}",
                 new { action = "Index", id = UrlParameter.Optional }
             );
            route.DataTokens["area"] = "Finance";
        }
    }
}