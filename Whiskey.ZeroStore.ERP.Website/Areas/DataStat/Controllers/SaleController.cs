using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Areas.DataStat.Controllers
{
    public class SaleController : Controller
    {
        protected readonly IStatisticsContract _statisticsContract;
        protected readonly IAdministratorContract _adminContract;


        public SaleController(IStatisticsContract statisticsContract,
            IAdministratorContract adminContract)
        {
            _statisticsContract = statisticsContract;
            _adminContract = adminContract;
        }
        // GET: DataStat/Sale
        [Layout]
        public ActionResult Index()
        {
            return View();


        }

        public ActionResult QueryOptions()
        {
            var res = _statisticsContract.QueryOptions();
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Query(SaleStatReq req)
        {
            var adminId = AuthorityHelper.OperatorId;
            if (!adminId.HasValue)
            {
                return Json(OperationResult.Error("参数错误"), JsonRequestBehavior.AllowGet);
            }
            req.AdminId = adminId;
            var res = _statisticsContract.SaleStat(req);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
    }
}