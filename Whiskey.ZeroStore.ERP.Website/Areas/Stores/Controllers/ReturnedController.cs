




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
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    /// <summary>
    /// 退货记录Controller
    /// </summary>
    [License(CheckMode.Verify)]
    public class ReturnedController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ReturnedController));

        protected readonly IReturnedContract _returnedContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IAdministratorContract _administratorContract;

        public ReturnedController(IReturnedContract returnedContract, IStorageContract storageContract,
             IAdministratorContract administratorContract,
            IStoreContract storeContract)
        {
            _returnedContract = returnedContract;
            _storageContract = storageContract;
            _storeContract = storeContract;
            _administratorContract = administratorContract;
        }


        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View(new Returned());
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
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(ReturnedDto dto)
        {
            var result = _returnedContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(ReturnedDto dto)
        {
            var result = _returnedContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _returnedContract.Edit(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _returnedContract.View(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? storeId)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Returned, bool>> predicate = FilterHelper.GetExpression<Returned>(request.FilterGroup);
            var count = 0;
            var query = _returnedContract.Returneds;

            //根据店铺权限筛选
            var enabledStores = _storeContract.FilterStoreId(AuthorityHelper.OperatorId, _administratorContract, storeId);
            query = query.Where(r => enabledStores.Contains(r.StoreId.Value));
            var list = query.Where<Returned, int>(predicate, request.PageCondition, out count)
            .Select(m => new
            {
                StoreName = m.Store.StoreName,
                MemberName = m.Member.RealName,
                ReturnedNumber = m.ReturnedNumber,
                RetailNumber = m.RetailNumber,
                EraseMoney = m.EraseMoney,
                Status = m.Status,
                Cash = m.Cash,
                Card = m.SwipCard,
                ConsumeScore = m.ConsumeScore,
                AchieveScore = m.AchieveScore,
                Balance = m.Balance,
                Coupon = m.Coupon,
                Id = m.Id,
                IsDeleted = m.IsDeleted,
                IsEnabled = m.IsEnabled,
                Sequence = m.Sequence,
                UpdatedTime = m.UpdatedTime,
                CreatedTime = m.CreatedTime,
                OperatorName = m.Operator.Member.MemberName,
            }).ToList();
            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _returnedContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _returnedContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _returnedContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _returnedContract.Enable(Id);
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
            var result = _returnedContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _returnedContract.Returneds.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Export(int? storeId)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));

            GridRequest request = new GridRequest(Request);
            Expression<Func<Returned, bool>> predicate = FilterHelper.GetExpression<Returned>(request.FilterGroup);
            //根据店铺权限筛选
            var enabledStores = _storeContract.FilterStoreId(AuthorityHelper.OperatorId, _administratorContract, storeId);
            var query = _returnedContract.Returneds.Where(r => enabledStores.Contains(r.StoreId.Value)).Where(predicate);
            var list = query.Select(m => new
            {
                StoreName = m.Store.StoreName,
                MemberName = m.Member.RealName,
                ReturnedNumber = m.ReturnedNumber,
                RetailNumber = m.RetailNumber,
                EraseMoney = m.EraseMoney,
                Status = m.Status + "",
                Cash = m.Cash,
                Card = m.SwipCard,
                ConsumeScore = m.ConsumeScore,
                AchieveScore = m.AchieveScore,
                Balance = m.Balance,
                Coupon = m.Coupon,
                UpdatedTime = m.UpdatedTime,
                OperatorName = m.Operator.Member.MemberName,
            }).ToList();

            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "退货记录查询");
        }



        /// <summary>
        /// 打印购物凭证
        /// </summary>
        /// <param name="numb">销售编号</param>
        /// <returns></returns>
        public ActionResult PrintReceipt(int Id)
        {
            Returned returned = _returnedContract.Returneds.FirstOrDefault(c => c.Id == Id);

            return PartialView(returned);
        }
        public ActionResult PView(string returnedNumber)
        {
            var entity = _returnedContract.Returneds.Where(r => !r.IsDeleted && r.IsEnabled)
                .Where(r => r.ReturnedNumber == returnedNumber)
                .FirstOrDefault();
            return PartialView(entity);
        }



        public ActionResult GetItemsByReturnedNumber(string returnedNumber)
        {
            var req = new GridRequest(Request);
            var dataList = _returnedContract.Returneds.Where(r => !r.IsDeleted && r.IsEnabled)
                .Where(r => r.ReturnedNumber == returnedNumber)
                .SelectMany(r => r.ReturnedItems)
                .Select(i => new
                {
                    i.Id,
                    i.ProductBarcode,
                    i.CreatedTime,
                    i.ReturnedNumber,
                    i.RetailPrice,
                    i.Quantity,
                    i.Inventory.Product.ThumbnailPath
                })
                .OrderByDescending(r => r.CreatedTime)
                .Skip(req.PageCondition.PageIndex)
                .Take(req.PageCondition.PageSize)
                .ToList();

            var data = new GridData<object>(dataList, dataList.Count, Request);
            return Json(data, JsonRequestBehavior.AllowGet);

        }





    }
}
