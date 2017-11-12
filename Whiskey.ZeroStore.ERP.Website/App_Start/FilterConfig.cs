using System.Web;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new TimerAttribute());
            //filters.Add(new ExceptionAttribute());
        }
    }
}
