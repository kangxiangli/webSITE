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
    public class CouponController : BaseController
    {
        #region 初始化业务层操作对象
                
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CouponController));

        protected readonly ICouponContract _couponContract;

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IPartnerContract _partnerContract;
        
        public CouponController(ICouponContract couponContract,
            IAdministratorContract administratorContract,
            IPartnerContract partnerContract)
        {
            _couponContract = couponContract;
            _partnerContract = partnerContract;
            _administratorContract = administratorContract;
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

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
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
                    SendQuantity=m.CouponItems.Where(x=>x.MemberId!=null).Count(),
                    UsedQuantity = m.CouponItems.Where(x => x.IsUsed==true).Count(),
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.StartDate,
                    m.EndDate,
                    m.Operator.Member.MemberName,
                    m.IsPartner,
                    //m.CouponQRCodePath,
                    m.CouponNum,
                    m.CouponImagePath,
                    m.IsRecommend,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Administrator admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
            int count =  admin.Roles.Where(x => x.Weight == 100).Count();
            bool isShow = false;
            if (count >0 )
            {
                isShow = true;   
            }
            List<SelectListItem> list = new List<SelectListItem>();
            ViewBag.IsShow = isShow;
            ViewBag.List = list;
            return PartialView();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(CouponDto dto)
        {
            if (dto.UniqueNum==null)
            {
                int adminId = AuthorityHelper.OperatorId??0;
                Administrator admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
                dto.UniqueNum = admin.Member.UniquelyIdentifies;
            }             
            var result = _couponContract.Insert(dto);
            return Json(result);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改数据界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Administrator admin = _administratorContract.Administrators.Where(x => x.Id == adminId).FirstOrDefault();
            int count = admin.Roles.Where(x => x.Weight == 100).Count();
            bool isShow = false;
            if (count > 0)
            {
                isShow = true;
            }
            ViewBag.IsShow = isShow;
            CouponDto dto = _couponContract.Edit(Id);
            admin = _administratorContract.Administrators.Where(x => x.Member.MemberName == dto.UniqueNum).FirstOrDefault();
            string realName = string.Empty;
            if (admin!=null)
            {
                realName = admin.Member.RealName;
            }
            
            int quantity = 0;
            if (dto.PartnerId != null)
            {
                Partner partner = _partnerContract.View(dto.PartnerId ?? 0);
                DateTime current = DateTime.Now;
                List<Coupon> listCoupon = partner.Coupons.Where(x => x.IsForever == true || (x.StartDate.CompareTo(current) <= 0 && x.EndDate.CompareTo(current)>=0)).ToList();                
                foreach (Coupon coupon in listCoupon)
                {
                     quantity=quantity+ coupon.CouponItems.Where(x => x.IsUsed == false && x.MemberId == null).Count();
                }
                if(partner.PartnerLevelId!=null)
                {
                    quantity = partner.PartnerLevel.CouponQuantity - quantity;
                }
            }
            ViewBag.Quantity = quantity;
            dto.RealName = realName;
            return PartialView(dto);
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(CouponDto dto)
        {
            Coupon coupon = _couponContract.Coupons.Where(x => x.Id == dto.Id).FirstOrDefault();            
            var res = _couponContract.Update(dto);
            return Json(res);
        }
        #endregion

        #region 查看数据详情
        /// <summary>
        /// 查看数据详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            
            var entity = _couponContract.View(Id);
            string strNum = entity.UniqueNum;
            Administrator admin = _administratorContract.Administrators.Where(x => x.Member.UniquelyIdentifies == strNum).FirstOrDefault();
            string realName = string.Empty;
            if (admin!=null)
            {
                realName = admin.Member.RealName;
            }
            ViewBag.RealName = realName;
            return PartialView(entity);
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Remove(int Id)
        {
            var res = _couponContract.Remove(Id);
            return Json(res);
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Recovery(int Id)
        {
            var res = _couponContract.Recovery(Id);
            return Json(res);
        }
        #endregion

        #region 上传图片
        public JsonResult UploadImage()
        {                         
            OperationResult oper = new OperationResult(OperationResultType.Error);
            HttpFileCollectionBase file = Request.Files;
            if (file == null)
            {
                oper.Message = "请选择图片";
                return Json(oper);
            }
            else
            {
                DateTime current = DateTime.Now;
                string strDate = current.Year.ToString() + "/" + current.Month.ToString() + "/" + current.Day.ToString() + "/" + current.Hour.ToString() + "/";
                string strFileName = current.ToString("yyyyMMddHHmmss") + ".jpg";
                string savePath = ConfigurationHelper.GetAppSetting("CouponImagePath") + strDate + strFileName;
                file[0].SaveAs(FileHelper.UrlToPath(savePath));
                var temp = file[0];
                oper.Message = "上传成功";
                oper.ResultType = OperationResultType.Success;
                oper.Data = savePath;
                return Json(oper);
            }
        }
        #endregion

        #region 获取员工
        /// <summary>
        /// 初始化员工界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Admin()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取员工数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AdminList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _administratorContract.Administrators.Where<Administrator, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.Member.MemberName,
                    m.Member.RealName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _couponContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 禁用数据
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _couponContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 初始化供应商优惠券界面
        /// <summary>
        /// 初始化供应商优惠券界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Coupon()
        {
            string strId=  Request["Id"];
            int id = int.Parse(strId);
            ViewBag.PartnerId = id;
            return PartialView();
        }
        #endregion

        #region 供应商优惠券列表
        public async Task<ActionResult> CouponList()
        {
            GridRequest request = new GridRequest(Request);
            var rule= request.FilterGroup.Rules.Where(x => x.Field == "PartnerId").FirstOrDefault();
            string strPartnerId = rule.Value.ToString();
            int partnerId = int.Parse(strPartnerId);
            request.FilterGroup.Rules.Remove(rule);
           
            Expression<Func<Coupon, bool>> predicate = FilterHelper.GetExpression<Coupon>(request.FilterGroup);
            IEnumerable<Coupon> listCoupon = _partnerContract.Partners.Where(x => x.Id == partnerId).FirstOrDefault().Coupons;
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = listCoupon.AsQueryable().Where<Coupon, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.CouponName,
                    m.CouponPrice,
                    m.Quantity,
                    SendQuantity = m.CouponItems.Where(x => x.MemberId != null).Count(),
                    UsedQuantity = m.CouponItems.Where(x => x.IsUsed == true).Count(),
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.StartDate,
                    m.EndDate,
                    m.Operator.Member.MemberName,
                    m.IsPartner,

                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}