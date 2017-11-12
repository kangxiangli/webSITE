using System.Web;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.MobileApi
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}