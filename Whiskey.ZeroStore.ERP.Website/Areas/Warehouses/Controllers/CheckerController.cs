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
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Transfers.OfficeInfo;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models;
using Whiskey.ZeroStore.ERP.Website.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Log;
using System.Web.Script.Serialization;
using System.Web.Security;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    public class CheckerController : BaseController
    {


        #region 初始化操作对象

        private static readonly ILogger _Logger = LogManager.GetLogger(typeof(BuyController));

        private readonly IProductContract _productContract;
        private readonly IStoreContract _storeContract;
        private readonly IStorageContract _storageContract;
        private readonly IBrandContract _brandContract;
        private readonly ICategoryContract _categoryContract;
        private readonly ISeasonContract _seasonContract;
        private readonly ISizeContract _sizeContract;
        private readonly IColorContract _colorContract;
        private readonly IInventoryContract _inventoryContract;
        private readonly ICheckerContract _checkerContract;
        private readonly ICheckerItemContract _checkItemContract;
        private readonly IOrderblankContract _orderblankContract;
        private readonly IProductBarcodeDetailContract _productBarcodeDetailContract;
        private readonly IOrderblankItemContract _orderblankItemContract;
        private readonly IProductTrackContract _productTrackContract;
        private readonly IAdministratorContract _administratorContract;

        public CheckerController(ICheckerContract checkerContract,
            ICheckerItemContract checkItemContract,
            IProductContract productContract,
            IStoreContract storeContract,
            IStorageContract storageContract,
            IBrandContract brandContract,
            ICategoryContract categoryContract,
            ISeasonContract seasonContract,
            ISizeContract sizeContract,
            IColorContract colorContract,
            IInventoryContract inventoryContract,
            IOrderblankContract orderblankContract,
            IProductBarcodeDetailContract productBarcodeDetailContract,
            IOrderblankItemContract orderblankItemContract,
            IProductTrackContract productTrackContract,
            IAdministratorContract administratorContract)
        {
            _checkerContract = checkerContract;
            _checkItemContract = checkItemContract;
            _productContract = productContract;
            _storeContract = storeContract;
            _storageContract = storageContract;
            _brandContract = brandContract;
            _categoryContract = categoryContract;
            _seasonContract = seasonContract;
            _sizeContract = sizeContract;
            _colorContract = colorContract;
            _inventoryContract = inventoryContract;
            _orderblankContract = orderblankContract;
            _productBarcodeDetailContract = productBarcodeDetailContract;
            _orderblankItemContract = orderblankItemContract;
            _productTrackContract = productTrackContract;
            _administratorContract = administratorContract;
        }
        #endregion

        /// <summary>
        /// 声明一个静态,用来装载Session的数据
        /// </summary>
        private static List<CheckerItemDto> CheckerItemDtos { get; set; }

        /// <summary>
        /// 盘点Cookie的key
        /// </summary>
        private string CheckerKey { get { return "2ac5975b964b7152"; } }

        /// <summary>
        /// 配货盘点Cookie的key
        /// </summary>
        private string OrderblankKey { get { return "b13955235245b249"; } }

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

        #region 初始化界面
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IsContinute">可选参数（false表示不继续，true表示继续）</param>
        /// <param name="Id">盘点Id</param>
        /// <returns></returns>
        [Layout]
        public ActionResult Index(bool IsContinute = false, string CheckGuid = "", string _onum = "")
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;
            var listStorage = CacheAccess.GetManagedStorageListItem(_storageContract, _administratorContract, false);
            CheckerDto checkerDto = new CheckerDto();
            string OrderblakNum = string.Empty;
            if (IsContinute == true && !string.IsNullOrEmpty(CheckGuid))
            {
                checkerDto = this.CheckerInventory(CheckGuid);
            }
            else
            {
                if (!string.IsNullOrEmpty(_onum))
                {
                    OrderblakNum = _onum;
                    checkerDto = this.CheckerOrderblank(_onum);
                }
            }
            ViewBag.StorageList = listStorage;
            ViewBag.BrandList = CacheAccess.GetBrand(_brandContract, true, false);
            ViewBag.CategoryList = CacheAccess.GetCategory(_categoryContract, true);
            ViewBag.IsContinute = IsContinute;
            ViewBag.OrderblakNum = OrderblakNum;
            return View(checkerDto);
        }
        #endregion

        #region 配货盘点
        private CheckerDto CheckerOrderblank(string num)
        {
            CheckerDto checkerDto = new CheckerDto();
            Orderblank orderblank = _orderblankContract.Orderblanks.FirstOrDefault(x => x.OrderBlankNumber == num);
            SessionAccess.Set("OrderblankNum", num, true);
            int totalCount = orderblank.OrderblankItems.Count();
            checkerDto.OrberblankId = orderblank.Id;
            //checkerDto.CheckQuantity = totalCount;
            checkerDto.CheckerName = DateTime.Now.ToString("yyyy年MM月dd日") + "配货盘点";
            checkerDto.CheckGuid = CreateChckerGuid();
            checkerDto.StoreId = orderblank.ReceiverStoreId;
            checkerDto.StorageId = orderblank.ReceiverStorageId;
            checkerDto.CheckerState = CheckerFlag.Checking;
            return checkerDto;
        }
        #endregion

        #region 仓库盘点
        private CheckerDto CheckerInventory(string checkGuid)
        {
            Checker checker = _checkerContract.Checkers.FirstOrDefault(x => x.CheckGuid == checkGuid);
            Mapper.CreateMap<Checker, CheckerDto>();
            CheckerDto checkerDto = Mapper.Map<Checker, CheckerDto>(checker);
            return checkerDto;
        }
        #endregion

        public ActionResult Create(PurchaseDto model)
        {
            var result = new OperationResult(OperationResultType.Error, "保存采购订单出错！");
            var list = Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>();

            if (list.Count > 0)
            {

            }
            else
            {

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #region 注释代码

        //public ActionResult AddToScan(string uuid, string number)
        //{
        //    var result = new OperationResult(OperationResultType.Error, "");
        //    if (Session["ScanValid"] == null) Session["ScanValid"] = new List<Product_Model>();
        //    if (Session["ScanInvalid"] == null) Session["ScanInvalid"] = new List<Product_Model>();
        //    if (number != null && number.Length > 0)
        //    {
        //        try
        //        {
        //            var validItem = new Product_Model();
        //            var validList = (List<Product_Model>)Session["ScanValid"];
        //            var invalidList = (List<Product_Model>)Session["ScanInvalid"];
        //            number = InputHelper.SafeInput(number);
        //            if (validList.Where(m => m.UUID == uuid).Count() == 0)
        //            {
        //                var entity = _productContract.Products.Where(m => m.IsDeleted == false && m.IsVerified == true && m.IsEnabled == true).FirstOrDefault(m => m.ProductNumber == number);
        //                if (entity != null)
        //                {
        //                    validItem = validList.FirstOrDefault(m => m.ProductNumber == entity.ProductNumber);
        //                    if (validItem != null)
        //                    {
        //                        validItem.Amount++;
        //                        validItem.UpdateTime = DateTime.Now;
        //                    }
        //                    else
        //                    {
        //                        validList.Add(new Product_Model
        //                        {
        //                            Id = entity.Id,
        //                            UUID = uuid,
        //                            Thumbnail = entity.ThumbnailPath,
        //                            InternalName = entity.InternalName,
        //                            ProductNumber = entity.ProductNumber,
        //                            TagPrice = entity.TagPrice,
        //                            RetailPrice = entity.RetailPrice,
        //                            WholesalePrice = entity.WholesalePrice,
        //                            Season = entity.Season.SeasonName,
        //                            Size = entity.Size.SizeName,
        //                        });
        //                    }
        //                    Session["ScanValid"] = validList;
        //                }
        //                else
        //                {
        //                    var invalidItem = invalidList.FirstOrDefault(m => m.ProductNumber == number);
        //                    if (invalidItem != null)
        //                    {
        //                        invalidItem.Amount++;
        //                        invalidItem.UpdateTime = DateTime.Now;
        //                    }
        //                    else
        //                    {
        //                        invalidList.Add(new Product_Model
        //                        {
        //                            UUID = uuid,
        //                            ProductNumber = number
        //                        });
        //                    }
        //                    Session["ScanInvalid"] = invalidList;
        //                }


        //                result = new OperationResult(OperationResultType.Success, "产品已进入缓存列表！", new { UUID = uuid, validCount = validList.Sum(m => m.Amount), invalidCount = invalidList.Sum(m => m.Amount) });

        //            }
        //            else
        //            {

        //                result = new OperationResult(OperationResultType.Error, "产品进入缓存列表出错，错误如下：此UUID已存在，不允许重复提交！");

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            result = new OperationResult(OperationResultType.Error, "产品进入缓存列表出错，错误如下：" + ex.Message, ex.ToString());
        //        }
        //    }
        //    else
        //    {
        //        result = new OperationResult(OperationResultType.Error, "扫码货号不能为空！");
        //    }
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region 添加盘点数据

        public JsonResult AddToScan(int Id, string Number, string uuid, string orderblankNumber)
        {
            OperationResult oper = new OperationResult(OperationResultType.Success);
            //object obj = SessionAccess.Get("OrderblankNum");
            if (string.IsNullOrEmpty(orderblankNumber))
            {
                oper.Data = AddInventory(Id, Number, uuid);
            }
            else
            {
                oper.Data = AddOrderblank(uuid, Number, orderblankNumber);
            }
            return Json(oper);
            #region 注销代码
            //IQueryable<CheckerItem> listCheckerItem = _checkItemContract.CheckerItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CheckerId == Id);
            //CheckerDto checkerDto = _checkerContract.Edit(Id);
            ////IQueryable<Inventory> listInventory = FilterProduct(checkerDto);         
            //string OrderblankNum = SessionAccess.Get("OrderblankNum").ToString();
            //Orderblank orderblank = _orderblankContract.Orderblanks.FirstOrDefault(x => x.OrderBlankNumber == OrderblankNum);
            //List<OrderblankItem> listOrderblankItem = orderblank.OrderblankItems.ToList();
            //CheckedType checkedType = new CheckedType();
            //checkedType.CheckQuantity = checkerDto.CheckQuantity;
            ////checkedType.CheckQuantity = listInventory.Count() - checkerDto.CheckedQuantity;


            //if (!string.IsNullOrEmpty(Number))
            //{
            //    string[] arrNum = Number.Split(new char[','], StringSplitOptions.RemoveEmptyEntries);
            //    foreach (string num in arrNum)
            //    {
            //        if (!string.IsNullOrEmpty(num))
            //        {
            //            checkedType.CheckedQuantity += 1;
            //            checkerDto.CheckedQuantity += 1;
            //            Product product = _productContract.Products.FirstOrDefault(x => x.ProductNumber == num);
            //            int count = listCheckerItem.Where(x => x.ProuctNum == num).Count();
            //            if (count > 0 || product == null)
            //            {
            //                checkedType.InvalidQuantity += 1;
            //                checkerDto.InvalidQuantity += 1;
            //            }
            //            else
            //            {
            //                OrderblankItem orderblankItem = listOrderblankItem.FirstOrDefault(x => x.ProductId == product.Id);
            //                if (orderblankItem == null)
            //                {
            //                    checkedType.InvalidQuantity += 1;
            //                    checkerDto.InvalidQuantity += 1;
            //                }
            //                else
            //                {
            //                    checkedType.ValidQuantity += 1;
            //                    checkedType.CheckQuantity -= 1;
            //                    checkerDto.ValidQuantity += 1;
            //                    checkerDto.CheckQuantity -= 1;
            //                    CheckerItemDto checkerItemDto = new CheckerItemDto()
            //                    {
            //                        CheckerItemType = (int)CheckerItemFlag.Valid,
            //                        CheckGuid = checkerDto.CheckGuid,
            //                        CheckerId = checkerDto.Id,
            //                        ProductId = orderblankItem.ProductId,
            //                        ProuctNum = num,
            //                    };
            //                    _checkerContract.Update(checkerDto);
            //                    oper = _checkItemContract.Insert(checkerItemDto);
            //                }
            //            }
            //        }
            //    }
            //}
            //checkedType.UUID = uuid;
            //oper.Data = checkedType;
            //return Json(oper);
            #endregion

        }

        #endregion

        #region 添加配货盘点
        private CheckedType AddOrderblank(string uuid, string Number, string OrderblankNum)
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
                    }
                    else
                    {
                        checkedType.InvalidQuantity += 1;
                    }
                }
                #region 注释代码

                //string[] arrNum = Number.Split(new char[','], StringSplitOptions.RemoveEmptyEntries);
                //foreach (string num in arrNum)
                //{
                //    if (!string.IsNullOrEmpty(num))
                //    {
                //        checkedType.CheckedQuantity += 1;                         
                //        Product product = _productContract.Products.FirstOrDefault(x => x.ProductNumber == num);
                //        int count = listNum.Where(x => x == num).Count();
                //        if (count > 0 || product == null)
                //        {
                //            checkedType.InvalidQuantity += 1;                             
                //        }
                //        else
                //        {
                //            OrderblankItem orderblankItem = listOrderblankItem.FirstOrDefault(x => x.ProductId == product.Id);
                //            if (orderblankItem == null)
                //            {
                //                checkedType.InvalidQuantity += 1;                             
                //            }
                //            else
                //            {
                //                checkedType.ValidQuantity += 1;
                //                checkedType.CheckQuantity -= 1;
                //                listNum.Add(num);
                //            }
                //        }
                //    }
                //}
                #endregion
            }
            else
            {
                checkedType.InvalidQuantity += 1;
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
            return checkedType;
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
        private CheckedType AddInventory(int Id, string Number, string uuid)
        {
            IQueryable<CheckerItem> listCheckerItem = _checkItemContract.CheckerItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CheckerId == Id);
            //从cookie内加载数据
            CheckerDto checkerDto = ReadCookie(CheckerKey);
            if (checkerDto == null)
            {
                checkerDto = _checkerContract.Edit(Id);
            }
            //全部仓库信息
            IQueryable<Inventory> allInventories = _inventoryContract.Inventorys.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status == (int)InventoryStatus.Default);
            //获取打印条码列表
            IQueryable<ProductBarcodeDetail> listProBar = _productBarcodeDetailContract.productBarcodeDetails.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status == (int)ProductBarcodeDetailFlag.AddStorage);
            //获取盘点仓库的信息
            IQueryable<Inventory> listInventory = FilterProduct(checkerDto);
            CheckedType checkedType = new CheckedType();
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
                            oper = _checkItemContract.Insert(checkerItemDto);
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
                }
                else
                {
                    checkerDto.InvalidQuantity += 1;
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
            this.WriteCookie(checkerDto, CheckerKey);
            SessionAccess.Set("Invalids", listInvalid, true);
            return checkedType;
        }
        #endregion

        #region 初始化详细盘点数据界面
        /// <summary>
        /// 初始化详细盘点数据界面
        /// </summary>
        /// <param name="CheckerState"></param>
        /// <returns></returns>
        public ActionResult CheckerDetail(int CheckerItemType, int CheckerId = 0)
        {
            ViewBag.CheckerItemType = CheckerItemType;
            object objDtos = SessionAccess.Get("Invalids");
            if (objDtos != null)
            {
                CheckerItemDtos = objDtos as List<CheckerItemDto>;
            }
            return PartialView();
        }
        #endregion

        #region 获取详细盘点数据列表
        /// <summary>
        /// 获取详细盘点数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> CheckerDetailList()
        {
            GridRequest request = new GridRequest(Request);

            object objCheckerState = null;
            FilterRule filterRule = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "CheckerItemType");
            //获取盘点类型
            int checkerItemType = 0;
            if (filterRule != null)
            {
                objCheckerState = filterRule.Value;
                checkerItemType = int.Parse(objCheckerState.ToString());
            }
            Expression<Func<CheckerItem, bool>> predicate = FilterHelper.GetExpression<CheckerItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                int index = 0;
                var count = 0;
                if (checkerItemType == (int)CheckerItemFlag.Invalid)
                {
                    List<CheckerItemDto> listDto = new List<CheckerItemDto>();
                    if (CheckerItemDtos != null && CheckerItemDtos.Count > 0)
                    {
                        listDto = CheckerItemDtos;
                        count = CheckerItemDtos.Count();
                    }
                    int pageIndex = request.PageCondition.PageIndex;
                    int pageSize = request.PageCondition.PageSize;

                    var list = listDto.Where(x => x.CheckerItemType == (int)CheckerItemFlag.Invalid).OrderBy(x => x.ProductBarcode).Skip(pageIndex).Take(pageSize).Select(x => new
                    {
                        Id = index + 1,
                        x.ProductBarcode,
                        x.CheckerItemType,
                    }).ToList();
                    return new GridData<object>(list, count, request.RequestInfo);
                }
                else
                {
                    //筛选缺货，需要从盘点仓库比对
                    IQueryable<CheckerItem> listCheckerItem = _checkItemContract.CheckerItems;
                    if (checkerItemType == (int)CheckerItemFlag.Lack)
                    {
                        //获取盘点Id
                        object objCheckerId = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "CheckerId").Value;
                        int checkerId = 0;
                        bool isNum = int.TryParse(objCheckerId.ToString(), out checkerId);
                        if (isNum == true)
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
                                int pageIndex = request.PageCondition.PageIndex;
                                int pageSize = request.PageCondition.PageSize;
                                listInventory = listInventory.Where(x => !listProductbars.Contains(x.ProductBarcode)).DistinctBy(b => b.ProductBarcode).AsQueryable();
                                count = listInventory.Count();
                                var entities = listInventory
                                .OrderBy(x => x.Id).Skip(pageIndex).Take(pageSize).Select(x => new
                                {
                                    Id = index + 1,
                                    x.ProductBarcode,
                                    CheckerItemType = (int)CheckerItemFlag.Lack,
                                }).ToList();
                                return new GridData<object>(entities, count, request.RequestInfo);
                            }
                        }
                    }
                    var list = _checkItemContract.CheckerItems.Where<CheckerItem, int>(predicate, request.PageCondition, out count).Select(x => new
                    {
                        Id = index + 1,
                        x.ProductBarcode,
                        x.CheckerItemType,
                    }).ToList();
                    return new GridData<object>(list, count, request.RequestInfo);
                }

            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        #endregion


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

        #region 注销代码
        //public ActionResult List(DataTable_Model param)
        //{

        //    var list = Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>();
        //    var count = list.Count();

        //    if (param.sSearch != null && param.sSearch.Length > 0)
        //    {
        //        list = list.FindAll(m => (m.ProductNumber + m.Color + m.Size).Contains(param.sSearch));
        //    }

        //    Reverser<Product_Model> reverser;
        //    var columns = param.sColumns.Split(',');
        //    if (columns.Length > 0)
        //    {
        //        var sortColumn = columns[param.iSortCol_0];
        //        if (sortColumn != null)
        //        {
        //            var sortDirection = param.sSortDir_0;

        //            if (sortDirection == "asc")
        //            {
        //                reverser = new Reverser<Product_Model>((new Product_Model()).GetType(), sortColumn, ReverserInfo.Direction.ASC);
        //            }
        //            else
        //            {
        //                reverser = new Reverser<Product_Model>((new Product_Model()).GetType(), sortColumn, ReverserInfo.Direction.DESC);
        //            }

        //            list.Sort(reverser);
        //        }

        //    }
        //    var data = param.iDisplayLength > 0 ? list.Skip(param.iDisplayStart).Take(param.iDisplayLength) : list.Skip(param.iDisplayStart);

        //    return Json(new
        //    {
        //        sEcho = param.sEcho,
        //        iDisplayStart = param.iDisplayStart,
        //        iTotalRecords = count,
        //        iTotalDisplayRecords = count,
        //        aaData = data
        //    }, JsonRequestBehavior.AllowGet);

        //}
        #endregion

        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);

            Expression<Func<CheckerItem, bool>> predicate = FilterHelper.GetExpression<CheckerItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                object checkerId = Session["CheckerId"];
                checkerId = checkerId ?? 0;
                int id = int.Parse(checkerId.ToString());
                IQueryable<CheckerItem> listCheckerItem = _checkItemContract.CheckerItems.Where(x => x.IsEnabled == true && x.IsDeleted == false && x.ProductId != null && x.CheckerId == id)
                    .Where(x => x.CheckerItemType == (int)CheckerItemFlag.Valid);
                var list = listCheckerItem.Where<CheckerItem, int>(predicate, request.PageCondition, out count).Select(x => new
                {
                    x.Id,
                    Thumbnail = x.Product.ThumbnailPath,
                    ProductName = x.Product.ProductName,
                    ProductNumber = x.Product.ProductNumber,
                    Season = x.Product.ProductOriginNumber.Season.SeasonName,
                    Color = x.Product.Color.ColorName,
                    Size = x.Product.Size.SizeName,
                    WholesalePrice = x.Product.ProductOriginNumber.WholesalePrice,
                    Amount = 1,
                    x.IsDeleted,
                    x.IsEnabled,
                    x.UpdatedTime
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
                #region 注销代码

                //object objNum = SessionAccess.Get("ListNum");
                //if (objNum==null)
                //{
                //    object checkerId = Session["CheckerId"];
                //    checkerId = checkerId ?? 0;
                //    int id = int.Parse(checkerId.ToString());
                //    IQueryable<CheckerItem> listCheckerItem = _checkItemContract.CheckerItems.Where(x => x.IsEnabled == true && x.IsDeleted == false && x.ProductId != null && x.CheckerId == id);
                //    var list = listCheckerItem.Where<CheckerItem, int>(predicate, request.PageCondition, out count).Select(x => new
                //    {
                //        x.Id,
                //        Thumbnail = x.Product.ThumbnailPath,
                //        InternalName = x.Product.ProductName,
                //        ProductNumber = x.Product.ProductNumber,
                //        Season = x.Product.Season.SeasonName,
                //        Color = x.Product.Color.ColorName,
                //        Size = x.Product.Size.SizeName,
                //        WholesalePrice = x.Product.WholesalePrice,
                //        Amount = 1,
                //        x.IsDeleted,
                //        x.IsEnabled,
                //        x.UpdatedTime
                //    }).ToList();
                //    return new GridData<object>(list, count, request.RequestInfo);
                //}
                //else
                //{
                //    List<string> ListNum= objNum as List<string>;
                //    var list = _productContract.Products.Where(x => ListNum.Contains(x.ProductNumber)).Select(x => new
                //    {
                //        x.Id,
                //        Thumbnail = x.ThumbnailPath,
                //        InternalName = x.ProductName,
                //        ProductNumber = x.ProductNumber,
                //        Season = x.Season.SeasonName,
                //        Color = x.Color.ColorName,
                //        Size = x.Size.SizeName,
                //        WholesalePrice = x.WholesalePrice,
                //        Amount = 1,
                //        x.IsDeleted,
                //        x.IsEnabled,
                //        x.UpdatedTime
                //    });
                //    return new GridData<object>(list, count, request.RequestInfo);
                //}
                #endregion
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            if (Id != null && Id.Length > 0)
            {
                var validList = (Session["ScanValid"] != null ? (List<Product_Model>)Session["ScanValid"] : new List<Product_Model>());
                validList.RemoveAll(m => Id.Contains(Convert.ToInt32(m.Id)));
                Session["ScanValid"] = validList;
                return Json(new OperationResult(OperationResultType.Success, "采购商品移除成功！", new { validCount = validList.Sum(m => m.Amount) }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new OperationResult(OperationResultType.Error, "移除ID列表不能为空！"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 继续盘点
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ContinueChecker()
        {
            var chec = Session["_checkedInfo"] as CheckedType;
            if (chec != null)
            {
                return Json(chec);
            }
            return null;

        }

        #region 注释代码-根据条件获取需要盘点的商品列表

        /// <summary>
        /// 根据条件获取需要盘点的商品列表
        /// </summary>
        /// <param name="reqDa"></param>
        /// <returns>返回商品总数</returns>
        //public int GetCheckList(string reqDa)
        //{
        //    //"StoreID=1&StorageID=1&BrandID=-1&CategoryId=&SeasonID=&SizeID=-1&ColorID=&CreateTime=&Notes="
        //    ClearSess();
        //    int idcou = 0; ;

        //    if (!string.IsNullOrEmpty(reqDa))
        //    {
        //        string[] reqs = reqDa.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
        //        var repex = _inventoryContract.Inventorys.Where(c => c.IsDeleted == false && c.IsEnabled == true);
        //        CheckedType chedTyp = new CheckedType();
        //        foreach (var rq in reqs)
        //        {
        //            #region MyRegion
        //            if (rq.Contains("StoreID"))
        //            {
        //                var te = rq.Split("=", true);
        //                if (te.Count() == 2 && te[1] != "-1")
        //                {
        //                    int stoId = int.Parse(te[1]);
        //                    repex = repex.Where(c => c.StoreId == stoId);
        //                    chedTyp.StoreId = te[1];
        //                }
        //            }
        //            else if (rq.Contains("StorageID"))
        //            {
        //                var te = rq.Split("=", true);
        //                if (te.Count() == 2 && te[1] != "-1")
        //                {
        //                    int storaId = int.Parse(te[1]);
        //                    repex = repex.Where(c => c.StorageId == storaId);
        //                    chedTyp.StorageId = te[1];
        //                }
        //            }
        //            else if (rq.Contains("BrandID"))
        //            {
        //                var te = rq.Split("=", true);
        //                if (te.Count() == 2 && te[1] != "-1")
        //                {
        //                    int brandId = int.Parse(te[1]);
        //                    repex = repex.Where(c => c.Product.BrandId == brandId);
        //                    chedTyp.BrandId = brandId;
        //                }


        //            }
        //            else if (rq.Contains("CategoryId"))
        //            {
        //                var te = rq.Split("=", true);
        //                if (te.Count() == 2 && te[1] != "-1")
        //                {
        //                    int categoryId = int.Parse(te[1]);
        //                    repex = repex.Where(c => c.Product.CategoryId == categoryId);
        //                    chedTyp.CategoryId = categoryId;
        //                }
        //            }
        //            else if (rq.Contains("SeasonID"))
        //            {
        //                var te = rq.Split("=", true);
        //                if (te.Count() == 2 && te[1] != "-1")
        //                {
        //                    int seasonId = int.Parse(te[1]);
        //                    repex = repex.Where(c => c.Product.SeasonId == seasonId);
        //                    chedTyp.SeasonId = seasonId;
        //                }
        //            }
        //            else if (rq.Contains("SizeID"))
        //            {
        //                var te = rq.Split("=", true);
        //                if (te.Count() == 2 && te[1] != "-1")
        //                {
        //                    int sizeId = int.Parse(te[1]);
        //                    repex = repex.Where(c => c.Product.SizeId == sizeId);
        //                    chedTyp.SizeId = sizeId;
        //                }
        //            }
        //            else if (rq.Contains("ColorID"))
        //            {
        //                var te = rq.Split("=", true);
        //                if (te.Count() == 2 && te[1] != "-1")
        //                {
        //                    int colorId = int.Parse(te[1]);
        //                    repex = repex.Where(c => c.Product.ColorId == colorId);
        //                    chedTyp.ColorId = colorId;
        //                }


        //            }
        //            else if (rq.Contains("CreateTime"))
        //            {
        //                var te = rq.Split("=", true);
        //                if (te.Count() == 2)
        //                {
        //                    //inv.CreatedTime=new DateTime()
        //                    DateTime dt = Convert.ToDateTime(te[1]);
        //                    repex = repex.Where(c => c.CreatedTime == dt);
        //                    chedTyp.CreateTime = dt;
        //                }
        //            }
        //            else if (rq.Contains("Notes"))
        //            {
        //                var te = rq.Split("=", true);
        //                if (te.Count() == 2 && te[1] != "")
        //                {
        //                    //repex = repex.Where(c => c.Description.Contains(te[1]));
        //                    chedTyp.Notes = te[1];
        //                }
        //            }
        //            else { }
        //            #endregion
        //        }

        //        var ids = repex.Select(c => new CheckDto_t()
        //        {
        //            ProductNumber = c.Product.ProductNumber,
        //            //Quantity = c.Quantity
        //        }).ToList();
        //        ids = (from c in ids group c by c.ProductNumber into b select new CheckDto_t() { ProductNumber = b.Key, Quantity = b.Sum(c => c.Quantity) }).ToList();

        //        if (ids.Count > 0)
        //            Session["checkCount_li"] = ids;
        //        idcou = ids.Select(c => c.Quantity).Sum();

        //        chedTyp.CheckQuantity = idcou;
        //        chedTyp.MissingQuantity = idcou;
        //        Session["_checkedInfo"] = chedTyp;
        //    }
        //    string cheTy = Request["ty"];
        //    if (!string.IsNullOrEmpty(cheTy))
        //    {
        //        //盘点开始时插入盘点记录 todo
        //        InsertStartInfo();
        //    }
        //    return idcou;
        //}
        #endregion

        public void ClearSess()
        {
            Session["checkCount_li"] = null;
            Session["checkedCount_li"] = null;
            Session["validCount_li"] = null;
            Session["residueCount_li"] = null;
            Session["invalidCount_li"] = null;
            Session["currCheckGuid"] = null;
            Session["_checkedInfo"] = null;
        }
        /// <summary>
        /// 获取当前盘点，已盘、有效、无效……的数量
        /// </summary>
        /// <returns></returns>
        public CheckedType GetCheckInfo()
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
        public ActionResult CheckerStart()
        {
            // uuid: globalUUID, number: scanNumber
            CheckedType chty = new CheckedType();
            List<CheckDto_t> li = Session["checkCount_li"] as List<CheckDto_t>;
            li = new List<CheckDto_t>();
            li.Add(new CheckDto_t() { ProductNumber = "2213sd", Quantity = 22 });
            if (li != null)
            {
                chty.CheckQuantity = 100;//li.Select(c => c.Quantity).Sum();
                string _num = Request["number"];
                chty.UUID = Request["uuid"];
                var cheda = li.Where(c => c.ProductNumber == _num).FirstOrDefault();
                #region 已盘点数量
                var checkedList = Session["checkedCount_li"] as List<CheckDto_t>;
                if (checkedList == null)
                {
                    checkedList = new List<CheckDto_t>();
                }
                bool iscont = checkedList.Select(c => c.ProductNumber).Contains(_num);
                if (iscont)
                {
                    var te = checkedList.Where(c => c.ProductNumber == _num).FirstOrDefault();
                    te.Quantity = te.Quantity + 1;
                }
                else
                {
                    checkedList.Add(new CheckDto_t()
                    {
                        ProductNumber = _num,
                        Quantity = 1
                    });
                }
                Session["checkedCount_li"] = checkedList;
                chty.CheckedQuantity = checkedList.Sum(c => c.Quantity);
                #endregion
                var checkList = Session["checkCount_li"] as List<CheckDto_t>;
                if (cheda != null && cheda.Quantity > 0) //在待盘记录中找到该记录并且待盘数量大于0
                {
                    #region 待盘数量
                    chty.Resultype = 1; //有效
                    //待盘数量
                    cheda.Quantity = cheda.Quantity - 1;
                    Session["checkCount_li"] = li;
                    chty.CheckQuantity = li.Select(c => c.Quantity).Sum();
                    #endregion

                    #region 有效数量
                    //有效数量
                    var validCountSes = Session["validCount_li"] as List<CheckDto_t>;
                    if (validCountSes == null)
                        validCountSes = new List<CheckDto_t>();
                    var valDa = validCountSes.Where(c => c.ProductNumber == _num).FirstOrDefault();
                    if (valDa != null)
                    {
                        valDa.Quantity = valDa.Quantity + 1;

                    }
                    else
                    {
                        validCountSes.Add(new CheckDto_t()
                        {
                            ProductNumber = _num,
                            Quantity = 1
                        });
                    }

                    Session["validCount_li"] = validCountSes;
                    chty.ValidQuantity = validCountSes.Select(c => c.Quantity).Sum();

                    #endregion

                }
                else if (cheda != null && cheda.Quantity == 0) //余货
                {
                    #region 余货
                    chty.Resultype = 4;
                    var residueCountSes = Session["residueCount_li"] as List<CheckDto_t> ?? new List<CheckDto_t>();

                    var residueDat = residueCountSes.Where(c => c.ProductNumber == _num).FirstOrDefault();
                    if (residueDat == null)
                    {
                        residueCountSes.Add(new CheckDto_t()
                        {
                            ProductNumber = _num,
                            Quantity = 1
                        });
                    }
                    else
                    {
                        residueDat.Quantity = residueDat.Quantity + 1;
                    }
                    Session["residueCount_li"] = residueCountSes;
                    chty.ResidueQuantity = residueCountSes.Select(c => c.Quantity).Sum();
                    #endregion
                }

                else if (cheda == null) //在列表中未找到
                {
                    var chedaList = _productContract.Products.Where(c => c.ProductNumber == _num);

                    if (chedaList != null && chedaList.Count() > 0) //余货
                    {
                        #region 余货
                        chty.Resultype = 4;
                        var residueCountSes = Session["residueCount_li"] as List<CheckDto_t> ?? new List<CheckDto_t>();

                        var residueDat = residueCountSes.Where(c => c.ProductNumber == _num).FirstOrDefault();
                        if (residueDat == null)
                        {
                            residueCountSes.Add(new CheckDto_t()
                            {
                                ProductNumber = _num,
                                Quantity = 1
                            });
                        }
                        else
                        {
                            residueDat.Quantity = residueDat.Quantity + 1;
                        }
                        Session["residueCount_li"] = residueCountSes;
                        chty.ResidueQuantity = residueCountSes.Select(c => c.Quantity).Sum();
                        #endregion
                    }
                    else
                    {
                        #region 无效货号

                        chty.Resultype = 2;
                        var invalidCountSes = Session["invalidCount_li"] as List<CheckDto_t> ?? new List<CheckDto_t>();
                        var invalDat = invalidCountSes.Where(c => c.ProductNumber == _num).FirstOrDefault();
                        if (invalDat == null)
                        {
                            invalidCountSes.Add(new CheckDto_t()
                            {
                                ProductNumber = _num,
                                Quantity = 1
                            });
                        }
                        else
                        {
                            invalDat.Quantity = invalDat.Quantity + 1;

                        }
                        Session["invalidCount_li"] = invalidCountSes;
                        chty.InvalidQuantity = invalidCountSes.Select(c => c.Quantity).Sum();
                        #endregion

                    }
                }
                else { }
                chty.MissingQuantity = chty.CheckQuantity;

            }
            return Json(chty);

        }
        [HttpPost]
        public ActionResult GetCheckData()
        {
            GridRequest reque = new GridRequest(Request);
            //rowStart是从0开始的
            int rowStart = Request["iDisplayStart"].CastTo<int>(0);
            int pageLeng = Request["iDisplayLength"].CastTo<int>(10);
            string getType = Request["type"].CastTo<string>();
            List<RetuCheckData> li = new List<RetuCheckData>();



            var checkList = new List<CheckDto_t>();

            //待盘和缺货
            if (getType == "checkCount" || getType == "missingCount")
            {
                checkList = Session["checkCount_li"] as List<CheckDto_t>;

            }
            else if (getType == "checkedCount")
            {
                checkList = Session["checkedCount_li"] as List<CheckDto_t>;
                if (checkList != null && checkList.Count > 0)
                {
                    var numList = checkList.Select(c => c.ProductNumber);
                    #region 从盘点记录中查找数据
                    var dali = _inventoryContract.Inventorys.Where(b => b.IsDeleted == false && b.IsEnabled == true && numList.Contains(b.Product.ProductNumber)).DistinctBy(c => c.Product.ProductNumber).Select(c => new RetuCheckData()
                    {
                        Id = c.Id,
                        ProductNumber = c.Product.ProductNumber,
                        Thumbnail = c.Product.ThumbnailPath,
                        // Color = string.IsNullOrEmpty(c.Product.ProductColor) ? c.Product.Colors.Where(b => b.Id == c.Product.ColorId).FirstOrDefault().ColorName : c.Product.ProductColor,
                        Color = c.Product.Color.ColorName,
                        Size = c.Product.Size.SizeName,
                        Count = checkList.Where(x => x.ProductNumber == c.Product.ProductNumber).FirstOrDefault().Quantity

                    }).ToList();
                    li.AddRange(dali);
                    #endregion
                    #region 从商品表中查数据
                    var rediLi = numList.Except(dali.Select(c => c.ProductNumber).ToList());//在库存中未找到的ID集合
                    List<string> numLis = new List<string>();
                    if (_productContract.Products.Where(c => rediLi.Contains(c.ProductNumber)).Count() > 0)
                    {
                        var rediDaList = _productContract.Products.Where(c => rediLi.Contains(c.ProductNumber)).Select(c => new RetuCheckData()
                        {
                            Id = c.Id,
                            Thumbnail = c.ThumbnailPath,
                            ProductNumber = c.ProductNumber,
                            Color = c.Color.ColorName,
                            Size = c.Size.SizeName,
                            Count = checkList.Where(x => x.ProductNumber == c.ProductNumber).FirstOrDefault().Quantity
                        }).ToList();
                        li.AddRange(rediDaList);
                        numLis = rediDaList.Select(c => c.ProductNumber).ToList();
                    }

                    #endregion
                    #region 在盘点记录和商品表中都找不到的数据作为无效数据
                    //在当前仓库和商品中都找不到的ID集合
                    var invaliList = rediLi.Except(numLis);
                    foreach (var item in invaliList)
                    {
                        li.Add(new RetuCheckData()
                        {
                            Id = -1,
                            Thumbnail = "",
                            ProductNumber = item,
                            ProductName = "",
                            Color = "",
                            Size = "",
                            Count = checkList.Where(c => c.ProductNumber == item).FirstOrDefault().Quantity
                        });
                    }
                    #endregion
                }

            }

            else if (getType == "validCount")
            {
                checkList = Session["validCount_li"] as List<CheckDto_t>;
            }
            else if (getType == "invalidCount")
            {
                #region 无效的盘点数据
                checkList = Session["invalidCount_li"] as List<CheckDto_t>;
                if (checkList != null && checkList.Count > 0)
                {
                    li.AddRange(checkList.Select(c => new RetuCheckData()
                    {
                        Id = -1,
                        ProductNumber = c.ProductNumber,
                        Thumbnail = "",
                        ProductName = "",
                        Color = "",
                        Size = "",
                        Count = c.Quantity
                    }));
                }

                #endregion
            }
            else if (getType == "residueCount")
            {
                #region 余货
                checkList = Session["residueCount_li"] as List<CheckDto_t>;
                if (checkList != null && checkList.Count > 0)
                {

                    var pnums = checkList.Select(x => x.ProductNumber).ToList();
                    var proli = _productContract.Products.Where(c => pnums.Contains(c.ProductNumber)).ToList();

                    var datLi = proli.Select(c => new RetuCheckData()
                    {
                        Id = c.Id,
                        ProductNumber = c.ProductNumber,
                        Thumbnail = c.ThumbnailPath,
                        Color = c.Color.ColorName,
                        Size = c.Size.SizeName,
                        Count = checkList.Where(b => b.ProductNumber == c.ProductNumber).ToList().FirstOrDefault().Quantity
                    }).ToList();
                    li.AddRange(datLi);
                }
                #endregion
            }

            //else if (getType == "backoutCount")
            //    checkList = Session["backoutCount_li"] as List<CheckData_t>;
            else
            {
            }

            if (checkList != null && checkList.Count > 0 && li.Count == 0)
            {
                var numList = checkList.Select(c => c.ProductNumber);
                var dali = _inventoryContract.Inventorys.Where(b => b.IsDeleted == false && b.IsEnabled == true && numList.Contains(b.Product.ProductNumber)).DistinctBy(c => c.Product.ProductNumber).Select(c => new RetuCheckData()
                {
                    Id = c.Id,
                    ProductNumber = c.Product.ProductNumber,
                    Thumbnail = c.Product.ThumbnailPath,
                    // Color = string.IsNullOrEmpty(c.Product.ProductColor) ? c.Product.Colors.Where(b => b.Id == c.Product.ColorId).FirstOrDefault().ColorName : c.Product.ProductColor,
                    Color = c.Product.Color.ColorName,
                    Size = c.Product.Size.SizeName,

                    Count = checkList.Where(x => x.ProductNumber == c.Product.ProductNumber).FirstOrDefault().Quantity

                }).ToList();
                li.AddRange(dali);
            }
            var griDa = li.OrderBy(c => c.ProductNumber).Skip(rowStart).Take(pageLeng).ToList();
            GridData<object> dat = new GridData<object>(griDa, li.Count, reque.RequestInfo);
            return Json(dat);
        }

        #region 注释代码- 插入一条盘点记录

        /// <summary>
        /// 插入一条盘点记录
        /// </summary>
        /// <returns></returns>
        //public JsonResult InsertChecked()
        //{
        //    /*
        //       ty: "unload", notes:notes
        //     */

        //    OperationResult rsult = new OperationResult(OperationResultType.Error);
        //    string cheTy = Request["ty"];
        //    if (!string.IsNullOrEmpty(cheTy))
        //    {
        //        switch (cheTy)
        //        {
        //            //开始盘点
        //            case "start":
        //                {
        //                    rsult = InsertStartInfo();
        //                    break;
        //                }
        //            //操作中断，未完成盘点  重置 、刷新
        //            case "unload":
        //                {
        //                    rsult = InsertCheckInfo(Request, 2);
        //                    ClearSess();
        //                    break;
        //                }
        //            //完成盘点
        //            case "save":
        //                {
        //                    rsult = InsertCheckInfo(Request, 3);
        //                    ClearSess();
        //                    break;
        //                }
        //            //撤消盘点
        //            case "annul":
        //                {
        //                    rsult = InsertCheckInfo(Request, 4);
        //                    break;
        //                }
        //            default:
        //                break;
        //        }
        //    }
        //    return null;
        //}
        #endregion

        #region 注销代码--插入一条盘点记录

        /// <summary>
        /// 插入一条盘点记录
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="state">2：盘点中断 3：盘点完成 4：盘点撤消</param>
        /// <returns></returns>
        //private OperationResult InsertCheckInfo(HttpRequestBase Request, int state)
        //{
        //    OperationResult resul = new OperationResult(OperationResultType.Error);

        //    string notes = Request["notes"] as string;
        //    CheckerItemDto checItem = new CheckerItemDto()
        //    {
        //        CheckGuid = GetCurrCheckGuid(),
        //        CheckQuantity = GetCheckItemStr("checkCount"),
        //        CheckedQuantity = GetCheckItemStr("checkedCount"),
        //        InvalidQuantity = GetCheckItemStr("invalidCount"),
        //        ValidQuantity = GetCheckItemStr("validCount"),
        //        MissingQuantity = GetCheckItemStr("missingCount"),
        //        ResidueQuantity = GetCheckItemStr("residueCount"),
        //        Notes = notes,
        //        CheckerState = state
        //    };
        //    string CheckGuid = GetCurrCheckGuid();
        //    Checker cher = _checkerContract.Checkers.Where(c => c.CheckGuid == CheckGuid).FirstOrDefault();
        //    if (cher != null)
        //    {
        //        cher.CheckGuid = GetCurrCheckGuid();
        //        cher.CheckQuantity = GetCheckerItemCount("checkCount");
        //        cher.CheckedQuantity = GetCheckerItemCount("checkedCount");
        //        cher.ValidQuantity = GetCheckerItemCount("validCount");
        //        cher.InvalidQuantity = GetCheckerItemCount("invalidCount");
        //        cher.MissingQuantity = GetCheckerItemCount("missingCount");
        //        cher.ResidueQuantity = GetCheckerItemCount("residueCount");
        //        cher.UpdatedTime = DateTime.Now;
        //        cher.Notes = notes;

        //        switch (state)
        //        {
        //            case 2:
        //                { //盘点中断
        //                    cher.CheckerState = 2;
        //                    break;
        //                }
        //            case 3:
        //                {//盘点完成
        //                    cher.CheckerState = 3;
        //                    cher.Storage.CheckLock = false;//取消盘点锁定
        //                    break;
        //                }
        //            case 4: //盘点撤消，对应的db状态依然是 盘点中
        //                {
        //                    cher.CheckerState = 1;
        //                    break;
        //                }
        //        }
        //    }
        //    CheckerDto checkDto = AutoMapper.Mapper.Map<CheckerDto>(cher);
        //    resul = _checkerContract.Update(checkDto);
        //    if (resul.ResultType == OperationResultType.Success)
        //    {
        //        resul = _checkItemContract.Insert(checItem);

        //    }
        //    return resul;
        //}
        #endregion

        #region 注释代码 插入一条开始盘点记录

        /// <summary>
        /// 插入一条开始盘点记录
        /// </summary>
        //private OperationResult InsertStartInfo()
        //{
        //    OperationResult res = new OperationResult(OperationResultType.Error);
        //    var che = Session["_checkedInfo"] as CheckedType;
        //    if (che != null)
        //    {
        //        string cheStr = new JavaScriptSerializer().Serialize(che);//将初始查询条件序列化保存
        //        res = _checkerContract.Insert(new Checker[]
        //        {
        //                     new Checker()
        //                     {
        //                         CheckGuid=GetCurrCheckGuid(),
        //                        StoreId=int.Parse(che.StoreId),
        //                        StorageId=int.Parse(che.StorageId),
        //                        CheckQuantity=che.CheckQuantity,
        //                        MissingQuantity=che.MissingQuantity,
        //                        CheckerName=GetCheckedName(int.Parse(che.StoreId),int.Parse(che.StorageId)),
        //                        CheckedQuantity=0,
        //                        ValidQuantity=0,
        //                        InvalidQuantity=0,
        //                        ResidueQuantity=0,
        //                        CheckerState=1,
        //                        Notes=che.Notes,
        //                        SelectWhere=cheStr
        //                     }
        //                    });
        //        if (res.ResultType == OperationResultType.Success)
        //        {
        //            var checkitem = new CheckerItem()
        //            {
        //                CheckGuid = GetCurrCheckGuid(),
        //                CheckQuantity = GetCheckItemStr("checkCount"),
        //                CheckedQuantity = GetCheckItemStr("checkedCount"),
        //                ValidQuantity = GetCheckItemStr("validCount"),
        //                InvalidQuantity = GetCheckItemStr("invalidCount"),
        //                MissingQuantity = GetCheckItemStr("missingCount"),
        //                ResidueQuantity = GetCheckItemStr("residueCount"),
        //                CreatedTime = DateTime.Now,
        //                CheckerState = 1,
        //                Notes = che.Notes,
        //                OperatorId = AuthorityHelper.OperatorId
        //            };
        //            CheckerItemDto chitdto = AutoMapper.Mapper.Map<CheckerItemDto>(checkitem);
        //            res = _checkItemContract.Insert(chitdto);
        //            if (res.ResultType == OperationResultType.Success)
        //            {
        //                Storage storag = _storageContract.Storages.Where(c => c.Id == Convert.ToInt32(che.StorageId) && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
        //                if (storag != null)
        //                {
        //                    storag.CheckLock = true;//盘点锁定
        //                    StorageDto dto = AutoMapper.Mapper.Map<StorageDto>(storag);
        //                    _storageContract.Update(dto);
        //                }
        //            }
        //        }
        //    }
        //    return res;
        //}
        #endregion

        /// <summary>
        /// 获取盘点各项对应的商品编号string格式
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string GetCheckItemStr(string str)
        {
            string resu = "";
            List<CheckDto_t> li = null;
            switch (str)
            {

                #region MyRegion
                case "checkCount":
                    {
                        li = Session["checkCount_li"] as List<CheckDto_t>;
                        break;
                    }
                case "missingCount":
                    {
                        li = Session["checkCount_li"] as List<CheckDto_t>;
                        break;
                    }
                case "checkedCount":
                    {
                        li = Session["checkedCount_li"] as List<CheckDto_t>;
                        break;
                    }
                case "validCount":
                    {
                        li = Session["validCount_li"] as List<CheckDto_t>;
                        break;
                    }
                case "residueCount":
                    {
                        li = Session["residueCount_li"] as List<CheckDto_t>;
                        break;
                    }
                case "invalidCount":
                    {
                        li = Session["invalidCount_li"] as List<CheckDto_t>;
                        break;
                    }
                default:
                    break;
                    #endregion
            }
            if (li != null)
            {
                resu = string.Join(",", li.Select(c => c.ProductNumber + "|" + c.Quantity).ToArray());
            }
            return resu;

        }
        /// <summary>
        /// 获取盘点的各项对应的数量
        /// </summary>
        /// <returns></returns>
        private int GetCheckerItemCount(string str)
        {
            List<CheckDto_t> li = new List<CheckDto_t>();
            switch (str)
            {
                #region MyRegion
                case "checkCount":
                    {
                        li = Session["checkCount_li"] as List<CheckDto_t>;
                        break;
                    }

                case "missingCount": //缺货和待盘数量相同
                    {
                        li = Session["checkCount_li"] as List<CheckDto_t>;
                        break;
                    }
                case "checkedCount":
                    {
                        li = Session["checkedCount_li"] as List<CheckDto_t>;
                        break;
                    }
                case "validCount":
                    {
                        li = Session["validCount_li"] as List<CheckDto_t>;
                        break;
                    }
                case "residueCount":
                    {
                        li = Session["residueCount_li"] as List<CheckDto_t>;
                        break;
                    }
                case "invalidCount":
                    {
                        li = Session["invalidCount_li"] as List<CheckDto_t>;
                        break;
                    }
                default:
                    break;
                    #endregion
            }
            if (li == null || li.Count == 0)
            {
                return 0;
            }
            else
                return li.Select(c => c.Quantity).Sum();
        }
        /// <summary>
        /// 获取当前盘点对应的编号
        /// </summary>
        /// <returns></returns>
        private string GetCurrCheckGuid()
        {
            string num = Session["currCheckGuid"] as string;
            if (string.IsNullOrEmpty(num))
            {
                num = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 9);
                Session["currCheckGuid"] = num;
            }
            return num;
        }
        /// <summary>
        /// 获取盘点名称
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="storageId"></param>
        /// <returns></returns>
        private string GetCheckedName(int storeId, int storageId)
        {
            string resul = Guid.NewGuid().ToString();
            string storeName = "", storageName = "";

            var store = _storeContract.Stores.Where(c => c.Id == storeId && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
            if (store != null)
                storeName = store.StoreName;
            var storag = _storageContract.Storages.Where(c => c.Id == storageId && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
            if (storag != null)
                storageName = storag.StorageName;

            if (!string.IsNullOrEmpty(storeName) || !string.IsNullOrEmpty(storageName))
            {
                resul = storeName + storageName + DateTime.Now.ToLongDateString();
            }
            return resul;
        }
        /// <summary>
        /// 撤销一条盘点记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AnnulCheck()
        {
            //cheTyp:opt,num:_num

            /*
             Session["checkCount_li"] = null;
            Session["checkedCount_li"] = null;
            Session["validCount_li"] = null;
            Session["residueCount_li"] = null;
            Session["invalidCount_li"] = null;
             */

            int hasErr = 0;
            bool changeCheckCoun = true; //是否需要修改待盘点数量
            string checkTyp = Request["cheTyp"];
            string num = Request["num"];
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
            return Json(resul);
        }

        #region 初始化导入界面
        /// <summary>
        /// 批量导入
        /// </summary>
        /// <returns></returns>
        //[OutputCache(Duration = 3600 * 12)]
        public ActionResult BatchImport()
        {
            //Response.Cache.SetOmitVaryStar(true);
            return PartialView();
        }
        #endregion

        #region 上传导入数据

        //yxk
        public JsonResult ExcelFileUpload()
        {

            OperationResult resul = new OperationResult(OperationResultType.Error);
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
                    var _key = "sess_excel_inpor_instorage_12";
                    SessionAccess.Set(_key, da, true);

                }
                return da;
            }
            return null;
        }
        #endregion

        #endregion

        #region 校验盘点数据
        public JsonResult ExcelBatchStrageCheck(int Id, string orderblankNumber)
        {
            var _key = "sess_excel_inpor_instorage_12";
            OperationResult oper = new OperationResult(OperationResultType.Success);
            List<List<string>> dat = SessionAccess.Get(_key) as List<List<string>>;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(orderblankNumber))
            {
                //配货盘点，不记录在表中
                CheckedType checkedType = VaidData(dat, orderblankNumber);
                oper.Data = checkedType;
            }
            else
            {
                CheckedType checkedType = VaidData(Id, dat);
                oper.Data = checkedType;
            }
            SessionAccess.Remove(_key);
            return Json(oper);
        }

        #endregion

        #region 配货盘点，不记录在表中
        /// <summary>
        /// 配货盘点，不记录在表中
        /// </summary>
        /// <param name="dat"></param>
        private CheckedType VaidData(List<List<string>> dat, string OrderblankNum)
        {
            Orderblank orderblank = _orderblankContract.Orderblanks.FirstOrDefault(x => x.OrderBlankNumber == OrderblankNum);
            List<OrderblankItem> listOrderblankItem = new List<OrderblankItem>();
            CheckerDto checkerDto = new CheckerDto();
            if (orderblank != null)
            {
                listOrderblankItem = _orderblankItemContract.OrderblankItems.Where(x => x.IsEnabled == true && x.IsDeleted == false && x.OrderblankId == orderblank.Id).ToList();
            }
            CheckedType checkedType = new CheckedType();
            object objcheckedType = SessionAccess.Get("CheckedType");
            if (objcheckedType != null)
            {
                checkedType = objcheckedType as CheckedType;
            }
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
            bool isHave = false;
            bool isVaild = false;
            int totalQuantity = listVaildNum == null ? 0 : listOrderblankItem.Count() - listVaildNum.Count();
            //checkedType.CheckQuantity = listInventory.Count() - checkerDto.CheckedQuantity;
            OperationResult oper = new OperationResult(OperationResultType.Success);
            if (dat != null)
            {
                foreach (List<string> item in dat)
                {
                    checkedType.CheckedQuantity += 1;
                    string Number = item[0];
                    if (!string.IsNullOrEmpty(Number))
                    {
                        isHave = listVaildNum.Any(x => x == Number);
                        if (isHave == true)
                        {
                            checkedType.InvalidQuantity += 1;
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
                            }
                            else
                            {
                                checkedType.InvalidQuantity += 1;
                            }
                        }
                    }
                    else
                    {
                        checkedType.InvalidQuantity += 1;
                    }
                }
            }
            if (totalQuantity < 0)
            {
                totalQuantity = 0;
            }
            checkedType.CheckQuantity = totalQuantity;
            checkedType.MissingQuantity = totalQuantity;
            SessionAccess.Set(VaildNums, listVaildNum, true);
            SessionAccess.Set(InvaildNums, listInvaildNum, true);
            SessionAccess.Set("CheckedType", checkedType, true);
            return checkedType;
        }
        #endregion

        #region 盘点批量校验
        /// <summary>
        /// 盘点批量校验
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="dat"></param>
        /// <returns></returns>
        private CheckedType VaidData(int Id, List<List<string>> dat)
        {
            IQueryable<CheckerItem> listCheckerItem = _checkItemContract.CheckerItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CheckerId == Id);

            CheckerDto checkerDto = ReadCookie(CheckerKey);
            if (checkerDto == null)
            {
                checkerDto = _checkerContract.Edit(Id);
            }
            //全部仓库信息
            IQueryable<Inventory> allInventories = _inventoryContract.Inventorys.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status == (int)InventoryStatus.Default);
            //获取盘点仓库的信息
            IQueryable<Inventory> listInventory = FilterProduct(checkerDto);
            CheckedType checkedType = new CheckedType();
            OperationResult oper = new OperationResult(OperationResultType.Success);
            //获取打印条码列表
            IQueryable<ProductBarcodeDetail> listProBar = _productBarcodeDetailContract.productBarcodeDetails.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Status == (int)ProductBarcodeDetailFlag.AddStorage);

            //存放可以有效余货等数据
            List<CheckerItemDto> listCheckerItemDto = new List<CheckerItemDto>();
            //装载无效数据
            List<CheckerItemDto> listInvalid = new List<CheckerItemDto>();
            object objInvalids = SessionAccess.Get("Invalids");
            if (objInvalids != null)
            {
                listInvalid = objInvalids as List<CheckerItemDto>;
            }
            if (dat != null)
            {
                IQueryable<Product> listProduct = _productContract.Products.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                CheckerItemDto dto = new CheckerItemDto()
                {
                    CheckerItemType = (int)CheckerItemFlag.Valid,
                    CheckGuid = checkerDto.CheckGuid,
                    CheckerId = checkerDto.Id,
                    //ProductId = product.Id,
                    //ProductBarcode = num,
                };
                //是否存在，默认不存在
                bool isHave = false;
                foreach (List<string> item in dat)
                {
                    string num = item[0];
                    if (!string.IsNullOrEmpty(num))
                    {
                        #region 盘点逻辑
                        int length = num.Length;
                        checkerDto.CheckedQuantity += 1;
                        if (length > NumberLength)
                        {
                            //根据编号查找商品是否存在
                            int startIndex = num.Length - 3;
                            //分解出商品货号                        
                            string strProductNumber = num.Substring(0, startIndex);
                            string strOnlyFlag = num.Substring(startIndex);
                            Product product = listProduct.FirstOrDefault(x => x.ProductNumber == strProductNumber);
                            CheckerItemDto checkerItemDto = dto.DeepClone();
                            checkerItemDto.ProductBarcode = num;
                            if (product == null)
                            {
                                checkerDto.InvalidQuantity += 1;
                                checkerItemDto.CheckerItemType = (int)CheckerItemFlag.Invalid;
                                listInvalid.Add(checkerItemDto);
                            }
                            else
                            {
                                //盘点仓库是否有这件商品
                                Inventory inventory = listInventory.FirstOrDefault(x => x.ProductBarcode == num);
                                if (inventory != null)
                                {
                                    checkerItemDto.ProductId = inventory.ProductId;
                                    //商品是否盘点过了
                                    isHave = listCheckerItem.Any(x => x.ProductBarcode == num);
                                    if (isHave == false && !listCheckerItemDto.Any(a => a.ProductBarcode == num))
                                    {
                                        checkerDto.ValidQuantity += 1;
                                        checkerDto.MissingQuantity -= 1;
                                        checkerItemDto.CheckerItemType = (int)CheckerItemFlag.Valid;
                                        listCheckerItemDto.Add(checkerItemDto);
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
                                    isHave = allInventories.Any(x => x.ProductBarcode == num);
                                    ProductBarcodeDetail proBar = listProBar.FirstOrDefault(x => x.ProductNumber == strProductNumber && x.OnlyFlag == strOnlyFlag);
                                    if (isHave == false && proBar != null)
                                    {
                                        checkerItemDto.ProductId = proBar.ProductId;
                                        //商品是否盘点过了
                                        isHave = listCheckerItem.Any(x => x.ProductBarcode == num);
                                        if (isHave == false)
                                        {
                                            checkerDto.ResidueQuantity += 1;
                                            checkerItemDto.CheckerItemType = (int)CheckerItemFlag.Surplus;
                                            listCheckerItemDto.Add(checkerItemDto);
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
                            }
                        }

                        #endregion
                    }
                }
                if (listCheckerItemDto.Count() > 0)
                {
                    _checkerContract.Update(checkerDto);
                    oper = _checkItemContract.Insert(listCheckerItemDto.ToArray());

                }
            }
            //将统计次数放入到Session中
            //SessionAccess.Set("CheckedQuantity", checkedCount, true);            
            checkedType.CheckedQuantity = checkerDto.CheckedQuantity;
            checkedType.InvalidQuantity = checkerDto.InvalidQuantity;
            checkedType.MissingQuantity = checkerDto.MissingQuantity;

            checkedType.ValidQuantity = checkerDto.ValidQuantity;
            //带盘数量和余货数量相等
            checkedType.CheckQuantity = checkerDto.CheckQuantity;
            checkedType.ResidueQuantity = checkerDto.ResidueQuantity;
            SessionAccess.Set("Invalids", listInvalid, true);
            this.WriteCookie(checkerDto, CheckerKey);
            //checkedType.UUID = uuid;
            return checkedType;
        }
        #endregion

        #region 批量校验
        /// <summary>
        /// 入库数据批量校验
        /// </summary>
        /// <param name="pre"></param>
        private void BatchInputStorageCheck(Dictionary<string, string> pre, bool isclear = false)
        {
            var scanValidKey = "ScanValid";
            var scanInvalidKey = "ScanInvalid";
            List<string> errli = new List<string>();

            List<Product_Model> validModels = new List<Product_Model>();
            List<Product_Model> inValidModels = new List<Product_Model>();
            if (!isclear)
            {
                validModels = SessionAccess.Get(scanValidKey) as List<Product_Model>;
                inValidModels = SessionAccess.Get(scanInvalidKey) as List<Product_Model>;
                if (validModels == null)
                    validModels = new List<Product_Model>();
                if (inValidModels == null)
                    inValidModels = new List<Product_Model>();
            }

            List<string> numbs = pre.Select(c => c.Value).ToList();
            List<string> valied = new List<string>();

            //是否与已校验通过的结果重复
            var exisVali = validModels.Where(c => numbs.Contains(c.ProductBarcode)).ToList();
            //是否与校验不通过的结果重复
            var exisInvali = inValidModels.Where(c => numbs.Contains(c.ProductBarcode)).ToList();
            int cuind = inValidModels.Count + validModels.Count + 1;
            if (exisVali.Any())
            {
                valied.AddRange(exisVali.Select(c => c.ProductBarcode));
                for (int i = 0; i < exisVali.Count(); i++)
                {
                    var ite = exisVali[0];
                    var t = CacheAccess.Clone<Product_Model>(ite);
                    t.Id = cuind;
                    t.Notes = "已进入缓存队列";
                    inValidModels.Add(t);
                }
                var exiscodes = exisVali.Select(c => c.ProductBarcode).ToList();
                numbs.RemoveAll(c => exiscodes.Contains(c));
            }
            else if (exisInvali.Any())
            {
                valied.AddRange(exisInvali.Select(c => c.ProductBarcode));
                for (int i = 0; i < exisInvali.Count(); i++)
                {
                    var ite = exisInvali[i];
                    var t = CacheAccess.Clone<Product_Model>(ite);
                    t.Id = cuind;
                    t.Notes += "，且已经重复";
                    inValidModels.Add(t);
                }

                var exiscodes = exisVali.Select(c => c.ProductBarcode).ToList();
                numbs.RemoveAll(c => exiscodes.Contains(c));
            }

            var plbarcode = numbs.Where(c => !valied.Contains(c)).ToList();//没有经过校验的条码
            if (plbarcode.Any())
            {
                //商品的打印记录
                var vadali = _productBarcodeDetailContract.productBarcodeDetails.Where(c => plbarcode.Contains(c.ProductNumber + c.OnlyFlag));
                //根据条码得到编号
                var pnums = numbs.Where(c => c.Length == 14).Select(c => c.Substring(0, 11)).ToList();
                //存在商品檔案的库存
                var exisnum = _productContract.Products.Where(c => pnums.Contains(c.ProductNumber))
                      .Select(c => c.ProductNumber)
                      .ToList();
                //入库校验
                foreach (var inda in plbarcode)
                {
                    //序列号
                    var ind = inValidModels.Count + validModels.Count + 1;
                    if (validModels.Any(c => c.ProductBarcode == inda))
                    {
                        var exc = validModels.FirstOrDefault(c => c.ProductBarcode == inda);
                        if (exc != null)
                        {
                            var t = CacheAccess.Clone<Product_Model>(exc);
                            t.Id = ind;
                            t.Notes = "已进入缓存队列";
                            inValidModels.Add(t);
                        }
                    }
                    else if (inValidModels.Any(c => c.ProductBarcode == inda))
                    {
                        var exc = inValidModels.FirstOrDefault(c => c.ProductBarcode == inda);
                        if (exc != null)
                        {
                            var te = CacheAccess.Clone<Product_Model>(exc);
                            te.Id = ind;
                            te.Notes += "，且已经重复";
                            inValidModels.Add(te);
                        }
                        var exiscodes = exisVali.Select(c => c.ProductBarcode).ToList();
                        numbs.RemoveAll(c => exiscodes.Contains(c));
                    }
                    else
                    {
                        //带校验对象
                        var di = pre.FirstOrDefault(c => c.Value == inda);
                        if (inda.Length == 14)
                        {
                            var prnum = inda.Substring(0, 11);
                            //打印记录
                            var barcode = vadali.FirstOrDefault(c => c.ProductNumber + c.OnlyFlag == inda);
                            //商品档案
                            var detai = exisnum.FirstOrDefault(c => c == prnum);

                            if (detai != null)
                            {
                                if (barcode != null)
                                {
                                    if (barcode.IsDeleted)
                                    {
                                        inValidModels.Add(new Product_Model
                                        {
                                            Id = ind,
                                            UUID = di.Key,
                                            ProductBarcode = di.Value,
                                            Notes = "商品档案存在,且有打印记录,但已经被移除到回收站"
                                        });
                                    }
                                    else
                                    {
                                        if (barcode.Status == 0)
                                        {
                                            validModels.Add(new Product_Model
                                            {
                                                Id = ind,
                                                UUID = di.Key,
                                                ProductBarcode = di.Value,
                                                Notes = "商品档案存在,且有打印记录,可以入库"
                                            });
                                        }
                                        else
                                        {
                                            string err = barcode.Status == 1 ? "已入库" : "已删除或禁用";
                                            inValidModels.Add(new Product_Model
                                            {
                                                Id = ind,
                                                UUID = di.Key,
                                                ProductBarcode = di.Value,
                                                Notes = err
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    inValidModels.Add(new Product_Model
                                    {
                                        Id = ind,
                                        UUID = di.Key,
                                        ProductBarcode = di.Value,
                                        Notes = "商品档案存在,但是没有打印记录"
                                    });
                                }
                            }
                            else
                            {
                                inValidModels.Add(new Product_Model
                                {
                                    Id = ind,
                                    UUID = di.Key,
                                    ProductBarcode = di.Value,
                                    Notes = "商品档案不存在"
                                });
                            }
                        }
                        else
                        {
                            inValidModels.Add(new Product_Model
                            {
                                Id = ind,
                                UUID = di.Key,
                                ProductBarcode = di.Value,
                                Notes = "录入的条码不符合14位数"
                            });
                        }
                    }
                }
            }

            SessionAccess.Set(scanValidKey, validModels, true);
            SessionAccess.Set(scanInvalidKey, inValidModels, true);
        }
        #endregion

        #region 校验
        private void CheckParams()
        {
            CheckedType chty = new CheckedType();
            List<CheckDto_t> li = Session["checkCount_li"] as List<CheckDto_t>;
            li = new List<CheckDto_t>();
            li.Add(new CheckDto_t() { ProductNumber = "2213sd", Quantity = 22 });
            if (li != null)
            {
                chty.CheckQuantity = 100;//li.Select(c => c.Quantity).Sum();
                string _num = Request["number"];
                chty.UUID = Request["uuid"];
                var cheda = li.Where(c => c.ProductNumber == _num).FirstOrDefault();
                #region 已盘点数量
                var checkedList = Session["checkedCount_li"] as List<CheckDto_t>;
                if (checkedList == null)
                {
                    checkedList = new List<CheckDto_t>();
                }
                bool iscont = checkedList.Select(c => c.ProductNumber).Contains(_num);
                if (iscont)
                {
                    var te = checkedList.Where(c => c.ProductNumber == _num).FirstOrDefault();
                    te.Quantity = te.Quantity + 1;
                }
                else
                {
                    checkedList.Add(new CheckDto_t()
                    {
                        ProductNumber = _num,
                        Quantity = 1
                    });
                }
                Session["checkedCount_li"] = checkedList;
                chty.CheckedQuantity = checkedList.Sum(c => c.Quantity);
                #endregion
                var checkList = Session["checkCount_li"] as List<CheckDto_t>;
                if (cheda != null && cheda.Quantity > 0) //在待盘记录中找到该记录并且待盘数量大于0
                {
                    #region 待盘数量
                    chty.Resultype = 1; //有效
                    //待盘数量
                    cheda.Quantity = cheda.Quantity - 1;
                    Session["checkCount_li"] = li;
                    chty.CheckQuantity = li.Select(c => c.Quantity).Sum();
                    #endregion

                    #region 有效数量
                    //有效数量
                    var validCountSes = Session["validCount_li"] as List<CheckDto_t>;
                    if (validCountSes == null)
                        validCountSes = new List<CheckDto_t>();
                    var valDa = validCountSes.Where(c => c.ProductNumber == _num).FirstOrDefault();
                    if (valDa != null)
                    {
                        valDa.Quantity = valDa.Quantity + 1;

                    }
                    else
                    {
                        validCountSes.Add(new CheckDto_t()
                        {
                            ProductNumber = _num,
                            Quantity = 1
                        });
                    }

                    Session["validCount_li"] = validCountSes;
                    chty.ValidQuantity = validCountSes.Select(c => c.Quantity).Sum();

                    #endregion

                }
                else if (cheda != null && cheda.Quantity == 0) //余货
                {
                    #region 余货
                    chty.Resultype = 4;
                    var residueCountSes = Session["residueCount_li"] as List<CheckDto_t> ?? new List<CheckDto_t>();

                    var residueDat = residueCountSes.Where(c => c.ProductNumber == _num).FirstOrDefault();
                    if (residueDat == null)
                    {
                        residueCountSes.Add(new CheckDto_t()
                        {
                            ProductNumber = _num,
                            Quantity = 1
                        });
                    }
                    else
                    {
                        residueDat.Quantity = residueDat.Quantity + 1;
                    }
                    Session["residueCount_li"] = residueCountSes;
                    chty.ResidueQuantity = residueCountSes.Select(c => c.Quantity).Sum();
                    #endregion
                }

                else if (cheda == null) //在列表中未找到
                {
                    var chedaList = _productContract.Products.Where(c => c.ProductNumber == _num);

                    if (chedaList != null && chedaList.Count() > 0) //余货
                    {
                        #region 余货
                        chty.Resultype = 4;
                        var residueCountSes = Session["residueCount_li"] as List<CheckDto_t> ?? new List<CheckDto_t>();

                        var residueDat = residueCountSes.Where(c => c.ProductNumber == _num).FirstOrDefault();
                        if (residueDat == null)
                        {
                            residueCountSes.Add(new CheckDto_t()
                            {
                                ProductNumber = _num,
                                Quantity = 1
                            });
                        }
                        else
                        {
                            residueDat.Quantity = residueDat.Quantity + 1;
                        }
                        Session["residueCount_li"] = residueCountSes;
                        chty.ResidueQuantity = residueCountSes.Select(c => c.Quantity).Sum();
                        #endregion
                    }
                    else
                    {
                        #region 无效货号

                        chty.Resultype = 2;
                        var invalidCountSes = Session["invalidCount_li"] as List<CheckDto_t> ?? new List<CheckDto_t>();
                        var invalDat = invalidCountSes.Where(c => c.ProductNumber == _num).FirstOrDefault();
                        if (invalDat == null)
                        {
                            invalidCountSes.Add(new CheckDto_t()
                            {
                                ProductNumber = _num,
                                Quantity = 1
                            });
                        }
                        else
                        {
                            invalDat.Quantity = invalDat.Quantity + 1;

                        }
                        Session["invalidCount_li"] = invalidCountSes;
                        chty.InvalidQuantity = invalidCountSes.Select(c => c.Quantity).Sum();
                        #endregion

                    }
                }
                else { }
                chty.MissingQuantity = chty.CheckQuantity;

            }
            //return Json(chty);
        }
        #endregion

        #region 读取Excel数据
        public JsonResult GetBatchImportExcelData()
        {
            GridRequest gr = new GridRequest(Request);
            var _key = "sess_excel_inpor_instorage_12";
            var dat = SessionAccess.Get(_key) as List<List<string>>;
            GridData<object> da = new GridData<object>(new List<object>(), 0, Request);
            if (dat != null)
            {
                var te = dat.Select(c => new
                {
                    Barcode = c[0],
                    RowInd = Convert.ToInt32(c[1]),
                }).ToList();

                var li =
                    te.OrderBy(c => c.RowInd).Skip(gr.PageCondition.PageIndex).Take(gr.PageCondition.PageSize).Select(c => new
                    {
                        ProductBarcode = c.Barcode,
                        c.RowInd
                    }
                        ).ToList();
                da = new GridData<object>(li, dat.Count, Request);
            }
            return Json(da);
        }
        #endregion

        [HttpPost]
        public ActionResult GetSizeOfCategory(int id)
        {
            var li = CacheAccess.GetSize(_sizeContract, id.ToString());
            return Json(li);
        }


        #region 开启盘点时
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="OrderblakNum">当不会空时，表示配货盘点不做记录</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult StartCheck(CheckerDto dto, string OrderblakNum = "")
        {
            OperationResult oper;
            if (string.IsNullOrEmpty(OrderblakNum))
            {
                oper = StartCheckerInventory(dto);
            }
            else
            {
                oper = StartCheckerOrderblank(OrderblakNum);
            }
            #region 注销代码

            //OperationResult oper = new OperationResult(OperationResultType.Success);
            //IQueryable<Inventory> listInventory = FilterProduct(dto);
            //Store store = _storeContract.Stores.FirstOrDefault(x=>x.Id==dto.StoreId);
            //Storage storage = store.Storages.FirstOrDefault(x=>x.Id==dto.StorageId);
            //int totalCount=listInventory.Count();
            //IQueryable<Checker> listChecker =  _checkerContract.Checkers.Where(x => x.IsEnabled == true && x.IsDeleted == false && x.StorageId == dto.StorageId && x.StoreId == x.StoreId);
            //Checker checker = listChecker.FirstOrDefault(x => x.CheckerState == (int)CheckerFlag.Checking || x.CheckerState == (int)CheckerFlag.Interrupt);

            //if (checker != null)
            //{
            //    Mapper.CreateMap<Checker, CheckerDto>();
            //    CheckerDto dtoEntity = Mapper.Map<Checker, CheckerDto>(checker);
            //    dto.Id = dtoEntity.Id;
            //    oper.Data = dtoEntity;
            //    oper.ResultType = OperationResultType.DataRepeat;
            //}
            //else
            //{                
            //    dto.CheckGuid = CreateChckerGuid();
            //    dto.CheckerName = store.StoreName + storage.StorageName + DateTime.Now.ToString("yyyy年MM月dd日");
            //    dto.CheckerState = (int)CheckerFlag.Checking;
            //    dto.CheckQuantity = totalCount;                
            //    oper=_checkerContract.Insert(dto);
            //    if (oper.ResultType==OperationResultType.Success)
            //    {
            //        int[] arr = oper.Data as int[];
            //        dto.Id = arr[0];
            //        oper.Data = dto;
            //    }
            //}
            //if (Session["CheckerId"] == null)
            //{
            //    Session["CheckerId"] = dto.Id;
            //}
            #endregion
            return Json(oper);
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
                using (var tran  = _checkerContract.GetTransaction())
                {
                    dto.CheckGuid = CreateChckerGuid();
                    dto.CheckerName = store.StoreName + storage.StorageName + DateTime.Now.ToString("yyyy年MM月dd日");
                    dto.CheckerState = CheckerFlag.Checking;
                    dto.MissingQuantity = totalCount;
                    dto.BeforeCheckQuantity = totalCount;
                    oper = _checkerContract.Insert(dto);
                    if (oper.ResultType != OperationResultType.Success)
                    {
                        tran.Rollback(); return OperationHelper.ReturnOperationResultDIY(OperationResultType.Error, $"开始盘点创建失败,{oper.Message}");
                    }
                    int[] arr = oper.Data as int[];
                    dto.Id = arr[0];
                    dto.InvalidQuantity = 0;
                    oper.Data = dto;
                    var inList = listInventory.Select(x => new
                    {
                        x.ProductBarcode,
                        x.ProductNumber,
                        x.Storage.StorageName
                    }).ToList();
                    if (inList.Count > 0)
                    {
                        var listpt = new List<ProductTrack>();
                        foreach (var item in inList)
                        {
                            string numBarcode = item.ProductBarcode;
                            if (!string.IsNullOrEmpty(numBarcode))
                            {
                                #region 商品追踪
                                ProductTrack pt = new ProductTrack();
                                pt.ProductNumber = item.ProductNumber;
                                pt.ProductBarcode = numBarcode;
                                pt.Describe = string.Format(ProductOptDescTemplate.ON_PRODUCT_CHECKER_START, item.StorageName);
                                listpt.Add(pt);

                                #endregion
                            }
                        }
                        var resPT = _productTrackContract.BulkInsert(listpt);
                        if (resPT.ResultType != OperationResultType.Success)
                        {
                            tran.Rollback(); return OperationHelper.ReturnOperationResultDIY(OperationResultType.Error, $"开始盘点创建失败,商品追踪插入失败,{resPT.Message}");
                        }
                    }
                    tran.Commit();
                }
            }
            object obj = SessionAccess.Get("CheckerId");
            if (obj == null)
            {
                SessionAccess.Set("CheckerId", dto.Id, true);
            }
            if (oper.ResultType == OperationResultType.Success)
            {
                WriteCookie(dto, CheckerKey);
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
        public JsonResult CheckerOk(int Id)
        {
            OperationResult oper = _checkerContract.CheckerOk(Id, null);
            if (oper.ResultType == OperationResultType.Success)
            {
                //将统计次数移除session中
                SessionAccess.Remove("Invalids");
                RemoveCookie(CheckerKey);
            }
            return Json(oper);
        }
        #endregion

        #region 委托-根据盘点条件对商品进行筛选
        private IQueryable<Inventory> FilterProduct(CheckerDto dto)
        {
            IQueryable<Inventory> invertoryList = _inventoryContract.Inventorys.Where(x => x.StorageId == dto.StorageId && x.StoreId == dto.StoreId && x.Status == (int)InventoryStatus.Default);
            Func<Inventory, bool> predicate = (invent) => invent.IsEnabled == true && invent.IsDeleted == false;
            //Expression<Func<Inventory, bool>> expression = (invent) => invent.IsEnabled == true && invent.IsDeleted == false;            

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
            //if (dto.ColorId != null)
            //{
            //    predicate += (
            //      (invent) => invent.Product.ColorId == dto.ColorId
            //   );
            //}
            //if (dto.SeasonId != null)
            //{
            //    predicate += (
            //      (invent) => invent.Product.ProductOriginNumber.SeasonId == dto.SeasonId
            //   );
            //}
            //if (dto.SizeId != null)
            //{
            //    predicate += (
            //      (invent) => invent.Product.SizeId == dto.SizeId
            //   );
            //}                         
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

        #region 是否继续盘点

        /// <summary>
        /// 是否继续盘点
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Confirm(int Id)
        {
            OperationResult oper = _checkerContract.Confirm(Id);
            return Json(oper);
        }
        #endregion

        #region 导出数据
        [Log]
        public FileResult Export(int CheckerId, int CheckerItemType)
        {

            List<CheckerItemExport> list = new List<CheckerItemExport>();
            if (CheckerItemType == (int)CheckerItemFlag.Invalid)
            {
                List<CheckerItemDto> listDto = new List<CheckerItemDto>();
                object objDtos = SessionAccess.Get("Invalids");
                if (objDtos != null)
                {
                    listDto = objDtos as List<CheckerItemDto>;
                }
                foreach (CheckerItemDto dto in listDto)
                {

                    list.Add(new CheckerItemExport()
                    {
                        ProductBarcode = dto.ProductBarcode,
                        StateDes = GetCheckerItemTypeString(dto.CheckerItemType)
                    });
                }
            }
            else
            {
                IQueryable<CheckerItem> listCheckerItem = _checkItemContract.CheckerItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CheckerId == CheckerId);
                foreach (CheckerItem checkerItem in listCheckerItem)
                {
                    list.Add(new CheckerItemExport()
                    {
                        ProductBarcode = checkerItem.ProductBarcode,
                        StateDes = GetCheckerItemTypeString(checkerItem.CheckerItemType)
                    });
                }
            }
            StringBuilder sbHtml = new StringBuilder();
            sbHtml.Append("<table border='1' cellspacing='0' cellpadding='0'>");
            sbHtml.Append("<tr>");
            var lstTitle = new List<string> { "编号", "编码", "状态" };
            foreach (var item in lstTitle)
            {
                sbHtml.AppendFormat("<td style='font-size: 14px;text-align:center;background-color: #DCE0E2; font-weight:bold;' height='25'>{0}</td>", item);
            }
            sbHtml.Append("</tr>");
            int count = 1;
            foreach (var item in list)
            {
                sbHtml.Append("<tr>");
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", count);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", item.ProductBarcode);
                sbHtml.AppendFormat("<td style='font-size: 12px;height:20px;'>{0}</td>", item.StateDes);
                sbHtml.Append("</tr>");
                ++count;
            }
            sbHtml.Append("</table>");
            //var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            //var group = new StringTemplateGroup("Warehouses", path, typeof(TemplateLexer));
            //var st = group.GetInstanceOf("Exporter");
            //st.SetAttribute("list", list);
            //string str1 = st.ToString();
            byte[] fileContents = Encoding.Default.GetBytes(sbHtml.ToString());
            var fileStream = new MemoryStream(fileContents);
            return File(fileStream, "application/ms-excel", "盘点数据.xls");
        }
        #endregion

        #region 返回枚举字符串
        private string GetCheckerItemTypeString(int CheckerItemType)
        {
            string strWord = string.Empty;

            switch (CheckerItemType)
            {
                case (int)CheckerItemFlag.Invalid:
                    strWord = "无效";
                    break;
                case (int)CheckerItemFlag.Lack:
                    strWord = "缺货";
                    break;
                case (int)CheckerItemFlag.Surplus:
                    strWord = "余货";
                    break;
                case (int)CheckerItemFlag.Valid:
                    strWord = "有效";
                    break;
                default:
                    strWord = "未知选项";
                    break;
            }
            return strWord;
        }
        #endregion

        #region 写入Cookie
        /// <summary>
        /// 写入Cookie
        /// </summary>
        private void WriteCookie(CheckerDto dto, string key)
        {
            HttpCookie httpCookie = Request.Cookies[key];
            string strData = string.Empty;
            JavaScriptSerializer jss = new JavaScriptSerializer();
            strData = jss.Serialize(dto);
            DateTime issueDate = DateTime.Now;
            DateTime expiration = DateTime.Now.Add(FormsAuthentication.Timeout);
            int verison = 3;
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(verison, key, issueDate, expiration, false, strData);
            if (httpCookie == null)
            {
                httpCookie = new HttpCookie(key, FormsAuthentication.Encrypt(ticket))
                {
                    HttpOnly = true,
                    Secure = FormsAuthentication.RequireSSL,
                    Domain = FormsAuthentication.CookieDomain,
                    Path = FormsAuthentication.FormsCookiePath
                };
            }
            else
            {
                httpCookie.Value = FormsAuthentication.Encrypt(ticket);
            }
            Response.Cookies.Remove(key);
            Response.Cookies.Add(httpCookie);
        }
        #endregion

        #region 读取cookie数据
        /// <summary>
        /// 读取cookie数据
        /// </summary>
        private CheckerDto ReadCookie(string key)
        {
            HttpCookie httpCookie = Request.Cookies[key];
            CheckerDto dto = null;
            if (httpCookie != null)
            {
                string strData = httpCookie.Value;
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(strData);
                JavaScriptSerializer jss = new JavaScriptSerializer();
                dto = jss.Deserialize<CheckerDto>(ticket.UserData);
            }
            return dto;
        }
        #endregion

        #region 移除cookie数据
        private void RemoveCookie(string key)
        {
            HttpCookie httpCookie = Request.Cookies[key];
            if (httpCookie != null)
            {
                httpCookie.Expires = DateTime.Now.AddDays(-1);
            }
        }
        #endregion
    }

}