using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;

namespace Whiskey.ZeroStore.MobileApi.Areas.Offices.Controllers
{
    // GET: Offices/TempOnline
    [LicenseAdmin]
    public class TempOnlineController : Controller
    {
        protected readonly string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(TempOnlineController));
        protected readonly IStoreCartContract _storeCartContract;
        protected readonly IStoreCartItemContract _storeCartItemContract;
        protected readonly IOnlinePurchaseProductContract _oppContract;
        protected readonly IProductContract _productContract;
        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly IPurchaseContract _purchaseContract;
        protected readonly IStoreContract _storeContract;

        public TempOnlineController(
             IStoreCartContract _storeCartContract
            , IStoreCartItemContract _storeCartItemContract
            , IOnlinePurchaseProductContract _oppContract
            , IProductContract _productContract
            , IProductOrigNumberContract _productOrigNumberContract
            , IPurchaseContract _purchaseContract
            , IStoreContract _storeContract

            )
        {
            this._storeCartContract = _storeCartContract;
            this._storeCartItemContract = _storeCartItemContract;
            this._oppContract = _oppContract;
            this._productContract = _productContract;
            this._productOrigNumberContract = _productOrigNumberContract;
            this._purchaseContract = _purchaseContract;
            this._storeContract = _storeContract;
        }

        #region 添加购物车

        /// <summary>
        /// 创建购物车
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddOrder(int AdminId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                var struser = Request["user"];
                var dtocart = struser.FromJsonString<StoreCartDto>();
                dtocart.OriginFlag = StoreCardOriginFlag.临时;
                return _storeCartContract.AddCartAuto(AdminId, dtocart, null);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "创建选货订单失败", true);
            });
            return Json(data);
        }

        /// <summary>
        /// 添加购物车
        /// </summary>
        /// <param name="AdminId"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AddCart(int AdminId, int CartId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                var strInfos = Request["infos"];
                var infos = strInfos.FromJsonString<StoreCartItemDto[]>();
                return _storeCartContract.AddCart(AdminId, CartId, infos);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "加入选货车失败", true);
            });
            return Json(data);
        }

        #endregion

        #region 修改购物车商品数量

        [HttpPost]
        public async Task<ActionResult> ChangeQuantity(int CartItemId, int CartId, int AdminId, int Quantity)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");
                StoreCart storeCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == CartId && x.IsOrder == false && x.CaptainId == AdminId);
                if (storeCart != null)
                {
                    StoreCartItem storeCartItem = storeCart.StoreCartItems.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == CartItemId);
                    if (storeCartItem != null)
                    {
                        storeCartItem.Quantity = Quantity;
                        storeCartItem.UpdatedTime = DateTime.Now;
                        storeCartItem.OperatorId = AdminId;
                        oper = _storeCartItemContract.Update(storeCartItem);
                    }
                    else
                    {
                        oper.Message = "修改的订单项不存在";
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

        #region 获取可选货商品列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> getOnlineProducts(int? CategoryId, int? ColorId, string BigProdNum = null, int PageIndex = 1, int PageSize = 10)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                DateTime current = DateTime.Now;
                var count = 0;

                //有效期的采购单
                IQueryable<OnlinePurchaseProduct> listOnlinePurchaseProduct = _oppContract.OnlinePurchaseProducts
                    .Where(x => x.StartDate.CompareTo(current) <= 0 && x.EndDate.CompareTo(current) >= 0 && x.IsEnabled && !x.IsDeleted);
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
                    ProductName = m.ProductOriginNumber.ProductName,
                    m.BigProdNum,
                    PurchasePrice = m.ProductOriginNumber.PurchasePrice,
                    TagPrice = m.ProductOriginNumber.TagPrice,
                    SeasonName = m.ProductOriginNumber.Season.SeasonName,
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

        #region 获取选货车中款号列表

        public async Task<ActionResult> getCartList(int AdminId, int CartId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");

                StoreCart storeCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == CartId && x.IsOrder == false && x.CaptainId == AdminId);
                if (storeCart == null)
                {
                    oper.Message = "选货车不存在";
                    return oper;
                }

                var rdata = (from s in storeCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted && w.ParentId == null)
                             let pon = _productOrigNumberContract.OrigNumbs.FirstOrDefault(f => f.BigProdNum == s.BigProdNum)
                             let items = s.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted)
                             select new
                             {
                                 Id = s.Id,
                                 s.BigProdNum,
                                 Count = items.Count(),//货号数量
                                 PieceCount = items.Any() ? items.Sum(ss => ss.Quantity) : 0,//件数
                                 pon.TagPrice,
                                 pon.PurchasePrice,
                                 ThumbnailPath = strWebUrl + pon.ThumbnailPath,
                                 pon.Season.SeasonName,
                             }).ToList();

                return new OperationResult(OperationResultType.Success, "", rdata);
            }, ex =>
            {
                _Logger.Debug<string>(ex.Message);
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取购物车列表失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取选货车中款号下货号列表

        public async Task<ActionResult> getCartInfoList(int AdminId, int CartId, int CartItemId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");
                StoreCart storeCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == CartId && x.IsOrder == false && x.CaptainId == AdminId);
                if (storeCart == null)
                {
                    oper.Message = "选货车不存在";
                    return oper;
                }
                var query = storeCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted && w.ParentId == CartItemId);
                var rdata = (from s in query
                             let p = s.Product
                             select new
                             {
                                 s.Id,
                                 p.ProductNumber,
                                 p.Size.SizeName,
                                 p.Color.ColorName,
                                 ColorImg = strWebUrl + p.Color.IconPath,
                                 s.Quantity,
                                 ThumbnailPath = strWebUrl + (p.ThumbnailPath == null ? p.ProductOriginNumber.ThumbnailPath : p.ThumbnailPath),
                                 p.ProductOriginNumber.TagPrice,
                                 p.ProductOriginNumber.PurchasePrice,
                             }).ToList();

                return new OperationResult(OperationResultType.Success, "", rdata);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取购物车列表失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取负责人下的所有购物车列表

        public async Task<ActionResult> getList(int AdminId, string Phone, int PageIndex = 1, int PageSize = 10)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                int count = 0;
                OperationResult oper = new OperationResult(OperationResultType.Error, "");
                var query = _storeCartContract.StoreCarts.Where(w => w.IsEnabled && !w.IsDeleted && w.CaptainId == AdminId && w.OriginFlag == StoreCardOriginFlag.临时);
                if (Phone.IsNotNullAndEmpty())
                {
                    query = query.Where(w => w.Phone.Contains(Phone));
                }

                count = query.Count();
                double page = (double)count / PageSize;
                int totalPage = (int)Math.Ceiling(page);

                var list = (from s in query
                            let p = _purchaseContract.Purchases.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.StoreCartId == s.Id)
                            let item = s.StoreCartItems.Where(w => w.ParentId == null && w.IsEnabled && !w.IsDeleted).SelectMany(ss => ss.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted))
                            let pitem = p.PurchaseItems.Where(w => w.IsEnabled && !w.IsDeleted)
                            select new
                            {
                                s.Id,
                                s.Phone,
                                s.Name,
                                Count = p != null ? pitem.Any() ? pitem.Sum(ss => ss.Quantity) : 0
                                                  : item.Any() ? item.Sum(ss => ss.Quantity) : 0,
                                s.IsOrder,
                                Status = p != null ? p.PurchaseStatus : -1,
                                PurchaserId = p != null ? p.Id : 0,

                            }).OrderByDescending(o => o.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();

                var rdata = new { total = count, totalPage = totalPage, list = list };

                return new OperationResult(OperationResultType.Success, "", rdata);
            }, ex =>
            {
                _Logger.Error(ex.Message);
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取列表失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _storeCartItemContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 移除选货车所有商品
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RemoveAll(int AdminId, int CartId)
        {
            StoreCart storeCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.CaptainId == AdminId && x.IsOrder == false && x.Id == CartId);
            OperationResult result = new OperationResult(OperationResultType.Error, "选货车不存在");
            if (storeCart != null)
            {
                var listIds = storeCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted).Select(s => s.Id).ToArray();
                if (listIds.IsNotNullOrEmptyThis())
                {
                    result = _storeCartItemContract.Remove(listIds);
                }
                else
                {
                    result.Message = "已清空";
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取选货车详情

        public async Task<ActionResult> getCartInfo(int AdminId, int CartId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");
                var modCart = _storeCartContract.StoreCarts.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.CaptainId == AdminId && w.Id == CartId && w.OriginFlag == StoreCardOriginFlag.临时);

                if (modCart.IsNull())
                {
                    oper.Message = "选货车不存在";
                    return oper;
                }

                var modPur = _purchaseContract.Purchases.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.StoreCartId == modCart.Id);

                var items = modCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted && w.ParentId == null).SelectMany(s => s.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted));

                var rdata = new
                {
                    CartId = modCart.Id,
                    modCart.Phone,
                    modCart.Name,
                    modCart.Address,
                    Quantity = items.Any() ? items.Sum(s => s.Quantity) : 0,
                    TagPrice = items.Any() ? items.Sum(s => s.Product.ProductOriginNumber.TagPrice * s.Quantity) : 0,
                    PurchasePrice = items.Any() ? items.Sum(s => s.Product.ProductOriginNumber.PurchasePrice * s.Quantity) : 0,
                    DespoitPrice = modPur?.DespoitPrice ?? 0,
                    Status = modPur?.PurchaseStatus ?? -1,
                    Discount = modPur?.Discount ?? 1
                };

                return new OperationResult(OperationResultType.Success, "", rdata);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取选货车详情失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 一键下单

        /// <summary>
        /// 直接下单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddPurchase(int CartId)
        {
            OperationResult oper = _storeCartContract.AddPurchaseDirect(null, CartId);
            return Json(oper);
        }

        #endregion

        #region 获取店铺仓库列表

        public JsonResult getStore(int AdminId)
        {
            var rdata = OperationHelper.Try(() =>
            {
                var storeids = _storeContract.QueryManageStoreId(AdminId);
                var list = _storeContract.Stores.Where(w => storeids.Contains(w.Id)).Select(s => new
                {
                    s.Id,
                    s.StoreName,
                    s.Balance,
                    s.StoreType.TypeName,
                    s.StoreType.IsPay,
                }).ToList();
                return new OperationResult(OperationResultType.Success, "", list);
            }, ex =>
            {
                _Logger.Error(ex.Message);
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取列表失败", true);
            });
            return Json(rdata, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getStorage(int AdminId, int StoreId)
        {
            var rdata = OperationHelper.Try(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");
                var storeids = _storeContract.QueryManageStoreId(AdminId);
                var modStore = _storeContract.Stores.FirstOrDefault(w => storeids.Contains(w.Id) && w.Id == StoreId);
                if (modStore.IsNull())
                {
                    oper.Message = "没有此店铺的管理权"; return oper;
                }

                var list = modStore.Storages.Where(w => w.IsEnabled && !w.IsDeleted).Select(s => new
                {
                    s.Id,
                    s.StorageName
                }).ToList();

                return new OperationResult(OperationResultType.Success, "", list);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取列表失败", true);
            });
            return Json(rdata, JsonRequestBehavior.AllowGet);
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
                var query = _productContract.Products.Where(x => x.BigProdNum == BigProdNum && x.IsEnabled && !x.IsDeleted).ToList();
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
                                    ColorImg = strWebUrl + s.Color.IconPath,
                                    ThumbnailPath = strWebUrl + (s.ProductCollocationImg.IsNotNullAndEmpty() ? s.ProductCollocationImg : s.ProductOriginNumber.ProductCollocationImg),
                                    Sizes = gx.Where(w => w.ColorId == s.ColorId).DistinctBy(xx => xx.SizeId).Select(ss => new
                                    {
                                        ss.SizeId,
                                        ss.Size.SizeName
                                    }),
                                })
                            }).FirstOrDefault();

                return OperationHelper.ReturnOperationResult(query.Any(), opera, list);

            }, "获取颜色尺码");

            return Json(data);
        }

        #endregion

        #region 采购单支付

        [HttpPost]
        public JsonResult Payment(int PurchaserId, float Discount, int ReceiptStoreId, int ReceiptStorageId, PaymentPurchaseType DiscountType)
        {
            var data = _purchaseContract.Payment(PurchaserId, ReceiptStorageId, ReceiptStoreId, DiscountType, Discount);
            return Json(data);
        }

        #endregion


        #region 采购单相关

        #region 获取采购单中款号列表

        public async Task<ActionResult> getPurchaseList(int AdminId, int PurchaseId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");

                var modpur = _purchaseContract.Purchases.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Id == PurchaseId && f.OriginFlag == StoreCardOriginFlag.临时);
                if (modpur.IsNull())
                {
                    oper.Message = "采购单不存在";
                    return oper;
                }
                else
                {
                    var rdata = (from s in modpur.PurchaseItems.Where(w => w.IsEnabled && !w.IsDeleted)
                                 group s by s.Product.BigProdNum into g
                                 let pon = g.FirstOrDefault().Product.ProductOriginNumber//BigProdNum相同ProductOriginNumber肯定相同
                                 select new
                                 {
                                     BigProdNum = g.Key,
                                     Count = g.Count(),
                                     PieceCount = g.Sum(ss => ss.Quantity),
                                     pon.TagPrice,
                                     pon.PurchasePrice,
                                     ThumbnailPath = strWebUrl + pon.ThumbnailPath,
                                     pon.Season.SeasonName,
                                     IsNewAdded = g.All(a => a.IsNewAdded),

                                 }).ToList();

                    return new OperationResult(OperationResultType.Success, "", rdata);
                }
            }, ex =>
            {
                _Logger.Debug<string>(ex.Message);
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取采购单列表失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取采购单中款号下货号列表

        public async Task<ActionResult> getPurchaseInfoList(int AdminId, int PurchaseId, string BigProdNum)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");
                var modpur = _purchaseContract.Purchases.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Id == PurchaseId && f.OriginFlag == StoreCardOriginFlag.临时);
                if (modpur.IsNull())
                {
                    oper.Message = "采购单不存在";
                    return oper;
                }
                else
                {
                    var rdata = (from s in modpur.PurchaseItems.Where(w => w.IsEnabled && !w.IsDeleted && w.Product.BigProdNum == BigProdNum)
                                 let p = s.Product
                                 select new
                                 {
                                     p.ProductNumber,
                                     p.Size.SizeName,
                                     p.Color.ColorName,
                                     ColorImg = strWebUrl + p.Color.IconPath,
                                     s.Quantity,
                                     QuantityReal = s.PurchaseItemProducts.Where(ww => ww.IsEnabled && !ww.IsDeleted).Count(),//真实配的数量
                                     ThumbnailPath = strWebUrl + (p.ThumbnailPath == null ? p.ProductOriginNumber.ThumbnailPath : p.ThumbnailPath),
                                     s.IsNewAdded,
                                     p.ProductOriginNumber.TagPrice,
                                     p.ProductOriginNumber.PurchasePrice,
                                 }).ToList();

                    return new OperationResult(OperationResultType.Success, "", rdata);
                }
            }, ex =>
            {
                _Logger.Debug<string>(ex.Message);
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取采购单列表失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取采购单中最终实配的货号列表

        public async Task<ActionResult> getPurchaseRealList(int AdminId, int PurchaseId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");
                var modpur = _purchaseContract.Purchases.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Id == PurchaseId && f.OriginFlag == StoreCardOriginFlag.临时);
                if (modpur.IsNull())
                {
                    oper.Message = "采购单不存在";
                    return oper;
                }
                else
                {
                    var rdata = (from s in modpur.PurchaseItems.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => w.PurchaseItemProducts.Where(ww => ww.IsEnabled && !ww.IsDeleted).Any())
                                 let p = s.Product
                                 select new
                                 {
                                     p.ProductNumber,
                                     p.Size.SizeName,
                                     p.Color.ColorName,
                                     ColorImg = strWebUrl + p.Color.IconPath,
                                     s.Quantity,
                                     QuantityReal = s.PurchaseItemProducts.Where(ww => ww.IsEnabled && !ww.IsDeleted).Count(),//真实配的数量
                                     ThumbnailPath = strWebUrl + (p.ThumbnailPath == null ? p.ProductOriginNumber.ThumbnailPath : p.ThumbnailPath),
                                     s.IsNewAdded,
                                     p.ProductOriginNumber.TagPrice,
                                     p.ProductOriginNumber.PurchasePrice,
                                 }).ToList();

                    return new OperationResult(OperationResultType.Success, "", rdata);
                }
            }, ex =>
            {
                _Logger.Debug<string>(ex.Message);
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取采购单列表失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取采购单详情

        public async Task<ActionResult> getPurchaseInfo(int AdminId, int PurchaseId)
        {
            var data = await OperationHelper.TryAsync(() =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "");

                var modpur = _purchaseContract.Purchases.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Id == PurchaseId && f.OriginFlag == StoreCardOriginFlag.临时);
                if (modpur.IsNull())
                {
                    oper.Message = "采购单不存在";
                    return oper;
                }

                var item = modpur.PurchaseItems.Where(w => w.IsEnabled && !w.IsDeleted).ToList();
                var itempro = item.SelectMany(s => s.PurchaseItemProducts.Where(w => w.IsEnabled && !w.IsDeleted)).ToList();
                var itemSto = modpur.StoreCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted && w.ParentId == null).ToList();

                var rdata = new
                {
                    Phone = modpur.StoreCart.Phone,
                    Name = modpur.StoreCart.Name,
                    Address = modpur.StoreCart.Address,
                    PurchasePrice = itempro.Any() ? itempro.Sum(s => s.PurchaseItem.Product.ProductOriginNumber.PurchasePrice) : 0,
                    TagPrice = itempro.Any() ? itempro.Sum(s => s.PurchaseItem.Product.ProductOriginNumber.TagPrice) : 0,

                    U_Count = itemSto.Any() ? itemSto.Count() : 0,//用户选择款数
                    U_PieceCount = itemSto.Any() ? itemSto.SelectMany(s => s.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted)).Sum(s => s.Quantity) : 0,//用户选择件数
                    Count = item.Any() ? item.GroupBy(w => w.Product.BigProdNum).Count() : 0,//应配款数
                    PieceCount = item.Any() ? item.Sum(s => s.Quantity) : 0,//应配件数
                    CountReal = itempro.Any() ? itempro.GroupBy(s => s.PurchaseItem.Product.BigProdNum).Count() : 0,//实配款数
                    PieceCountReal = itempro.Any() ? itempro.Count() : 0,//实配件数

                    DespoitPrice = modpur.DespoitPrice,
                    Status = modpur.PurchaseStatus,
                    Discount = modpur.Discount
                };

                return new OperationResult(OperationResultType.Success, "", rdata);
            }, ex =>
            {
                _Logger.Error(ex.Message);
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取采购单详情失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #endregion
    }
}