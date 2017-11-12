using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Website.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    public class BuyController : Controller
    {

        #region 定义接口及初始化
        private static readonly ILogger _Logger = LogManager.GetLogger(typeof(BuyController));

        private readonly IProductContract _productContract;
        private readonly IStorageContract _storageContract;
        private readonly IStoreContract _storeContract;
        private readonly IInventoryContract _inventoryContract;
        private readonly IBrandContract _brandContract;
        private readonly IPurchaseContract _purchaseContract;
        private readonly IPurchaseItemContract _purchaseItemContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IProductTrackContract _productTrackContract;

        private static object _objlock = new object();
        private readonly string buyValid_key;
        private readonly string buyInvalid_key;
        public BuyController(IProductContract productContract,
            IStorageContract storageContract, 
            IStoreContract storeContract,
            IInventoryContract inventoryContract,
            IBrandContract brandContract, 
            IPurchaseContract purchaseContract,
            IPurchaseItemContract purchaseItemContract,
            IProductTrackContract _productTrackContract,
            IAdministratorContract administratorContract
            )
        {
            _administratorContract = administratorContract;
            _productContract = productContract;
            _storageContract = storageContract;
            _storeContract = storeContract;
            _inventoryContract = inventoryContract;
            _brandContract = brandContract;
            _purchaseContract = purchaseContract;
            _purchaseItemContract = purchaseItemContract;
            this._productTrackContract = _productTrackContract;
            buyValid_key = "buyValid_data_1102";
            buyInvalid_key = "buyInvalid_data_1102";
        }
        #endregion

        /// <summary>
        /// 缓存键
        /// </summary>
        public readonly string _key = "sess_excel_inpor_instorage_14";

        #region 初始化界面

        [Layout]
        [Log]
        public ActionResult Index()
        {
            Session[buyInvalid_key] = null;//清除无效数据
            string purnum = Request["_pnum"];
            ViewBag._purnum = purnum;
            if (purnum != null && purnum != "")
            {
                #region 从采购管理界面跳转
                ViewBag._purnum = purnum;
                Purchase pur = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == purnum).FirstOrDefault();
                ViewBag.inStorage = new List<SelectListItem>();
                ViewBag.Storages = new List<SelectListItem>();
                if (pur != null)
                {
                    var _storage = _storageContract.Storages.Where(c => c.Id == pur.ReceiverStorageId).FirstOrDefault();
                    if (_storage != null)
                    {
                        var inStorage = new SelectListItem()
                        {
                            Text = _storage.StorageName,
                            Value = _storage.Id.ToString()
                        };
                        ViewBag.inStorage = new List<SelectListItem>() { inStorage };

                        if (pur.OriginFlag != ERP.Models.Enums.StoreCardOriginFlag.工厂)
                        {
                            var outstorage = _storageContract.Storages.Where(c => c.IsOrderStorage == true && c.IsEnabled == true && c.IsDeleted == false).Select(c => new SelectListItem()
                            {
                                Text = c.StorageName,
                                Value = c.Id + ""
                            }).Where(c => c.Value != inStorage.Value).ToList();
                            ViewBag.Storages = outstorage;
                        }
                        else
                        {
                            if (pur.StorageId.HasValue)
                            {
                                ViewBag.Storages = new List<SelectListItem>() { new SelectListItem() { Text = pur.Storage.StorageName, Value = pur.StorageId.Value + "" } };
                            }
                        }
                    }
                    ViewBag.InStoreId = pur.Receiver.Id;
                }
                ViewBag.ScanValidCount = 0;
                ViewBag.ScanInvalidCount = 0;
                #endregion
            }
            else
            {
                #region 直接创建采购单
                ViewBag.StoreList = new List<SelectListItem>();
                ViewBag.ScanValidCount = (Session["BuyScanValid"] != null ? (List<Product_Model>)Session["BuyScanValid"] : new List<Product_Model>()).Sum(m => m.Amount);
                ViewBag.ScanInvalidCount = (Session["BuyScanInvalid"] != null ? (List<Product_Model>)Session["BuyScanInvalid"] : new List<Product_Model>()).Sum(m => m.Amount);

                ViewBag.Storages = _storageContract.Storages.Where(c => c.IsOrderStorage == true && c.IsEnabled == true && c.IsDeleted == false).Select(c => new SelectListItem()
                {
                    Text = c.StorageName,
                    Value = c.Id.ToString()
                }).ToList();

               
                ViewBag.inStorage = new List<SelectListItem>() { };
                #endregion

            }
            return View();
        }
        #endregion

        #region 配货完成
        /// <summary>
        /// 配货完成
        /// </summary>
        /// <param name="PurchaseNumber">配货单</param>
        /// <returns></returns>
        public JsonResult Create(string PurchaseNumber)
        {
            OperationResult oper = _purchaseContract.CreateOrderBlank(PurchaseNumber);
            return Json(oper);
        }
        #endregion

        #region 检测已配数量



        #endregion

        //yxk 2015-11
        /// <summary>
        /// 修改采购单
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdatePurchase()
        {
            //purId  data
            OperationResult resul = new OperationResult(OperationResultType.Error);
            string uuid = Request["uuid"];
            var key = buyValid_key + "_" + uuid;
            var da = SessionAccess.Get(key) as List<Product_Model> ?? new List<Product_Model>();
            string purnum = Request["purId"];
            List<PurchaseItem> purchItList = GetPurchItems(da);
            var pur = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == purnum && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
            if (pur != null)
                purchItList.Each(c => c.PurchaseId = pur.Id);
            //将已经标记为删除的采购记录删除
            var list = Session["BuyScanValid"] != null ? (List<Product_Model>)Session["BuyScanValid"] : new List<Product_Model>();
            var deids = list.Where(c => c.Other == "-1").Select(c => Convert.ToInt32(c.Id)).ToList();
            resul = _purchaseItemContract.Delete(deids.ToArray());
            if (resul.ResultType == OperationResultType.Success || resul.ResultType == OperationResultType.NoChanged)
            {
                //将不存在的采购记录添加到采购单中
                var newpurItems = purchItList.Where(c => c.Id == 0);
                List<PurchaseItemDto> newdto = AutoMapper.Mapper.Map<List<PurchaseItemDto>>(newpurItems);
                resul = _purchaseItemContract.Insert(newdto.ToArray());
                if (resul.ResultType == OperationResultType.Success || resul.ResultType == OperationResultType.NoChanged)
                {
                    //修改已有的采购记录
                    var exipurItems = purchItList.Where(c => c.Id != 0);
                    List<PurchaseItemDto> dto = AutoMapper.Mapper.Map<List<PurchaseItemDto>>(exipurItems);
                    resul = _purchaseItemContract.Update(dto.ToArray());
                }
            }

            return Json(resul);
        }

        //yxk 2015-9
        /// <summary>
        /// 返回一个不重复的采购单号
        /// </summary>
        /// <returns></returns>
        private string GetOnlyNumb()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            //return string.Format("{0:x}", i - DateTime.Now.Ticks);
            string _num = string.Format("{0:x}", i - DateTime.Now.Ticks);

            lock (_objlock)
            {
                var curnum = CacheAccess.GetBuyNumber(_purchaseContract);
                var lastcode = new XKMath36.Math36().To36(curnum);
                var num = _num.Substring(0, 6) + lastcode.PadLeft(4, '0');
                return num;
            }

        }

        private List<PurchaseItem> GetPurchItems(List<Product_Model> da)
        {
            List<PurchaseItem> li = new List<PurchaseItem>();
            var gropData = da.GroupBy(c => c.ProductNumber);
            foreach (var dat in gropData)
            {
                var ent = dat.FirstOrDefault();
                li.Add(new PurchaseItem()
                {
                    ProductId = ent.ProductId,
                    TagPrice = ent.TagPrice,
                    WholesalePrice = ent.WholesalePrice,
                    PurchasePrice = ent.PurchasePrice,
                    Quantity = dat.Count(),
                    //Barcodes = "," + string.Join(",", dat.Select(c => c.ProductBarcode)) + ","
                });
            }
            return li;
        }

        private List<PurchaseItem> ConvertPurList(List<Tem_t> temList)
        {
            List<PurchaseItem> li = new List<PurchaseItem>();
            if (temList != null && temList.Count() > 0)
            {
                foreach (var e in temList)
                {
                    li.Add(new PurchaseItem()
                    {
                        Id = int.Parse(e.pitemId),
                        ProductId = e.Id,
                        TagPrice = GetPrice(e.Id, "TagPrice"),
                        WholesalePrice = GetPrice(e.Id, "WholesalePrice"),
                        PurchasePrice = GetPrice(e.Id, "PurchasePrice"),
                        Quantity = e.Cou
                    });
                }
            }
            return li;
        }

        private float GetPrice(int productId, string flag)
        {
            if (productId > 0 && !string.IsNullOrEmpty(flag))
            {
                switch (flag)
                {
                    case "TagPrice":
                        {
                            return _productContract.Products.Where(c => c.Id == productId).Select(c => c.ProductOriginNumber.TagPrice).FirstOrDefault();

                        }
                    case "WholesalePrice":
                        {
                            return _productContract.Products.Where(c => c.Id == productId).Select(c => c.ProductOriginNumber.WholesalePrice).FirstOrDefault();
                        }
                    case "PurchasePrice":
                        {
                            return _productContract.Products.Where(c => c.Id == productId).Select(c => c.ProductOriginNumber.PurchasePrice).FirstOrDefault();
                        }
                    default: return 0;
                }
            }
            else
                return 0;
        }

        public ActionResult AddToScan1(string uuid, string number)
        {
            var result = new OperationResult(OperationResultType.Error, "");
            if (Session["ScanValid"] == null) Session["ScanValid"] = new List<Product_Model>();
            if (Session["ScanInvalid"] == null) Session["ScanInvalid"] = new List<Product_Model>();
            if (number != null && number.Length > 0)
            {
                try
                {
                    var validItem = new Product_Model();
                    var validList = (List<Product_Model>)Session["ScanValid"];
                    var invalidList = (List<Product_Model>)Session["ScanInvalid"];
                    number = InputHelper.SafeInput(number);
                    if (validList.Where(m => m.UUID == uuid).Count() == 0)
                    {
                        var entity = _productContract.Products.Where(m => m.IsDeleted == false && m.ProductOriginNumber.IsVerified == CheckStatusFlag.通过 && m.IsEnabled == true).FirstOrDefault(m => m.ProductNumber == number);
                        if (entity != null)
                        {
                            validItem = validList.FirstOrDefault(m => m.ProductNumber == entity.ProductNumber);
                            if (validItem != null)
                            {
                                validItem.Amount++;
                                validItem.UpdateTime = DateTime.Now;
                            }
                            else
                            {
                                validList.Add(new Product_Model
                                {
                                    Id = entity.Id,
                                    UUID = uuid,
                                    Thumbnail = entity.ThumbnailPath ?? entity.ProductOriginNumber.ThumbnailPath,
                                    ProductNumber = entity.ProductNumber,
                                    TagPrice = entity.ProductOriginNumber.TagPrice,
                                    WholesalePrice = entity.ProductOriginNumber.WholesalePrice,
                                    Season = entity.ProductOriginNumber.Season.SeasonName,
                                    Size = entity.Size.SizeName,
                                });
                            }
                            Session["ScanValid"] = validList;
                        }
                        else
                        {
                            var invalidItem = invalidList.FirstOrDefault(m => m.ProductNumber == number);
                            if (invalidItem != null)
                            {
                                invalidItem.Amount++;
                                invalidItem.UpdateTime = DateTime.Now;
                            }
                            else
                            {
                                invalidList.Add(new Product_Model
                                {
                                    UUID = uuid,
                                    ProductNumber = number
                                });
                            }
                            Session["ScanInvalid"] = invalidList;
                        }


                        result = new OperationResult(OperationResultType.Success, "产品已进入缓存列表！", new { UUID = uuid, validCount = validList.Sum(m => m.Amount), invalidCount = invalidList.Sum(m => m.Amount) });

                    }
                    else
                    {

                        result = new OperationResult(OperationResultType.Error, "产品进入缓存列表出错，错误如下：此UUID已存在，不允许重复提交！");

                    }

                }
                catch (Exception ex)
                {
                    result = new OperationResult(OperationResultType.Error, "产品进入缓存列表出错，错误如下：" + ex.Message, ex.ToString());
                }
            }
            else
            {
                result = new OperationResult(OperationResultType.Error, "扫码货号不能为空！");
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SetAmount(string name, int value)
        {
            if (name != null && name.Length > 0)
            {
                var validList = (Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>());
                var entity = validList.FirstOrDefault(m => m.ProductNumber == name);
                if (entity != null)
                {
                    entity.Amount = value;
                    entity.UpdateTime = DateTime.Now;
                }
                Session["ScanValid"] = validList;
                return Json(new OperationResult(OperationResultType.Success, "商品数量已修改！", new { validCount = validList.Sum(m => m.Amount) }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new OperationResult(OperationResultType.Error, "商品货号及数量不能为空！"), JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Invalid()
        {
            var entities = (Session["ScanInvalid"] != null ? (List<Product_Model>)Session["ScanInvalid"] : new List<Product_Model>());
            return PartialView(entities);
        }

        public ActionResult List(DataTable_Model param)
        {

            var list = Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>();
            var count = list.Count();

            if (param.sSearch != null && param.sSearch.Length > 0)
            {
                list = list.FindAll(m => (m.ProductNumber + m.Color + m.Size).Contains(param.sSearch));
            }

            Reverser<Product_Model> reverser;
            var columns = param.sColumns.Split(',');
            if (columns.Length > 0)
            {
                var sortColumn = columns[param.iSortCol_0];
                if (sortColumn != null)
                {
                    var sortDirection = param.sSortDir_0;

                    if (sortDirection == "asc")
                    {
                        reverser = new Reverser<Product_Model>((new Product_Model()).GetType(), sortColumn, ReverserInfo.Direction.ASC);
                    }
                    else
                    {
                        reverser = new Reverser<Product_Model>((new Product_Model()).GetType(), sortColumn, ReverserInfo.Direction.DESC);
                    }

                    list.Sort(reverser);
                }

            }
            var data = param.iDisplayLength > 0 ? list.Skip(param.iDisplayStart).Take(param.iDisplayLength) : list.Skip(param.iDisplayStart);

            return Json(new
            {
                sEcho = param.sEcho,
                iDisplayStart = param.iDisplayStart,
                iTotalRecords = count,
                iTotalDisplayRecords = count,
                aaData = data
            }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 验证采购信息是否有效
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="number">商品货号</param>
        /// <returns></returns>
        public JsonResult AddToScan(string uuid, string number, string PurchaseNumber)
        {
            var result = new OperationResult(OperationResultType.Error, "");
            var storag = Request["StorageId"];
            var guid = Request["guid"];
            var listValidBarCode = new List<string>();
            Purchase purchase = _purchaseContract.Purchases.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaseNumber == PurchaseNumber);
            if (purchase.IsNotNull())
            {
                listValidBarCode = purchase.PurchaseItems.SelectMany(s => s.PurchaseItemProducts.Select(ss => ss.ProductBarcode)).ToList();
            }

            var invalidli = SessionAccess.Get(buyInvalid_key) as List<Product_Model> ?? new List<Product_Model>();
            int storageId = storag == null ? 0 : Convert.ToInt32(storag);
            
            var model = new Product_Model()
            {
                UUID = uuid,
                ProductBarcode = number
            };
            if (!string.IsNullOrEmpty(number))
            {
                //没有配货完毕
                bool isReady = false;
                #region MyRegion
                try
                {
                    if (listValidBarCode.Any(c => c == number))
                    {
                        model.Notes = "条码" + number + "已经进入缓存队列";
                        model.IsValided = false;
                    }
                    else if (invalidli.Any(c => c.ProductBarcode == number))
                    {
                        model.Notes = "条码" + number + "已经经过校验无效,且重复出现";
                        model.IsValided = false;
                    }
                    else
                    {

                        Inventory ent = _inventoryContract.Inventorys.FirstOrDefault(c => c.ProductBarcode == number);

                        if (ent == null)
                        {
                            model.Notes = "库存" + number + "不存在";
                            model.IsValided = false;
                        }
                        else
                        {
                            if (ent.StorageId != storageId)
                            {
                                model.Notes = "库存" + number + "存在，但是不属于当前选择的出货仓库";
                                model.IsValided = false;
                            }
                            else if (!ent.IsEnabled)
                            {
                                model.Notes = "库存" + number + "已经被禁用";
                                model.IsValided = false;
                            }
                            else if (ent.IsDeleted)
                            {
                                model.Notes = "库存" + number + "被移除至回收站";
                                model.IsValided = false;
                            }
                            else if (ent.Status >= InventoryStatus.PurchasStart &&
                                     ent.Status <= InventoryStatus.PurchasEnd)
                            {
                                model.Notes = "库存" + number + "已经进入采购阶段";
                                model.IsValided = false;
                            }
                            else if (ent.Status >= InventoryStatus.DeliveryStart &&
                                     ent.Status <= InventoryStatus.DeliveryEnd)
                            {
                                model.Notes = "库存" + number + "已经进入配货阶段";
                                model.IsValided = false;
                            }
                            else if (ent.Status >= InventoryStatus.SaleStart &&
                                     ent.Status <= InventoryStatus.SaleEnd)
                            {
                                model.Notes = "库存" + number + "已经进入销售阶段";
                                model.IsValided = false;
                            }
                            
                           
                            else
                            {
                                //Purchase purchase = _purchaseContract.Purchases.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaseNumber == PurchaseNumber);
                                if (purchase == null)
                                {
                                    model.Notes = "库存" + number + "存在，但是不属于当前选择的出货仓库";
                                    model.IsValided = false;
                                }
                                else
                                {
                                    List<PurchaseItem> listPurchaseItem = purchase.PurchaseItems.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                                    //判断是否为采购商品
                                    PurchaseItem purchaseItem = listPurchaseItem.FirstOrDefault(x => x.ProductId == ent.ProductId);
                                    if (purchaseItem == null)
                                    {
                                        model.Notes = "采购单异常";
                                        model.IsValided = false;

                                        #region 不是采购商品自动添加为采购商品

                                        purchaseItem = new PurchaseItem();
                                        purchaseItem.IsNewAdded = true;
                                        purchaseItem.OperatorId = AuthorityHelper.OperatorId;
                                        purchaseItem.ProductId = ent.ProductId;
                                        purchaseItem.PurchaseId = purchase.Id;
                                        purchaseItem.Quantity = 1;
                                        purchaseItem.PurchaseItemProducts.Add(new PurchaseItemProduct()
                                        {
                                            ProductBarcode = number,
                                            OperatorId = AuthorityHelper.OperatorId,
                                        });
                                        OperationResult oper = _purchaseItemContract.Insert(purchaseItem);
                                        if (oper.ResultType == OperationResultType.Success)
                                        {
                                            model.IsValided = true;
                                            model.Notes = "采购项自动创建";
                                            listPurchaseItem.Add(purchaseItem);
                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        int total = purchaseItem.PurchaseItemProducts.Count(); ;

                                        if (total < purchaseItem.Quantity)
                                        {
                                            bool isHave = purchaseItem.PurchaseItemProducts.Any(x => x.ProductBarcode == number);
                                            if (isHave == true)
                                            {
                                                model.Notes = "库存" + number + "已经加入配货单";
                                                model.IsValided = false;
                                            }
                                            else
                                            {
                                                model.IsValided = true;
                                                PurchaseItemProduct pro = new PurchaseItemProduct()
                                                {
                                                    ProductBarcode = number,
                                                    PurchaseItemId = purchaseItem.Id,
                                                    OperatorId = AuthorityHelper.OperatorId,
                                                };
                                                OperationResult oper = _purchaseItemContract.Insert(pro);
                                                ent.Status = InventoryStatus.Purchased;
                                                _inventoryContract.Update(ent);
                                                if (oper.ResultType != OperationResultType.Success)
                                                {
                                                    model.IsValided = false;
                                                    model.Notes = "配货失败";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            model.Notes = "库存" + number + "已经配货完毕";
                                            model.IsValided = false;

                                            #region 如果库管选择了多余的自动追加一条

                                            PurchaseItem purchaseItemT = listPurchaseItem.FirstOrDefault(x => x.ProductId == ent.ProductId && x.IsNewAdded);

                                            if (purchaseItemT.IsNotNull())
                                            {
                                                int hasTotal = purchaseItemT.PurchaseItemProducts.Count();
                                                if (hasTotal == purchaseItemT.Quantity)
                                                {
                                                    purchaseItemT.Quantity += 1;
                                                }

                                                purchaseItemT.PurchaseItemProducts.Add(new PurchaseItemProduct()
                                                {
                                                    ProductBarcode = number,
                                                    OperatorId = AuthorityHelper.OperatorId,
                                                });
                                                OperationResult oper2 = _purchaseItemContract.Update(purchaseItemT);
                                                if (oper2.ResultType == OperationResultType.Success)
                                                {
                                                    model.IsValided = true;
                                                    model.Notes = "采购项自动创建";
                                                }
                                            }
                                            else
                                            {
                                                purchaseItemT = new PurchaseItem();
                                                purchaseItemT.IsNewAdded = true;
                                                purchaseItemT.OperatorId = AuthorityHelper.OperatorId;
                                                purchaseItemT.ProductId = ent.ProductId;
                                                purchaseItemT.PurchaseId = purchase.Id;
                                                purchaseItemT.Quantity = 1;
                                                purchaseItemT.PurchaseItemProducts.Add(new PurchaseItemProduct()
                                                {
                                                    ProductBarcode = number,
                                                    OperatorId = AuthorityHelper.OperatorId,
                                                });
                                                OperationResult oper2 = _purchaseItemContract.Insert(purchaseItemT);
                                                if (oper2.ResultType == OperationResultType.Success)
                                                {
                                                    model.IsValided = true;
                                                    model.Notes = "采购项自动创建";
                                                    listPurchaseItem.Add(purchaseItemT);
                                                }
                                            }

                                            #endregion

                                        }
                                    }
                                }
                                if (model.IsValided == true)
                                {
                                    model.Notes = "";
                                    model.IsValided = true;
                                    model.Id = ent.Id;
                                    model.ProductNumber = ent.ProductNumber;
                                    model.Brand = ent.Product.ProductOriginNumber.Brand.BrandName;
                                    model.Category = ent.Product.ProductOriginNumber.Category.CategoryName;
                                    model.Color = ent.Product.Color.ColorName;
                                    model.Size = ent.Product.Size.SizeName;
                                    model.Season = ent.Product.ProductOriginNumber.Season.SeasonName;
                                    model.UpdateTime = ent.UpdatedTime;
                                    model.ProductId = ent.ProductId;
                                    model.TagPrice = ent.Product.ProductOriginNumber.TagPrice;
                                    model.Thumbnail = ent.Product.ThumbnailPath ?? ent.Product.ProductOriginNumber.ThumbnailPath;
                                    //model.PurchasePrice = ent.PurchasePrice;
                                    //model.WholesalePrice = ent.WholesalePrice;
                                    isReady = GetReadyAny(PurchaseNumber);

                                    ent.Status = InventoryStatus.Purchased;
                                    ent.IsLock = true;

                                    var opera = _inventoryContract.Update(ent);
                                    if (opera.ResultType == OperationResultType.Success)
                                    {
                                        _productTrackContract.Insert(new ProductTrackDto()
                                        {
                                            ProductBarcode = ent.ProductBarcode,
                                            ProductNumber = ent.ProductNumber,
                                            CreatedTime = DateTime.Now,
                                            Describe = string.Format(ProductOptDescTemplate.ON_PRODUCT_PURCHASE_ADD, ent.Store.StoreName, ent.Storage.StorageName)
                                        });
                                    }

                                    listValidBarCode.Add(number);
                                }
                            }
                        }
                    }
                    if (!model.IsValided) invalidli.Add(model);

                    SessionAccess.Set(buyInvalid_key, invalidli);
                    result = new OperationResult(OperationResultType.Success, "", new { validCou = listValidBarCode.Count, invalidCou = invalidli.Count, uuid = uuid, isReady = isReady });
                }
                catch (Exception ex)
                {
                    result = new OperationResult(OperationResultType.Error, "产品进入缓存列表出错，错误如下：" + ex.Message, ex.ToString());
                }
                #endregion
            }
            else
            {
                result = new OperationResult(OperationResultType.Error, number + ":扫码编号不能为空或者编号长度不符合要求！");
                result.Data = new { validCount = listValidBarCode.Count, invalidCount = invalidli.Count, uuid = uuid };
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 校验配货单是否完成
        /// </summary>
        /// <param name="PurchaseNumber"></param>
        /// <returns></returns>
        private bool GetReady(string PurchaseNumber)
        {
            //缺省为配货完毕
            bool isReady = true;
            Purchase purchase = _purchaseContract.Purchases.FirstOrDefault(x => x.PurchaseNumber == PurchaseNumber);
            if (purchase != null)
            {
                List<PurchaseItem> listPurchaseItems = purchase.PurchaseItems.ToList();
                int count = 0;
                foreach (PurchaseItem item in listPurchaseItems)
                {
                    count = item.PurchaseItemProducts.Where(x => x.IsDeleted == false && x.IsEnabled == true).Count();
                    if (item.Quantity != count)
                    {
                        isReady = false;
                        break;
                    }
                }
            }
            return isReady;
        }
        /// <summary>
        /// 检验采购单是否存在任何一件已配商品
        /// </summary>
        /// <param name="PurchaseNumber"></param>
        /// <returns></returns>
        private bool GetReadyAny(string PurchaseNumber)
        {
            bool isAny = false;

            var pur = _purchaseContract.Purchases.FirstOrDefault(x => x.PurchaseNumber == PurchaseNumber && x.IsEnabled && !x.IsDeleted);
            if (pur.IsNotNull())
            {
                isAny = pur.PurchaseItems.Where(w => w.IsEnabled && !w.IsDeleted).Any(a => a.PurchaseItemProducts.Any(aa => aa.IsEnabled && !aa.IsDeleted));
            }

            return isAny;
        }

        #region 获取有效的数据列表  
        /// <summary>
        /// 获取有效的数据列表
        /// </summary>
        /// <param name="uuid"></param>
        /// <returns></returns>

        public ActionResult ValidList(string uuid)
        {
            return new EmptyResult();
        }
        #endregion
        /// <summary>
        /// 采购选择商品
        /// </summary>
        /// <returns></returns>
        //[OutputCache(Duration = 3600)]
        public ActionResult BuyProductSelect()
        {
            Response.Cache.SetOmitVaryStar(true);
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true);
            return PartialView();

        }
        /// <summary>
        /// 获取订购仓库中的商品信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult BuyProductList()
        {
            GridRequest requ = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Inventory>(requ.FilterGroup);
            var alldata = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract).Where(pred)
                .Where(c => c.IsEnabled && !c.IsLock && !c.IsDeleted && c.Status == InventoryStatus.Default)
                .GroupBy(c => c.ProductNumber);
            var da = alldata.OrderBy(c => c.Key).Skip(requ.PageCondition.PageIndex).Take(requ.PageCondition.PageSize).Select(c => new
            {
                c.Key,
                c.FirstOrDefault().ProductBarcode,
                c.FirstOrDefault().Product.Color.ColorName,
                c.FirstOrDefault().Product.ProductOriginNumber.Season.SeasonName,
                c.FirstOrDefault().Product.ProductOriginNumber.Brand.BrandName,
                c.FirstOrDefault().Product.Size.SizeName,
                ThumbnailPath = c.FirstOrDefault().Product.ThumbnailPath ?? c.FirstOrDefault().Product.ProductOriginNumber.ThumbnailPath,
                Childs = c.Select(g => new
                {
                    g.Id,
                    g.ProductBarcode,
                })
            }).ToList();
            List<object> li = new List<object>();
            foreach (var ite in da)
            {
                li.Add(new
                {
                    Id = ite.Key,
                    ParentId = "",
                    ite.ColorName,
                    ite.SeasonName,
                    ite.BrandName,
                    ite.SizeName,
                    ite.ThumbnailPath
                });
                li.AddRange(ite.Childs.Select(c => new
                {
                    Id = c.Id,
                    ParentId = ite.Key,
                    c.ProductBarcode
                }).ToList());
            }
            GridData<Object> objdata = new GridData<object>(li, alldata.Count(), Request);
            return Json(objdata);
        }

        public ActionResult GetValidList()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取验证列表 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetValidDa(string PurchaseNumber)
        {
            Purchase purchase = _purchaseContract.Purchases.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaseNumber == PurchaseNumber);
            var listValidBarCode = purchase.PurchaseItems.SelectMany(s => s.PurchaseItemProducts.Select(ss => ss.ProductBarcode)).ToList();
            var list = (from s in _inventoryContract.Inventorys.Where(w => listValidBarCode.Contains(w.ProductBarcode)).GroupBy(g => g.ProductNumber)
                        let p = s.FirstOrDefault()
                        select new
                        {
                            p.ProductId,
                            p.ProductNumber,
                            p.Product.ProductName,
                            p.Product.Size.SizeName,
                            p.Product.Color.ColorName,
                            p.Product.ProductOriginNumber.Season.SeasonName,
                            Count = s.Count()
                        }).ToList();

            List<Product_Model> li1 = (List<Product_Model>)Session[buyInvalid_key];

            return Json(new
            {
                invalid = li1,
                valid = list
            });
        }
        /// <summary>
        /// 获取有效无效数量
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetValidCount(string PurchaseNumber)
        {
            Purchase purchase = _purchaseContract.Purchases.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaseNumber == PurchaseNumber);
            var validCount = purchase.PurchaseItems.SelectMany(s => s.PurchaseItemProducts.Select(ss => ss.ProductBarcode)).Count();

            List<Product_Model> li1 = (List<Product_Model>)Session[buyInvalid_key];

            return Json(new { valid = validCount, invalid= li1?.Count ?? 0 });
        }

        /// <summary>
        /// 创建采购单页面批量移除session中的数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Remove(int[] ids, string uuid)
        {
            var data = OperationHelper.Try((opera) =>
            {
                var list = _purchaseItemContract.PurchaseItems.Where(w => ids.Contains(w.Id) && w.PurchaseItemProducts.Count > 0).ToList();
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        foreach (var item2 in item.PurchaseItemProducts)
                        {
                            var ent = _inventoryContract.Inventorys.Where(w => w.ProductBarcode == item2.ProductBarcode).FirstOrDefault();
                            ent.IsLock = false;
                            ent.Status = InventoryStatus.Default;
                           var oper = _inventoryContract.Update(ent);
                            if (oper.ResultType == OperationResultType.Success)
                            {
                                _productTrackContract.Insert(new ProductTrackDto()
                                {
                                    ProductBarcode = ent.ProductBarcode,
                                    ProductNumber = ent.ProductNumber,
                                    CreatedTime = DateTime.Now,
                                    Describe = string.Format(ProductOptDescTemplate.ON_PRODUCT_PURCHASE_DROP, ent.Store.StoreName, ent.Storage.StorageName)
                                });
                            }
                        }
                        item.PurchaseItemProducts.Clear();
                    }
                }
                else
                {
                    return new OperationResult(OperationResultType.QueryNull, "该项无需重置");
                }

                return _purchaseItemContract.Update(list.ToArray());

            }, "移除选择项");

            return Json(data);
        }

        /// <summary>
        /// 初始化批量上传界面
        /// </summary>
        /// <returns></returns>
        public ActionResult BatchImport()
        {
            return PartialView();
        }

        #region 上传导入数据

        public JsonResult ExcelFileUpload()
        {

            OperationResult resul = new OperationResult(OperationResultType.Error);
            //string fileName = uploadfile.FileName;
            //string savepath = "./Warehouses/Content/uploadFiles/" + fileName;
            //uploadfile.SaveAs(savepath);
            int te = Request.Files.Count;
            if (System.Web.HttpContext.Current.Request.Files.Count > 0)
            {

                var file = Request.Files[0];
                string fileName = file.FileName;
                string savePath = Server.MapPath("/Content/UploadFiles/Excels/") + DateTime.Now.ToString("yyyyMMddHH");
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string fullName = savePath + "\\" + fileName;

                if (System.IO.File.Exists(fullName))
                {
                    System.IO.File.Delete(fullName);
                }
                file.SaveAs(fullName);
                var reda = ExcelToJson(fullName);
                System.IO.File.Delete(fullName);
                if (reda.Any())
                    resul = new OperationResult(OperationResultType.Success);
            }
            return Json(resul);
        }

        #region 读取Excel文件

        private List<List<String>> ExcelToJson(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                var da = new List<List<String>>();
                if (Path.GetExtension(fileName) == ".txt")
                {
                    string st = System.IO.File.ReadAllText(fileName);
                    var retda = st.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var li = new List<List<string>>();
                    retda.Each(c =>
                    {
                        var t = new List<string>() { c };
                        li.Add(t);
                    });
                    da = li;
                }
                else
                {
                    YxkSabri.ExcelUtility excel = new YxkSabri.ExcelUtility();
                    da = excel.ExcelToArray(fileName, 0, 0);
                    //var _key = "sess_excel_inpor_instorage_14";
                    SessionAccess.Set(_key, da, true);

                }
                return da;
            }
            return null;
        }
        #endregion

        #endregion

        #region 读取Excel数据
        public JsonResult GetBatchImportExcelData()
        {
            GridRequest gr = new GridRequest(Request);
            //var _key = "sess_excel_inpor_instorage_13";
            var dat = SessionAccess.Get(_key) as List<List<string>>;
            GridData<object> da = new GridData<object>(new List<object>(), 0, Request);
            if (dat != null)
            {
                var te = dat.Select(c => new
                {
                    Barcode = c[0],
                    RowInd = Convert.ToInt32(c[1]),
                }).ToList();


                var li = te.OrderBy(c => c.RowInd).Skip(gr.PageCondition.PageIndex).Take(gr.PageCondition.PageSize).Select(c => new
                {
                    ProductBarcode = c.Barcode,
                    c.RowInd
                }).ToList();
                da = new GridData<object>(li, dat.Count, Request);
            }
            return Json(da);
        }
        #endregion

        #region 校验盘点数据
        public JsonResult ExcelBatchStrageCheck(string PurchaseNumber)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success);
            List<List<string>> dat = SessionAccess.Get(_key) as List<List<string>>;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(PurchaseNumber))
            {
                oper.ResultType = OperationResultType.Error;
                oper.Message = "配货单异常";
            }
            else
            {
                oper = VaidData(dat, PurchaseNumber);
            }
            SessionAccess.Remove(_key);
            return Json(oper);

        }

        #endregion

        #region 批量导入采购单
        /// <summary>
        /// 配货盘点，不记录在表中
        /// </summary>
        /// <param name="dat"></param>
        private OperationResult VaidData(List<List<string>> dat, string PurchaseNumber)
        {
            var guid = Request["guid"];
            List<Product_Model> invalidli = SessionAccess.Get(buyInvalid_key) as List<Product_Model> ?? new List<Product_Model>();
            string storag = Request["StorageId"];
            int storageId = storag == null ? 0 : Convert.ToInt32(storag);
            OperationResult oper = new OperationResult(OperationResultType.Success);
            //没有配货完毕
            bool isReady = false;
            List<PurchaseItemProduct> listPurchaseItemProduct = new List<PurchaseItemProduct>();
            List<Inventory> listInventory = new List<Inventory>();

            var listValidBarCode = new List<string>();
            Purchase purchase = _purchaseContract.Purchases.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.PurchaseNumber == PurchaseNumber);
            if (purchase.IsNotNull())
            {
                listValidBarCode = purchase.PurchaseItems.SelectMany(s => s.PurchaseItemProducts.Select(ss => ss.ProductBarcode)).ToList();
            }

            if (dat != null)
            {
                foreach (List<string> item in dat)
                {
                    string number = item[0];
                    if (!string.IsNullOrEmpty(number))
                    {
                        #region MyRegion
                        try
                        {
                            Product_Model model = new Product_Model()
                            {
                                //UUID = uuid,
                                ProductBarcode = number
                            };
                            if (listValidBarCode.Any(c => c == number))
                            {
                                model.Notes = "条码" + number + "已经进入缓存队列";
                                model.IsValided = false;
                            }
                            else if (invalidli.Any(c => c.ProductBarcode == number))
                            {
                                model.Notes = "条码" + number + "已经经过校验无效,且重复出现";
                                model.IsValided = false;
                            }
                            else
                            {

                                Inventory ent = _inventoryContract.Inventorys.FirstOrDefault(c => c.ProductBarcode == number);

                                if (ent == null)
                                {
                                    model.Notes = "库存" + number + "不存在";
                                    model.IsValided = false;
                                }
                                else
                                {
                                    if (ent.StorageId != storageId)
                                    {
                                        model.Notes = "库存" + number + "存在，但是不属于当前选择的出货仓库";
                                        model.IsValided = false;
                                    }
                                    else if (!ent.IsEnabled)
                                    {
                                        model.Notes = "库存" + number + "已经被禁用";
                                        model.IsValided = false;
                                    }
                                    else if (ent.IsDeleted)
                                    {
                                        model.Notes = "库存" + number + "被移除至回收站";
                                        model.IsValided = false;
                                    }
                                    else if (ent.Status >= InventoryStatus.PurchasStart &&
                                             ent.Status <= InventoryStatus.PurchasEnd)
                                    {
                                        model.Notes = "库存" + number + "已经进入采购阶段";
                                        model.IsValided = false;
                                    }
                                    else if (ent.Status >= InventoryStatus.DeliveryStart &&
                                             ent.Status <= InventoryStatus.DeliveryEnd)
                                    {
                                        model.Notes = "库存" + number + "已经进入配货阶段";
                                        model.IsValided = false;
                                    }
                                    else if (ent.Status >= InventoryStatus.SaleStart &&
                                             ent.Status <= InventoryStatus.SaleEnd)
                                    {
                                        model.Notes = "库存" + number + "已经进入销售阶段";
                                        model.IsValided = false;
                                    }

                                    
                                    else
                                    {
                                        if (purchase == null)
                                        {
                                            model.Notes = "库存" + number + "存在，但是不属于当前选择的出货仓库";
                                            model.IsValided = false;
                                        }
                                        else
                                        {
                                            List<PurchaseItem> listPurchaseItem = purchase.PurchaseItems.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                                            //判断是否为采购商品
                                            PurchaseItem purchaseItem = listPurchaseItem.FirstOrDefault(x => x.ProductId == ent.ProductId);
                                            if (purchaseItem == null)
                                            {
                                                model.Notes = "采购单异常";
                                                model.IsValided = false;

                                                #region 不是采购商品自动添加为采购商品

                                                purchaseItem = new PurchaseItem();
                                                purchaseItem.IsNewAdded = true;
                                                purchaseItem.OperatorId = AuthorityHelper.OperatorId;
                                                purchaseItem.ProductId = ent.ProductId;
                                                purchaseItem.PurchaseId = purchase.Id;
                                                purchaseItem.Quantity = 1;
                                                purchaseItem.PurchaseItemProducts.Add(new PurchaseItemProduct()
                                                {
                                                    ProductBarcode = number,
                                                    OperatorId = AuthorityHelper.OperatorId,
                                                });
                                                OperationResult oper2 = _purchaseItemContract.Insert(purchaseItem);
                                                if (oper2.ResultType == OperationResultType.Success)
                                                {
                                                    model.IsValided = true;
                                                    model.Notes = "采购项自动创建";
                                                    purchase.PurchaseItems.Add(purchaseItem);
                                                }
                                                #endregion

                                            }
                                            else
                                            {
                                                int total = purchaseItem.PurchaseItemProducts.Count(); ;

                                                if (total < purchaseItem.Quantity)
                                                {
                                                    bool isHave = purchaseItem.PurchaseItemProducts.Any(x => x.ProductBarcode == number);
                                                    if (isHave == true)
                                                    {
                                                        model.Notes = "库存" + number + "已经加入配货单";
                                                        model.IsValided = false;
                                                    }
                                                    else
                                                    {
                                                        model.IsValided = true;
                                                        PurchaseItemProduct pro = new PurchaseItemProduct()
                                                        {
                                                            ProductBarcode = number,
                                                            PurchaseItemId = purchaseItem.Id,
                                                            OperatorId = AuthorityHelper.OperatorId,
                                                        };
                                                        ent.UpdatedTime = DateTime.Now;
                                                        ent.OperatorId = AuthorityHelper.OperatorId;
                                                        oper = _purchaseItemContract.Insert(pro);
                                                        if (oper.ResultType != OperationResultType.Success)
                                                        {
                                                            model.IsValided = false;
                                                            model.Notes = "配货失败";
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    model.Notes = "库存" + number + "已经配货完毕";
                                                    model.IsValided = false;


                                                    #region 如果库管选择了多余的自动追加一条

                                                    PurchaseItem purchaseItemT = listPurchaseItem.FirstOrDefault(x => x.ProductId == ent.ProductId && x.IsNewAdded);

                                                    if (purchaseItemT.IsNotNull())
                                                    {
                                                        int hasTotal = purchaseItemT.PurchaseItemProducts.Count();
                                                        if (hasTotal == purchaseItemT.Quantity)
                                                        {
                                                            purchaseItemT.Quantity += 1;
                                                        }

                                                        purchaseItemT.PurchaseItemProducts.Add(new PurchaseItemProduct()
                                                        {
                                                            ProductBarcode = number,
                                                            OperatorId = AuthorityHelper.OperatorId,
                                                        });
                                                        OperationResult oper2 = _purchaseItemContract.Update(purchaseItemT);
                                                        if (oper2.ResultType == OperationResultType.Success)
                                                        {
                                                            model.IsValided = true;
                                                            model.Notes = "采购项自动创建";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        purchaseItemT = new PurchaseItem();
                                                        purchaseItemT.IsNewAdded = true;
                                                        purchaseItemT.OperatorId = AuthorityHelper.OperatorId;
                                                        purchaseItemT.ProductId = ent.ProductId;
                                                        purchaseItemT.PurchaseId = purchase.Id;
                                                        purchaseItemT.Quantity = 1;
                                                        purchaseItemT.PurchaseItemProducts.Add(new PurchaseItemProduct()
                                                        {
                                                            ProductBarcode = number,
                                                            OperatorId = AuthorityHelper.OperatorId,
                                                        });
                                                        OperationResult oper2 = _purchaseItemContract.Insert(purchaseItemT);
                                                        if (oper2.ResultType == OperationResultType.Success)
                                                        {
                                                            model.IsValided = true;
                                                            model.Notes = "采购项自动创建";
                                                            listPurchaseItem.Add(purchaseItemT);
                                                        }
                                                    }

                                                    #endregion

                                                }
                                            }
                                        }
                                        if (model.IsValided == true)
                                        {
                                            model.Notes = "";
                                            model.IsValided = true;
                                            model.Id = ent.Id;
                                            model.ProductNumber = ent.ProductNumber;
                                            model.Brand = ent.Product.ProductOriginNumber.Brand.BrandName;
                                            model.Category = ent.Product.ProductOriginNumber.Category.CategoryName;
                                            model.Color = ent.Product.Color.ColorName;
                                            model.Size = ent.Product.Size.SizeName;
                                            model.Season = ent.Product.ProductOriginNumber.Season.SeasonName;
                                            model.UpdateTime = ent.UpdatedTime;
                                            model.ProductId = ent.ProductId;
                                            model.TagPrice = ent.Product.ProductOriginNumber.TagPrice;
                                            model.Thumbnail = ent.Product.ThumbnailPath ?? ent.Product.ProductOriginNumber.ThumbnailPath;
                                            //model.PurchasePrice = ent.PurchasePrice;
                                            //model.WholesalePrice = ent.WholesalePrice;
                                            isReady = GetReadyAny(PurchaseNumber);

                                            ent.Status = InventoryStatus.Purchased;
                                            ent.IsLock = true;

                                            var opera = _inventoryContract.Update(ent);
                                            if (opera.ResultType == OperationResultType.Success)
                                            {
                                                _productTrackContract.Insert(new ProductTrackDto()
                                                {
                                                    ProductBarcode = ent.ProductBarcode,
                                                    ProductNumber = ent.ProductNumber,
                                                    CreatedTime = DateTime.Now,
                                                    Describe = string.Format(ProductOptDescTemplate.ON_PRODUCT_PURCHASE_ADD, ent.Store.StoreName, ent.Storage.StorageName)
                                                });
                                            }

                                            listValidBarCode.Add(number);
                                        }
                                    }
                                }
                            }
                            if (!model.IsValided) invalidli.Add(model);
                        }
                        catch (Exception ex)
                        {
                            oper = new OperationResult(OperationResultType.Error, "产品进入缓存列表出错，错误如下：" + ex.Message, ex.ToString());
                        }
                        #endregion
                    }
                }
                SessionAccess.Set(buyInvalid_key, invalidli);
                oper = new OperationResult(OperationResultType.Success, "", new { validCou = listValidBarCode.Count, invalidCou = invalidli.Count, isReady = isReady });
            }
            return oper;
        }
        #endregion

    }
}