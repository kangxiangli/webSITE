using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse;
using Whiskey.ZeroStore.ERP.Services.Implements.Warehouse;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.Articles;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models;
using System.Web.Caching;
using Whiskey.ZeroStore.ERP.Website.App_Start;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Website.Models;
using XKMath36;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{

    [License(CheckMode.Verify)]
    public class InventoryController : BaseController
    {
        //protected static  readonly ILogger _Logger = LogManager.GetLogger(typeof(InventoryController));
        protected readonly ILogContract _logContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IInventoryRecordContract _inventoryRecordContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IProductContract _productContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IColorContract _colorContract;
        protected readonly IProductBarcodePrintInfoContract _productBarcodePrintInfoContract;
        protected readonly IProductBarcodeDetailContract _productBarcodeDetailContract;
        protected readonly IStoreRecommendContract _storeRecommendContract;
        private readonly ISeasonContract _seasonContract;
        protected readonly IProductTrackContract _productTrackContract;
        protected readonly IAdministratorContract _administratorContract;
        public InventoryController(IInventoryContract inventoryContract,
            IStoreContract storeContract,
            IStorageContract storageContract,
            IProductContract productContract,
            IBrandContract brandContract,
            ICategoryContract categoryContract,
            IColorContract colorContract,
            ILogContract logContract,
            IProductBarcodeDetailContract productBarcodeDetailContract,
            IProductBarcodePrintInfoContract productBarcodePrintInfoContract,
            IInventoryRecordContract inventoryRecordContract,
            IStoreRecommendContract storeRecommendContract,
            ISeasonContract seasonContract,
            IProductTrackContract productTrackContract,
            IAdministratorContract administratorContract)
        {
            _administratorContract = administratorContract;
            _inventoryContract = inventoryContract;
            _storeContract = storeContract;
            _productContract = productContract;
            _storageContract = storageContract;
            _brandContract = brandContract;
            _categoryContract = categoryContract;
            _colorContract = colorContract;
            _productBarcodePrintInfoContract = productBarcodePrintInfoContract;
            _productBarcodeDetailContract = productBarcodeDetailContract;
            _logContract = logContract;
            _inventoryRecordContract = inventoryRecordContract;
            _storeRecommendContract = storeRecommendContract;
            _seasonContract = seasonContract;
            _productTrackContract = productTrackContract;

            ViewBag.Products = CacheAccess.GetProductListItem(_productContract, true);

        }


        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]

        public ActionResult Index()
        {
            ViewBag.Seasons =  CacheAccess.GetSeason(_seasonContract, true);
            return View();
        }

        public ActionResult GetCategory()
        {
            var catList = _categoryContract.Categorys.Where(c => !c.IsDeleted && c.IsEnabled && !c.ParentId.HasValue)
                .Select(c => new { c.Id, c.CategoryName })
                .ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, catList));
        }


        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {

            ViewBag.Products = (_productContract.Selectlist(null, c => c.IsDeleted == false && c.IsEnabled == true).Select(m => new SelectListItem { Text = m.Value, Value = m.Key })).ToList();
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
        public ActionResult Create(InventoryDto dto)
        {
            var result = _inventoryContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(InventoryDto dto)
        {
            var result = _inventoryContract.Update(new InventoryDto[] { dto });
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            //var result = _inventoryContract.Edit(Id);
            Inventory result = _inventoryContract.Inventorys.Where(c => c.Id == Id && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id, bool showDetail = false)
        {
            var result = _inventoryContract.View(Id);
            var num = _inventoryContract.Inventorys.Where(c => c.Id == Id).Select(c => c.Product.ProductNumber).FirstOrDefault();
            ViewBag.num = num;
            ViewBag.StoragCou = _inventoryContract.Inventorys.Count(c => (c.ProductNumber == num) && !c.IsDeleted && c.IsEnabled);
            ViewBag.ShowDetail = showDetail ? 1 : 0;
            return PartialView(result);
        }

        /// <summary>
        /// 根据商品编号获取对应的库存数据
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public ActionResult GetInventorysByProductNum()
        {
            GridRequest grq = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Inventory>(grq.FilterGroup);
            //获取未出库的数据
            var allda = _inventoryContract.Inventorys.Where(pred).Where(c => c.Status != InventoryStatus.JoinOrder /*已出库*/);
            int cou = allda.Count();
            var data = allda.OrderByDescending(c => c.UpdatedTime).ThenByDescending(c => c.Id).Skip(grq.PageCondition.PageIndex).Take(grq.PageCondition.PageSize).Select(c => new
            {
                c.Id,
                c.IsDeleted,
                c.IsEnabled,
                c.CreatedTime,
                c.UpdatedTime,
                c.Status,
                c.ProductBarcode
            }).ToList().Select(c => new
            {
                c.Id,
                c.IsDeleted,
                c.IsEnabled,
                c.CreatedTime,
                c.UpdatedTime,
                Status = GetDescriptionText((InventoryStatus)c.Status),
                c.ProductBarcode
            });
            GridData<object> obj = new GridData<object>(data, cou, Request);
            return Json(obj);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(string BrandName, int? CategoryId, int? storeId, int? storageId,int?seasonId)
        {

            GridRequest request = new GridRequest(Request);
            var storeIds = _storeContract.FilterStoreId(AuthorityHelper.OperatorId, _administratorContract, storeId);
            var storageIds = _storageContract.FilterStorageId(AuthorityHelper.OperatorId, _administratorContract, storageId);

            Expression<Func<Inventory, bool>> predicate;
            CacheAccess.SaveHttpContextState();

            try
            {
                predicate = FilterHelper.GetExpression<Inventory>(request.FilterGroup);

                var inventoryQuery = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                                                .Where(i => !i.IsLock && i.Status == InventoryStatus.Default)
                                                .Where(i => storeIds.Contains(i.StoreId))
                                                .Where(i => storageIds.Contains(i.StorageId))
                                                .Where(predicate)
                                                .Include(i => i.Product)
                                                .Include(i => i.Product.Size)
                                                .Include(i => i.Product.Color)
                                                .Include(i => i.Product.ProductOriginNumber)
                                                .Include(i => i.Product.ProductOriginNumber.Season)
                                                .Include(i => i.Product.ProductOriginNumber.Category)
                                                .Include(i => i.Product.ProductOriginNumber.Brand);

                if (!string.IsNullOrEmpty(BrandName))
                {
                    inventoryQuery = inventoryQuery.Where(i => i.Product.ProductOriginNumber.Brand.BrandName == BrandName);
                }

                if (CategoryId.HasValue)
                {
                    inventoryQuery = inventoryQuery.Where(i => i.Product.ProductOriginNumber.Category.ParentId == CategoryId.Value);
                }
                if (seasonId.HasValue)
                {
                    inventoryQuery = inventoryQuery.Where(i => i.Product.ProductOriginNumber.SeasonId == seasonId.Value);
                }


                //获取分页信息
                var groupQuery = inventoryQuery.GroupBy(i => i.Product.ProductOriginNumber.BigProdNum);
                var inventoryTotalCount = groupQuery.Count();
                var groupedDataList = groupQuery.OrderBy(g => g.Key)
                    .Skip(request.PageCondition.PageIndex)
                    .Take(request.PageCondition.PageSize)
                    .ToList();

                var li = new List<object>();
                var index = 0;
                foreach (var groupItem in groupedDataList)
                {
                    //构造父节点
                    li.Add(new
                    {
                        ParentId = "",
                        Id = "par" + index,
                        BigProdNum = groupItem.Key,
                        BrandName = groupItem.First().Product.ProductOriginNumber.Brand.BrandName,
                        CategoryName = groupItem.First().Product.ProductOriginNumber.Category.CategoryName,
                        ColorName = string.Empty,
                        IconPath = string.Empty,
                        ThumbnailPath = groupItem.First().Product.ProductOriginNumber.ThumbnailPath,
                        SizeName = string.Empty,
                        Quantity = groupItem.Count(),
                        StoreName = "",
                        StorageName = "",
                        ProductName = "",
                        TagPrice = "",
                        RetailPrice = "",
                        WholesalePrice = "",
                        PurchasePrice = "",
                        IsDeleted = "",
                        IsEnabled = "",
                        UpdatedTime = "",
                        CreatedTime = "",
                        SeasonName = groupItem.First().Product.ProductOriginNumber.Season.SeasonName

                    });

                    // 构造子节点
                    li.AddRange(
                        groupItem.GroupBy(h => h.ProductNumber).Select(c => new
                        {
                            ParentId = "par" + index,
                            Id = c.First().Id,
                            BigProdNum = "",
                            ProductNumber = c.Key,
                            StoreName = c.First().Store.StoreName,
                            StorageName = c.First().Storage.StorageName,
                            ProductName = c.First().Product.ProductName,
                            BrandName = c.First().Product.ProductOriginNumber.Brand.BrandName,
                            CategoryName = c.First().Product.ProductOriginNumber.Category.CategoryName,
                            SeasonName = string.Empty,
                            ColorName = c.First().Product.Color.ColorName,
                            SizeName = c.First().Product.Size.SizeName,
                            IconPath = c.First().Product.Color.IconPath,
                            Quantity = c.Count(),
                            TagPrice = c.First().Product.ProductOriginNumber.TagPrice,
                            ThumbnailPath = c.First().Product.ThumbnailPath ?? c.First().Product.ProductOriginNumber.ThumbnailPath,
                            IsDeleted = c.First().IsDeleted,
                            IsEnabled = c.First().IsEnabled,
                            UpdatedTime = c.First().UpdatedTime,
                            CreatedTime = c.First().CreatedTime
                        }));
                    index++;
                }
                var data = new GridDataResul<object>(li, inventoryTotalCount, request.RequestInfo)
                {
                    Other = inventoryQuery.Count().ToString() + "|" + groupedDataList.Sum(g => g.Count())
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new GridData<object>(new List<string>(), 0, request.RequestInfo));

            }

        }


        public ActionResult GetStorageList(int? storeId)
        {
            var optId = AuthorityHelper.OperatorId;
            if (!optId.HasValue)
            {
                throw new Exception("optId is null");
            }
            var query = _storeContract.QueryManageStorage(AuthorityHelper.OperatorId.Value).AsQueryable();
            if (storeId.HasValue)
            {
                query = query.Where(s => s.StoreId == storeId.Value);
            }
            var data = query.Select(s => new SelectListItem() { Text = s.StorageName, Value = s.Id.ToString() })
                                     .ToList();
            data.Insert(0, new SelectListItem() { Text = "请选择仓库", Value = string.Empty });
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data));
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

            var result = _inventoryContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _inventoryContract.Delete(Id);
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
            var result = _inventoryContract.Recovery(Id);
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
            var result = _inventoryContract.Enable(Id);
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
            var result = _inventoryContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _inventoryContract.Inventorys.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _inventoryContract.Inventorys.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        //da= [{ProduId:22,StorCou:120,StoreId:12,StorageId:2},{ProduId:22,StorCou:120,StoreId:12,StorageId:2}]
        //yxk 
        public JsonResult AddInventory(string[] prCodes, int storageId, string notes, string recordOrderNumber, DateTime? CreatedTime)
        {
            if (!CreatedTime.HasValue)
            {
                CreatedTime = DateTime.Now;
            }
            OperationResult optresul = new OperationResult(OperationResultType.Error, "入库失败");
            List<Inventory> inves = new List<Inventory>();
            if (prCodes == null)
                prCodes = new string[0];
            string key = "ScanValid";
            var productsModeList = Session[key] as List<Product_Model>;
            int err = 0;
            if (productsModeList == null || productsModeList.Count == 0)
            {
                optresul.Message = "在服务器中未查到相关的入库操作，可能是操作超时";
            }
            else
            {
                var instorageCode = prCodes.Any() ? productsModeList.Where(c => prCodes.Contains(c.ProductBarcode)).Select(c => c.ProductBarcode).ToList()
                                                  : productsModeList.Select(c => c.ProductBarcode).Distinct().ToList();
                //检查是否有已经入库的条码
                if (_inventoryContract.Inventorys.Any(i => instorageCode.Contains(i.ProductBarcode)))
                {
                    return Json(OperationResult.Error("检测到有已经入过库的流水号"));
                }
                int adminid = (int)AuthorityHelper.OperatorId;

                var storage = CacheAccess.GetManagedStorage(_storageContract, _administratorContract).FirstOrDefault(f => f.Id == storageId);

                if (storage.IsNull())
                {
                    optresul.Message = "当前用户无权限操作该仓库，ID:" + storageId;
                }
                else
                {
                    var barcodesFromDb = _productBarcodeDetailContract.productBarcodeDetails
                        .Where(c => instorageCode.Contains(c.ProductNumber + c.OnlyFlag))
                        .ToList();
                    var distinceBarcodes = new List<ProductBarcodeDetail>();
                    foreach (var code in barcodesFromDb)
                    {
                        if (distinceBarcodes.Any(i => i.ProductNumber == code.ProductNumber && i.OnlyFlag == code.OnlyFlag))
                        {
                            continue;
                        }
                        else
                        {
                            distinceBarcodes.Add(code);
                        }
                    }
                    var productNumbersFromBarcode = distinceBarcodes.Select(c => c.ProductNumber).ToList();
                    var products = _productContract.Products.Where(c => productNumbersFromBarcode.Contains(c.ProductNumber))
                                                                            .Select(c => new
                                                                            {
                                                                                c.ProductNumber,
                                                                                c.Id,
                                                                                c.ProductOriginNumber.TagPrice,
                                                                                c.ProductOriginNumber.WholesalePrice,
                                                                                c.ProductOriginNumber.PurchasePrice
                                                                            })
                                                                                .ToList();
                    var productNumbersFromProduct = products.Select(c => c.ProductNumber).ToList();
                    var errproduct = distinceBarcodes.Where(c => !productNumbersFromProduct.Contains(c.ProductNumber));
                    List<string> errbarcode = new List<string>();
                    if (errproduct.Any())
                    {
                        //与该商品相关的商品档案不存在
                        //写日志 待完善

                        errbarcode = errproduct.Select(c => c.ProductNumber + c.OnlyFlag).ToList();
                        //inves = new List<Inventory>(); //清空需要入库的数据
                        var errst = "";
                        if (errbarcode.Count > 4)
                            errst = string.Join(",", errbarcode.Take(4).ToArray());
                        else
                            errst = string.Join(",", errbarcode);
                        _logContract.Insert(new LogDto()
                        {
                            Description = string.Join(",", errbarcode) + "未查找到商品档案",

                        });
                        optresul.Message = "部分商品未查找到商品档案：" + errst + "……,详情请查看日志";
                    }
                    else
                    {

                        inves = distinceBarcodes.Select(c => new Inventory()
                        {
                            ProductNumber = c.ProductNumber,
                            OnlyFlag = c.OnlyFlag,
                            ProductLogFlag = Guid.NewGuid().ToString().Replace("-", ""),
                            ProductBarcode = c.ProductNumber + c.OnlyFlag,
                            StoreId = storage.StoreId,
                            StorageId = storageId,
                            ProductId = products.FirstOrDefault(g => g.ProductNumber == c.ProductNumber).Id,
                            Description = notes,

                        }).ToList();
                    }
                    if (inves.Any())
                    {
                        // 计算本次入库总吊牌价
                        float totalTagPrice = 0;
                        foreach (var inventory in inves)
                        {
                            totalTagPrice += products.First(p => p.Id == inventory.ProductId).TagPrice;
                        }
                        // 根据生成的库存信息,插入入库记录
                        var record = new InventoryRecord()
                        {
                            Quantity = inves.Count,
                            OperatorId = adminid,
                            StorageId = storageId,
                            StoreId = storage.StoreId,
                            TagPrice = totalTagPrice,
                            RecordOrderNumber = recordOrderNumber,
                            CreatedTime = CreatedTime.Value
                        };

                        using (var tran = _inventoryContract.GetTransaction())
                        {
                            var res = _inventoryRecordContract.Insert(record);
                            if (res.ResultType != OperationResultType.Success)
                            {
                                tran.Rollback();
                                return Json(OperationResult.Error("库存记录插入失败"));
                            }

                            // 将库存信息与入库记录进行关联
                            inves.Each(i => i.InventoryRecordId = record.Id);
                            // 保存库存信息
                            optresul = _inventoryContract.BulkInsert(inves);
                            if (optresul.ResultType != OperationResultType.Success)
                            {
                                tran.Rollback();
                                return Json(OperationResult.Error("保存库存信息失败"));
                            }
                            List<ProductTrack> listpt = new List<ProductTrack>();
                            foreach (var item in inves)
                            {
                                #region 商品追踪
                                ProductTrack pt = new ProductTrack();
                                pt.ProductNumber = item.ProductNumber;
                                pt.ProductBarcode = item.ProductNumber + item.OnlyFlag;
                                pt.Describe = String.Format(ProductOptDescTemplate.ON_PRODUCT_INVENTORY, storage.StorageName);

                                listpt.Add(pt);
                                #endregion
                            }
                            var resPT = _productTrackContract.BulkInsert(listpt);
                            if (resPT.ResultType != OperationResultType.Success)
                            {
                                tran.Rollback();
                                return Json(OperationResult.Error("商品追踪插入失败"));
                            }
                            List<string> proCodes = inves.Select(c => c.ProductBarcode).ToList();
                            var details = _productBarcodeDetailContract.productBarcodeDetails.Where(c => proCodes.Contains(c.ProductNumber + c.OnlyFlag));
                            details.Each(c => c.Status = 1);

                            var resPBD = _productBarcodeDetailContract.BulkUpdate(details);
                            if (resPBD.ResultType != OperationResultType.Success)
                            {
                                tran.Rollback();
                                return Json(OperationResult.Error("商品条码信息更新失败"));
                            }
                            productsModeList.RemoveAll(c => proCodes.Contains(c.ProductBarcode));
                            SessionAccess.Set(key, productsModeList);

                            tran.Commit();
                        }
                    }
                }
            }
            return Json(optresul, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取枚举值对应的描述信息
        /// </summary>
        /// <param name="enumName"></param>
        /// <returns></returns>
        private string GetDescriptionText(InventoryStatus enumName)
        {
            string str = enumName.ToString();
            FieldInfo field = enumName.GetType().GetField(str);
            object[] objs = field.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length == 0) return str;
            DescriptionAttribute da = (DescriptionAttribute)objs[0];
            return da.Description;
        }
        private List<InventoryDto_t> Deserializer(string da)
        {//ProduId=24&StorCou=2&StoreId=7&StorageId=6|ProduId=16&StorCou=2&StoreId=7&StorageId=6

            List<InventoryDto_t> liInventD = new List<InventoryDto_t>();
            if (!string.IsNullOrEmpty(da))
            {
                string[] li = da.Split("|");
                foreach (string st in li)
                {
                    InventoryDto_t invet = new InventoryDto_t();
                    string[] woli = st.Split("&");
                    foreach (string t in woli)
                    {
                        string[] keyval = t.Split("=");
                        var prop = typeof(InventoryDto_t).GetProperty(keyval[0]);
                        Type ty = prop.PropertyType;
                        prop.SetValue(invet, Convert.ChangeType(keyval[1], ty));
                    }
                    liInventD.Add(invet);
                }

            }
            return liInventD;

        }


        /// <summary>
        /// 设置/取消推荐品类
        /// </summary>
        /// <param name="type">1推荐,0取消推荐</param>
        /// <param name="storeId">商店id</param>
        /// <param name="bigProdNum">大品类名称</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetRecommend(int type, int storeId, string bigProdNum)
        {
            try
            {
                List<string> bigProdNums = new List<string>();
                var arr = bigProdNum.Split(',');
                for (int i = 0; i < arr.Length; i++)
                {
                    bigProdNums.Add(arr[i]);
                }
                //校验权限
                var inventoryQuery = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                    .Where(i => i.IsDeleted == false && i.IsEnabled == true);
                var availableStoreIds = inventoryQuery.Select(i => i.StoreId).Distinct().ToList();
                if (!availableStoreIds.Contains(storeId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "权限不足"));
                }
                //校验当前商店库存所有的品类中是否包含要操作的品类
                var bigProdNumberInStore = inventoryQuery.Where(i => i.StoreId == storeId).Select(i => i.Product.BigProdNum).Distinct().ToList();
                if (bigProdNums.Any(i => !bigProdNumberInStore.Contains(i)))
                {
                    return Json(new OperationResult(OperationResultType.Error, "商铺下未找到所有选中的分类"));
                }

                //查看记录是否已经存在
                var existDataList = _storeRecommendContract.StoreRecommends.Where(s => s.StoreId == storeId && bigProdNums.Contains(s.BigProdNum)).ToList();

                if (type == 1)  //设置推荐
                {
                    if (existDataList != null && existDataList.Count > 0 && existDataList.Count == arr.Length)//所有要推荐的品类都已经存在于数据表中了
                    {
                        //修改推荐状态
                        foreach (var item in existDataList)
                        {
                            item.IsEnabled = true;
                            item.IsDeleted = false;
                        }
                        //更新
                        var res = _storeRecommendContract.Update(existDataList.ToArray());
                        return Json(res);
                    }

                    //取出数据库中尚未插入过的记录
                    var arr2 = existDataList.Select(i => i.BigProdNum).ToList();
                    var dataToInsert = bigProdNums.Where(i => !arr2.Contains(i)).ToList();

                    //插入新记录
                    List<StoreRecommendDto> dtos = new List<StoreRecommendDto>();
                    foreach (var item in dataToInsert)
                    {
                        var dto = new StoreRecommendDto()
                        {
                            BigProdNum = item,
                            StoreId = storeId
                        };
                        dtos.Add(dto);
                    }
                    var result = _storeRecommendContract.Insert(dtos.ToArray());
                    return Json(result);
                }
                else   //取消推荐
                {
                    if (existDataList == null)
                    {
                        return Json(new OperationResult(OperationResultType.Success));
                    }
                    var res = _storeRecommendContract.Disable(existDataList.Select(i => i.Id).ToArray());
                    return Json(res);
                }

            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }
        }


        #region 全部生产静态页
        private void CreateHtml()
        {
            string strConfigPath = "/home/Product/";
            string strDate = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/";
            IQueryable<Brand> listBrand = _brandContract.Brands.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            IQueryable<Category> listCategory = _categoryContract.Categorys.Where(x => x.IsDeleted == false && x.IsDeleted == true);
            IQueryable<Color> listColor = _colorContract.Colors.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            IQueryable<Product> listProduct = _productContract.Products.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            int pageIndex = 1;
            int pageSize = 5;
            while (true)
            {
                IQueryable<Brand> listPartBrand = listBrand.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                var listEntity = from pb in listPartBrand
                                 join
                                  pro in listProduct
                                  on
                                 pb.Id equals pro.ProductOriginNumber.BrandId
                                 join
                                  ca in listCategory
                                  on
                                 pro.ProductOriginNumber.CategoryId equals ca.Id
                                 join
                                  co in listColor
                                 on
                                 pro.ColorId equals co.Id
                                 select new
                                 {
                                     Product = pro,
                                     BrandId = pb.Id,
                                     pb.BrandName,
                                     CategoryId = ca.Id,
                                     ca.CategoryName,
                                     ColorId = co.Id,
                                     co.ColorName
                                 };
                foreach (var entity in listEntity)
                {

                }

            }

            //IQueryable<
        }
        #endregion


        private class InventoryEntry
        {
            public string Id { get; set; }
            public string ParentId { get; set; }
            public string BigProdNum { get; set; }
            public string BrandName { get; set; }
            public string CategoryName { get; set; }
            public string ColorName { get; set; }
            public string IconPath { get; set; }
            public string ThumbnailPath { get; set; }
            public string SizeName { get; set; }
            public string SeasonName { get; set; }
            public int Quantity { get; set; }
            public string StoreName { get; set; }
            public string StorageName { get; set; }
            public string ProductName { get; set; }
            public float TagPrice { get; set; }
            public float RetailPrice { get; set; }
            public float WholesalePrice { get; set; }
            public float PurchasePrice { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsEnabled { get; set; }
            public DateTime UpdatedTime { get; set; }
            public DateTime CreatedTime { get; set; }
        }


    }
}
