using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Class;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Factory.Controllers
{
    // GET: Factory/FactoryOnline
    [License(CheckMode.Verify)]
    public class FactoryOnlineController : BaseController
    {
        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(FactoryOnlineController));

        protected readonly IStoreContract _storeContract;

        protected readonly IStorageContract _storageContract;

        protected readonly IStoreCartContract _storeCartContract;

        protected readonly IProductContract _productContract;

        protected readonly IStoreCartItemContract _storeCartItemContract;

        protected readonly IColorContract _colorContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IPurchaseContract _purchaseContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IDesignerContract _DesignerContract;
        protected readonly IFactorysContract _factoryContract;

        public FactoryOnlineController(IStoreContract storeContract,
            IStorageContract storageContract,
            IGalleryContract galleryContract,
            IInventoryContract inventoryContract,
            IStoreCartContract storeCartContract,
            IProductContract prodcutContract,
             IAdministratorContract administratorContract,
            IStoreCartItemContract storeCartItemContract,
            IColorContract colorContract,
            IProductContract productContract,
            IProductOrigNumberContract _productOrigNumberContract,
            IBrandContract _brandContract,
            IPurchaseContract _purchaseContract,
            IDesignerContract _DesignerContract,
            IFactorysContract _factoryContract,
            ICategoryContract categoryContract)
        {
            _storeContract = storeContract;
            _storageContract = storageContract;
            _storeCartContract = storeCartContract;
            _productContract = prodcutContract;
            _storeCartItemContract = storeCartItemContract;
            _colorContract = colorContract;
            _categoryContract = categoryContract;
            this._brandContract = _brandContract;
            this._administratorContract = administratorContract;
            this._productOrigNumberContract = _productOrigNumberContract;
            ViewBag.Color = _colorContract.ParentSelectList("请选择");
            ViewBag.Category = CacheAccess.GetCategory(_categoryContract, true);
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true);
            this._purchaseContract = _purchaseContract;
            this._DesignerContract = _DesignerContract;
            this._factoryContract = _factoryContract;
        }
        #endregion

        [Layout]
        public ActionResult Index()
        {
            ViewBag.Factorys = CacheAccess.GetFactorys(_factoryContract, true);
            return View();
        }

        #region 选货车

        [Layout]
        public ActionResult CartIndex()
        {
            ViewBag.Factorys = CacheAccess.GetFactorys(_factoryContract, true);
            return View();
        }

        public async Task<ActionResult> CartList()
        {
            GridRequest request = new GridRequest(Request);
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Expression<Func<StoreCart, bool>> predicate = FilterHelper.GetExpression<StoreCart>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                int count = 0;
                var query = _storeCartContract.StoreCarts.Where(x => x.IsOrder == false).Where(w => w.OriginFlag == StoreCardOriginFlag.工厂);

                var list = (from s in query.Where<StoreCart, int>(predicate, request.PageCondition, out count)
                            let children = s.StoreCartItems.Where(c => c.IsEnabled && !c.IsDeleted && c.ParentId != null)
                            select new
                            {
                                s.Id,
                                s.StoreCartNum,
                                s.Factory.FactoryName,
                                s.Factory.Brand.BrandName,
                                Count = children.Any() ? children.Sum(s => s.Quantity) : 0,
                                TagPrices = children.Any() ? children.Sum(s => (s.Product.ProductOriginNumber.TagPrice) * s.Quantity) : 0,
                                OriginFlag = s.OriginFlag + "",
                                s.IsDeleted,
                                s.IsEnabled,
                                s.CreatedTime,

                            }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
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
            var OperatorId = AuthorityHelper.OperatorId.Value;
            var data = await Task.Run(() =>
            {
                var listpons = _productOrigNumberContract.OrigNumbs.Where(w => w.IsEnabled && !w.IsDeleted && w.DesignerId.HasValue).Select(s => s.BigProdNum).ToList();

                var list = _productContract.Products.Where(x => listpons.Contains(x.BigProdNum)).DistinctQueryBy(x => x.BigProdNum).Where<Product, int>(predicate, request.PageCondition, out count).ToList().Select(m => new
                {
                    m.Id,
                    m.ProductOriginNumber.Designer.FactoryId,
                    ThumbnailPath = (m.ThumbnailPath.IsNullOrEmpty() ? m.ProductOriginNumber.ThumbnailPath : m.ThumbnailPath),
                    ProductName = m.ProductOriginNumber?.ProductName ?? m.ProductName,
                    m.BigProdNum,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
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
            dtoCart.OriginFlag = StoreCardOriginFlag.工厂;
            OperationResult oper = _storeCartContract.AddCartAuto(AuthorityHelper.OperatorId.Value, dtoCart, infos);
            return Json(oper);
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

        #region 一件下单

        [HttpPost]
        public JsonResult AddPurchaseDirect(int CartId)
        {
            OperationResult oper = _storeCartContract.AddPurchaseDirect(null, CartId);
            return Json(oper);
        }
        #endregion

        #region 初始化选购车界面
        [Layout]
        public ActionResult OrderCartIndex(string CartNum)
        {
            if (CartNum.IsNotNullAndEmpty())
            {
                var mod = _storeCartContract.StoreCarts.FirstOrDefault(f => f.StoreCartNum == CartNum && f.IsEnabled && !f.IsDeleted && f.IsOrder == false);
                if (mod.IsNotNull())
                {
                    ViewBag.CartId = mod.Id;
                }
            }
            return View();
        }
        #endregion

        #region 获取购物车数据
        /// <summary>
        /// 获取购物车数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> OrderCartList(int? CartId)
        {
            GridRequest request = new GridRequest(Request);
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Expression<Func<StoreCartItem, bool>> predicate = FilterHelper.GetExpression<StoreCartItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                StoreCart storeCart = new StoreCart();
                var query = _storeCartContract.StoreCarts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrder == false);
                if (CartId.HasValue)
                {
                    var mod = query.FirstOrDefault(f => f.Id == CartId);
                    if (mod.IsNotNull())
                    {
                        storeCart = mod;
                    }
                }

                int count = 0;
                ICollection<StoreCartItem> listEntity = new List<StoreCartItem>();
                List<string> listNum = new List<string>();
                if (storeCart != null)
                {
                    listEntity = storeCart.StoreCartItems;
                }

                int pageIndex = request.PageCondition.PageIndex;
                int pageSize = request.PageCondition.PageSize;
                List<StoreCartItem> parents = listEntity.Where(m => m.ParentId == null).AsQueryable()
                    .Where<StoreCartItem, int>(predicate, request.PageCondition, out count).ToList();
                List<string> listNumber = parents.Select(x => x.BigProdNum).ToList();
                var listBigProdNumImgs = _productOrigNumberContract.OrigNumbs.Where(x => x.IsDeleted == false && x.IsEnabled == true && listNumber.Any(k => k == x.BigProdNum) == true).Select(s => new { s.BigProdNum, s.ThumbnailPath }).ToList();

                Func<IEnumerable<StoreCartItem>, List<StoreCartItem>> getSoure = null;
                getSoure = (soure) =>
                {
                    var children = soure.OrderByDescending(x => x.CreatedTime);
                    List<StoreCartItem> listStoreCartItem = new List<StoreCartItem>();
                    foreach (var child in children)
                    {
                        listStoreCartItem.Add(child);
                        var childList = child.StoreCartItems;
                        listStoreCartItem.AddRange(getSoure(childList));
                    }
                    return listStoreCartItem;
                };

                var list = (from m in getSoure(parents).AsQueryable().Where(predicate)
                            let bigProdNumAndImg = listBigProdNumImgs.FirstOrDefault(f => f.BigProdNum == m.BigProdNum)
                            let bigProdNumImg = bigProdNumAndImg != null ? bigProdNumAndImg.ThumbnailPath : ""
                            let productImg = m.Product != null ? (m.Product.ThumbnailPath != null && m.Product.ThumbnailPath != "") ? m.Product.ThumbnailPath : bigProdNumImg : bigProdNumImg
                            select new
                            {
                                ParentId = m.ParentId,
                                Id = m.Id.ToString(),
                                m.BigProdNum,
                                ProductNumber = m.ParentId == null ? "" : m.Product.ProductNumber,
                                ProductName = m.Product == null ? "" : m.Product.ProductOriginNumber.ProductName,
                                ThumbnailPath = m.Product == null ? bigProdNumImg : productImg,
                                ColorIconPath = m.ParentId == null ? "" : m.Product.Color.IconPath,
                                SizeName = m.ParentId == null ? "" : m.Product.Size.SizeName,
                                Quantity = m.Quantity,
                                Price = m.Product == null ? "" : (m.Product.ProductOriginNumber == null ? "" : m.Product.ProductOriginNumber.TagPrice.ToString())
                            }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 购物车中所有商品的统计信息,件数款数吊牌价

        public JsonResult OrderCartStaInfo(int? CartId)
        {
            float allPieceCount, AllTagPrice, allBigCount, allBigCountReal;
            allPieceCount = AllTagPrice = allBigCount = allBigCountReal = 0;
            try
            {
                StoreCart storeCart = new StoreCart();
                var query = _storeCartContract.StoreCarts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrder == false);
                if (CartId.HasValue)
                {
                    storeCart = query.FirstOrDefault(f => f.Id == CartId);
                }
                if (storeCart.IsNotNull())
                {
                    var items = storeCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted && w.ParentId == null).SelectMany(s => s.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted)).ToList();
                    allPieceCount = items.Sum(s => s.Quantity);
                    AllTagPrice = items.Sum(s => s.Quantity * s.Product.ProductOriginNumber.TagPrice);
                    allBigCountReal = items.GroupBy(g => g.ParentId).Count();//有商品的总款数
                    allBigCount = storeCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted && w.ParentId == null).Count();//总款数
                }
            }
            catch { }
            var rdata = new
            {
                allPieceCount = allPieceCount,
                AllTagPrice = AllTagPrice,
                allBigCount = allBigCount,
                allBigCountReal = allBigCountReal,
            };

            return Json(new OperationResult(OperationResultType.Success, "", rdata), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 初始化添加同款式界面
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="CartId"></param>
        /// <returns></returns>
        public ActionResult ChoiceIndex(string BigProdNum, int? CartId)
        {
            ViewBag.BigProNum = BigProdNum;
            ViewBag.CartId = CartId;
            return PartialView();
        }

        /// <summary>
        /// 提交购物车变更的数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ChoiceIndex(StoreCartItemDto infos)
        {
            OperationResult oper = _storeCartContract.AddCart(infos.StoreCartId, infos);
            return Json(oper);
        }
        #endregion


        #region 数量改变
        public JsonResult ChangeQuantity(int Id, int Quantity, int? CartId)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            OperationResult oper = new OperationResult(OperationResultType.Error, "更新失败");
            var query = _storeCartContract.StoreCarts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrder == false);
            StoreCart storeCart = null;
            if (CartId.HasValue)
            {
                storeCart = query.FirstOrDefault(f => f.Id == CartId);
            }
            if (storeCart != null)
            {
                StoreCartItem storeCartItem = storeCart.StoreCartItems.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == Id);
                if (storeCartItem != null)
                {
                    storeCartItem.Quantity = Quantity;
                    storeCartItem.UpdatedTime = DateTime.Now;
                    storeCartItem.OperatorId = adminId;
                    oper = _storeCartItemContract.Update(storeCartItem);
                }
            }
            else
            {
                oper.Message = "购物车不存在";
            }
            return Json(oper);
        }
        #endregion
    }
}