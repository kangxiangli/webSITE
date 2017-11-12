using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Models.Entities;
using System.Threading.Tasks;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{
    [License(CheckMode.Check)]
    public class HomeController : Controller
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(HomeController));

        protected readonly IStatisticsContract _statisticsContract;
        protected readonly IAdministratorContract _adminContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IRetailContract _retailContract;

        public HomeController(
             IStatisticsContract _statisticsContract
            , IAdministratorContract _adminContract
            , IStoreContract _storeContract
            , IRetailContract _retailContract
            )
        {
            this._statisticsContract = _statisticsContract;
            this._adminContract = _adminContract;
            this._storeContract = _storeContract;
            this._retailContract = _retailContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FrontDay"></param>
        /// <param name="TopCount"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetSomeCount(int? FrontDay, int TopCount = 6)
        {
            var data = await Task.Run(() =>
            {
                return _statisticsContract.GetSomeCount(FrontDay, TopCount);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 用户掌管的店铺列表，空时 返回 0-FASHION
        /// </summary>
        /// <param name="HitName"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetManagedStores(string HitName = "0-FASHION")
        {
            var adminId = AuthorityHelper.OperatorId.Value;//线程中拿不到OperatorId
            var data = await Task.Run(() =>
            {
                return _statisticsContract.GetManagedStores(adminId, HitName);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取销售统计信息
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetSaleInfo(int StoreId, int? CategoryId, int FrontDay = 7)
        {
            var adminId = AuthorityHelper.OperatorId.Value;//线程中拿不到OperatorId
            var data = await Task.Run(() =>
            {
                return _statisticsContract.GetSaleInfo(adminId, StoreId, CategoryId, FrontDay);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取库存品类信息
        /// </summary>
        /// <param name="StoreId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetCategoryInfo(int StoreId, int? CategoryId)
        {
            var adminId = AuthorityHelper.OperatorId.Value;//线程中拿不到OperatorId
            var data = await Task.Run(() =>
            {
                return _statisticsContract.GetCategoryInfo(adminId, StoreId, CategoryId);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetStorageInfo(int StoreId, int FrontDay = 7)
        {
            var adminId = AuthorityHelper.OperatorId.Value;//线程中拿不到OperatorId
            var data = await Task.Run(() =>
            {
                return _statisticsContract.GetStorageInfo(adminId, StoreId, FrontDay);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetStoreLocationInfo(int? StoreId, int TopCount = 3)
        {
            var data = await Task.Run(() =>
            {
                return _statisticsContract.GetStoreLocationInfo(StoreId, TopCount);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCategorySaleStatInfo(int? storeId)
        {
            if (!storeId.HasValue)
            {
                return Json(OperationResult.OK(), JsonRequestBehavior.AllowGet);
            }
            var adminId = AuthorityHelper.OperatorId.Value;//线程中拿不到OperatorId
            var res = _statisticsContract.GetCategorySaleStatInfo(adminId, storeId.Value);
            return Json(res, JsonRequestBehavior.AllowGet);

        }


    }
}