using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    public class AddProductDetailController : BaseController
    {
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IStoreContract _storeContract;

        public AddProductDetailController(IInventoryContract inventoryContract, IStorageContract storageContract,
             IAdministratorContract administratorContract,
             IStoreContract _storeContract,
            IBrandContract brandContract)
        {
            _inventoryContract = inventoryContract;
            _storageContract = storageContract;
            _brandContract = brandContract;
            this._administratorContract = administratorContract;
            this._storeContract = _storeContract;
        }
        // GET: /Warehouses/AddProductDetail/
        [Layout]
        public ActionResult Index()
        {
           
            ViewBag.Brands = CacheAccess.GetBrand(_brandContract, true);
            return View();
        }

        public ActionResult List()
        {
            GridRequest rq = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Inventory>(rq.FilterGroup);

            int cou = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract).Count(pred);
            var inves = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract).Where(pred).OrderByDescending(c => c.CreatedTime).Skip(rq.PageCondition.PageIndex).Take(rq.PageCondition.PageSize).Select(c => new {
                c.Id,
                c.Store.StoreName,
                c.Product.ProductOriginNumber.Brand.BrandName,
                c.Product.ProductOriginNumber.Category.CategoryName,
                c.Product.ProductOriginNumber.Season.SeasonName,
                c.Product.Size.SizeName,
                c.Product.Color.ColorName,
                c.Product.Color.IconPath,
                c.Storage.StorageName,
                c.Product.ThumbnailPath,
                c.ProductBarcode,
                c.CreatedTime,
                c.Operator.Member.MemberName
            }).ToList<object>();
            GridData<object> ojbdat = new GridData<object>(inves, cou, Request);
            return Json(ojbdat);
        }
    }
}