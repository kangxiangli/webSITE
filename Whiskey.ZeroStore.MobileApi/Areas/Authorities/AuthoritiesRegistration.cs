using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi.Areas.Authorities
{
    public class AuthoritiesAPIRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Authorities";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Authorities_Api",
                "Api/Authorities/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }

}