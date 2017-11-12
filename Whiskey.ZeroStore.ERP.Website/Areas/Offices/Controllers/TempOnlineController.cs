using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Utility.Helper;
using System.Web.Security;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Stores;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class TempOnlineController : Controller
    {
        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(TempOnlineController));

        protected readonly IStoreContract _storeContract;

        protected readonly IStorageContract _storageContract;

        protected readonly IStoreCartContract _storeCartContract;

        protected readonly IProductContract _productContract;

        protected readonly IStoreCartItemContract _storeCartItemContract;

        protected readonly IColorContract _colorContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly IOnlinePurchaseProductContract _oppContract;

        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IStoreTypeContract _storeTypeContract;
        protected readonly IPurchaseContract _purchaseContract;
        protected readonly IBrandContract _brandContract;

        public TempOnlineController(IStoreContract storeContract,
            IStorageContract storageContract,
            IGalleryContract galleryContract,
            IInventoryContract inventoryContract,
            IStoreCartContract storeCartContract,
            IProductContract prodcutContract,
             IAdministratorContract administratorContract,
            IStoreCartItemContract storeCartItemContract,
            IColorContract colorContract,
            IOnlinePurchaseProductContract oppContract,
            IProductContract productContract,
            IProductOrigNumberContract _productOrigNumberContract,
            IStoreTypeContract _storeTypeContract,
            IBrandContract _brandContract,
            IPurchaseContract _purchaseContract,
            ICategoryContract categoryContract)
        {
            _storeContract = storeContract;
            _storageContract = storageContract;
            _storeCartContract = storeCartContract;
            _productContract = prodcutContract;
            _storeCartItemContract = storeCartItemContract;
            _colorContract = colorContract;
            _categoryContract = categoryContract;
            _oppContract = oppContract;
            this._brandContract = _brandContract;
            this._administratorContract = administratorContract;
            this._productOrigNumberContract = _productOrigNumberContract;
            ViewBag.Color = _colorContract.ParentSelectList("请选择");
            ViewBag.Category = CacheAccess.GetCategory(_categoryContract, true);
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true);
            this._storeTypeContract = _storeTypeContract;
            this._purchaseContract = _purchaseContract;
        }
        #endregion

        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Product, bool>> predicate = FilterHelper.GetExpression<Product>(request.FilterGroup);
            var count = 0;

            var data = await Task.Run(() =>
            {
                string strWeb = ConfigurationHelper.GetAppSetting("WebUrl");

                DateTime current = DateTime.Now;
                //有效期的采购单
                IQueryable<OnlinePurchaseProduct> listOnlinePurchaseProduct = _oppContract.OnlinePurchaseProducts.Where(w => w.IsEnabled && !w.IsDeleted)
                    .Where(x => x.StartDate.CompareTo(current) <= 0 && x.EndDate.CompareTo(current) >= 0);
                //有效期采购单下的商品
                IQueryable<OnlinePurchaseProductItem> listItems = _oppContract.OnlinePurchaseProductItems
                    .Where(x => x.IsDeleted == false && x.IsEnabled == true && listOnlinePurchaseProduct.Any(k => k.Id == x.OnlinePurchaseProductId) == true);

                var list = _productContract.Products.Where(x => listItems.Any(k => k.BigProdNum == x.BigProdNum) == true).DistinctQueryBy(x => x.BigProdNum).Where<Product, int>(predicate, request.PageCondition, out count).ToList().Select(m => new
                {
                    m.Id,
                    ThumbnailPath = (m.ThumbnailPath.IsNullOrEmpty() ? m.ProductOriginNumber.ThumbnailPath : m.ThumbnailPath),
                    ProductName = m.ProductOriginNumber?.ProductName ?? m.ProductName,
                    m.BigProdNum,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取大货号下的其他颜色和尺码
        /// <summary>
        /// 获取大货号下的其他颜色和尺码
        /// </summary>
        /// <param name="BigProdNum"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetColorAndSize(string BigProdNum)
        {
            var data = OperationHelper.Try((opera) =>
            {
                var query = _productContract.Products.Where(x => x.BigProdNum == BigProdNum).ToList();
                var list = (from x in query
                            group x by new { x.ProductOriginNumber.TagPrice, x.ProductOriginNumber.PurchasePrice } into gx
                            select new
                            {
                                gx.Key.TagPrice,
                                gx.Key.PurchasePrice,
                                list = gx.DistinctBy(b => b.ColorId).Select(s => new
                                {
                                    s.ColorId,
                                    s.Color.ColorName,
                                    ColorImg = s.Color.IconPath,
                                    ThumbnailPath = s.ThumbnailPath.IsNotNullAndEmpty() ? s.ThumbnailPath : s.ProductOriginNumber.ThumbnailPath,
                                    Sizes = gx.Where(w => w.ColorId == s.ColorId).DistinctBy(xx => xx.SizeId).Select(ss => new
                                    {
                                        ss.SizeId,
                                        ss.Size.SizeName
                                    }),
                                })
                            }).FirstOrDefault();

                return OperationHelper.ReturnOperationResult(true, opera, list);

            }, "获取颜色尺码");

            return Json(data);
        }

        [HttpPost]
        public JsonResult GetColor(string BigProdNum)
        {
            IQueryable<Product> listProduct = _productContract.Products.Where(x => x.BigProdNum == BigProdNum);
            var listColor = listProduct.DistinctBy(x => x.ColorId).Select(x => new
            {
                x.ColorId,
                x.Color.ColorName,
                x.Color.IconPath,
            }).ToList();

            return Json(new { Colors = listColor });
        }

        #endregion

        #region 加入采购车

        /// <summary>
        /// 加入采购车
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddCart(StoreCartDto dtoCart, params StoreCartItemDto[] infos)
        {
            OperationResult oper = _storeCartContract.AddCartAuto(AuthorityHelper.OperatorId.Value, dtoCart, infos);
            return Json(oper);
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

            var result = _storeCartItemContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 移除全部数据
        public JsonResult RemoveAll()
        {
            OperationResult oper = _storeCartContract.RemoveAll();
            return Json(oper);
        }
        #endregion

        #region 获取尺码
        [HttpPost]
        public JsonResult GetSizes(string BigProdNum, int ColorId)
        {
            var listSize = _productContract.Products.Where(x => x.BigProdNum == BigProdNum && x.ColorId == ColorId)
                .DistinctBy(x => x.SizeId).Select(x => new
                {
                    x.SizeId,
                    x.Size.SizeName
                }).ToList();
            return Json(listSize);
        }
        #endregion

        #region 根据手机号获取之前添加的姓名地址

        public JsonResult getCartInfoByPhone(string phone)
        {
            var modCart = _storeCartContract.StoreCarts.FirstOrDefault(f => f.OriginFlag == StoreCardOriginFlag.临时 && !f.IsOrder && f.Phone == phone && f.IsEnabled && !f.IsDeleted);
            var data = new
            {
                Name = modCart?.Name ?? "",
                Address = modCart?.Address ?? "",
            };

            return Json(data);
        }

        #endregion
    }
}