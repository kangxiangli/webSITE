using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.WebMember.Controllers
{
    public class HomeController : Controller
    {
        //protected readonly IProductOrigNumberContract _productOrigNumberContract;
        public HomeController(
            //IProductOrigNumberContract _productOrigNumberContract
            )
        {
            //this._productOrigNumberContract = _productOrigNumberContract;
        }

        [_Theme]
        [_AuthInfo]
        public ActionResult Index()
        {
            return View();
        }
        [Layout]
        public ActionResult ShopList()
        {
            return View();
        }

        [Layout]
        public ActionResult Collocation()
        {
            return View();
        }
        [Layout]
        public ActionResult Online()
        {
            return View();
        }
        [Layout]
		public ActionResult NotOpen()
        {
            return View();
        }
        public JsonResult Error()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "操作异常");
            return Json(oper, JsonRequestBehavior.AllowGet);
        }

    }
}