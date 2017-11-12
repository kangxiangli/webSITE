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
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Coupons.Controllers
{
    /// <summary>
    /// 优惠卷
    /// </summary>
    [License(CheckMode.Verify)]
    public class CouponActivityController : Controller
    {
        #region 初始化业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CouponController));

        protected readonly ICouponActivityContract _couponContract;

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IPartnerContract _partnerContract;

        public CouponActivityController(ICouponActivityContract couponContract,
            IAdministratorContract administratorContract,
            IPartnerContract partnerContract)
        {
            _couponContract = couponContract;
            _partnerContract = partnerContract;
            _administratorContract = administratorContract;
        }
        #endregion



        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create([Bind(Include = "ActivityName,ActivityStartDate,ActivityEndDate,CouponStartDate,CouponEndDate,Notes,CouponType")]CouponActivity entity)
        {
            if (!ModelState.IsValid)
            {
                return View(entity);
            }
            entity.ActivityGUID = Guid.NewGuid().ToString("N");
            var result = _couponContract.Insert(entity);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create()
        {
            return PartialView();
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update([Bind(Include = "Id,ActivityName,ActivityStartDate,ActivityEndDate,CouponStartDate,CouponEndDate,Notes")]CouponActivity dto)
        {
            var entity = _couponContract.Entities.FirstOrDefault(e => e.Id == dto.Id);
            entity.ActivityName = dto.ActivityName;
            entity.ActivityStartDate = dto.ActivityStartDate;
            entity.ActivityEndDate = dto.ActivityEndDate;
            entity.CouponStartDate = dto.CouponStartDate;
            entity.CouponEndDate = dto.CouponEndDate;
            entity.Notes = dto.Notes;
            var result = _couponContract.Update(new CouponActivity[] { entity });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _couponContract.View(Id);
            return PartialView(result);
        }
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var result = _couponContract.View(Id);
            return PartialView(result);
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(string ActivityName, CouponActivityTypeEnum? CouponType, DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var query = _couponContract.Entities;
            query = query.Where(e => e.IsEnabled == isEnabled);
            if (!string.IsNullOrEmpty(ActivityName))
            {
                query = query.Where(e => e.ActivityName.StartsWith(ActivityName));
            }
            if (CouponType.HasValue)
            {
                query = query.Where(e => e.CouponType == CouponType.Value);

            }
            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                e.ActivityGUID,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.ActivityName,
                                e.CreatedTime,
                                e.ActivityStartDate,
                                e.ActivityEndDate,
                                e.CouponStartDate,
                                e.CouponEndDate,
                                MemberName = e.Operator.Member.MemberName,
                                CouponType = e.CouponType.ToString(),
                                IsChecked = false
                            }).ToList();
            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        public class PageDto
        {
            public int pageIndex { get; set; }
            public int pageSize { get; set; }
            public int totalCount { get; set; }
            public int pageCount
            {
                get
                {
                    return (int)Math.Ceiling(totalCount * 1.0 / pageSize);
                }
            }
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _couponContract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _couponContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _couponContract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _couponContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult SetupCoupon(int id)
        {
            var entity = _couponContract.View(id);
            return PartialView(entity);
        }


        public ActionResult ViewCoupon(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }


        public ActionResult CouponList(string couponNumber, DateTime? startDate, DateTime? endDate, int activityId, int pageIndex = 1, int pageSize = 10)
        {

            var query = _couponContract.LBSCouponEntities;
            query = query.Where(e => e.CouponActivityId == activityId);
            if (!string.IsNullOrEmpty(couponNumber))
            {
                query = query.Where(c => c.CouponNumber.StartsWith(couponNumber));
            }
            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                e.Name,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.CouponNumber,
                                ActivityName = e.CouponActivity.ActivityName,
                                e.CreatedTime,
                                e.Amount,
                                e.MemberId,
                                MemberName = e.Member.MemberName, //领取会员
                                e.IsUsed,
                                e.CouponActivity.CouponStartDate,
                                e.CouponActivity.CouponEndDate,
                                OptName = e.Operator.Member.MemberName, //操作人
                                CouponType = e.CouponType.ToString(),
                                IsChecked = false
                            }).ToList();
            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveCoupon(int activityId, params LBSCouponEntity[] coupons)
        {
            var activityEntity = _couponContract.View(activityId);
            if (activityEntity == null)
            {
                return Json(OperationResult.Error("活动不存在"));
            }

            //优惠券校验
            if (coupons == null || coupons.Length <= 0)
            {
                return Json(OperationResult.Error("优惠券数量不能为空"));
            }
            if (coupons.Any(c => c.Amount <= 0 || c.Amount > 50))
            {
                return Json(OperationResult.Error("优惠券金额必须在0~50之间"));
            }

            if (coupons.Any(c => string.IsNullOrEmpty(c.Longtitude) || string.IsNullOrEmpty(c.Latitude)))
            {
                return Json(OperationResult.Error("优惠券位置不可为空"));
            }


            if (coupons.Any(c => string.IsNullOrEmpty(c.Name)))
            {
                return Json(OperationResult.Error("优惠券名称不可为空"));
            }


            var entities = coupons.Select(s => new LBSCouponEntity
            {
                Name = s.Name,
                CouponNumber = Guid.NewGuid().ToString("N"),
                CouponType = activityEntity.CouponType,
                CouponActivityId = activityEntity.Id,
                Amount = s.Amount,
                Longtitude = s.Longtitude,
                Latitude = s.Latitude,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                OperatorId = AuthorityHelper.OperatorId
            });
            entities.Each(e => activityEntity.Coupons.Add(e));
            var res = _couponContract.Update(activityEntity);
            return Json(res);

        }

    }
}