using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using AutoMapper;
using Antlr3.ST;
using Antlr3.ST.Language;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Linq.Expressions;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Factory.Controllers
{
    [License(CheckMode.Verify)]
    public class FactorysController : BaseController
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _log = LogManager.GetLogger(typeof(FactorysController));
        public static object objlock = new object();
        protected readonly IFactorysContract _factorysContract;
        protected readonly IDesignerContract _designerContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IStoreContract _StoreContract;
        protected readonly IDepartmentContract _DepartmentContract;

        public FactorysController(
            IFactorysContract factorysContract,
            IDesignerContract designerContract,
            IAdministratorContract administratorContract
            , IBrandContract _brandContract
            , IStoreContract _StoreContract
            , IDepartmentContract _DepartmentContract
            )
        {
            _factorysContract = factorysContract;
            _designerContract = designerContract;
            _administratorContract = administratorContract;
            this._brandContract = _brandContract;
            this._StoreContract = _StoreContract;
            this._DepartmentContract = _DepartmentContract;
        }

        #endregion
        [Layout]

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Update(int Id)
        {
            var result = _factorysContract.View(Id);
            Mapper.CreateMap<Factorys, FactorysDto>();
            FactorysDto dto = Mapper.Map<Factorys, FactorysDto>(result);
            ViewBag.Departments = CacheAccess.GetDepartmentListItem(_DepartmentContract, true, DepartmentTypeFlag.设计);
            ViewBag.Brands = CacheAccess.GetBrand(_brandContract, true, false);
            ViewBag.Stores = result.Department.Stores.Where(w => w.IsEnabled && !w.IsDeleted).Select(s => new SelectListItem
            {
                Value = s.Id + "",
                Text = s.StoreName,
            }).ToList();
            ViewBag.Storages = _StoreContract.Stores.Where(w => w.IsEnabled && !w.IsDeleted && w.Id == result.StoreId).SelectMany(s => s.Storages)
                    .Where(w => w.IsEnabled && !w.IsDeleted).Select(s => new SelectListItem
                    {
                        Value = s.Id + "",
                        Text = s.StorageName,
                    }).ToList();

            return PartialView(dto);
        }

        [HttpPost]
        [ValidateInput(false)]
        [Log]
        public ActionResult Update(FactorysDto dto)
        {
            var result = _factorysContract.Update(dto);
            return Json(result);
        }


        public ActionResult Create()
        {
            ViewBag.Departments = CacheAccess.GetDepartmentListItem(_DepartmentContract, true, DepartmentTypeFlag.设计);
            ViewBag.Brands = CacheAccess.GetBrand(_brandContract, true, false);
            return PartialView();
        }
        /// <summary>
        /// 获取部门下所有店铺
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public JsonResult GetDepartmentStores(int departmentId)
        {
            var data = new List<SelectListItem>();
            var liststores = _DepartmentContract.Departments.Where(w => w.IsEnabled && !w.IsDeleted && w.Id == departmentId).SelectMany(s => s.Stores)
                    .Where(w => w.IsEnabled && !w.IsDeleted).Select(s => new SelectListItem
                    {
                        Value = s.Id + "",
                        Text = s.StoreName,
                    }).ToList();
            if (liststores.IsNotNull())
            {
                data.AddRange(liststores);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取店铺下所有仓库
        /// </summary>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public JsonResult GetStoreStorages(int storeId)
        {
            var data = new List<SelectListItem>();
            var liststores = _StoreContract.Stores.Where(w => w.IsEnabled && !w.IsDeleted && w.Id == storeId).SelectMany(s => s.Storages)
                    .Where(w => w.IsEnabled && !w.IsDeleted).Select(s => new SelectListItem
                    {
                        Value = s.Id + "",
                        Text = s.StorageName,
                    }).ToList();
            if (liststores.IsNotNull())
            {
                data.AddRange(liststores);
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Create(FactorysDto drod)
        {
            OperationResult oper = _factorysContract.Insert(drod);
            return Json(oper);
        }
        
        [Log]
        public ActionResult View(int Id)
        {
            var result = _factorysContract.View(Id);
            return PartialView(result);
        }


        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Factorys, bool>> predicate = FilterHelper.GetExpression<Factorys>(request.FilterGroup);

            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _factorysContract.SelectFactorys.Where<Factorys, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.FactoryName,
                    m.FactoryAddress,
                    m.Leader,
                    m.MobilePhone,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Brand.BrandName,
                    m.Department.DepartmentName,
                    m.Store.StoreName,
                    m.Storage.StorageName,
                    m.CreatedTime,

                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult Remove(int[] Id)
        {
            var result = _factorysContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult Delete(int[] Id)
        {
            var result = _factorysContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult Recovery(int[] Id)
        {
            var result = _factorysContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [Log]
        public ActionResult Enable(int[] Id)
        {
            var result = _factorysContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult Disable(int[] Id)
        {
            var result = _factorysContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _factorysContract.SelectFactorys.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _factorysContract.SelectFactorys.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
    }
}