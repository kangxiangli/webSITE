using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.Games.Controllers
{
    // GET: Games/Game
    [License(CheckMode.Verify)]
    public class CouponActivityController : Controller
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(GameController));

        protected readonly ICouponActivityContract _activityContract;

        public CouponActivityController(
            ICouponActivityContract gameContract
            )
        {
            this._activityContract = gameContract;
        }

        // 获取制定活动下的优惠券
        [HttpPost]
        public ActionResult GetAllLBSCoupons(string couponActivityGUID)
        {
            try
            {
                if (string.IsNullOrEmpty(couponActivityGUID))
                {
                    return Json(OperationResult.Error("参数错误"));
                }
                var activityEntity = _activityContract.Entities.Include(a => a.Coupons).FirstOrDefault(a => !a.IsDeleted && a.IsEnabled && a.ActivityGUID == couponActivityGUID);
                if (DateTime.Now < activityEntity.ActivityStartDate.Date)
                {
                    return Json(OperationResult.Error("活动尚未开始"));
                }

                if (DateTime.Now > activityEntity.ActivityEndDate.Date)
                {
                    return Json(OperationResult.Error("活动已结束"));
                }

                var lbsCoupons = activityEntity.Coupons.Where(c => !c.IsDeleted && c.IsEnabled && !c.IsUsed && !c.MemberId.HasValue).Select(c => new
                {
                    c.CouponNumber,
                    CouponType = c.CouponType,
                    c.Name,
                    c.Amount,
                    c.Longtitude,
                    c.Latitude,
                }).ToList();


                return Json(new OperationResult(OperationResultType.Success, string.Empty, lbsCoupons));
            }
            catch (Exception e)
            {
                _Logger.Error(e.Message + e.StackTrace);
                return Json(OperationResult.Error("系统繁忙,请稍后再试"));
            }



        }


        /// <summary>
        /// 获取指定会员的优惠券
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetMyLBSCoupons(int memberId)
        {
            try
            {
                var coupons = _activityContract.LBSCouponEntities.Where(c => !c.IsDeleted && c.IsEnabled && c.MemberId == memberId)
                .Select(c => new
                {
                    c.MemberId,
                    c.Name,
                    c.CouponNumber,
                    c.Amount,
                    c.Longtitude,
                    c.Latitude,
                    c.CouponType,
                    c.CreatedTime,
                    c.IsUsed,
                    c.UsedTime,
                    CouponStartDate = c.CouponActivity.CouponStartDate,
                    CouponEndDate = c.CouponActivity.CouponEndDate
                }).ToList()
                .Select(c => new
                {
                    c.MemberId,
                    c.Name,
                    c.CouponNumber,
                    c.Amount,
                    c.Longtitude,
                    c.Latitude,
                    c.CouponType,
                    c.IsUsed,
                    c.UsedTime,
                    CreatedTime = c.CreatedTime.ToUnixTime(),
                    CouponStartDate = c.CouponStartDate.ToUnixTime(),
                    CouponEndDate = c.CouponEndDate.ToUnixTime()
                });

                return Json(new OperationResult(OperationResultType.Success, string.Empty, coupons));
            }
            catch (Exception e)
            {

                _Logger.Error(e.Message + e.StackTrace);
                return Json(OperationResult.Error("系统繁忙,请稍后再试"));
            }

        }



        /// <summary>
        /// 优惠券领取接口
        /// </summary>
        /// <param name="couponNumber"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult BindLBSCoupon(string couponNumber, int memberId)
        {
            try
            {
                var key = "bindcoupon:" + memberId;
                if (!RedisCacheHelper.SetNX(key, memberId.ToString(), TimeSpan.FromSeconds(2)))
                {
                    return Json(OperationResult.Error("请求次数过于频繁"));
                }

                var coupon = _activityContract.LBSCouponEntities.FirstOrDefault(c => !c.IsDeleted && c.IsEnabled && c.CouponNumber == couponNumber);
                if (coupon == null)
                {
                    return Json(OperationResult.Error("优惠券不存在"));
                }
                if (coupon.IsUsed || coupon.UsedTime.HasValue)
                {
                    return Json(OperationResult.Error("无效的优惠券"));
                }
                if (coupon.MemberId.HasValue)
                {
                    if (coupon.MemberId == memberId)
                    {
                        return Json(OperationResult.Error("不可重复领取"));
                    }
                    else
                    {
                        return Json(OperationResult.Error("来晚了,已被别人抢先一步"));
                    }
                }

                // 校验会员优惠券数量
                if (_activityContract.LBSCouponEntities.Count(c => !c.IsDeleted && c.IsEnabled && c.MemberId == memberId) >= 3)
                {
                    return Json(OperationResult.Error("领取次数已超过最大次数"));
                }

                coupon.MemberId = memberId;

                var res = _activityContract.UpdateCoupon(coupon);
                if (res.ResultType != OperationResultType.Success)
                {
                    return Json(OperationResult.Error("领取失败,请稍后再试"));

                }
                return Json(OperationResult.OK("领取成功"));
            }
            catch (Exception e)
            {
                _Logger.Error(e.Message + e.StackTrace);
                return Json(OperationResult.OK("领取失败,请稍后再试"));
            }




        }



    }
}