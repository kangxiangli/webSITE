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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    public class OnlineController : Controller
    {
        #region 初始化操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(OnlineController));

        protected readonly IStoreContract _storeContract;

        protected readonly IStorageContract _storageContract;

        //protected readonly IGalleryContract _galleryContract;

        //protected readonly IInventoryContract _inventoryContract;

        protected readonly IStoreCartContract _storeCartContract;

        protected readonly IProductContract _productContract;

        protected readonly IStoreCartItemContract _storeCartItemContract;

        protected readonly IColorContract _colorContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly IOnlinePurchaseProductContract _oppContract;

        protected readonly IProductOrigNumberContract _productOrigNumberContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IStoreTypeContract _storeTypeContract;
        protected readonly IBrandContract _brandContract;

        public OnlineController(IStoreContract storeContract,
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
            ICategoryContract categoryContract)
        {
            _storeContract = storeContract;
            _storageContract = storageContract;
            //_galleryContract = galleryContract;
            //_inventoryContract = inventoryContract;
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
        }
        #endregion

        /// <summary>
        /// 定一个常量作为cookie的键
        /// </summary>

        internal readonly string OrderKey = "773359240eb9a1d9";

        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            //重新加载就要清除存在cookie中的打包数据            
            HttpCookie cookie = Request.Cookies[OrderKey];
            RemoveCookie(cookie);
            return View();
        }
        #endregion

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List(BigProdNumStateEnum? state)
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Product, bool>> predicate = FilterHelper.GetExpression<Product>(request.FilterGroup);
            var count = 0;

            var data = await Task.Run(() =>
            {
                var strWeb = ConfigurationHelper.GetAppSetting("WebUrl");
                var config = _oppContract.GetConfig();
                var current = DateTime.Now;

                var bigProdNumStatDic = _oppContract.GetOnlinePurchaseBigProdNumState();
                // 有效期采购单下的款号
                var query = _oppContract.OnlinePurchaseProducts
                                                            .Where(w => w.IsEnabled && !w.IsDeleted)
                                                            .Where(x => x.StartDate <= current && x.EndDate >= current)
                                                            .SelectMany(x => x.OnlinePurchaseProductItems.Where(i => !i.IsDeleted && i.IsEnabled));
                // 款号过滤
                if (state.HasValue)
                {
                    var bigNums = bigProdNumStatDic.Where(i => i.Value == state.Value).Select(i => i.Key);
                    query = query.Where(i => bigNums.Contains(i.BigProdNum));
                }

                var bigProdNums = query.Select(i => i.BigProdNum).ToArray();

                
                // 根据款号查询货号信息
                var list = _productContract.Products.Where(p => !p.IsDeleted && p.IsEnabled)
                                            .Where(p => bigProdNums.Contains(p.BigProdNum))
                                            .GroupBy(p => p.BigProdNum)
                                            .Select(g => g.FirstOrDefault())
                                            .Where<Product, int>(predicate, request.PageCondition, out count)
                                            .Select(m => new
                                            {
                                                m.Id,
                                                ProductName = m.ProductOriginNumber.ProductName,
                                                m.BigProdNum,
                                                ThumbnailPath = m.ThumbnailPath ?? m.ProductOriginNumber.ThumbnailPath
                                            })
                                            .ToList()
                                            .Select(m => new
                                            {
                                                m.Id,
                                                ThumbnailPath = string.IsNullOrEmpty(m.ThumbnailPath) ? string.Empty : strWeb + m.ThumbnailPath,
                                                ProductName = m.ProductName,
                                                m.BigProdNum,
                                                State = bigProdNumStatDic[m.BigProdNum].ToString()
                                            }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打包写入Cookie
        /// <summary>
        /// 写入Cookie
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Pack(string BigProdNum)
        {
            DateTime expiration = DateTime.Now.Add(FormsAuthentication.Timeout);
            DateTime issueDate = DateTime.Now;
            List<string> list = GetData(Request);
            //票证的版本号
            int version = 3;
            list.Add(BigProdNum);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            string strData = jss.Serialize(list);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(version, OrderKey, issueDate, expiration, false, strData, FormsAuthentication.FormsCookiePath);
            HttpCookie cookie = new HttpCookie(OrderKey, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Secure = FormsAuthentication.RequireSSL,
                Domain = FormsAuthentication.CookieDomain,
                Path = FormsAuthentication.FormsCookiePath,
            };
            Response.Cookies.Remove(cookie.Name);
            Response.Cookies.Add(cookie);
            return Json(new OperationResult(OperationResultType.Success));
        }

        /// <summary>
        /// 获取存在Cookie的数据
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private List<string> GetData(HttpRequestBase request)
        {
            HttpCookie cookie = request.Cookies[OrderKey];
            List<string> list = new List<string>();
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket != null && !string.IsNullOrEmpty(ticket.Name) && !string.IsNullOrEmpty(ticket.UserData))
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    list = jss.Deserialize<List<string>>(ticket.UserData);
                }
            }
            return list;
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
                            group x by new { x.ProductOriginNumber.TagPrice, x.ProductOriginNumber.PurchasePrice, x.ProductOriginNumber.HtmlPath } into gx
                            select new
                            {
                                gx.Key.TagPrice,
                                gx.Key.PurchasePrice,
                                gx.Key.HtmlPath,
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

        #region 移除cookie

        /// <summary>
        /// 移除Cookie不响应任何操作
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public EmptyResult RemoveCookie()
        {
            HttpCookie cookie = Request.Cookies[OrderKey];
            RemoveCookie(cookie);
            return null;
        }

        /// <summary>
        /// 移除cookie
        /// </summary>
        /// <param name="cookie"></param>
        private void RemoveCookie(HttpCookie cookie)
        {
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
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
                    var mod = query.FirstOrDefault(f => f.Id == CartId);
                    if (mod.IsNotNull())
                    {
                        storeCart = mod;
                    }
                }
                else
                {
                    storeCart = query.FirstOrDefault(x => x.PurchaserId == AuthorityHelper.OperatorId);
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

        #region 当前操作人购物车中商品的数量

        public JsonResult getCartProductCount()
        {
            int count = 0;
            var modCart = _storeCartContract.StoreCarts.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrder == false && x.PurchaserId == AuthorityHelper.OperatorId);
            if (modCart.IsNotNull())
            {
                var items = modCart.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted && w.ParentId == null).SelectMany(s => s.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted)).ToList();
                count = items.Any() ? items.Sum(s => s.Quantity) : 0;
            }

            return Json(new OperationResult(OperationResultType.Success, "", count), JsonRequestBehavior.AllowGet);
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
                else
                {
                    storeCart = query.FirstOrDefault(x => x.PurchaserId == adminId);
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

        #region 移除还原数据
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

        [Log]
        [HttpPost]
        public ActionResult RemoveCart(int[] Id)
        {
            var result = _storeCartContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        [HttpPost]
        public ActionResult RecoveryCart(int[] Id)
        {
            var result = _storeCartContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 启用禁用数据

        [Log]
        [HttpPost]
        public ActionResult DisableCart(int[] Id)
        {
            var result = _storeCartContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        [HttpPost]
        public ActionResult EnableCart(int[] Id)
        {
            var result = _storeCartContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
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

        #region 一件下单

        /// <summary>
        /// 初始化一键下单界面
        /// </summary>
        /// <returns></returns>
        public ActionResult AddPurchase()
        {
            StoreCart storeCart = _storeCartContract.StoreCarts.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsOrder == false)
                                    .FirstOrDefault(x => x.PurchaserId == AuthorityHelper.OperatorId);
            var allPrice = storeCart?.StoreCartItems.Where(w => w.IsEnabled && !w.IsDeleted)?.Sum(s => s.Quantity * s.Product?.ProductOriginNumber?.PurchasePrice ?? 0) ?? 0;
            ViewBag.AllPrice = allPrice;

            ViewBag.StoreTypes = CacheAccess.GetStoreType(_storeTypeContract, true);

            return PartialView();
        }


        /// <summary>
        /// 一键下单
        /// </summary>
        /// <param name="ReceiptStorageId"></param>
        /// <param name="ReceiptStoreId"></param>
        /// <param name="SenderStorageId"></param>
        /// <param name="WithoutMoney">无需结算</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddPurchase(int ReceiptStorageId, int ReceiptStoreId, bool WithoutMoney = false)
        {
            var ispay = _storeContract.View(ReceiptStoreId).StoreType.IsPay;

            OperationResult oper = _storeCartContract.AddPurchase(ReceiptStorageId, ReceiptStoreId, !ispay);
            return Json(oper);
        }
        /// <summary>
        /// 直接下单
        /// </summary>
        /// <param name="ReceiptStorageId"></param>
        /// <param name="ReceiptStoreId"></param>
        /// <param name="WithoutMoney"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddPurchaseDirect(int? CartId)
        {
            OperationResult oper = _storeCartContract.AddPurchaseDirect(null, CartId);
            return Json(oper);
        }
        #endregion

        #region 获取店铺下的仓库
        /// <summary>
        /// 获取店铺下的仓库
        /// </summary>
        /// <param name="StoreId">店铺Id</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetStorages(int StoreId)
        {
            IQueryable<Storage> listStorage = _storageContract.Storages.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.StoreId == StoreId);
            var list = listStorage.Select(x => new
            {
                x.Id,
                x.StorageName,
            }).ToList();
            var modStore = _storeContract.View(StoreId);
            var data = new
            {
                list = list,
                StoreType = modStore.StoreTypeId,
                IsPay = modStore.StoreType.IsPay,
                Balance = modStore.Balance,
            };
            return Json(data);
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
            else
            {
                storeCart = query.FirstOrDefault(x => x.PurchaserId == adminId);
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


        [Layout]
        public ActionResult CartIndex()
        {
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
                var query = _storeCartContract.StoreCarts.Where(x => x.IsOrder == false)
                            .Where(w => (w.OriginFlag == StoreCardOriginFlag.临时 && w.CaptainId == adminId));//(w.PurchaserId == adminId && w.OriginFlag == StoreCardOriginFlag.个人) || 

                var list = (from s in query.Where<StoreCart, int>(predicate, request.PageCondition, out count)
                            let isadmin = s.OriginFlag == StoreCardOriginFlag.个人
                            let children = s.StoreCartItems.Where(c => c.IsEnabled && !c.IsDeleted && c.ParentId != null)
                            select new
                            {
                                s.Id,
                                s.StoreCartNum,
                                Phone = isadmin ? s.Purchaser.Member.MobilePhone : s.Phone,
                                Name = isadmin ? string.Empty : s.Name,
                                Captain = isadmin ? s.Purchaser.Member.RealName : s.Captain.Member.RealName,
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
    }
}