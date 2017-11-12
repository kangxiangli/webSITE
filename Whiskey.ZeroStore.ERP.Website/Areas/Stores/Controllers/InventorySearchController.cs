using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    public class InventorySearchController : BaseController
    {
        protected readonly IRetailContract _retailContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IProductContract _productContract;
        protected readonly IAdministratorContract _adminContract;
        public InventorySearchController(IRetailContract retailContract, IStorageContract storageContract, IBrandContract brandContract, IInventoryContract inventoryContract
            , IAdministratorContract _adminContract
            , IStoreContract storeContract
            , IProductContract _productContract)
        {
            _retailContract = retailContract;
            _storageContract = storageContract;
            _brandContract = brandContract;
            _inventoryContract = inventoryContract;
            this._productContract = _productContract;
            this._adminContract = _adminContract;
            _storeContract = storeContract;
        }

        //在店铺管理模块，跨店铺查找库存记录
        // GET: /Stores/InventorySearch/
        [Layout]
        public ActionResult Index()
        {
            ViewBag.Brands = CacheAccess.GetBrand(_brandContract, true);
            return View();
        }

        public ActionResult List(string bigProdNum)
        {
            if (string.IsNullOrEmpty(bigProdNum) || bigProdNum.Length > 7)
            {
                return Json(OperationResult.Error("参数错误"));
            }
            GridRequest gr = new GridRequest(Request);

            var managedStoreIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            if (managedStoreIds == null || !managedStoreIds.Any())
            {
                return Json(OperationResult.Error("权限不足"));
            }


            //获取权限之外且不是独立库存的店铺
            var outPermissionAndNotIndependentStoreIds = _storeContract.Stores
                .Where(s => !s.IsDeleted && s.IsEnabled)
                .Where(s => !managedStoreIds.Contains(s.Id))
                .Where(s => !s.StoreType.IndependentStorage)
                .Select(s => s.Id).ToList();


            var list = _productContract.Products.Where(c => !c.IsDeleted && c.IsEnabled)
                                                .Where(c => c.BigProdNum.StartsWith(bigProdNum))
                                                .Select(c => new SearchEnry
                                                {
                                                    Id = c.Id,
                                                    ProductNumber = c.ProductNumber,
                                                    ThumbnailPath = c.ThumbnailPath ?? c.ProductOriginNumber.ThumbnailPath,
                                                    IconPath = c.Color.IconPath,
                                                    BrandName = c.ProductOriginNumber.Brand.BrandName,
                                                    SizeName = c.Size.SizeName,
                                                    CategoryName = c.ProductOriginNumber.Category.CategoryName,
                                                    ColorName = c.Color.ColorName,
                                                    SeasonName = c.ProductOriginNumber.Season.SeasonName,
                                                    TagPrice = c.ProductOriginNumber.TagPrice,
                                                    Cou = _inventoryContract.Inventorys.Where(i=> i.IsEnabled && !i.IsDeleted)
                                                                                       .Where(i=> i.ProductId == c.Id)
                                                                                       .Where(i=> i.Status == (int)InventoryStatus.Default&& !i.IsLock)
                                                                                       .Where(i=> managedStoreIds.Contains(i.StoreId) || outPermissionAndNotIndependentStoreIds.Contains(i.StoreId))
                                                                                       .Count()

                                                }).ToList();




            GridData<object> grdat = new GridData<object>(list, list.Count, Request);
            return Json(grdat);


        }
        public class SearchEnry
        {
            public int Id { get; set; }
            public string ProductNumber { get; set; }
            public string ThumbnailPath { get; set; }
            public string IconPath { get; set; }
            public string BrandName { get; set; }
            public string ColorName { get; set; }
            public string SizeName { get; set; }
            public string CategoryName { get; set; }
            public string SeasonName { get; set; }
            public float TagPrice { get; set; }
            public int Cou { get; set; }
        }



        /// <summary>
        /// 根据商品货号获取可用的库存
        /// </summary>
        /// <param name="PrNum">商品货号</param>
        /// <returns></returns>
        public new ActionResult View(string PrNum)
        {
            ViewBag.pNum = PrNum;
            return PartialView();
        }

        public ActionResult GetProductsByProdNum()
        {

            var managedStoreIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            if (managedStoreIds == null || !managedStoreIds.Any())
            {
                return Json(OperationResult.Error("权限不足"));
            }


            //获取权限之外且不是独立库存的店铺
            var outPermissionAndNotIndependentStoreIds = _storeContract.Stores
                .Where(s => !s.IsDeleted && s.IsEnabled)
                .Where(s => !managedStoreIds.Contains(s.Id))
                .Where(s => !s.StoreType.IndependentStorage)
                .Select(s => s.Id).ToList();


            GridRequest req = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Inventory>(req.FilterGroup);
            var allinv = _inventoryContract.Inventorys
             .Where(pred)
             .Where(c=> managedStoreIds.Contains(c.StoreId) || outPermissionAndNotIndependentStoreIds.Contains(c.StoreId))
             .Where(c => c.IsEnabled && !c.IsDeleted && c.Status == (int)InventoryStatus.Default && !c.IsLock);
            var invd = allinv.Select(c => new
            {
                c.Id,
                c.ProductNumber,
                c.ProductBarcode,
                c.Product.ProductOriginNumber.Brand.BrandName,
                c.Product.ProductOriginNumber.Category.CategoryName,
                c.Product.Color.ColorName,
                IconPath = c.Product.Color.IconPath,
                c.Product.ProductOriginNumber.Season.SeasonName,
                c.Product.Size.SizeName,
                c.Storage.StorageName,
                c.Store.StoreName,
                ThumbnailPath = c.Product.ThumbnailPath ?? c.Product.ProductOriginNumber.ThumbnailPath
            }).OrderBy(c => c.Id).Skip(req.PageCondition.PageIndex).Take(req.PageCondition.PageSize);
            int cou = allinv.Count();
            GridData<object> data = new GridData<object>(invd, cou, Request);
            return Json(data);
        }
    }
}