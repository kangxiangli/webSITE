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
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Website.Areas.Products.Models;
using AutoMapper;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Commons.Controllers
{
    [License(CheckMode.Verify)]
    public class SaleAutoGenController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SaleAutoGenController));

        protected readonly ISaleAutoGenContract _SaleAutoGenContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IColorContract _colorContract;
        protected readonly ISeasonContract _seasonContract;
        protected readonly IProductCrowdContract _productCrowdContract;
        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IProductContract _productContract;

        public SaleAutoGenController(
            ISaleAutoGenContract _SaleAutoGenContract,
            IBrandContract _brandContract,
            ICategoryContract _categoryContract,
            IColorContract _colorContract,
            ISeasonContract _seasonContract,
            IProductCrowdContract _productCrowdContract,
            IProductOrigNumberContract _productOrigNumberContract,
            IStorageContract _storageContract,
            IAdministratorContract _administratorContract,
            IMemberContract _memberContract,
            IStoreContract _storeContract,
            IProductContract _productContract
            )
        {
            this._SaleAutoGenContract = _SaleAutoGenContract;
            this._brandContract = _brandContract;
            this._categoryContract = _categoryContract;
            this._colorContract = _colorContract;
            this._seasonContract = _seasonContract;
            this._productCrowdContract = _productCrowdContract;
            this._productOrigNumberContract = _productOrigNumberContract;
            this._storageContract = _storageContract;
            this._administratorContract = _administratorContract;
            this._memberContract = _memberContract;
            this._storeContract = _storeContract;
            this._productContract = _productContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult VProduct()
        {
            ViewBag.Color = CacheAccess.GetColorsName(_colorContract, true);
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true, false);
            ViewBag.Category = CacheAccess.GetCategory(_categoryContract, true);
            ViewBag.Season = CacheAccess.GetSeason(_seasonContract, true);
            ViewBag.Crowds = CacheAccess.GetProductCrowd(_productCrowdContract, true);

            return PartialView();
        }

        public ActionResult VStorage()
        {
            return PartialView();
        }

        public async Task<ActionResult> ProductList()
        {
            List<object> lis = new List<object>();
            GridRequest request = new GridRequest(Request);
            GridData<object> data = await Task.Run(() =>
            {
                #region 商品款号逻辑

                Expression<Func<Product, bool>> predicate = FilterHelper.GetExpression<Product>(request.FilterGroup);

                List<ProductTree> parens = new List<ProductTree>();
                var query = _productContract.Products.Where(predicate).Where(w => !string.IsNullOrEmpty(w.BigProdNum)).Where(w => w.ProductOriginNumber != null && w.ProductOriginNumber.IsVerified == CheckStatusFlag.通过);

                //其他信息筛选
                int cou = query.GroupBy(c => c.BigProdNum).Count();
                var proli = query.GroupBy(c => c.BigProdNum)
                                 .OrderByDescending(c => c.Max(g => g.CreatedTime))
                                 .Skip(request.PageCondition.PageIndex)
                                 .Take(request.PageCondition.PageSize);

                int rec = 0;
                proli.Each(c =>
                {
                    rec += c.Select(g => g.Id).Count();
                });

                foreach (var item in proli)
                {
                    #region 新版逻辑

                    var modPON = item.Select(f => f.ProductOriginNumber).FirstOrDefault();
                    if (modPON.IsNotNull())
                    {
                        var par = new
                        {
                            Id = "par" + modPON.Id,
                            BigProdNum = modPON.BigProdNum,
                            ParentId = "",
                            BrandName = modPON.Brand.BrandName,
                            CategoryName = modPON.Category.CategoryName,
                            SeasonName = modPON.Season.SeasonName,
                            ProductNumber = "",
                            SizeName = "",
                            ThumbnailPath = modPON.ThumbnailPath,
                            ColorName = "",
                            TagPrice = modPON.TagPrice,
                            AllPrice = modPON.TagPrice * modPON.Products.Count(c => c.IsEnabled && !c.IsDeleted),
                            modPON.IsEnabled,
                            ProductCount = modPON.Products.Count(c => c.IsEnabled && !c.IsDeleted)
                        };
                        lis.Add(par);

                        var childs = item.OrderByDescending(c => c.UpdatedTime).Where(x => !string.IsNullOrEmpty(x.BigProdNum) && !x.BigProdNum.StartsWith("-"))
                                .Select(x => GetDataByProduct(x, par.Id));

                        lis.AddRange(childs);
                    }
                    else
                    {
                        continue;
                    }

                    #endregion
                }

                return new GridData<object>(lis, cou, request.RequestInfo);

                #endregion
            });
            return Json(data);
        }
        /// <summary>
        /// 根据product得到返回的数据
        /// </summary>
        /// <param name="x">product</param>
        /// <param name="parid">父类ID，</param>
        /// <param name="isverif">是否通过审核</param>
        /// <returns></returns>
        private object GetDataByProduct(Product x, string parid)
        {
            var modPON = x.ProductOriginNumber;//原始款号是不应该为NULL的
            return new
            {
                Id = x.Id + "",
                BigProdNum = x.BigProdNum,
                ParentId = parid,
                BrandName = "",//modPON.Brand.BrandName,
                CategoryName = "",//modPON.Category.CategoryName,
                SeasonName = "",//modPON.Season.SeasonName,
                ProductNumber = x.ProductNumber,
                SizeName = x.Size == null ? "" : x.Size.SizeName,
                ThumbnailPath = x.ThumbnailPath, //显示子类的第一张图
                ColorName = x.ColorId == null ? "" : _colorContract.Colors.FirstOrDefault(m => m.Id == x.ColorId).ColorName,
                ColorImg = x.ColorId == null ? "" : _colorContract.Colors.Where(m => m.Id == x.ColorId).FirstOrDefault().IconPath,
                TagPrice = modPON.TagPrice,
                IsEnabled = x.IsEnabled,
                ProductCount = "",
            };
        }

        #region 获取仓库
        public JsonResult GetStorages(int storeId)
        {
            var da = _storageContract.Storages.Where(c => !c.IsDeleted && c.IsEnabled && c.StoreId == storeId).Select(c => new { Id = c.Id, Name = c.StorageName, IsDefault = c.IsDefaultStorage });
            return Json(da);
        }

        public ActionResult StorageList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Storage, bool>> predicate = FilterHelper.GetExpression<Storage>(request.FilterGroup);
            var count = 0;

            var list = (from s in _storageContract.Storages.Where<Storage, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.CreatedTime,
                            s.Store.StoreName,
                            s.StorageName,
                            s.StorageAddress,
                            s.CheckLock,
                            s.StorageType,
                            StoreId = s.StoreId,

                        }).ToList();
            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取销售员&会员

        public ActionResult VSellerMember(int storeid)
        {
            return PartialView();
        }

        public ActionResult SellerList(int StoreId)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Administrator, bool>> predicate = FilterHelper.GetExpression<Administrator>(request.FilterGroup);
            var count = 0;

            var queryDepIds = _storeContract.Stores.Where(w => w.IsEnabled && !w.IsDeleted && w.Id == StoreId && w.DepartmentId.HasValue).Select(s => s.DepartmentId.Value);
            var query = _administratorContract.Administrators.Where(w => w.JobPositionId.HasValue && queryDepIds.Contains(w.JobPosition.DepartmentId));

            var list = (from s in query.Where<Administrator, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.Member.RealName,
                            Gender = s.Member.Gender == 0 ? "女" : "男",
                            s.Member.MobilePhone,
                            s.Member.UserPhoto,

                        }).ToList();
            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult MemberList(int StoreId)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Member, bool>> predicate = FilterHelper.GetExpression<Member>(request.FilterGroup);
            var count = 0;

            var list = (from s in _memberContract.Members.Where(w => w.StoreId == StoreId).Where<Member, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.MemberName,
                            s.MobilePhone,
                            s.UserPhoto,
                            Gender = s.Gender == 0 ? "女" : "男",
                            s.MemberType.MemberTypeName,
                            s.RealName,
                            s.CreatedTime,

                        }).ToList();
            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

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
        public ActionResult Create(SaleAutoGenDto dto)
        {
            var result = _SaleAutoGenContract.Insert(sendPopLoadingAction, dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _SaleAutoGenContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<SaleAutoGen, bool>> predicate = FilterHelper.GetExpression<SaleAutoGen>(request.FilterGroup);
            var count = 0;

            var list = (from s in _SaleAutoGenContract.Entities.Where<SaleAutoGen, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.IsDeleted,
                            s.IsEnabled,
                            s.CreatedTime,
                            s.StartTime,
                            s.EndTime,
                            s.AllSaleCount,
                            s.Operator.Member.RealName,
                            s.SendStore.StoreName,
                            s.SendStorage.StorageName,
                            s.Discount,

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
            var result = _SaleAutoGenContract.DeleteOrRecovery(true, Id);
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
            var result = _SaleAutoGenContract.DeleteOrRecovery(false, Id);
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
            var result = _SaleAutoGenContract.EnableOrDisable(true, Id);
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
            var result = _SaleAutoGenContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}

