using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Recharge
{
    public class RechargeAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Recharge";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Recharge_Api",
                "Api/Recharge/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}