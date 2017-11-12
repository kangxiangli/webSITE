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
using Whiskey.ZeroStore.ERP.Website.Areas.Coupons.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class PartnerController : BaseController
    {
        #region 初始化业务层操作对象
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(PartnerController));

        protected readonly IPartnerContract _partnerContract;

        protected readonly IPartnerLevelContract _partnerLevelContract;

        public PartnerController(IPartnerContract partnerContract,
            IPartnerLevelContract partnerLevelContract)
        {
            _partnerContract = partnerContract;
            _partnerLevelContract = partnerLevelContract;
        }
        #endregion

        #region 初始化供应商界面
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取数据列表
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Partner, bool>> predicate = FilterHelper.GetExpression<Partner>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                List<Partner> listPartner = _partnerContract.Partners.Where<Partner, int>(predicate, request.PageCondition, out count).OrderByDescending(x => x.CreatedTime).ToList();
                List<M__Coupon> listM = new List<M__Coupon>();
                DateTime current = DateTime.Now;
                foreach (Partner partner in listPartner)
                {
                    List<Coupon> listCoupon = partner.Coupons.Where(x => x.IsForever == true || (x.StartDate.CompareTo(current) <= 0 && x.EndDate.CompareTo(current)>=0)).ToList();
                    int quantity = 0;
                    foreach (Coupon coupon in listCoupon)
                    {
                         quantity=quantity+ coupon.CouponItems.Where(x => x.IsUsed == false && x.MemberId == null).Count();
                    }
                    if(partner.PartnerLevelId!=null)
                    {
                        quantity = partner.PartnerLevel.CouponQuantity - quantity;
                    }
                    listM.Add(new M__Coupon() { PartnerId=partner.Id,Quantity=quantity});
                }
                var list = listPartner.Select(x => new
                {
                    x.Id,
                    x.PartnerName,
                    x.Contacts,
                    x.TelPhone,
                    x.PhoneNum,
                    x.IsCooperation,                    
                    x.IsDeleted,
                    x.IsEnabled,
                    x.IconPath,
                    Level=x.PartnerLevelId==null?string.Empty:x.PartnerLevel.Level.ToString(),
                    CouponQuantity = x.PartnerLevelId == null ? 0 : x.PartnerLevel.CouponQuantity,
                    EnableQuantity = listM.FirstOrDefault(k => k.PartnerId == x.Id)==null ? 0 : listM.FirstOrDefault(k => k.PartnerId == x.Id).Quantity,
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 添加数据
        /// <summary>
        /// 初始化添加数据界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(PartnerDto dto)
        {             
            var res =  _partnerContract.Insert(dto);
            return Json(res);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改数据界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            
            var entity = _partnerContract.Edit(Id);
            return PartialView(entity);
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(PartnerDto dto)
        {            
            var res = _partnerContract.Update(dto);
            return Json(res);
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _partnerContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _partnerContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 查看数据
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var entity=_partnerContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 启用和禁用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _partnerContract.Enable(Id);
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
            var result = _partnerContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 初始化选择界面
        public ActionResult ChooseIndex()
        {
            return PartialView();
        }        
        #endregion

        #region 上传图片
        public JsonResult UploadImage()
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            HttpFileCollectionBase file = Request.Files;
            if (file==null)
            {
                oper.Message = "请选择图片";
                return Json(oper);
            }
            else
            {
                DateTime current = DateTime.Now;
                string strDate = current.Year.ToString() + "/" + current.Month.ToString() + "/" + current.Day.ToString() + "/" + current.Hour.ToString() + "/";
                string strFileName = current.ToString("yyyyMMddHHmmss") + ".jpg";
                string savePath = ConfigurationHelper.GetAppSetting("PartnerImagePath") + strDate + strFileName;
                file[0].SaveAs(FileHelper.UrlToPath(savePath));
                var temp = file[0];
                oper.Message = "上传成功";
                oper.ResultType = OperationResultType.Success;
                oper.Data = savePath;
                return Json(oper);
            }
        }
        #endregion

        #region 初始化等级界面
        public ActionResult PartnerLevelIndex(int Id)
        {
            ViewBag.PartnerId = Id;
            return PartialView();
        }
        #endregion

        #region 获取等级数据列表
        public async Task<ActionResult> PartnerLevelList()
        {
            GridRequest request = new GridRequest(Request);
            int partnerId = 0;
            FilterRule rule = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "PartnerId");
            if (rule!=null)
            {
                partnerId = int.Parse(rule.Value.ToString());
                request.FilterGroup.Rules.Remove(rule);
            }
            Expression<Func<PartnerLevel, bool>> predicate = FilterHelper.GetExpression<PartnerLevel>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Partner partner = _partnerContract.View(partnerId);
                List<PartnerLevel> listPartnerLevel = _partnerLevelContract.PartnerLevels.ToList();
                if (partner!=null)
                {
                    PartnerLevel currentLevel = listPartnerLevel.FirstOrDefault(x => x.Id == partner.PartnerLevelId);
                    if (currentLevel!=null)
                    {
                        listPartnerLevel = listPartnerLevel.Where(x => x.Id != currentLevel.Id && x.Level > currentLevel.Level).ToList();
                    }
                }
                var count = 0;
                var list = listPartnerLevel.AsQueryable().Where<PartnerLevel, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.LevelName,
                    m.Level,
                    m.Experience,
                    m.Price,
                    m.CouponQuantity,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 购买等级
        public JsonResult OrderLevel(int Id, int PartnerId)
        {
            OperationResult oper =  _partnerLevelContract.OrderLevel(Id, PartnerId);
            return Json(oper);
        }
        #endregion

    }
}