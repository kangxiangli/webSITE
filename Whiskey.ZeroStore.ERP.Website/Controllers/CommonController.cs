using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Models;
using System;
using Whiskey.ZeroStore.ERP.Models.DTO;
using System.Linq.Expressions;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class CommonController : Controller
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(CommonController));

        private readonly IStoreContract _storeContract;
        private readonly IModuleContract _moduleContract;
        private readonly IPermissionContract _permisstionContract;
        private readonly IAdministratorContract _adminContract;
        private readonly IDepartmentContract _departContract;
        private readonly IMemberContract _memberContract;
        private readonly IJobPositionContract _jobPositionContract;
        private readonly IStoreTypeContract _storeTypeContract;
        private readonly IStoreDepositContract _storeDepositContract;
        private readonly IStoreLevelContract _storeLevelContract;
        private readonly IDesignerContract _designerContract;
        private readonly ICategoryContract _categoryContract;
        private readonly IProductAttributeContract _productAttributeContract;
        private readonly IProductContract _productContract;
        private readonly IProductOrigNumberContract _productOrigNumberContract;
        private readonly IStorageContract _storageContract;
        private const int _cacheDuratiaon = 600;

        public CommonController(
            IStoreContract storeContract,
            IModuleContract moduleContract,
            IDepartmentContract departContract,
            IAdministratorContract adminContract,
            IMemberContract memberContract,
            IStoreTypeContract storeTypeContract,
            IStoreDepositContract storeDepositContract,
            IStoreLevelContract storeLevelContract,
            IJobPositionContract jobPositionContract,
            IDesignerContract designerContract,
            IPermissionContract permisstionContract,
            ICategoryContract categoryContract,
            IProductAttributeContract productAttributeContract,
            IProductContract productContract,
            IProductOrigNumberContract productOrigNumberContract,
            IStorageContract storageContract

            )
        {
            _storeContract = storeContract;
            _moduleContract = moduleContract;
            _departContract = departContract;
            _adminContract = adminContract;
            _memberContract = memberContract;
            _jobPositionContract = jobPositionContract;
            _storeTypeContract = storeTypeContract;
            _storeDepositContract = storeDepositContract;
            _storeLevelContract = storeLevelContract;
            _designerContract = designerContract;
            _permisstionContract = permisstionContract;
            _categoryContract = categoryContract;
            _productAttributeContract = productAttributeContract;
            _productContract = productContract;
            _productOrigNumberContract = productOrigNumberContract;
            _storageContract = storageContract;
        }

        [OutputCache(Duration = _cacheDuratiaon)]
        public ActionResult QueryAllStore(bool? onlyAttach)
        {
            var query = _storeContract.QueryAllStore().AsQueryable();
            if (onlyAttach.HasValue && onlyAttach.Value)
            {
                query = query.Where(s => s.IsAttached);
            }
            var stores = query.OrderBy(s => s.StoreTypeCreateTime).ThenBy(s => s.StoreTypeId).ThenBy(s => s.CreateTime).Select(s => new
            {
                s.Id,
                s.StoreName,
                s.StoreTypeName,
                s.City,
                s.Telephone
            }).ToList();

            return Json(new OperationResult(OperationResultType.Success, string.Empty, stores), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查询权限下的店铺信息
        /// </summary>
        /// <param name="onlyAttach">仅归属店铺</param>
        /// <param name="isDesigner">是否设计师</param>
        /// <returns></returns>
        public ActionResult QueryManageStore(bool? onlyAttach)
        {
            var adminId = AuthorityHelper.OperatorId;
            if (!adminId.HasValue)
            {
                return Json(OperationResult.Error("请登录"), JsonRequestBehavior.AllowGet);
            }

            var storeIds = _storeContract.QueryManageStoreId(adminId.Value);
            // 设计师
            var designerStoreIds = _designerContract.SelectDesigner.Where(d => !d.IsDeleted && d.IsEnabled && d.AdminId == adminId.Value).Select(d => d.Factory.StoreId).Distinct().ToList();
            if (designerStoreIds.Any())
            {
                storeIds = designerStoreIds;
            }

            var closedStoreIds = _storeContract.GetStoresClosed();
            var checkingStoreIds = _storeContract.GetStoresInChecking();
            var orderblankingStoreIds = _storeContract.GetStoresInOrderblanking();
            var storeQuery = _storeContract.QueryAllStore()
                                           .Where(s => storeIds.Contains(s.Id));
            if (onlyAttach.HasValue && onlyAttach.Value)
            {
                storeQuery = storeQuery.Where(s => s.IsAttached);
            }
            var storeDtos = storeQuery.Select(s => new StoreSelectionDTO
            {
                Id = s.Id,
                StoreName = s.StoreName,
                StoreTypeName = s.StoreTypeName,
                Telephone = s.Telephone,
                City = s.City,
                IsClosed = false,
                IsOrderBlanking = false,
                IsChecking = false
            }).ToList();
            storeDtos.Where(s => closedStoreIds.Contains(s.Id)).Each(s => s.IsClosed = true);
            storeDtos.Where(s => checkingStoreIds.Contains(s.Id)).Each(s => s.IsChecking = true);
            storeDtos.Where(s => orderblankingStoreIds.Contains(s.Id)).Each(s => s.IsOrderBlanking = true);
            return Json(new OperationResult(OperationResultType.Success, string.Empty, storeDtos), JsonRequestBehavior.AllowGet);
        }
        public ActionResult QueryAllStorage(int storeId)
        {
            var data = _storageContract.Storages.Where(s => !s.IsDeleted && s.IsEnabled && s.StoreId == storeId)
                .Select(s => new
                {
                    s.Id,
                    s.StorageName
                }).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data), JsonRequestBehavior.AllowGet);
        }
        public ActionResult QueryManageStorage()
        {
            return View();
        }


        [OutputCache(Duration = _cacheDuratiaon)]
        public ActionResult GetCategory()
        {
            var data = _categoryContract.Categorys.Where(c => !c.IsDeleted && c.IsEnabled && !c.ParentId.HasValue).Select(c => new
            {
                c.CategoryName,
                c.Id
            }).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = _cacheDuratiaon)]
        public ActionResult GetProductAttr()
        {
            var data = _productAttributeContract.ProductAttributes.Where(c => !c.IsDeleted && c.IsEnabled && !c.ParentId.HasValue).Select(p => new
            {
                p.Id,
                p.AttributeName,
                p.ParentId,
                IsChecked = false,
                Children = p.Children.Where(c => !c.IsDeleted && c.IsEnabled).Select(c => new
                {
                    c.ParentId,
                    c.AttributeName,
                    c.Id,
                    IsChecked = false
                })
            }).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = _cacheDuratiaon)]
        public ActionResult ProductList(int? categoryId, string tags, string productNumber, bool? hasInventory, int pageIndex = 1, int pageSize = 10)
        {
            var tagArr = new string[0];

            if (!string.IsNullOrEmpty(tags) && tags.Length > 0)
            {
                tagArr = tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }


            var query = _productOrigNumberContract.OrigNumbs.Where(p => !p.IsDeleted && p.IsEnabled)
                                                            .Where(p => p.IsRecommend.Value);
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Category.ParentId == categoryId);
            }
            if (tagArr != null && tagArr.Length > 0)
            {
                query = query.Where(p => tagArr.All(t => p.ProductAttributes.Any(a => a.AttributeName == t)));
            }

            var bigProdNumberQuery = query.Select(p => p.BigProdNum);

            var productQuery = _productContract.Products.Where(p => !p.IsDeleted && p.IsEnabled && bigProdNumberQuery.Contains(p.BigProdNum));

            if (!string.IsNullOrEmpty(productNumber) && productNumber.Length > 0)
            {
                if (productNumber.IndexOf(",") == -1)  //单个number
                {
                    productQuery = productQuery.Where(p => p.ProductNumber.StartsWith(productNumber));
                }
                else  //多个number
                {
                    var numberArr = productNumber.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    productQuery = productQuery.Where(p => numberArr.Contains(p.ProductNumber));
                }
            }
            Expression<Func<Inventory, bool>> inventoryFilter = i => !i.IsDeleted && i.IsEnabled && i.Status == InventoryStatus.Default && !i.IsLock;

            if (hasInventory.HasValue)
            {
                if (hasInventory.Value)
                {
                    productQuery = productQuery.Where(p => p.Inventories.AsQueryable().Count(inventoryFilter) > 0);
                }
                else
                {
                    productQuery = productQuery.Where(p => p.Inventories.AsQueryable().Count(inventoryFilter) <= 0);
                }
            }



            var list = productQuery.OrderByDescending(p => p.UpdatedTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.BigProdNum,
                    p.ProductNumber,
                    ProductCollocationImg = p.ProductCollocationImg ?? p.ProductOriginNumber.ProductCollocationImg,
                    p.Color.ColorName,
                    p.Size.SizeName,
                    p.ProductOriginNumber.Season.SeasonName,
                    p.ProductOriginNumber.Brand.BrandName,
                    p.ProductOriginNumber.TagPrice,
                    InventoryCount = p.Inventories.AsQueryable().Where(inventoryFilter).Count()

                })

                .ToList();
            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = productQuery.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public class StoreSelectionDTO
        {
            public int Id { get; set; }
            public string StoreName { get; set; }
            public string StoreTypeName { get; set; }
            public string Telephone { get; set; }
            public string City { get; set; }
            public bool IsClosed { get; set; }
            public bool IsOrderBlanking { get; set; }
            public bool IsChecking { get; set; }
        }
    }
}