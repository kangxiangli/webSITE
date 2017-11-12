using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.DataStat.Controllers
{
    /// <summary>
    /// 数据统计->销售统计
    /// </summary>
    public class SaleController : Controller
    {
        protected readonly IAdministratorContract _adminContract;
        protected readonly IStatisticsContract _statisticsContract;
        public SaleController(IAdministratorContract adminContract,
            IStatisticsContract statisticsContract)
        {
            _adminContract = adminContract;
            _statisticsContract = statisticsContract;
        }
        /// <summary>
        /// 数据统计->销售统计->品类统计
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ActionResult Category(int? storeId, int? adminId)
        {
            if (!storeId.HasValue || !adminId.HasValue)
            {
                return Json(OperationResult.Error("参数错误"), JsonRequestBehavior.AllowGet);
            }

            var res = _statisticsContract.GetCategorySaleStatInfo(adminId.Value, storeId.Value);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
    }
}