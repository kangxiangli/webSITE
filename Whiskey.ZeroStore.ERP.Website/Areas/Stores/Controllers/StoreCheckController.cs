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
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.Utility.Data;
using AutoMapper;
using Whiskey.Web.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    [License(CheckMode.Verify)]
    public class StoreCheckController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreCheckController));

        protected readonly IStoreCheckItemContract _storeTypeContract;

        public StoreCheckController(
            IStoreCheckItemContract _storeTypeContract
            )
        {
            this._storeTypeContract = _storeTypeContract;
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
        [HttpPost]
        public ActionResult Create(StoreCheckDTO dto, params string[] checkItem)
        {
            // items 序列化 [{optionName:<name>,IsCheck:false},...]
            var items = checkItem.Select(s => new CheckDetail() { IsCheck = false, OptionName = s }).ToArray();
            dto.Items = JsonHelper.ToJson(items);
            dto.ItemsCount = checkItem.Length;
            var result = _storeTypeContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(StoreCheckDTO dto, params string[] checkItem)
        {
            var items = checkItem.Select(s => new CheckDetail() { IsCheck = false, OptionName = s }).ToArray();
            dto.Items = JsonHelper.ToJson(items);
            dto.ItemsCount = checkItem.Length;
            dto.OperatorId = AuthorityHelper.OperatorId;
            var result = _storeTypeContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _storeTypeContract.Edit(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var result = _storeTypeContract.View(Id);
            var dto = Mapper.Map<StoreCheckDTO>(result);
            return PartialView(dto);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? storeId)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<StoreCheckItem, bool>> predicate = FilterHelper.GetExpression<StoreCheckItem>(request.FilterGroup);
            var count = 0;

            var list = _storeTypeContract.Entities.Where<StoreCheckItem, int>(predicate, request.PageCondition, out count)
            .Select(s => new
            {
                s.Id,
                s.CheckName,
                s.PunishScore,
                s.ItemsCount,
                s.Items,
                s.IsDeleted,
                s.IsEnabled,
                s.CreatedTime,
                s.Standard,
                s.Operator.Member.MemberName,

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
            var result = _storeTypeContract.DeleteOrRecovery(true, Id);
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
            var result = _storeTypeContract.DeleteOrRecovery(false, Id);
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
            var result = _storeTypeContract.EnableOrDisable(true, Id);
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
            var result = _storeTypeContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}