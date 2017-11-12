using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class PartnerStatisticsController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(PartnerStatisticsController));

        protected readonly IPartnerStatisticsContract _PartnerStatisticsContract;

        public PartnerStatisticsController(
            IPartnerStatisticsContract _PartnerStatisticsContract
            )
        {
            this._PartnerStatisticsContract = _PartnerStatisticsContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }

        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(PartnerStatisticsDto dto)
        {
            var result = _PartnerStatisticsContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(PartnerStatisticsDto dto)
        {
            var result = _PartnerStatisticsContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _PartnerStatisticsContract.Edit(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _PartnerStatisticsContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<PartnerStatistics, bool>> predicate = FilterHelper.GetExpression<PartnerStatistics>(request.FilterGroup);
            var count = 0;

            var list = (from s in _PartnerStatisticsContract.Entities.Where<PartnerStatistics, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.IsDeleted,
                            s.IsEnabled,
							s.CreatedTime,
                            s.MemberCount,
                            s.OrderCount,
                            s.OrderMoney,
                            s.PartnerCount,
                            s.SaleCount,
                            s.SaleMoney,

                        }).ToList();

            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetListStatistics()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<PartnerStatistics, bool>> predicate = FilterHelper.GetExpression<PartnerStatistics>(request.FilterGroup);

            var query = _PartnerStatisticsContract.Entities.Where(predicate);

            var data = (from b in query
                        let _MemberCount = query.Sum(s => s.MemberCount)
                        let _OrderCount = query.Sum(s => s.OrderCount)
                        let _OrderMoney = query.Sum(s => s.OrderMoney)
                        let _PartnerCount = query.Sum(s => s.PartnerCount)
                        let _SaleCount = query.Sum(s => s.SaleCount)
                        let _SaleMoney = query.Sum(s => s.SaleMoney)
                        select new
                        {
                            _MemberCount,
                            _OrderCount,
                            _OrderMoney,
                            _PartnerCount,
                            _SaleCount,
                            _SaleMoney,
                        }).FirstOrDefault();

            if (data == null)
            {
                var dataDefault = new
                {
                    _MemberCount = 0,
                    _OrderCount = 0,
                    _OrderMoney = 0,
                    _PartnerCount = 0,
                    _SaleCount = 0,
                    _SaleMoney = 0
                };
                return Json(dataDefault, JsonRequestBehavior.AllowGet);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _PartnerStatisticsContract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _PartnerStatisticsContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _PartnerStatisticsContract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _PartnerStatisticsContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}

