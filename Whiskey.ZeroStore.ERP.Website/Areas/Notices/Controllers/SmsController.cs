using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Notices.Controllers
{
    [License(CheckMode.Verify)]
    public class SmsController : BaseController
    {
        #region 初始化

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SmsController));

        protected readonly IMemberContract _memberContract;
        protected readonly ISmsContract _SmsContract;
        protected readonly IStoreContract _StoreContract;

        public SmsController(
            IMemberContract _memberContract,
            IStoreContract _StoreContract,
            ISmsContract _SmsContract
            )
        {
            this._memberContract = _memberContract;
            this._SmsContract = _SmsContract;
            this._StoreContract = _StoreContract;
        }

        #endregion

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        #region 获取数据列表

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Sms, bool>> predicate = FilterHelper.GetExpression<Sms>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _SmsContract.Entities.Where<Sms, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.UpdatedTime,
                    m.SendTime,
                    m.IsSend,
                    Operator = m.Operator.Member.MemberName,
                    StoreCount = m.Stores.Where(w => w.IsEnabled && !w.IsDeleted).Count(),
                    MemberCount = m.Members.Where(w => w.IsEnabled && !w.IsDeleted).Count(),

                }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 选择归属店铺

        public ActionResult SelStore()
        {
            return PartialView();
        }

        public async Task<ActionResult> GetStoreSelectList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Store, bool>> predicate = FilterHelper.GetExpression<Store>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _StoreContract.Stores.Where(s => !s.IsDeleted && s.IsEnabled && s.IsAttached).Where<Store, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.StoreName,
                    m.StoreType.TypeName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetStoreSelectListAll()
        {
            var data = await Task.Run(() =>
            {
                var list = _StoreContract.Stores.Where(s => !s.IsDeleted && s.IsEnabled && s.IsAttached).Select(m => new
                {
                    m.Id,
                    m.StoreName,
                    m.StoreType.TypeName,
                }).ToList();
                return list;
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 创建

        public ActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult Create(SmsDto dto)
        {
            var data = _SmsContract.Insert(dto);
            return Json(data);
        }

        #endregion

        #region 修改

        public ActionResult Update(int Id)
        {
            var data = _SmsContract.Edit(Id);

            ViewBag.Stores = _StoreContract.Stores.Where(w => w.IsEnabled && !w.IsDeleted && w.IsAttached).Where(w => data.StoreIds.Contains(w.Id)).Select(s => new SelectListItem()
            {
                Text = s.StoreName,
                Value = s.Id + "",
                Selected = true,
            }).ToList();

            return PartialView(data);
        }

        [HttpPost]
        public ActionResult Update(SmsDto dto)
        {
            var data = _SmsContract.Update(dto);
            return Json(data);
        }

        #endregion

        #region 查看

        public ActionResult View(int Id)
        {
            var data = _SmsContract.View(Id);
            return PartialView(data);
        }

        public ActionResult ViewStore()
        {
            return PartialView();
        }

        public async Task<ActionResult> GetSendStores(int Id)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Store, bool>> predicate = FilterHelper.GetExpression<Store>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var query = _SmsContract.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.Id == Id).SelectMany(s => s.Stores);
                var list = query.Where(s => !s.IsDeleted && s.IsEnabled && s.IsAttached).Where<Store, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.StoreName,
                    m.StoreType.TypeName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 发送

        public JsonResult ConfirmSend(int Id)
        {
            var data = _SmsContract.ConfirmSend(Id);
            return Json(data);
        }

        #endregion

        #region 获取短信平台剩余短信条数

        public JsonResult GetPlatformCount()
        {
            var remain = _SmsContract.GetRemainSmsCount();
            var sended = _SmsContract.GetSendSmsCount();
            return Json(new { RemainCount = remain, SendedCount = sended }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}