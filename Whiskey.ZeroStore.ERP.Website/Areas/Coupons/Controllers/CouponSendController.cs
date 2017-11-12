using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Coupons.Controllers
{
    /// <summary>
    /// 优惠卷发送记录
    /// </summary>
    [License(CheckMode.Verify)]
    public class CouponSendController : BaseController
    {
        #region 初始化业务层操作对象
                
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CouponController));

        protected readonly ICouponContract _couponContract;        

        protected readonly IMemberContract _memberContract;

        public CouponSendController(ICouponContract couponContract,
            IMemberContract memberContract)
        {
            _couponContract = couponContract;
            _memberContract = memberContract;
		}
        #endregion

        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取优惠卷发送记录
        /// <summary>
        /// 获取优惠卷发送记录
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            var rule = request.FilterGroup.Rules.Where(x => x.Field == "MemberName").FirstOrDefault();
            string strMemberName = string.Empty;
            if (rule!=null)
            {
                strMemberName = rule.Value.ToString();
            }
            Expression<Func<CouponItem, bool>> predicate = FilterHelper.GetExpression<CouponItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<CouponItem> listCouponItem = _couponContract.CouponItems;
                if (string.IsNullOrEmpty(strMemberName))
                {
                    listCouponItem = listCouponItem.Where(x => x.Member.MemberName.Contains(strMemberName));
                }
                var list = listCouponItem.Where<CouponItem, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.Member.MemberName,
                    m.Coupon.CouponName,
                    m.Coupon.CouponPrice,                    
                    m.Coupon.StartDate,
                    m.Coupon.EndDate,
                    m.IsUsed,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 优惠卷发送
        /// <summary>
        /// 初始化发送界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Send()
        {
            return View();
        }

        [HttpPost]
        public JsonResult SendCoupon()
        {
            OperationResult oper= new OperationResult(OperationResultType.Success,"添加成功");
            try
            {
                string strCouponId = Request["CouponId"];
                string strMemberIds = Request["MemberIds"];
                if (string.IsNullOrEmpty(strCouponId))
                {
                    oper.ResultType=OperationResultType.Error;
                    oper.Message="请选择要发送的优惠卷";
                    return Json(oper);
                }
                if (string.IsNullOrEmpty(strMemberIds))
                {
                    oper.ResultType=OperationResultType.Error;
                    oper.Message="请选择接受的会员";
                    return Json(oper);
                }
                int couponId = int.Parse(strCouponId);
                string[] arrMemberId = strMemberIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                List<int> listMemberId = new List<int>();
                foreach (string strMemberId in arrMemberId)
                {
                    int memberId = int.Parse(strMemberId);
                    if (listMemberId.Contains(memberId))
                    {
                        oper.ResultType = OperationResultType.Error;
                        oper.Message = "发送会员出现重复";
                        return Json(oper);                 
                    }
                    else
                    {
                        listMemberId.Add(memberId);
                    }
                }
               oper= _couponContract.Send(couponId,listMemberId);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.ResultType=OperationResultType.Error;
                oper.Message="数据异常，请稍后访问";
            }
            return Json(oper);
        }
        #endregion

        #region 获取优惠券
        /// <summary>
        /// 初始化优惠卷界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Coupon()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取优惠卷
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> CouponList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Coupon, bool>> predicate = FilterHelper.GetExpression<Coupon>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _couponContract.Coupons.Where<Coupon, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.CouponName,
                    m.CouponPrice,
                    m.Quantity,
                    SurplusQuantity = m.CouponItems.Where(x => x.MemberId == null && x.IsUsed==false).Count(),
                    m.StartDate,
                    m.EndDate,
                    m.Operator.Member.MemberName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取会员

        /// <summary>
        /// 初始化会员界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Member()
        {
            string strCouponId = Request["CouponId"];
            ViewBag.CouponId = strCouponId;
            return PartialView();
        }

        /// <summary>
        /// 获取会员数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> MemberList()
        {
            GridRequest request = new GridRequest(Request);
            var rule = request.FilterGroup.Rules.Where(x => x.Field == "CouponId").FirstOrDefault();
            int couponId = 0;
            if (rule!=null)
            {
                couponId = int.Parse(rule.Value.ToString());
                request.FilterGroup.Rules.Remove(rule);
            }
            Expression<Func<Member, bool>> predicate = FilterHelper.GetExpression<Member>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Coupon coupon =  _couponContract.Coupons.Where(x => x.Id == couponId).FirstOrDefault();
                List<int> listMemberId= new List<int>();
                if (coupon !=null)
                {
                    listMemberId = coupon.CouponItems.Where(x => x.MemberId != null).Select(x => x.MemberId??0).ToList();
                }
                var count = 0;
                var list = _memberContract.Members.Where<Member, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.MemberName,
                    m.MobilePhone,
                    IsSend=listMemberId.Contains(m.Id),
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}