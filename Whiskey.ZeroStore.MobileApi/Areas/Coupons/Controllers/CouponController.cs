using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Controllers;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.MobileApi.Areas.Coupons.Controllers
{
    [License(CheckMode.Verify)]
    public class CouponController : BaseController
    {
        #region 初始化数据层操作对象
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(CouponController));
        protected readonly ICouponContract _couponContract;

        public CouponController(
             ICouponContract couponContract)
        {
            _couponContract = couponContract;
        }
        #endregion

        #region 领取优惠券

        public JsonResult Get(int MemberId, string CouponNum)
        {
            var data = OperationHelper.Try(() =>
               {
                   var res = new OperationResult(OperationResultType.Error);
                   if (CouponNum.IsNullOrEmpty())
                   {
                       return OperationHelper.ReturnOperationResultDIY(OperationResultType.Error, "优惠券无效");
                   }
                   return _couponContract.Get(CouponNum, MemberId);
               }, "获取优惠券");

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 使用优惠券

        /// <summary>
        /// 使用优惠券
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        public JsonResult Use(string Number, int MemberId)
        {
            var data = OperationHelper.Try(() =>
            {
                if (Number.IsNullOrEmpty())
                {
                    return OperationHelper.ReturnOperationResultDIY(OperationResultType.Error, "优惠券无效");
                }
                return _couponContract.Use(Number, MemberId);
            }, "使用优惠券");

            return Json(data);
        }
        #endregion

        #region 获取优惠券推荐列表

        public JsonResult GetList(int PageIndex = 1, int PageSize = 10)
        {
            var rdata = OperationHelper.Try(() =>
            {
                var query = _couponContract.Coupons.Where(w => w.IsEnabled && !w.IsDeleted && w.IsRecommend).OrderByDescending(o => o.UpdatedTime);

                var totalCount = 0;
                var totalPage = 0;

                var list = (from s in query.Where<Coupon, int>(PageIndex, PageSize, out totalCount, out totalPage)
                            select new
                            {
                                CouponImagePath = s.CouponImagePath != null ? WebUrl + s.CouponImagePath : s.CouponImagePath,
                                s.CouponName,
                                s.CouponNum,
                                s.StartDate,
                                s.EndDate

                            }).ToList();

                var data = new
                {
                    totalCount = totalCount,
                    totalPage = totalPage,
                    List = list,
                };

                return OperationHelper.ReturnOperationResultDIY(OperationResultType.Success, "", data);

            }, "获取优惠券列表");

            return Json(rdata, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}