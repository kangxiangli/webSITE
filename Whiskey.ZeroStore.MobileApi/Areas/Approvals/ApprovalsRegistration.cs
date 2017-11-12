using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Approvals
{
    public class ApprovalsRegistration : AreaRegistration
    {
        public override string AreaName 
        {
            get
            {
                return "Approvals";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Approvals_API",
                "API/Approvals/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}