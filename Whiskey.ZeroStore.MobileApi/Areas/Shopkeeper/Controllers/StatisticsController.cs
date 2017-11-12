using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.Shopkeeper.Controllers
{
    //  Api/Shopkeeper/Statistics/
    public class StatisticsController : Controller
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StatisticsController));

        protected readonly IStatisticsContract _statisticsContract;
        protected readonly IAdministratorContract _administratorContract;
        public StatisticsController(
             IStatisticsContract _statisticsContract
            , IAdministratorContract administratorContract
            )
        {
            this._statisticsContract = _statisticsContract;
            this._administratorContract = administratorContract;
        }
        /// <summary>
        /// 获取销售统计信息
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetSaleInfo(int AdminId, int StoreId, int? CategoryId, int FrontDay = 7)
        {
            var data = await Task.Run(() =>
            {
                return _statisticsContract.GetSaleInfo(AdminId, StoreId, CategoryId, FrontDay);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取库存品类信息
        /// </summary>
        /// <param name="StoreId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetCategoryInfo(int AdminId, int StoreId, int? CategoryId)
        {
            var data = await Task.Run(() =>
            {
                return _statisticsContract.GetCategoryInfo(AdminId, StoreId, CategoryId);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}