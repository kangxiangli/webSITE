using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Web.Helper;

// GET: Stores/Online
namespace Whiskey.ZeroStore.MobileApi.Areas.Stores.Controllers
{
    public class OnlineController : Controller
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(OnlineController));
        protected readonly IOnlinePurchaseProductContract _oppContract;
        protected readonly IColorContract _colorContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IProductContract _productContract;
        protected readonly IStoreCartContract _storeCartContract;
        protected readonly IStoreCartItemContract _storeCartItemContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly IPurchaseContract _purchaseContract;
        protected readonly IStorageContract _storageContract;

        protected readonly string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        public OnlineController(
            IOnlinePurchaseProductContract _oppContract
            , IColorContract _colorContract
            , IStoreContract _storeContract
            , IProductContract _productContract
            , IStoreCartContract _storeCartContract
            , IStoreCartItemContract _storeCartItemContract
            , IAdministratorContract _administratorContract
            , IProductOrigNumberContract _productOrigNumberContract
            , IPurchaseContract _purchaseContract
            , IStorageContract _storageContract
            )
        {
            this._oppContract = _oppContract;
            this._colorContract = _colorContract;
            this._storeContract = _storeContract;
            this._productContract = _productContract;
            this._storeCartContract = _storeCartContract;
            this._storeCartItemContract = _storeCartItemContract;
            this._administratorContract = _administratorContract;
            this._productOrigNumberContract = _productOrigNumberContract;
            this._purchaseContract = _purchaseContract;
            this._storageContract = _storageContract;
        }

        #region 获取在线可选购的商品

        /// <summary>
        /// 获取在线可选购的商品
        /// </summary>
        /// <param name="CategoryId"></param>
        /// <param name="ColorId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public async Task<ActionResult> getOnlineProducts(int? CategoryId, int? ColorId, string BigProdNum = null,  int PageIndex = 1, int PageSize = 10)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                DateTime current = DateTime.Now;
                var count = 0;

                //有效期的采购单
                IQueryable<OnlinePurchaseProduct> listOnlinePurchaseProduct = _oppContract.OnlinePurchaseProducts.Where(w => w.IsEnabled && !w.IsDeleted)
                    .Where(x => x.StartDate.CompareTo(current) <= 0 && x.EndDate.CompareTo(current) >= 0);
                //有效期采购单下的商品
                IQueryable<OnlinePurchaseProductItem> listItems = _oppContract.OnlinePurchaseProductItems
                    .Where(x => x.IsDeleted == false && x.IsEnabled == true && listOnlinePurchaseProduct.Any(k => k.Id == x.OnlinePurchaseProductId) == true);

                var query = _productContract.Products.Where(x => x.IsDeleted == false && x.IsEnabled == true)
                    .Where(x => listItems.Any(k => k.BigProdNum == x.BigProdNum) == true).DistinctBy(x => x.BigProdNum);
                if (CategoryId.HasValue)
                {
                    query = query.Where(w => w.ProductOriginNumber.CategoryId == CategoryId || w.ProductOriginNumber.Category.ParentId == CategoryId);
                }
                if (ColorId.HasValue)
                {
                    query = query.Where(w => w.ColorId == ColorId);
                }
                if (BigProdNum.IsNotNullAndEmpty())
                {
                    query = query.Where(w => w.BigProdNum == BigProdNum);
                }

                count = query.Count();
                double page = (double)count / PageSize;
                int totalPage = (int)Math.Ceiling(page);
                query = query.OrderByDescending(x => x.CreatedTime).Skip((PageIndex - 1) * PageSize).Take(PageSize);

                List<Product> listProduct = query.ToList();
                var list = listProduct.Select(m => new
                {
                    m.Id,
                    ThumbnailPath = strWebUrl + (m.ThumbnailPath.IsNullOrEmpty() ? m.ProductOriginNumber.ThumbnailPath : m.ThumbnailPath),
                    ProductName = m.ProductOriginNumber != null ? m.ProductOriginNumber.ProductName : m.ProductName,
                    m.BigProdNum,
                    PurchasePrice = m.ProductOriginNumber != null ? m.ProductOriginNumber.PurchasePrice : 0
                }).ToList();

                var rdata = new { total = count, totaPage = totalPage, result = list };

                OperationResult oResult = new OperationResult(OperationResultType.Success, "", rdata);

                return oResult;
            }, (ex) =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取在线商品失败", true);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取款号对应的尺码和颜色及搭配图

        /// <summary>
        /// 获取款号对应的尺码和颜色及搭配图
        /// </summary>
        /// <param name="BigProdNum"></param>
        /// <returns></returns>
        public async Task<ActionResult> getColorAndSize(string BigProdNum)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                IQueryable<Product> listProduct = _productContract.Products.Where(x => x.BigProdNum == BigProdNum);
                var listColor = listProduct.DistinctBy(x => x.ColorId).Select(x => new
                {
                    x.ColorId,
                    x.Color.ColorName,
                    ColorImg = strWebUrl + x.Color.IconPath,
                    CollocationImg = strWebUrl + (x.ProductCollocationImg.IsNullOrEmpty() ? x.ProductOriginNumber?.ProductCollocationImg ?? "" : x.ProductCollocationImg),
                    Sizes = listProduct.Where(w => w.ColorId == x.ColorId).DistinctBy(xx => xx.SizeId).Select(s => new
                    {
                        s.SizeId,
                        s.Size.SizeName
                    }),
                    x.ProductOriginNumber.TagPrice,
                    x.ProductOriginNumber.PurchasePrice,
                }).ToList();

                #region 细节图

                //Product product = listProduct.Where(x => x.ProductImages.Any()).FirstOrDefault();
                //List<string> list = new List<string>();
                //if (product != null)
                //{
                //    if (product.ProductImages.IsNullOrEmptyThis())
                //    {
                //        list = product.ProductOriginNumber.ProductImages.Select(s => s.ThumbnailSmallPath).ToList();
                //    }
                //    else
                //    {
                //        if (product.ProductImages.Any())
                //        {
                //            list = product.ProductImages.Select(x => x.ThumbnailSmallPath).ToList();
                //        }
                //        else
                //        {
                //            list.Add(product.ProductOriginNumber.ThumbnailPath);
                //        }
                //    }
                //}
                //else
                //{
                //    product = listProduct.FirstOrDefault(x => x.ProductOriginNumber != null && !string.IsNullOrEmpty(x.ProductOriginNumber.ThumbnailPath));
                //    if (product != null)
                //    {
                //        list.Add(product.ProductOriginNumber.ThumbnailPath);
                //    }
                //}

                #endregion

                var rdata = new { Colors = listColor };

                OperationResult oResult = new OperationResult(OperationResultType.Success, "", rdata);
                return oResult;
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取尺码颜色失败", true);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 添加购物车

        /// <summary>
        /// 添加购物车
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddMyCart(int AdminId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                var strInfos = Request["infos"];
                var infos = strInfos.FromJsonString<StoreCartItemDto[]>();
                return _storeCartContract.AddCartAuto(AdminId, new StoreCartDto { OriginFlag = ERP.Models.Enums.StoreCardOriginFlag.个人 }, infos);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "添加购物车失败", true);
            });
            return Json(data);
        }

        #endregion

        #region 修改购物车商品数量

        public async Task<ActionResult> ChangeQuantity(int Id,int AdminId, int Quantity)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");
                StoreCart storeCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaserId == AdminId && x.IsOrder == false);
                if (storeCart != null)
                {
                    StoreCartItem storeCartItem = storeCart.StoreCartItems.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == Id);
                    if (storeCartItem != null)
                    {
                        storeCartItem.Quantity = Quantity;
                        storeCartItem.UpdatedTime = DateTime.Now;
                        storeCartItem.OperatorId = AdminId;
                        oper = _storeCartItemContract.Update(storeCartItem);
                    }
                }
                else
                {
                    oper.Message = "订单不存在";
                }
                return oper;
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "更新失败", true);
            });
            return Json(data);
        }

        #endregion

        #region 一键下单

        public async Task<ActionResult> AddPurchase(int AdminId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error);

                var modAdmin = _administratorContract.View(AdminId);
                var storeId = modAdmin?.Department?.Stores?.FirstOrDefault()?.Id ?? 0;

                var storeAndStorage = PermissionHelper.ManagedStorages(AdminId, _administratorContract, s => new
                {
                    s.Id,
                    s.StoreId,
                    s.Store.StoreType.IsPay,//0为直营店
                    s.IsDefaultStorage,
                    s.IsDeleted,
                    s.IsEnabled
                }).Where(w => w.IsDefaultStorage && w.StoreId == storeId && !w.IsDeleted && w.IsEnabled).FirstOrDefault();

                if (storeAndStorage.IsNotNull())
                {
                    oper = _storeCartContract.AddPurchase(AdminId, storeAndStorage.Id, storeAndStorage.StoreId, !storeAndStorage.IsPay);
                }
                else
                {
                    oper.Message = "请提升店铺管理权并设置默认仓库";
                }
                return oper;
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "下单失败", true);
            });
            return Json(data);
        }

        #endregion

        #region 获取购物车中的商品

        /// <summary>
        /// 获取购物车中的商品
        /// </summary>
        /// <param name="AdminId"></param>
        /// <returns></returns>
        public async Task<ActionResult> getCartList(int AdminId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                StoreCart storeCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaserId == AdminId && x.IsOrder == false);
                ICollection<StoreCartItem> listEntity = new List<StoreCartItem>();
                List<string> listNum = new List<string>();
                if (storeCart != null)
                {
                    listEntity = storeCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted).ToList();
                }

                List<StoreCartItem> parents = listEntity.Where(m => m.ParentId == null).ToList();
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
                        var childList = child.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted);
                        listStoreCartItem.AddRange(getSoure(childList));
                    }
                    return listStoreCartItem;
                };

                var rdata = (from m in getSoure(parents).AsQueryable()
                             let bigProdNumAndImg = listBigProdNumImgs.FirstOrDefault(f => f.BigProdNum == m.BigProdNum)
                             let bigProdNumImg = bigProdNumAndImg != null ? bigProdNumAndImg.ThumbnailPath : ""
                             let productImg = m.Product != null ? (m.Product.ThumbnailPath != null && m.Product.ThumbnailPath != "") ? m.Product.ThumbnailPath : bigProdNumImg : bigProdNumImg
                             select new
                             {
                                 ParentId = m.ParentId,
                                 Id = m.Id.ToString(),
                                 m.BigProdNum,
                                 ProductNumber = m.ParentId == null ? "" : m.Product.ProductNumber,
                                 ThumbnailPath = strWebUrl + (m.Product == null ? bigProdNumImg : productImg),
                                 ColorIconPath = m.ParentId == null ? "" : m.Product.Color.IconPath,
                                 SizeName = m.ParentId == null ? "" : m.Product.Size.SizeName,
                                 Quantity = m.Quantity,
                                 Price = m.Product == null ? "" : (m.Product.ProductOriginNumber == null ? "" : m.Product.ProductOriginNumber.PurchasePrice.ToString())
                             }).ToList();

                return new OperationResult(OperationResultType.Success, "", rdata);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取购物车列表失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取当前购物车中的款数
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> getCarBigProdNums(int AdminId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult result = new OperationResult(OperationResultType.QueryNull);
                StoreCart storeCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaserId == AdminId && x.IsOrder == false);
                if (storeCart.IsNull())
                {
                    result.Message = "订单不存在";
                }
                else
                {
                    result.ResultType = OperationResultType.Success;

                    var bigprodnum = storeCart.StoreCartItems.Where(w => w.ParentId == null && w.IsEnabled && !w.IsDeleted).DistinctBy(d => d.BigProdNum);

                    var bigprodNumCount = bigprodnum.Count();//款数
                    var pieceCount = bigprodnum.SelectMany(s => s.StoreCartItems).Where(c => c.IsEnabled && !c.IsDeleted).Sum(s => s.Quantity);
                    result.Data = new
                    {
                        StyleCount = bigprodNumCount,
                        PieceCount = pieceCount
                    };
                }
                return result;
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "服务器正忙，请稍候再试", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取订单列表

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> getPurchaseList(int AdminId,int StoreId, int PageIndex = 1, int PageSize = 10)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                var query = _purchaseContract.Purchases.Where(c => c.IsDeleted == false && c.IsEnabled == true).Where(w => w.ReceiverId == StoreId);
                var list = from m in query
                           let SendStorage = _storageContract.Storages.Where(c => c.Id == m.StorageId).FirstOrDefault()//发货仓库
                           let ReceiverStorage = _storageContract.Storages.Where(c => c.Id == m.ReceiverStorageId).FirstOrDefault()//进货仓库
                           let ReceiverStore = _storeContract.Stores.Where(c => c.Id == m.ReceiverId).FirstOrDefault()
                           let OrderBlankNumber = m.Orderblanks.Where(x => x.IsEnabled && !x.IsDeleted).OrderByDescending(s => s.CreatedTime).FirstOrDefault()
                           let PurchaseItemAllPrice = m.PurchaseItems.Sum(s => s.PurchasePrice)
                           orderby m.CreatedTime descending
                           select new
                           {
                               m.PurchaseNumber,
                               StyleCount = m.PurchaseItems.Count,
                               PieceCount = m.PurchaseItems.Sum(s=>s.Quantity),
                               SendStorageName = SendStorage.StorageName,
                               ReceiverStoreId = StoreId,
                               ReceiverStoreName = ReceiverStore.StoreName,
                               ReceiverStorageName = ReceiverStorage.StorageName,
                               m.PurchaseStatus,
                               m.Notes,
                               m.Id,
                               m.CreatedTime,
                               OrderBlankNumber = OrderBlankNumber.OrderBlankNumber ?? "",
                               PurchasePrice = m.OrgPrice,
                               PurchaseDespoitPrice = m.DespoitPrice,
                           };

                var count = list.Count();
                double page = count / PageSize;
                int totalPage = (int)Math.Ceiling(page);
                list = list.OrderByDescending(o => o.CreatedTime).Skip(PageIndex - 1).Take(PageSize);

                var rdata = new { total = count, totalPage = totalPage, result = list };
                return new OperationResult(OperationResultType.Success, "", rdata);

            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取订单列表失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取店铺余额

        /// <summary>
        /// 获取店铺余额
        /// </summary>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        public async Task<ActionResult> getStoreBalance(int StoreId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                var modStore = _storeContract.View(StoreId);

                var rdata = new
                {
                    Balance = modStore.Balance,
                    IsPay = modStore.StoreType.IsPay,
                    Discount = modStore.StoreDiscount,
                    //Discount = _storeCartContract.GetStoreDepositDiscount(modStore.Id),
                };

                return new OperationResult(OperationResultType.Success, "", rdata);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取店铺余额失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 购物车应付和实付价格

        public async Task<ActionResult> getOrderPrice(int AdminId,int StoreId)
        {
            var data = await OperationHelper.TryAsync((opera) =>
            {

                StoreCart storeCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaserId == AdminId && x.IsOrder == false);
                var listitems = storeCart?.StoreCartItems.Where(w => w.ParentId != null && w.IsEnabled && !w.IsDeleted && w.Parent.IsEnabled && !w.Parent.IsDeleted).Select(s => new { s.Quantity, s.Product.ProductOriginNumber.PurchasePrice }).ToList();

                var moStore = _storeContract.View(StoreId);
                //var discount = _storeCartContract.GetStoreDepositDiscount(StoreId);
                var discount = moStore.StoreDiscount;
                var allprice = listitems.Sum(s => s.PurchasePrice * s.Quantity);

                var rdata = new
                {
                    AllPrice = allprice,
                    DepositPrice = allprice * discount,
                    Discount = discount
                };

                return new OperationResult(OperationResultType.Success, "", rdata);

            }, "获取购物车价格");

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 移除购物车中商品

        [HttpPost]
        public ActionResult RemoveCartProduct(int AdminId,int[] Id, bool isAll = false)
        {
            OperationResult result = new OperationResult(OperationResultType.Error);
            if (isAll)
            {
                result = _storeCartContract.RemoveAll(AdminId);
            }
            else
            {
                result = _storeCartItemContract.Remove(Id);
            }
            return Json(result);
        }

        /// <summary>
        /// 移除购物车中某一个款号及款号下所有选购的商品
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="BigProdNum"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RemoveCarBigProdNum(int AdminId,string BigProdNum)
        {
            OperationResult result = new OperationResult(OperationResultType.Error);
            if (BigProdNum.IsNullOrEmpty())
            {
                result.Message = "参数无效";
            }
            else
            {
                StoreCart storeCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaserId == AdminId && x.IsOrder == false);
                if (storeCart.IsNull())
                {
                    result.Message = "订单不存在";
                }
                else
                {
                    var data = (from p in storeCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted && w.BigProdNum == BigProdNum && w.ParentId == null)
                                let c = p.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted && w.ParentId == p.Id)
                                select new
                                {
                                    p.Id,
                                    childrens = c.Select(s => s.Id)
                                }).ToList();
                    if (data.IsNotNullOrEmptyThis())
                    {
                        int[] ids = data.Select(s => s.Id).Union(data.SelectMany(s => s.childrens)).ToArray();
                        result = _storeCartItemContract.Remove(ids);
                    }
                    else {
                        result.Message = "款号不存在";
                    }

                }
            }
            return Json(result);
        }

        #endregion
    }
}