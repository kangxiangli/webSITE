using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Core.Data.Extensions;
using System.Linq.Expressions;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Articles;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Article;
using Whiskey.Web.Helper;
using Whiskey.Utility.Helper;
using System.Text;
using System.Drawing.Imaging;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Coupon;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class MyWalletController : Controller
    {

        #region 初始化操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MyWalletController));
        //声明业务层操作对象
        protected readonly ICouponContract _couponContract;

        protected readonly IMemberContract _memberContract;

        protected readonly IMemberDepositContract _memberDepositContract;

        public MyWalletController(ICouponContract couponContract,
            IMemberContract memberContract,
            IMemberDepositContract memberDepositContract)
        {
            _couponContract = couponContract;
            _memberContract = memberContract;
            _memberDepositContract = memberDepositContract;
        }
        #endregion

        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        #region 获取我的钱包
        /// <summary>
        /// 获取我的钱包
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get(int MemberId, int CouponType, int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                Member member = _memberContract.View(MemberId);
                IQueryable<CouponItem> listCouponItem = _couponContract.CouponItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == MemberId && !x.Coupon.IsDeleted && x.Coupon.IsEnabled);
                DateTime nowTime = DateTime.Now;
                if (CouponType == (int)CouponFlag.Used)
                {
                    listCouponItem = listCouponItem.Where(x => x.IsUsed == true);
                }
                else if (CouponType == (int)CouponFlag.Unused)
                {
                    listCouponItem = listCouponItem.Where(x => x.IsUsed == false && (nowTime.CompareTo(x.Coupon.StartDate) >= 0 && nowTime.CompareTo(x.Coupon.EndDate) <= 0 || x.Coupon.IsForever == true));
                }
                else if (CouponType == (int)CouponFlag.Expired)
                {
                    listCouponItem = listCouponItem.Where(x => x.Coupon.IsForever == false && x.IsUsed == false && nowTime.CompareTo(x.Coupon.EndDate) > 0);
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "异常操作"));
                }
                var allcount = listCouponItem.Count();
                listCouponItem = listCouponItem.OrderByDescending(x => x.CreatedTime).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                var entities = listCouponItem.Select(x => new
                {
                    CouponImagePath = strWebUrl + x.Coupon.CouponImagePath,
                    x.Coupon.CouponName,
                    CouponItemId = x.Id,
                    x.CouponNumber,
                });
                var data = new
                {
                    member.CardNumber,
                    ListCouPon = entities,
                };
                return Json(new PagedOperationResult(OperationResultType.Success, "获取成功", data) { AllCount = allcount, PageSize = PageSize });
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }

        }
        #endregion

        #region 获取储值列表
        /// <summary>
        /// 获取储值列表
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="DepositType"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public JsonResult GetDeposit(int MemberId, int DepositType, int PageIndex = 1, int PageSize = 10)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success);
            IQueryable<MemberDeposit> listMemberDeposit = _memberDepositContract.MemberDeposits.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == MemberId);
            List<MemberDeposit> memberDeposits = listMemberDeposit.OrderBy(x => x.CreatedTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();
            if (DepositType == (int)MemberActivityFlag.Recharge)
            {
                var data = memberDeposits.Where(x => x.MemberActivityType == MemberActivityFlag.Recharge).Select(x => new
                {
                    x.Id,
                    x.Card,
                    x.Cash,
                    x.Coupon,
                    CreatedTime = x.CreatedTime.ToString("yyyy/MM/dd"),
                }).ToList();
                oper.Data = data;
            }
            else if (DepositType == (int)MemberActivityFlag.Score)
            {
                var data = memberDeposits.Where(x => x.MemberActivityType == MemberActivityFlag.Score).Select(x => new
                {
                    x.Id,
                    x.Score,
                    x.Coupon,
                    CreatedTime = x.CreatedTime.ToString("yyyy/MM/dd"),
                }).ToList();
                oper.Data = data;
            }
            else
            {
                oper.ResultType = OperationResultType.Error;
                oper.Message = "操作异常";
                oper.Data = null;
            }
            return Json(oper);
        }
        #endregion


        /// <summary>
        /// 获取会员储值记录
        /// </summary>
        /// <param name="MemberId">会员id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetDepositInfo(int MemberId)
        {
            try
            {
                var list = _memberDepositContract.MemberDeposits.Where(d => d.IsDeleted == false && d.IsEnabled == true)
                 .Where(m => m.MemberId == MemberId && m.MemberActivityType == (int)MemberActivityFlag.Recharge)
                .OrderByDescending(d => d.CreatedTime)
                .Select(m => new
                {
                    m.Price,
                    m.Cash,
                    m.Card,
                    m.Coupon,
                    m.AfterBalance,
                    m.CreatedTime
                }).ToList();

                var data = list.Select(m => new
                {
                    msg = new
                    {
                        m.Price,
                        m.Cash,
                        m.Card,
                        m.Coupon,
                        m.AfterBalance,
                    },
                    CreatedTime = m.CreatedTime.ToUnixTime()
                }).ToList();

                return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
            }
            catch (Exception ex)
            {

                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }

        }

        [HttpPost]
        public ActionResult GetScoreInfo(int MemberId)
        {
            try
            {
                var list = _memberDepositContract.MemberDeposits.Where(d => d.IsDeleted == false && d.IsEnabled == true)
                 .Where(m => m.MemberId == MemberId && m.MemberActivityType == MemberActivityFlag.Score)
                .OrderByDescending(d => d.CreatedTime)
                .Select(m => new
                {
                    m.Score,
                    m.AfterScore,
                    m.CreatedTime,
                    m.Coupon
                })
                .ToList();
                var data = list.Select(m => new
                {
                    msg = new
                    {
                        m.Score,
                        m.AfterScore,
                    },
                    CreatedTime = m.CreatedTime.ToUnixTime()
                }).ToList();

                return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
            }
            catch (Exception ex)
            {

                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }
    }
}