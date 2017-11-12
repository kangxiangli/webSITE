using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute;
using Whiskey.Web.Helper;
using Whiskey.Utility.Class;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Cars.Controllers
{
    [License(CheckMode.Verify)]
    public class CarController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CarController));
        protected readonly IMemberContract _memberContract;

        public CarController(
            IMemberContract _memberContract
            )
        {
            this._memberContract = _memberContract;
        }

        [Layout]
        [NavInfo]
        public ActionResult Index()
        {
            return View();
        }

    }
}