using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Models.DTO;
using AutoMapper;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Utility.Data;
using System.Collections.Generic;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    [License(CheckMode.Verify)]
    public class StoreCheckRecordController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreCheckRecordController));

        protected readonly IStoreCheckRecordContract _storeCheckRecordContract;
        protected readonly IStoreCheckItemContract _storeCheckContract;
        protected readonly IStoreContract _storeContract;

        public StoreCheckRecordController(
            IStoreCheckRecordContract _storeCheckRecordContract,
            IStoreCheckItemContract storeCheckContract,
            IStoreContract storeContract
            )
        {
            this._storeCheckRecordContract = _storeCheckRecordContract;
            _storeCheckContract = storeCheckContract;
            _storeContract = storeContract;
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
        [Layout]
        public ActionResult Create()
        {
            var entities = _storeCheckContract.Entities.Where(s => !s.IsDeleted && s.IsEnabled)
               .ToList();
            var storeChecks = entities.Select(s => Mapper.Map<StoreCheckDTO>(s)).ToList();
            ViewBag.StoreChecks = storeChecks;
            
            return View(new StoreCheckRecordDTO());
        }




        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(StoreCheckRecordDTO dto)
        {
            if (string.IsNullOrEmpty(dto.CheckDetails))
            {
                return Json(OperationResult.Error("参数错误"));
            }
            var checkInfo = JsonHelper.FromJson<List<CheckInfoModel>>(dto.CheckDetails);
            if (checkInfo.Count <= 0)
            {
                return Json(OperationResult.Error("数据不可为空"));
            }
            var allCheckItems = _storeCheckContract.Entities.Where(s => !s.IsDeleted && s.IsEnabled).ToList();


            var result = _storeCheckRecordContract.Insert(dto, checkInfo);

            return Json(result, JsonRequestBehavior.AllowGet);
        }





        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(StoreCheckRecordDTO dto)
        {
            var result = _storeCheckRecordContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _storeCheckRecordContract.Edit(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var entity = _storeCheckRecordContract.View(Id);
            var dto = Mapper.Map<StoreCheckRecordDTO>(entity);
            return PartialView(dto);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? storeId)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<StoreCheckRecord, bool>> predicate = FilterHelper.GetExpression<StoreCheckRecord>(request.FilterGroup);
            var count = 0;

            var list = _storeCheckRecordContract.Entities.Where<StoreCheckRecord, int>(predicate, request.PageCondition, out count)
            .Select(s => new
            {
                s.Id,
                s.StoreId,
                s.StoreName,
                s.CheckTime,
                s.RatingPoints,
                s.IsDeleted,
                s.IsEnabled,
                s.CheckDetails,
                s.TotalPunishScore


            }).ToList();
            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _storeCheckRecordContract.DeleteOrRecovery(true, Id);
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
            var result = _storeCheckRecordContract.DeleteOrRecovery(false, Id);
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
            var result = _storeCheckRecordContract.EnableOrDisable(true, Id);
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
            var result = _storeCheckRecordContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}