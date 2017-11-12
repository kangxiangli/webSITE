using AutoMapper;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Log;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;
using Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Models;
using static Whiskey.ZeroStore.ERP.Website.Controllers.CommonController;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    public class ApiController : Controller
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CheckedController));

        protected readonly ICheckerContract _checkerContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IStorageContract _storageContract;
        protected readonly ICheckerItemContract _checkerItemContract;
        protected readonly IBrandContract _brandContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IProductTrackContract _productTrackContract;
        protected readonly IAdministratorContract _administratorContract;
        private readonly IOrderblankContract _orderblankContract;
        protected readonly IInventoryContract _inventoryContract;
        private readonly IProductBarcodeDetailContract _productBarcodeDetailContract;
        private readonly IProductContract _productContract;

        public ApiController(ICheckerContract checkerContract,
            IStoreContract storeContract,
            IStorageContract storageContract,
            ICheckerItemContract checkerItemContract,
            IBrandContract brandContract,
            ICategoryContract categoryContract,
            IProductTrackContract productTrackContract,
            IAdministratorContract administratorContract,
            IOrderblankContract orderblankContrac,
            IInventoryContract inventoryContract,
            IProductBarcodeDetailContract productBarcodeDetailContract,
            IProductContract productContract)
        {
            _administratorContract = administratorContract;
            _checkerContract = checkerContract;
            _storeContract = storeContract;
            _storageContract = storageContract;
            _checkerItemContract = checkerItemContract;
            _brandContract = brandContract;
            _categoryContract = categoryContract;
            _productTrackContract = productTrackContract;
            _orderblankContract = orderblankContrac;
            _inventoryContract = inventoryContract;
            _productBarcodeDetailContract = productBarcodeDetailContract;
            _productContract = productContract;
        }
        #endregion

        /// <summary>
        /// 声明一个静态,用来装载Session的数据
        /// </summary>
        private static List<CheckerItemDto> CheckerItemDtos { get; set; }

        /// <summary>
        /// 校验编码的长度至少为7
        /// </summary>
        private int NumberLength { get { return 7; } }

        /// <summary>
        /// 配货盘点存在Session中有效数据的key
        /// </summary>
        private string VaildNums { get { return "VaildNums"; } }

        /// <summary>
        /// 配货盘点存在Session中无效数据的key
        /// </summary>

        private string InvaildNums { get { return "InvaildNums"; } }

        #region 获取数据列表
        public JsonResult ApiGetList(int adminId, int pageIndex, int pageSize)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            try
            {
                if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
                {
                    opera.Message = "用户不存在";
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }


                var enableStoreIds = _storeContract.QueryManageStoreId(adminId);

                int count = _checkerContract.Checkers.Count(c => c.IsEnabled && !c.IsDeleted && enableStoreIds.Contains(c.StoreId));
                var list = _checkerContract.Checkers.Where(c => c.IsEnabled && !c.IsDeleted && enableStoreIds.Contains(c.StoreId)).OrderByDescending(c => c.CreatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(m => new
                {
                    m.CheckGuid,
                    m.StorageId,
                    m.Store.StoreName,
                    m.Storage.StorageName,
                    CheckerName = m.CheckerName,
                    CheckedCount = m.ValidQuantity + m.ResidueQuantity + m.MissingQuantity,
                    CreatedTime = m.UpdatedTime.ToString("yyyy-MM-dd hh:mm:ss"),
                    CheckerState = m.CheckerState
                }).ToList();

                opera.ResultType = OperationResultType.Success;
                opera.Data = list;
                opera.Other = count;
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                opera.Message = "服务器错误";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取某一家店盘点信息
        public JsonResult ApiGetModel(int adminId, string CheckGuid)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            try
            {
                if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
                {
                    opera.Message = "用户不存在";
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }

                var enableStoreIds = _storeContract.QueryManageStoreId(adminId);

                var list = _checkerContract.Checkers.Where(c => c.IsEnabled && !c.IsDeleted && enableStoreIds.Contains(c.StoreId) && c.CheckGuid == CheckGuid).ToList().Select(m => new
                {
                    m.Id,
                    m.CheckGuid,
                    ParentId = "",
                    m.StoreId,
                    m.StorageId,
                    m.BrandId,
                    m.CategoryId,
                    StoreName = m.Store != null ? m.Store.StoreName : "",
                    StorageName = m.Storage != null ? m.Storage.StorageName : "",
                    BrandName = _brandContract.Brands.FirstOrDefault(x => x.Id == m.BrandId && x.IsDeleted == false && x.IsEnabled == true) == null ? "全部" : _brandContract.Brands.FirstOrDefault(x => x.Id == m.BrandId && x.IsDeleted == false && x.IsEnabled == true).BrandName,
                    CategoryName = _categoryContract.Categorys.FirstOrDefault(x => x.Id == m.CategoryId && x.IsDeleted == false && x.IsEnabled == true) == null ? "全部" : _categoryContract.Categorys.FirstOrDefault(x => x.Id == m.CategoryId && x.IsDeleted == false && x.IsEnabled == true).CategoryName,
                    CheckerName = m.CheckerName,
                    BeforeCheckQuantity = m.BeforeCheckQuantity,
                    CheckedQuantity = m.CheckedQuantity,
                    CheckedCount = m.ValidQuantity + m.ResidueQuantity + m.MissingQuantity,
                    ValidCount = m.ValidQuantity,
                    ResidueCount = m.ResidueQuantity,
                    MissingCount = m.MissingQuantity,
                    Notes = m.Notes,
                    CreatedTime = m.UpdatedTime.ToString("yyyy-MM-dd hh:mm:ss"),
                    CheckerState = m.CheckerState,
                    AdminName = m.Operator != null && m.Operator.Member != null ? m.Operator.Member.RealName : "",
                    StorePhoto = m.Store != null ? m.Store.StorePhoto : ""
                }).FirstOrDefault();

                opera.ResultType = OperationResultType.Success;
                opera.Data = list;
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                opera.Message = "服务器错误";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region 展示盘点数量
        /// <summary>
        /// 展示盘点数量
        /// </summary>
        /// <param name="Id">盘点Id</param>
        /// <param name="Flag">参看CheckerItemFlag</param>
        /// <returns></returns>
        public JsonResult QuantityList(int adminId, string CheckGuid, int CheckerItemType, int pageIndex, int pageSize)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            try
            {
                if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
                {
                    opera.Message = "用户不存在";
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }

                var count = _checkerItemContract.CheckerItems.Count(c => c.CheckGuid == CheckGuid && !c.IsDeleted && c.IsEnabled && c.CheckerItemType == CheckerItemType);
                var list = _checkerItemContract.CheckerItems.Where(c => c.CheckGuid == CheckGuid && !c.IsDeleted && c.IsEnabled && c.CheckerItemType == CheckerItemType).OrderByDescending(c => c.CreatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(m => new
                {
                    ProductName = m.Product != null && m.Product.ProductOriginNumber != null ? m.Product.ProductOriginNumber.ProductName : "",
                    m.ProductBarcode,
                    m.CheckerItemType,
                    m.Id,
                    m.CheckGuid,
                    m.Sequence,
                    CreatedTime = m.CreatedTime.ToString("yyyy-MM-dd hh:mm:ss"),
                    AdminName = m.Operator != null && m.Operator.Member != null ? m.Operator.Member.MemberName : "",
                });

                opera.ResultType = OperationResultType.Success;
                opera.Data = list;
                opera.Other = count;
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                opera.Message = "服务器错误";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 查询权限下的店铺信息
        /// <summary>
        /// 查询权限下的店铺信息
        /// </summary>
        /// <param name="onlyAttach">仅归属店铺</param>
        /// <returns></returns>
        public JsonResult QueryManageStore(int adminId, bool? onlyAttach)
        {
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                return Json(OperationResult.Error("用户不存在"), JsonRequestBehavior.AllowGet);
            }

            var storeIds = _storeContract.QueryManageStoreId(adminId);
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
            })
                                          .ToList();
            storeDtos.Where(s => closedStoreIds.Contains(s.Id)).Each(s => s.IsClosed = true);
            storeDtos.Where(s => checkingStoreIds.Contains(s.Id)).Each(s => s.IsChecking = true);
            storeDtos.Where(s => orderblankingStoreIds.Contains(s.Id)).Each(s => s.IsOrderBlanking = true);
            return Json(new OperationResult(OperationResultType.Success, string.Empty, storeDtos), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取店铺下的可显示仓库
        /// <summary>
        /// 获取店铺下的可显示仓库
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public JsonResult GetStorageList(int adminId, int storeId = 0)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            if (storeId == 0)
            {
                var data = _storeContract.QueryManageStorage(adminId)
                                         .Select(s => new { s.StorageName, s.Id })
                                         .ToList();

                opera.ResultType = OperationResultType.Success;
                opera.Data = data;
            }
            else
            {
                var data = _storeContract.QueryManageStorage(adminId).Where(s => s.StoreId == storeId)
                                         .Select(s => new { s.StorageName, s.Id })
                                         .ToList();

                opera.ResultType = OperationResultType.Success;
                opera.Data = data;
            }
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取品牌列表
        /// <summary>
        /// 获取品牌列表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public JsonResult GetBrandList(int adminId)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            var data = _brandContract.Brands.Where(c => !c.IsDeleted && c.IsEnabled && c.ParentId != null && c.ParentId > 0).ToList().Select
                (b => new
                {
                    b.Id,
                    b.BrandName
                });

            opera.ResultType = OperationResultType.Success;
            opera.Data = data;

            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取款式列表
        /// <summary>
        /// 获取款式列表
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public JsonResult GetCategoryList(int adminId)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            var data = _categoryContract.Categorys.Where(c => !c.IsDeleted && c.IsEnabled && c.ParentId != null && c.ParentId > 0).ToList().Select
                (b => new
                {
                    b.Id,
                    b.CategoryName
                });

            opera.ResultType = OperationResultType.Success;
            opera.Data = data;

            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region 开启盘点时
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="OrderblakNum">当不会空时，表示配货盘点不做记录</param>
        /// <returns></returns>
        public JsonResult StartCheck(int adminId, int storeId, int storageId, int? brandId, int? categoryId, string notes = "", string OrderblakNum = "")
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            CheckerDto dto = new CheckerDto();
            dto.StoreId = storeId;
            dto.StorageId = storageId;
            dto.BrandId = brandId;
            dto.CategoryId = categoryId;
            dto.Notes = notes;
            dto.OperatorId = adminId;

            if (string.IsNullOrEmpty(OrderblakNum))
            {
                opera = StartCheckerInventory(dto);
            }
            else
            {
                opera = StartCheckerOrderblank(OrderblakNum);
            }
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 仓库盘点开始
        /// <summary>
        /// 仓库盘点开始
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private OperationResult StartCheckerInventory(CheckerDto dto)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success);
            IQueryable<Inventory> listInventory = FilterProduct(dto);
            Store store = _storeContract.Stores.FirstOrDefault(x => x.Id == dto.StoreId);
            Storage storage = store.Storages.FirstOrDefault(x => x.Id == dto.StorageId);

            if (_storeContract.IsInOrderblankingStat(store.Id))
            {
                return new OperationResult(OperationResultType.Error, "店铺正在配货中,无法盘点");
            }

            int totalCount = listInventory.Count();
            IQueryable<Checker> listChecker = _checkerContract.Checkers.Where(x => x.IsEnabled == true && x.IsDeleted == false && x.StorageId == dto.StorageId && x.StoreId == x.StoreId);
            Checker checker = listChecker.FirstOrDefault(x => x.CheckerState == CheckerFlag.Checking || x.CheckerState == CheckerFlag.Interrupt);
            //盘点数据存在，在原有的基础上进行盘点
            if (checker != null)
            {
                Mapper.CreateMap<Checker, CheckerDto>();
                CheckerDto dtoEntity = Mapper.Map<Checker, CheckerDto>(checker);
                dto.Id = dtoEntity.Id;
                dtoEntity.InvalidQuantity = 0;
                oper.Data = dtoEntity;
                oper.ResultType = OperationResultType.DataRepeat;
            }
            else
            {
                dto.CheckGuid = CreateChckerGuid();
                dto.CheckerName = store.StoreName + storage.StorageName + DateTime.Now.ToString("yyyy年MM月dd日");
                dto.CheckerState = CheckerFlag.Checking;
                dto.MissingQuantity = totalCount;
                dto.BeforeCheckQuantity = totalCount;
                oper = _checkerContract.Insert(dto);
                if (oper.ResultType == OperationResultType.Success)
                {
                    int[] arr = oper.Data as int[];
                    dto.Id = arr[0];
                    dto.InvalidQuantity = 0;
                    oper.Data = dto;
                    var inList = listInventory.Select(x => new
                    {
                        x.ProductBarcode,
                        x.Storage.StorageName
                    }).ToList();
                    foreach (var item in inList)
                    {
                        string numBarcode = item.ProductBarcode;
                        if (!string.IsNullOrEmpty(numBarcode))
                        {
                            //分解出商品货号                        
                            string ProductNumber = numBarcode.Substring(0, numBarcode.Length - 3);
                            #region 商品追踪
                            ProductTrackDto pt = new ProductTrackDto();
                            pt.ProductNumber = ProductNumber;
                            pt.ProductBarcode = numBarcode;
                            pt.Describe = string.Format(ProductOptDescTemplate.ON_PRODUCT_CHECKER_START, item.StorageName);

                            _productTrackContract.Insert(pt);
                            #endregion
                        }
                    }
                }
            }
            object obj = SessionAccess.Get("CheckerId");
            if (obj == null)
            {
                SessionAccess.Set("CheckerId", dto.Id, true);
            }
            return oper;
        }
        #endregion

        #region 配货盘点开始
        private OperationResult StartCheckerOrderblank(string OrderblakNum)
        {
            Orderblank orderblank = _orderblankContract.Orderblanks.FirstOrDefault(x => x.OrderBlankNumber == OrderblakNum);
            OperationResult oper = new OperationResult(OperationResultType.Success);
            CheckerDto dto = new CheckerDto();
            if (orderblank != null)
            {
                dto.StoreId = orderblank.ReceiverStoreId;
                dto.StorageId = orderblank.ReceiverStorageId;
            }
            oper.Data = dto;
            return oper;
        }
        #endregion

        #region 结束盘点
        /// <summary>
        /// 结束盘点
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult CheckerOk(int adminId, int Id)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            opera = _checkerContract.CheckerOk(Id, adminId);
            if (opera.ResultType == OperationResultType.Success)
            {
                ClearSess(adminId);
            }
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion



        #region 委托-根据盘点条件对商品进行筛选
        private IQueryable<Inventory> FilterProduct(CheckerDto dto)
        {
            IQueryable<Inventory> invertoryList = _inventoryContract.Inventorys.Where(x => x.StorageId == dto.StorageId && x.StoreId == dto.StoreId && x.Status == (int)InventoryStatus.Default);
            Func<Inventory, bool> predicate = (invent) => invent.IsEnabled == true && invent.IsDeleted == false;

            if (dto.BrandId != null && dto.BrandId > 0)
            {
                predicate += (
                  (invent) => invent.Product.ProductOriginNumber.BrandId == dto.BrandId
               );
            }
            if (dto.CategoryId != null)
            {
                predicate += (
                  (invent) => invent.Product.ProductOriginNumber.CategoryId == dto.CategoryId
               );
            }
            return invertoryList.Where(predicate).AsQueryable();
        }
        #endregion

        #region 生成GUid
        private string CreateChckerGuid()
        {
            IQueryable<Checker> checkers = _checkerContract.Checkers;
            string num = string.Empty;
            while (true)
            {
                num = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 9);
                int index = checkers.Where(x => x.CheckGuid == num).Count();
                if (index == 0)
                {
                    break;
                }
            }
            return num;
        }
        #endregion

        #region 修改商品数量
        /// <summary>
        /// 修改商品数量
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public JsonResult SetAmount(int adminId, string name, int value)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

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
        #endregion

        #region 查看无效数据
        /// <summary>
        /// 查看无效数据
        /// </summary>
        /// <returns></returns>
        public JsonResult Invalid(int adminId)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            opera.Data = (Session["ScanInvalid"] != null ? (List<Product_Model>)Session["ScanInvalid"] : new List<Product_Model>());
            opera.ResultType = OperationResultType.Success;
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加盘点数据
        /// <summary>
        /// 添加盘点数据
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="Id"></param>
        /// <param name="Number"></param>
        /// <param name="uuid"></param>
        /// <param name="orderblankNumber"></param>
        /// <returns></returns>
        public JsonResult AddToScan(int adminId, int Id, string Number, string uuid, string orderblankNumber)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(orderblankNumber))
            {
                oper = AddInventory(Id, Number, uuid);
            }
            else
            {
                oper = AddOrderblank(uuid, Number, orderblankNumber);
            }
            return Json(oper, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 添加配货盘点
        private OperationResult AddOrderblank(string uuid, string Number, string OrderblankNum)
        {
            try
            {
                Orderblank orderblank = _orderblankContract.Orderblanks.FirstOrDefault(x => x.OrderBlankNumber == OrderblankNum);
                List<OrderblankItem> listOrderblankItem = orderblank.OrderblankItems.ToList();
                //拿到存在Session中的有效和无效数据
                List<string> listVaildNum = new List<string>();
                List<string> listInvaildNum = new List<string>();
                object objVaildNum = SessionAccess.Get(VaildNums);
                object objInvaildNum = SessionAccess.Get(InvaildNums);
                if (objVaildNum != null)
                {
                    listVaildNum = objVaildNum as List<string>;
                }
                if (objInvaildNum != null)
                {
                    listInvaildNum = objInvaildNum as List<string>;
                }
                CheckedType checkedType = new CheckedType();
                object objcheckedType = SessionAccess.Get("CheckedType");
                if (objcheckedType != null)
                {
                    checkedType = objcheckedType as CheckedType;
                }
                int totalQuantity = listVaildNum == null ? 0 : listOrderblankItem.Count() - listVaildNum.Count();
                OperationResult oper = new OperationResult(OperationResultType.Success);
                bool isHave = false;
                bool isVaild = false;
                checkedType.CheckedQuantity += 1;
                if (!string.IsNullOrEmpty(Number) && Number.Length > NumberLength)
                {
                    isHave = listVaildNum.Any(x => x == Number);
                    if (isHave == true)
                    {
                        checkedType.InvalidQuantity += 1;
                        checkedType.OtherInfo = (int)CheckerItemFlag.Invalid;
                    }
                    else
                    {
                        //仓库和配货单中有数据校验成功
                        isHave = _inventoryContract.Inventorys
                            .Where(x => x.IsDeleted == false && x.IsEnabled == true)
                            .Any(x => x.ProductBarcode == Number);
                        isVaild = listOrderblankItem.Any(x => x.OrderBlankBarcodes.Contains(Number));
                        if (isHave == true && isVaild == true)
                        {
                            checkedType.ValidQuantity += 1;
                            totalQuantity -= 1;
                            listVaildNum.Add(Number);
                            checkedType.OtherInfo = (int)CheckerItemFlag.Valid;
                        }
                        else
                        {
                            checkedType.InvalidQuantity += 1;
                            checkedType.OtherInfo = (int)CheckerItemFlag.Invalid;
                        }
                    }
                }
                else
                {
                    checkedType.InvalidQuantity += 1;
                    checkedType.OtherInfo = (int)CheckerItemFlag.Invalid;
                }
                if (totalQuantity < 0)
                {
                    totalQuantity = 0;
                }
                checkedType.CheckQuantity = totalQuantity;
                checkedType.MissingQuantity = totalQuantity;
                SessionAccess.Set(VaildNums, listVaildNum, true);
                SessionAccess.Set(InvaildNums, listInvaildNum, true);
                checkedType.UUID = uuid;
                SessionAccess.Set("CheckedType", checkedType, true);
                return new OperationResult(OperationResultType.Success, "", checkedType);
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, ex.Message);
            }
        }
        #endregion

        #region 添加仓库盘点
        /// <summary>
        /// 添加仓库盘点
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Number"></param>
        /// <param name="uuid"></param>
        /// <returns></returns>
        private OperationResult AddInventory(int Id, string Number, string uuid)
        {
            IQueryable<CheckerItem> listCheckerItem = _checkerItemContract.CheckerItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CheckerId == Id);
            CheckerDto checkerDto = _checkerContract.Edit(Id);
            var checkedType = Session["_checkedInfo"] as CheckedType;
            if (checkedType != null)
            {
                if (checkerDto == null)
                {
                    return new OperationResult(OperationResultType.Error, "请先开启盘点", "");
                }

                checkerDto.InvalidQuantity = checkedType.InvalidQuantity;
                checkerDto.CheckedQuantity = checkedType.CheckedQuantity;
                checkerDto.ValidQuantity = checkedType.ValidQuantity;
                //计算缺货数量            
                checkerDto.MissingQuantity = checkedType.MissingQuantity;
                checkerDto.ResidueQuantity = checkedType.ResidueQuantity;
                checkerDto.CheckQuantity = checkedType.MissingQuantity;
                checkerDto.CheckerState = (CheckerFlag)checkedType.Resultype;
            }
            else
            {
                checkedType = new CheckedType();
            }
            //全部仓库信息
            IQueryable<Inventory> allInventories = _inventoryContract.Inventorys.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status == (int)InventoryStatus.Default);
            //获取打印条码列表
            IQueryable<ProductBarcodeDetail> listProBar = _productBarcodeDetailContract.productBarcodeDetails.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status == (int)ProductBarcodeDetailFlag.AddStorage);
            //获取盘点仓库的信息
            IQueryable<Inventory> listInventory = FilterProduct(checkerDto);
            //CheckedType checkedType = new CheckedType();
            OperationResult oper = new OperationResult(OperationResultType.Success);
            //装载无效数据
            List<CheckerItemDto> listInvalid = new List<CheckerItemDto>();
            object objInvalids = SessionAccess.Get("Invalids");
            if (objInvalids != null)
            {
                listInvalid = objInvalids as List<CheckerItemDto>;
            }
            if (!string.IsNullOrEmpty(Number))
            {
                //string[] arrNum = Number.Split(new char[','], StringSplitOptions.RemoveEmptyEntries);
                #region 盘点逻辑

                IQueryable<Product> listProduct = _productContract.Products.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                //拿到编码的长度
                int length = Number.Length;
                checkerDto.CheckedQuantity += 1;
                //根据编号查找商品是否存在
                if (length > NumberLength)
                {
                    int startIndex = Number.Length - 3;
                    //分解出商品货号                        
                    string strProductNumber = Number.Substring(0, startIndex);
                    string strOnlyFlag = Number.Substring(startIndex);
                    Product product = listProduct.FirstOrDefault(x => x.ProductNumber == strProductNumber);
                    CheckerItemDto checkerItemDto = new CheckerItemDto()
                    {
                        CheckerItemType = (int)CheckerItemFlag.Valid,
                        CheckGuid = checkerDto.CheckGuid,
                        CheckerId = checkerDto.Id,
                        ProductBarcode = Number,
                    };
                    if (product == null)
                    {
                        checkerDto.InvalidQuantity += 1;
                        checkerItemDto.CheckerItemType = (int)CheckerItemFlag.Invalid;
                        listInvalid.Add(checkerItemDto);
                    }
                    else
                    {
                        //盘点仓库是否有这件商品
                        Inventory inventory = listInventory.FirstOrDefault(x => x.ProductBarcode == Number);
                        if (inventory != null)
                        {
                            checkerItemDto.ProductId = inventory.ProductId;
                            //商品是否盘点过了
                            bool isHave = listCheckerItem.Any(x => x.ProductBarcode == Number);
                            if (isHave == false)
                            {
                                checkerDto.ValidQuantity += 1;
                                checkerDto.MissingQuantity -= 1;
                                checkerItemDto.CheckerItemType = (int)CheckerItemFlag.Valid;
                            }
                            else
                            {
                                checkerDto.InvalidQuantity += 1;
                                checkerItemDto.CheckerItemType = (int)CheckerItemFlag.Invalid;
                                listInvalid.Add(checkerItemDto);
                            }
                        }
                        else
                        {
                            //检验这件商品是否在其他库存中。
                            bool isHave = allInventories.Any(x => x.ProductBarcode == Number);
                            ProductBarcodeDetail proBar = listProBar.FirstOrDefault(x => x.ProductNumber == strProductNumber && x.OnlyFlag == strOnlyFlag);
                            if (isHave == true && proBar != null)
                            {
                                checkerItemDto.ProductId = proBar.ProductId;
                                //商品是否盘点过了
                                isHave = listCheckerItem.Any(x => x.ProductBarcode == Number);
                                if (isHave == false)
                                {
                                    checkerDto.ResidueQuantity += 1;
                                    checkerItemDto.CheckerItemType = (int)CheckerItemFlag.Surplus;
                                }
                                else
                                {
                                    checkerDto.InvalidQuantity += 1;
                                    checkerItemDto.CheckerItemType = (int)CheckerItemFlag.Invalid;
                                    listInvalid.Add(checkerItemDto);
                                }
                            }
                            else
                            {
                                checkerDto.InvalidQuantity += 1;
                                checkerItemDto.CheckerItemType = (int)CheckerItemFlag.Invalid;
                                listInvalid.Add(checkerItemDto);
                            }
                        }
                        //当不是无效数据的时候添加到数据库
                        if (checkerItemDto.CheckerItemType != (int)CheckerItemFlag.Invalid)
                        {
                            oper = _checkerItemContract.Insert(checkerItemDto);
                            string numBarcode = checkerItemDto.ProductBarcode;
                            if (!string.IsNullOrEmpty(numBarcode))
                            {
                                //分解出商品货号                        
                                string ProductNumber = numBarcode.Substring(0, numBarcode.Length - 3);
                                #region 商品追踪
                                ProductTrackDto pt = new ProductTrackDto();
                                pt.ProductNumber = ProductNumber;
                                pt.ProductBarcode = numBarcode;
                                pt.Describe = ProductOptDescTemplate.ON_PRODUCT_CHECKER_START;

                                _productTrackContract.Insert(pt);
                                #endregion
                            }
                            _checkerContract.Update(checkerDto);
                        }
                    }
                    checkedType.OtherInfo = checkerItemDto.CheckerItemType;
                }
                else
                {
                    checkerDto.InvalidQuantity += 1;
                    checkedType.OtherInfo = (int)CheckerItemFlag.Invalid;
                }

                #endregion
            }
            int invalidCount = listInvalid.Count();
            checkedType.InvalidQuantity = invalidCount;
            checkedType.CheckedQuantity = checkerDto.CheckedQuantity;
            checkedType.ValidQuantity = checkerDto.ValidQuantity;
            //计算缺货数量            
            checkedType.MissingQuantity = checkerDto.MissingQuantity;
            checkedType.ResidueQuantity = checkerDto.ResidueQuantity;
            checkedType.CheckQuantity = checkerDto.MissingQuantity;
            checkedType.Resultype = (int)checkerDto.CheckerState;
            checkedType.UUID = uuid;
            SessionAccess.Set("Invalids", listInvalid, true);
            SessionAccess.Set("_checkedInfo", checkedType, true);
            return new OperationResult(OperationResultType.Success, "", checkedType);
        }
        #endregion

        #region 继续盘点
        /// <summary>
        /// 继续盘点
        /// </summary>
        /// <returns></returns>
        public JsonResult ContinueChecker(int adminId)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            var chec = Session["_checkedInfo"] as CheckedType;
            opera.Data = chec;

            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 清除Session
        /// <summary>
        /// 清除Session
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private JsonResult ClearSess(int adminId)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            Session["checkCount_li"] = null;
            Session["checkedCount_li"] = null;
            Session["validCount_li"] = null;
            Session["residueCount_li"] = null;
            Session["invalidCount_li"] = null;
            Session["currCheckGuid"] = null;
            Session["_checkedInfo"] = null;

            opera.ResultType = OperationResultType.Success;
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 撤销一条盘点记录
        /// <summary>
        /// 撤销一条盘点记录
        /// </summary>
        /// <returns></returns>
        public JsonResult AnnulCheck(int adminId, string checkTyp, string num)
        {
            OperationResult opera = new OperationResult(OperationResultType.Error);
            if (!_administratorContract.CheckExists(a => a.Id == adminId && !a.IsDeleted && a.IsEnabled))
            {
                opera.Message = "该用户不存在";
                return Json(opera, JsonRequestBehavior.AllowGet);
            }

            int hasErr = 0;
            bool changeCheckCoun = true; //是否需要修改待盘点数量
            if (checkTyp == "validCount")
            {

                #region 改变有效盘点记录
                var validLis = Session["validCount_li"] as List<CheckDto_t>;
                if (validLis != null)
                {
                    var chedt = validLis.Where(c => c.ProductNumber == num).FirstOrDefault();
                    if (chedt != null && chedt.Quantity > 0)
                    {
                        chedt.Quantity = chedt.Quantity - 1;
                    }
                    if (chedt.Quantity <= 0)
                        validLis.Remove(chedt);
                    Session["validCount_li"] = validLis;
                }
                else
                {
                    hasErr = 1;
                }
                #endregion
            }
            else if (checkTyp == "invalidCount")
            {
                #region 改变无效盘点
                var invaLis = Session["invalidCount_li"] as List<CheckDto_t>;
                if (invaLis != null)
                {
                    var invDt = invaLis.Where(c => c.ProductNumber == num).FirstOrDefault();
                    if (invDt != null)
                    {
                        invDt.Quantity = invDt.Quantity - 1;
                        if (invDt.Quantity <= 0)
                            invaLis.Remove(invDt);
                    }
                    Session["invalidCount_li"] = invaLis;
                }
                else
                {
                    hasErr = 1;
                }
                changeCheckCoun = false;
                #endregion
            }
            else if (checkTyp == "residueCount")
            {
                var residLis = Session["residueCount_li"] as List<CheckDto_t>;
                if (residLis != null)
                {
                    var resiDat = residLis.Where(c => c.ProductNumber == num).FirstOrDefault();
                    if (resiDat != null)
                    {
                        resiDat.Quantity = resiDat.Quantity - 1;
                        if (resiDat.Quantity <= 0)
                            residLis.Remove(resiDat);
                    }
                    Session["residueCount_li"] = residLis;
                }
                else
                {
                    hasErr = 1;
                }
                changeCheckCoun = false;

            }
            else { }


            #region 改变待盘点记录
            if (changeCheckCoun)
            {
                var checkLis = Session["checkCount_li"] as List<CheckDto_t>;
                if (checkLis != null)
                {
                    var che = checkLis.Where(c => c.ProductNumber == num).FirstOrDefault();
                    if (che != null)
                    {
                        che.Quantity = che.Quantity + 1;
                    }
                    else
                    {
                        checkLis.Add(new CheckDto_t()
                        {
                            ProductNumber = num,
                            Quantity = 1
                        });
                    }
                    Session["checkCount_li"] = checkLis;
                }
            }

            #endregion

            #region 改变已盘点记录
            var checkedlis = Session["checkedCount_li"] as List<CheckDto_t>;
            if (checkedlis != null)
            {
                var che = checkedlis.Where(c => c.ProductNumber == num).FirstOrDefault();
                if (che != null)
                {
                    che.Quantity = che.Quantity - 1;
                    if (che.Quantity <= 0)
                        checkedlis.Remove(che);
                }
                Session["checkedCount_li"] = checkedlis;
            }
            else
                hasErr = 1;
            #endregion
            CheckedType resul = GetCheckInfo();
            resul.OtherInfo = hasErr;
            opera.Data = resul;
            return Json(opera, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取当前盘点，已盘、有效、无效……的数量
        /// <summary>
        /// 获取当前盘点，已盘、有效、无效……的数量
        /// </summary>
        /// <returns></returns>
        private CheckedType GetCheckInfo()
        {
            CheckedType che = new CheckedType();
            var cheLi = Session["checkCount_li"] as List<CheckDto_t>;
            if (cheLi != null)
                che.CheckQuantity = cheLi.Select(c => c.Quantity).Sum();
            var checkedLi = Session["checkedCount_li"] as List<CheckDto_t>;
            if (checkedLi != null)
                che.CheckedQuantity = checkedLi.Select(c => c.Quantity).Sum();
            var validLi = Session["validCount_li"] as List<CheckDto_t>;
            if (validLi != null)
                che.ValidQuantity = validLi.Select(c => c.Quantity).Sum();
            var invaliLi = Session["invalidCount_li"] as List<CheckDto_t>;
            if (invaliLi != null)
                che.InvalidQuantity = invaliLi.Select(c => c.Quantity).Sum();
            var residLi = Session["residueCount_li"] as List<CheckDto_t>;
            if (residLi != null)
                che.ResidueQuantity = residLi.Select(c => c.Quantity).Sum();
            return che;
        }
        #endregion


        #region 获取详细盘点数据列表
        /// <summary>
        /// 获取详细盘点数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult CheckerDetailList(int checkerItemType, int checkerId, int pageIndex, int pageSize)
        {
            OperationResult opera = new OperationResult(OperationResultType.Success);
            var count = 0;
            if (checkerItemType == (int)CheckerItemFlag.Invalid)
            {
                List<CheckerItemDto> listDto = new List<CheckerItemDto>();
                if (CheckerItemDtos != null && CheckerItemDtos.Count > 0)
                {
                    listDto = CheckerItemDtos;
                }

                count = listDto.Count(x => x.CheckerItemType == (int)CheckerItemFlag.Invalid);
                var invalids = listDto.Where(x => x.CheckerItemType == (int)CheckerItemFlag.Invalid).OrderByDescending(x => x.ProductBarcode).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(x => new
                {
                    x.ProductBarcode,
                    x.ProductId,
                    ProductName = _productContract.Edit(x.ProductId ?? 0).ProductName
                }).ToList();

                opera.Data = invalids;
                opera.Other = count;
                return Json(opera, JsonRequestBehavior.AllowGet);
            }
            //筛选缺货，需要从盘点仓库比对
            IQueryable<CheckerItem> listCheckerItem = _checkerItemContract.CheckerItems;
            if (checkerItemType == (int)CheckerItemFlag.Lack)
            {
                CheckerDto checkerDto = _checkerContract.Edit(checkerId);
                if (checkerDto != null)
                {
                    //根据条件筛选盘点仓库
                    IQueryable<Inventory> listInventory = FilterProduct(checkerDto);
                    IQueryable<CheckerItem> listEntities = listCheckerItem.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CheckerId == checkerId && x.CheckerItemType == (int)CheckerItemFlag.Valid);
                    //List<int?> listProductId = new List<int?>();
                    List<string> listProductbars = new List<string>();
                    if (listEntities != null && listEntities.Count() > 0)
                    {
                        listProductbars = listEntities.Select(x => x.ProductBarcode).Distinct().ToList();
                    }
                    var list_Inventory = listInventory.Where(x => !listProductbars.Contains(x.ProductBarcode)).DistinctBy(b => b.ProductBarcode).ToList();
                    count = list_Inventory.Count();
                    var entities = list_Inventory
                    .OrderByDescending(x => x.CreatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(x => new
                    {
                        x.ProductBarcode,
                        x.ProductId,
                        ProductName = _productContract.Edit(x.ProductId).ProductName
                    }).ToList();
                    opera.Data = entities;
                    opera.Other = count;
                    return Json(opera, JsonRequestBehavior.AllowGet);
                }
            }
            count = _checkerItemContract.CheckerItems.Count(x => x.CheckerItemType == checkerItemType && !x.IsDeleted && x.IsEnabled && x.CheckerId == checkerId);
            var list = _checkerItemContract.CheckerItems.Where(x => x.CheckerItemType == checkerItemType && !x.IsDeleted && x.IsEnabled && x.CheckerId == checkerId).OrderByDescending(x => x.CreatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList().Select(x => new
            {
                x.ProductBarcode,
                x.ProductId,
                ProductName = _productContract.Edit(x.ProductId ?? 0).ProductName
            }).ToList();
            opera.Data = list;
            opera.Other = count;
            return Json(opera, JsonRequestBehavior.AllowGet);

        }
        #endregion
    }
}