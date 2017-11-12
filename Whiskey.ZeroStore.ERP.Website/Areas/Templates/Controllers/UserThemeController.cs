using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
    public class UserThemeController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(UserThemeController));
        //protected readonly ITemplateThemeContract _templateThemeContract;
        public UserThemeController(
            //ITemplateThemeContract _templateThemeContract
            )
        {
            //this._templateThemeContract = _templateThemeContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }
    }
}