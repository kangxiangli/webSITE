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

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Orders.Controllers
{
    [License(CheckMode.Verify)]
    public class OrderController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(OrderController));
        protected readonly IMemberContract _memberContract;

        public OrderController(
            IMemberContract _memberContract
            )
        {
            this._memberContract = _memberContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return NoOpenView;
        }

        [Layout]
        public ActionResult PreOrder()
        {
            return NoOpenView;
        }

        [Layout]
        public ActionResult Payment()
        {
            return NoOpenView;
        }

        [Layout]
        public ActionResult Paymented()
        {
            return NoOpenView;
        }
    }
}