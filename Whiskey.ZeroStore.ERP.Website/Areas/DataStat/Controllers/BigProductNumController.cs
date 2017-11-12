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
    public class BigProductNumController : Controller
    {
        protected readonly IStatisticsContract _statisticsContract;
        protected readonly IAdministratorContract _adminContract;


        public BigProductNumController(IStatisticsContract statisticsContract,
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
        /// <summary>
        /// 获取款号下可供查询的颜色和尺码
        /// </summary>
        public ActionResult QueryOptions(int? storeId, string bigProductNum)
        {
            var res = _statisticsContract.QueryOptions(storeId, bigProductNum);
            return Json(res, JsonRequestBehavior.AllowGet);
        }



        public ActionResult Query(BigProductNumStatReq req)
        {
            var adminId = AuthorityHelper.OperatorId;
            if (!adminId.HasValue)
            {
                return Json(OperationResult.Error("参数错误"), JsonRequestBehavior.AllowGet);
            }
            req.AdminId = adminId;
            var res = _statisticsContract.BigProductNumStat(req);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
    }
}