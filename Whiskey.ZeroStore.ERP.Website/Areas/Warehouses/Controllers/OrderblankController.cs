using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Warehouse;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Models;
using XKMath36;
using System.IO;
using System.Threading.Tasks;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Extensions;
using Whiskey.jpush.api;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using System.Threading;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models;
using Antlr3.ST.Language;
using Antlr3.ST;
using System.Text;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    [CheckStoreIsClosed]
    public class OrderblankController : BaseController
    {

        #region 声明接口及初始化
        protected readonly IAdministratorContract _adminContarct;
        protected readonly IBrandContract _brandContract;
        protected readonly IColorContract _colorContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IModuleContract _moduleContract;
        protected readonly INotificationContract _notificationcontract;
        protected readonly IOrderblankAuditContract _orderblankAuditContract;
        protected readonly IOrderblankContract _orderblankContract;
        protected readonly IOrderblankItemContract _orderblankItemContract;
        protected readonly IPermissionContract _permissionContract;
        protected readonly IProductContract _productContract;
        protected readonly IPurchaseContract _purchaseContract;
        protected readonly IPurchaseItemContract _purchaseItemContract;

        protected readonly IStorageContract _storageContract;
        protected readonly IStoreContract _storeContract;
        protected readonly ITemplateNotificationContract _templateNotificationContract;
        protected readonly ITimeoutSettingContract _timeoutSettingContract;
        protected readonly ITimeoutRequestContract _timeoutRequestContract;
        protected readonly IMemberContract _memberContract;
        private readonly IProductTrackContract _productTrackContract;
        private readonly IPunishScoreRecordContract _adminPunishScoreRecordContract;
        private readonly IAppointmentContract _appointmentContract;
        private const string SESSION_KEY_INVALID_LIST = "order_pro_Invalid";
        private const string SESSION_KEY_ORDERBLANK_LIST = "orderblank_data";
        private const string SESSION_KEY_VALID_LIST = "order_pro_Valid";

        public OrderblankController(IStorageContract storageContract,
            IStoreContract storeContract,
            IOrderblankContract orderblankContract,
            IProductContract productContract,
            IBrandContract brandContract,
            IPurchaseContract purchaseContract,
            IPurchaseItemContract purchaseItemContract,
            IAdministratorContract adminContarct,
            IColorContract colorContract,
            IInventoryContract inventoryContract,
            IOrderblankItemContract orderblankItemContract,
            IOrderblankAuditContract orderblankAuditContract
            , INotificationContract notificationcontract
            , IModuleContract modulecontract
            , IPermissionContract permissioncontract
            , ITemplateNotificationContract templateNotificationContract
            , ITimeoutSettingContract timeoutSettingContract
            , IMemberContract memberContract
            , IProductTrackContract productTractContract,
            IPunishScoreRecordContract adminPunishScoreRecordContract,
            IAppointmentContract appointmentContract,
            ITimeoutRequestContract timeoutRequestContract
            )
        {
            _storageContract = storageContract;
            _storeContract = storeContract;
            _orderblankContract = orderblankContract;
            _productContract = productContract;
            _brandContract = brandContract;
            _purchaseContract = purchaseContract;
            _purchaseItemContract = purchaseItemContract;
            _adminContarct = adminContarct;
            _colorContract = colorContract;
            _inventoryContract = inventoryContract;
            _orderblankItemContract = orderblankItemContract;
            _orderblankAuditContract = orderblankAuditContract;
            _notificationcontract = notificationcontract;
            _moduleContract = modulecontract;
            _permissionContract = permissioncontract;
            _templateNotificationContract = templateNotificationContract;
            _timeoutSettingContract = timeoutSettingContract;
            _memberContract = memberContract;
            _productTrackContract = productTractContract;
            _adminPunishScoreRecordContract = adminPunishScoreRecordContract;
            _appointmentContract = appointmentContract;
            _timeoutRequestContract = timeoutRequestContract;
        }
        #endregion

        #region 私有方法


        /// <summary>
        /// 发货通知【异步操作】
        /// </summary>
        /// <param name="instorage">收货仓库Id</param>
        /// <param name="outstorage">发货仓库Id</param>
        private void SendOrderBlankNotification(OrderblankNoticeModel model)
        {
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                var curmoduleId = Utils.GetCurrPageModuleId("/Warehouses/Orderblank", EntityContract._moduleContract);
                if (curmoduleId.HasValue)
                {
                    var inStorage = EntityContract._storageContract.Storages.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == model.ReceiverStorageId).FirstOrDefault();
                    var outStorage = EntityContract._storageContract.Storages.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.Id == model.OutStorageId).FirstOrDefault();
                    if (inStorage.IsNotNull() && outStorage.IsNotNull())
                    {
                        var receivestore = inStorage.Store;
                        var sendstore = outStorage.Store;
                        if (receivestore.IsNotNull() && sendstore.IsNotNull())
                        {
                            var receiveAdminIds = EntityContract._adminContract.Administrators.Where(w => w.IsEnabled && !w.IsDeleted && w.DepartmentId == receivestore.DepartmentId).Select(s => s.Id).ToList();

                            receiveAdminIds = receiveAdminIds.Where(w => PermissionHelper.HasModulePermission(w, curmoduleId.Value, EntityContract._adminContract, EntityContract._permissionContract)).ToList();
                            if (receiveAdminIds.IsNotNullOrEmptyThis())
                            {
                                var modTN = EntityContract._templateNotificationContract.templateNotifications.FirstOrDefault(f => f.NotifciationType == TemplateNotificationType.Picking);
                                if (modTN.IsNotNull())
                                {
                                    var modTemp = modTN.Templates.FirstOrDefault(f => f.IsDefault && !f.IsDeleted && f.IsEnabled);
                                    if (modTemp.IsNotNull())
                                    {
                                        var title = modTemp.TemplateName ?? "配货通知";
                                        if (model.IsReject)
                                        {
                                            title = "拒绝收货通知";
                                        }
                                        var content = modTemp.TemplateHtml ?? "$sendName 配到 $receiveName 的配货单，需要处理";

                                        content = content.Replace("$sendName", sendstore.StoreName)
                                                         .Replace("$receiveName", receivestore.StoreName)
                                                         .Replace("$sendTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                                                         .Replace("$sendId", sendstore.Id.ToString())
                                                         .Replace("$receiveId", receivestore.Id.ToString())
                                                         .Replace("$sendPhone", sendstore.Telephone ?? sendstore.MobilePhone)
                                                         .Replace("$receivePhone", receivestore.Telephone ?? receivestore.MobilePhone)
                                                         .Replace("$sendAddress", sendstore.Address).Replace("$receiveAddress", receivestore.Address);
                                        content += string.Format(" 配货单号:{0}", model.OrderblankNumer);
                                        if (model.IsReject)
                                        {
                                            content += string.Format(" 拒收原因:{0}", model.RejectReason);
                                        }
                                        var result = EntityContract._notificationContract.Insert(sendNotificationAction, new NotificationDto()
                                        {
                                            Title = title,
                                            AdministratorIds = receiveAdminIds,
                                            Description = content,
                                            IsEnableApp = false,
                                            NoticeTargetType = (int)NoticeTargetFlag.Admin,
                                            NoticeType = (int)NoticeFlag.Immediate
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }


        private Tuple<bool, string> CheckBarcode(Product_Model model, List<Product_Model> validList, List<Product_Model> invalidlist, List<OrderblankItem> orderblankItemsFromDb, List<Inventory> inventoryList)
        {
            var result = new OperationResult(OperationResultType.Error, "");
            var number = model.ProductBarcode;
            // 长度校验
            if (number.Length != 14)
            {
                return Tuple.Create(false, "扫入的商品条码不符合要求");
            }

            // 排重
            if (validList.Any(c => c.ProductBarcode == number))
            {
                return Tuple.Create(false, "商品条码已经扫入：" + number);
            }

            if (invalidlist.Any(c => c.ProductBarcode == number))
            {
                return Tuple.Create(false, "商品条码无效，并且已重复出现：" + number);
            }

            #region 校验库存信息
            var inventoryEntity = inventoryList.FirstOrDefault(c => c.ProductBarcode == number);
            if (inventoryEntity == null)
            {
                return Tuple.Create(false, "在库存中未查找到该商品条码：" + number + ",可能是当前用户对该库存不具有操作权限");
            }

            if (inventoryEntity.IsDeleted)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经被移至回收站");
            }
            if (!inventoryEntity.IsEnabled)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存处于禁用状态");
            }
            if (inventoryEntity.IsLock)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存处于锁定状态");
            }

            if (inventoryEntity.Status >= InventoryStatus.PurchasStart &&
                inventoryEntity.Status <= InventoryStatus.PurchasEnd)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经进入采购状态");
            }
            if (inventoryEntity.Status >= InventoryStatus.DeliveryStart &&
              inventoryEntity.Status <= InventoryStatus.DeliveryEnd)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经进入配货状态");
            }

            if (inventoryEntity.Status >= InventoryStatus.SaleStart &&
              inventoryEntity.Status <= InventoryStatus.SaleEnd)
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经进入销售状态");
            }


            #endregion



            if (inventoryEntity.Status == InventoryStatus.Default)
            {
                if (orderblankItemsFromDb.Any(o => o.OrderBlankBarcodes.Contains(number)))
                {
                    return Tuple.Create(false, "配货单中已存在该商品条码：" + number);
                }
                // 装载数据
                model.Id = inventoryEntity.Id;
                model.ProductId = inventoryEntity.ProductId;
                model.IsValided = true;
                model.ProductNumber = inventoryEntity.ProductNumber;
                model.Thumbnail = inventoryEntity.Product.ThumbnailPath;
                model.Brand = inventoryEntity.Product.ProductOriginNumber.Brand.BrandName;
                model.Size = inventoryEntity.Product.Size.SizeName;
                model.Color = inventoryEntity.Product.Color.ColorName;
                model.Season = inventoryEntity.Product.ProductOriginNumber.Season.SeasonName;
                model.Category = inventoryEntity.Product.ProductOriginNumber.Category.CategoryName;
                return Tuple.Create(true, string.Empty);
            }
            else
            {
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存由于其他原因被锁定");
            }

        }
        private Tuple<bool, string> CheckOrderblankEntity(Orderblank entity)
        {
            if (entity == null)
            {
                return Tuple.Create(false, "配货单不存在");
            }
            if (entity.IsDeleted)
            {
                return Tuple.Create(false, "配货单已经被删除");
            }
            if (!entity.IsEnabled)
            {
                return Tuple.Create(false, "配货单被禁用");
            }

            return Tuple.Create(true, string.Empty);
        }
        private OperationResult BatchAddOrderblankItem(Orderblank orderblankEntity, List<Inventory> inventoryList, params Product_Model[] models)
        {
            var barcodes = models.Select(m => m.ProductBarcode).ToList();
            var orderblankItemsFromDb = orderblankEntity.OrderblankItems.ToList();
            var barcodseFromDb = orderblankItemsFromDb.SelectMany(o => o.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            var errorCodes = barcodseFromDb.Intersect(barcodes);
            if (errorCodes.Any())
            {
                return new OperationResult(OperationResultType.Error, "存在已添加的条码" + string.Join(",", errorCodes));
            }
            // 根据productId分组统计出要添加的数量,及barcodes
            var groupList = models.GroupBy(m => m.ProductId).Select(g => new
            {
                Quantity = g.Count(),
                Barcodes = g.Select(m => m.ProductBarcode).ToList(),
                ProductId = g.Key
            }).ToList();

            foreach (var groupItem in groupList)
            {
                var itemEntity = orderblankItemsFromDb.FirstOrDefault(i => i.ProductId == groupItem.ProductId);
                if (itemEntity == null)
                {
                    //新增
                    var entity = new OrderblankItem()
                    {
                        OrderblankId = orderblankEntity.Id,
                        OrderblankNumber = orderblankEntity.OrderBlankNumber,
                        ProductId = groupItem.ProductId,
                        OrderBlankBarcodes = string.Join(",", groupItem.Barcodes),
                        Quantity = groupItem.Quantity,
                        OperatorId = AuthorityHelper.OperatorId
                    };
                    orderblankEntity.OrderblankItems.Add(entity);
                }
                else//更新数量
                {
                    var barcodesBeforeAdd = itemEntity.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    barcodesBeforeAdd.AddRange(groupItem.Barcodes);
                    itemEntity.OrderBlankBarcodes = string.Join(",", barcodesBeforeAdd);
                    itemEntity.Quantity += groupItem.Quantity;
                }
            }

            using (var transaction = _orderblankContract.GetTransaction())
            {
                // 更新配货单
                var res = _orderblankContract.Update(orderblankEntity);
                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "配货单保存失败" + res.Message);
                }
                // 锁定库存
                inventoryList.Each(i =>
                {
                    i.Status = InventoryStatus.DeliveryLock;
                    i.IsLock = true;
                });
                res = _inventoryContract.BulkUpdate(inventoryList);

                if (res.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return new OperationResult(OperationResultType.Error, "库存状态更新失败" + res.Message);
                }

                // 记录锁定日志
                if (inventoryList.Count > 0)
                {
                    res = _orderblankContract.LogWhenOrderblankAdd(orderblankEntity, inventoryList.ToArray());
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return new OperationResult(OperationResultType.Error, res.Message);
                    }
                }


                transaction.Commit();
                return new OperationResult(OperationResultType.Success, string.Empty);

            }
        }
        #endregion



        #region 初始化界面

        [Layout]
        public ActionResult Index(string number)
        {
            var listinStores = new List<SelectListItem>();
            var listoutStores = new List<SelectListItem>();
            var data = _adminContarct.GetDesignerStoreStorageList(AuthorityHelper.OperatorId.Value);
            if (data.Item1)
            {
                listoutStores = data.Item2;
                var daStorage = _storageContract.Storages.Where(w => w.IsForAddInventory && w.IsEnabled && !w.IsDeleted).FirstOrDefault();
                if (daStorage.IsNotNull())
                {
                    listinStores.Add(new SelectListItem() { Text = daStorage.Store.StoreName, Value = daStorage.StoreId + "" });
                }
            }
            ViewBag.IsDesigner = data.Item1;

            ViewBag.inStores = listinStores;
            ViewBag.outStores = listoutStores;//发货店铺
            ViewBag.Number = number ?? string.Empty;

            return View(new OrderblankDto());
        }
        #endregion

        #region 获取数据列表

        public ActionResult List()
        {

            var gridRequest = new GridRequest(Request);
            Expression<Func<Orderblank, bool>> predicate = FilterHelper.GetExpression<Orderblank>(gridRequest.FilterGroup);
            var orderblankQuery = _orderblankContract.Orderblanks.Where(c => c.IsEnabled).Where(predicate);

            List<object> li = new List<object>();
            var emptyList = new GridData<object>(li, 0, Request);
            if (!AuthorityHelper.OperatorId.HasValue)
            {
                return Json(emptyList);
            }
            int adminId = AuthorityHelper.OperatorId.Value;
            var adminEntity = _adminContarct.Administrators.Where(a => !a.IsDeleted && a.IsEnabled)
                                                            .Where(a => a.Id == adminId)
                                                            .FirstOrDefault();
            if (adminEntity == null)
            {
                return Json(emptyList);
            }

            // 仓库权限校验
            //var listStorageId = PermissionHelper.ManagedStorages(adminId, _adminContarct, s => s, f => f.IsEnabled && !f.IsDeleted).Select(s => s.Id).ToList();
            var listStorageId = _storeContract.QueryManageStorageId(adminId);
            if (!listStorageId.Any())
            {
                return Json(emptyList);
            }

            // 仓库筛选 发货仓库/收货仓库都包含
            orderblankQuery = orderblankQuery.Where(x => listStorageId.Contains(x.OutStorageId)
                                                      || listStorageId.Contains(x.ReceiverStorageId));
            //根修改时间降序
            var orderblanks = orderblankQuery.Include(o => o.Purchase)
                .Include(o => o.OutStore)
                .Include(o => o.ReceiverStore)
                .Include((o => o.Operator.Member))
                                             .OrderByDescending(c => c.CreatedTime)
                                             .Skip(gridRequest.PageCondition.PageIndex)
                                             .Take(gridRequest.PageCondition.PageSize)
                                             .ToList();


            foreach (var orderblankEntity in orderblanks)
            {
                // 是否有发货店铺权限
                var IsSender = false;
                // 是否有收货店铺权限
                var IsAccept = false;
                // 是否既有收货,又有发货的权限
                var IsBoth = false;

                // 发货店铺是否已闭店
                var isSenderClosed = orderblankEntity.OutStore.IsClosed;

                var isReceiverStoreClosed = orderblankEntity.ReceiverStore.IsClosed;

                // 判断是否有发货的权限
                if (listStorageId.Contains(orderblankEntity.OutStorageId))
                {
                    IsSender = true;
                }

                // 判断是否有收货的权限
                if (listStorageId.Contains(orderblankEntity.ReceiverStorageId))
                {
                    IsAccept = true;
                }

                // 既有收货,又有发货的权限
                if (IsSender && IsAccept)
                {
                    IsBoth = true;
                }

                li.Add(new
                {
                    orderblankEntity.AppointmentNumber,
                    Id = orderblankEntity.Id,
                    IsSender = IsSender,
                    IsAccept = IsAccept,
                    IsBoth = IsBoth,
                    PurchaseNumber = orderblankEntity.Purchase == null ? string.Empty : orderblankEntity.Purchase.PurchaseNumber,
                    OrderBlankNumber = orderblankEntity.OrderBlankNumber,
                    OutStoreName = orderblankEntity.OutStore.StoreName, //发货店铺
                    OutStorageName = orderblankEntity.OutStorage.StorageName,//发货仓库
                    ReceiverStoreName = orderblankEntity.ReceiverStore.StoreName,//收货店铺
                    ReceiverStorageName = orderblankEntity.ReceiverStorage.StorageName,//收货仓库
                    Quantity = GetCount(orderblankEntity.OrderblankItems.ToList()),//库存数量
                    Status = orderblankEntity.Status,//状态
                    MemberName = orderblankEntity.Operator == null ? string.Empty : orderblankEntity.Operator.Member.MemberName,//创建人
                    Weight = orderblankEntity.Weight,
                    CreatedTime = orderblankEntity.CreatedTime,//创建时间
                    isSenderClosed = isSenderClosed,
                    isReceiverStoreClosed = isReceiverStoreClosed
                });
            }
            emptyList = new GridData<object>(li, orderblankQuery.Count(), Request);


            return Json(emptyList);
        }

        /// <summary>
        /// 获取配货单下关联的库存数量
        /// </summary>
        private int GetCount(List<OrderblankItem> list)
        {
            int count = 0;
            if (list.Any())
            {
                List<string> listBarcodes = list.Select(x => x.OrderBlankBarcodes).ToList();
                List<string> listTemp = new List<string>();
                foreach (string barcode in listBarcodes)
                {
                    string[] arrBarcodes = barcode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    count += arrBarcodes.Length;
                }
            }
            return count;
        }
        #endregion



        #region 移除数据

        public ActionResult Remove(int? id)
        {
            var resul = new OperationResult(OperationResultType.Error);
            if (id.HasValue)
            {
                resul = _orderblankContract.Delete(id.Value);
            }
            else
            {
                resul.Message = "删除失败";
            }
            return Json(resul);
        }
        #endregion

        #region 拒绝配货

        /// <summary>
        /// 拒绝配货表单
        /// </summary>
        /// <returns></returns>
        public ActionResult RejectOrder()
        {
            ViewBag.orderblankNum = Request["odnum"];
            return PartialView();
        }

        //yxk 2015-11
        /// <summary>
        /// 拒绝配货操作
        /// </summary>
        /// <returns></returns>
        public ActionResult RejectOrderblank(string num, string msg)
        {

            //num: _num, meg: _meg            
            OperationResult oper = _orderblankContract.Refuse(num, msg, SendOrderBlankNotification);

            // 给仓库管理员发送通知
            return Json(oper);
        }
        #endregion

        #region 直接创建配货单

        /// <summary>
        /// 直接创建配货单
        /// </summary>
        /// <returns></returns>
        [Layout(overrideActionName: "Index")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(OrderblankDto dto)
        {
            if (dto.OutStorageId == dto.ReceiverStorageId)
            {
                return Json(new OperationResult(OperationResultType.Error, "发货仓库不能与收货仓库相同"));
            }

            OperationResult oper = _orderblankContract.Insert(true, dto);
            return Json(oper);
        }
        #endregion
        /// <summary>
        /// 修改/配货
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult StartOrderblank()
        {
            string num = Request["_num"];
            if (!string.IsNullOrEmpty(num))
            {
                var orderblank = _orderblankContract.Orderblanks.Where(c => c.OrderBlankNumber == num && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
                if (orderblank != null)
                {

                    var storage = _storageContract.Storages.Where(c => c.Id == orderblank.OutStorageId).FirstOrDefault();
                    var instorag = _storageContract.Storages.Where(c => c.Id == orderblank.ReceiverStorageId).FirstOrDefault();
                    //出货店铺
                    ViewBag.outstore = storage.Store.StoreName;
                    ViewBag.outStorageId = storage.Id;
                    //出货库
                    ViewBag.outstorage = storage.StorageName;
                    //入货店铺
                    ViewBag.inStore = instorag.Store.StoreName;
                    //入货仓库
                    ViewBag.inStorage = instorag.StorageName;
                    //备注
                    ViewBag.notes = orderblank.Notes;
                    //配货单号
                    ViewBag.Ordernum = num;
                    ViewBag.OrderblankId = orderblank.Id;
                    return View();
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "未找到配货记录"));
                }
            }
            else
            {
                return Json(new OperationResult(OperationResultType.Error, "配货单编号不为空"));
            }

        }

        #region 采购选择商品

        public ActionResult GetProductList(string orderblankNum)
        {

            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true);
            ViewBag.orderblankNum = orderblankNum;
            return PartialView();
        }
        public JsonResult ProductList(string productNumber, string productBarcode, int? brandId, string orderblankNum)
        {
            var orderbalnkEntity = _orderblankContract.Orderblanks.FirstOrDefault(o => o.OrderBlankNumber == orderblankNum);
            var res = CheckOrderblankEntity(orderbalnkEntity);
            if (!res.Item1)
            {
                return Json(new GridData<object>(new List<object>(), 0, Request));
            }

            var requ = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Inventory>(requ.FilterGroup);

            //对仓库下的所有库存分组
            var querySouce = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                                    .Where(pred)
                                    .Where(c => c.IsEnabled && !c.IsLock && !c.IsDeleted && c.Status == InventoryStatus.Default)
                                    .Where(i => i.StoreId == orderbalnkEntity.OutStoreId && i.StorageId == orderbalnkEntity.OutStorageId)
                                    .Where(x => !string.IsNullOrEmpty(x.ProductBarcode));
            if (!string.IsNullOrEmpty(productBarcode))
            {
                querySouce = querySouce.Where(i => i.ProductBarcode == productBarcode);
            }
            if (!string.IsNullOrEmpty(productNumber))
            {
                querySouce = querySouce.Where(i => i.Product.ProductNumber == productNumber);
            }
            if (brandId.HasValue && brandId != -1)
            {
                querySouce = querySouce.Where(i => i.Product.ProductOriginNumber.BrandId == brandId.Value);
            }
            var alldata = querySouce.GroupBy(c => c.ProductNumber);
            var da = alldata.OrderByDescending(c => c.Count())
                .ThenByDescending(c => c.Key)
                .Skip(requ.PageCondition.PageIndex)
                .Take(requ.PageCondition.PageSize)
                .Select(c => new
                {
                    c.Key,
                    c.FirstOrDefault().ProductBarcode,
                    c.FirstOrDefault().Product.Color.ColorName,
                    c.FirstOrDefault().Product.ProductOriginNumber.Season.SeasonName,
                    c.FirstOrDefault().Product.ProductOriginNumber.Brand.BrandName,
                    c.FirstOrDefault().Product.Size.SizeName,
                    c.FirstOrDefault().Product.ThumbnailPath,
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
                    ite.ThumbnailPath,
                    Count = ite.Childs.Count()
                });
                li.AddRange(ite.Childs.Select(c => new
                {
                    Id = c.ProductBarcode,
                    ParentId = ite.Key,
                    c.ProductBarcode,
                    Count = 1
                }).ToList());
            }
            GridData<Object> objdata = new GridData<object>(li, alldata.Count(), Request);
            return Json(objdata);
        }


        #endregion

        #region 查看配货单明细

        /// <summary>
        /// 查看配货单明细
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderblankView()
        {
            var orderblankId = Request["id"] ?? string.Empty;
            if (!string.IsNullOrEmpty(orderblankId))
            {
                ViewBag.Brands = CacheAccess.GetBrand(_brandContract, true, false);
                int ordid = Convert.ToInt32(orderblankId);
                var orderblankEntity = _orderblankContract.Orderblanks.Where(c => c.Id == ordid).FirstOrDefault();
                if (orderblankEntity != null)
                {
                    TempData["ReceiverId"] = orderblankEntity.ReceiverStore.StoreName;
                    TempData["ReceiverStorageId"] = orderblankEntity.ReceiverStorage.StorageName;
                    TempData["storageOut"] = orderblankEntity.OutStorage.StorageName;
                    TempData["Operator"] = orderblankEntity.Operator?.Member?.MemberName;
                }
                return PartialView(orderblankEntity);
            }
            return Json(new OperationResult(OperationResultType.Error));
        }

        #endregion


        /// <summary>
        /// 获取配货单明细
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderblankViewList(string orderblankNumber, int? brandId, string barcode)
        {
            var req = new GridRequest(Request);

            var orderblankIemQuery = _orderblankItemContract.OrderblankItems.Where(c => c.OrderblankNumber == orderblankNumber)
                                                                        .Select(c => new
                                                                        {
                                                                            Id = c.Id,
                                                                            productId = c.ProductId,
                                                                            ProductNumber = c.Product.ProductNumber,
                                                                            BrandId = c.Product.ProductOriginNumber.BrandId,
                                                                            Brand = c.Product.ProductOriginNumber.Brand.BrandName,
                                                                            Size = c.Product.Size.SizeName,
                                                                            Season = c.Product.ProductOriginNumber.Season.SeasonName,
                                                                            Color = c.Product.Color.ColorName,
                                                                            Thumbnail = c.Product.ThumbnailPath ?? c.Product.ProductOriginNumber.ThumbnailPath,
                                                                            WholesalePrice = c.Product.ProductOriginNumber.WholesalePrice,
                                                                            TagPrice = c.Product.ProductOriginNumber.TagPrice,
                                                                            Quantity = c.Quantity,
                                                                            Childs = c.OrderBlankBarcodes
                                                                        });

            if (!orderblankIemQuery.Any())
            {
                return Json(new GridData<object>(new List<object>(), 0, Request));
            }

            if (brandId.HasValue && brandId > 0)
            {
                orderblankIemQuery = orderblankIemQuery.Where(i => i.BrandId == brandId.Value);
            }

            if (!string.IsNullOrEmpty(barcode))
            {
                orderblankIemQuery = orderblankIemQuery.Where(i => i.Childs.Contains(barcode));
            }

            var pagedList = orderblankIemQuery.OrderByDescending(i => i.Id)
                                            .Skip(req.PageCondition.PageIndex)
                                            .Take(req.PageCondition.PageSize).ToList();
            var dataList = new List<object>();

            // 根据每一项的barcodes生成child节点
            for (int i = 0; i < pagedList.Count; i++)
            {
                var item = pagedList[i];
                var codes = item.Childs.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var parent = new
                {
                    item.Id,
                    ParentId = "",
                    item.productId,
                    Number = item.ProductNumber,
                    item.Brand,
                    item.Size,
                    item.Season,
                    item.Color,
                    item.Thumbnail,
                    item.WholesalePrice,
                    item.TagPrice,
                    Quantity = codes.Count()
                };

                if (codes.Any())
                {
                    dataList.Add(parent);
                    dataList.AddRange(codes.Select(c => new
                    {
                        Id = c,
                        ParentId = item.Id,
                        Number = c,
                        Storage = _inventoryContract.Inventorys.FirstOrDefault(g => g.ProductBarcode == c).Storage.StorageName
                    }));
                }
            }
            GridData<object> dat = new GridData<object>(dataList, orderblankIemQuery.Count(), Request);
            return Json(dat);

        }

        /// <summary>
        /// 从配货单中移除指定的流水号
        /// </summary>
        /// <param name="barcodesFromUser">提交流水号</param>
        /// <param name="orderblankNum">配货单号</param>
        /// <param name="uuid">缓存key</param>
        /// <returns></returns>
        public ActionResult RemoveBarcodeFromOrderblank(string[] barcodesFromUser, string orderblankNum, string uuid)
        {
            var validListFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_VALID_LIST + uuid) ?? new List<Product_Model>();
            var invalidlistFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_INVALID_LIST + uuid) ?? new List<Product_Model>();


            var orderblankEntity = _orderblankContract.Orderblanks.Where(o => !o.IsDeleted && o.IsEnabled)
                                                                      .Where(o => o.OrderBlankNumber == orderblankNum)
                                                                      .Include(o => o.OrderblankItems)
                                                                      .FirstOrDefault();
            var checkRes = CheckOrderblankEntity(orderblankEntity);
            if (!checkRes.Item1)
            {
                return Json(new OperationResult(OperationResultType.Error, checkRes.Item2));
            }
            var orderblankItems = orderblankEntity.OrderblankItems.ToList();
            if (!orderblankItems.Any())
            {
                return Json(new OperationResult(OperationResultType.Error, "配货单明细为空,无法删除"));
            }

            var barcodeListFromDb = orderblankItems.SelectMany(code => code.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            if (barcodesFromUser.Except(barcodeListFromDb).Count() > 0)
            {
                return Json(new OperationResult(OperationResultType.Error, "流水号不存在,无法删除"));
            }
            var inventoriesToRemove = _inventoryContract.Inventorys.Where(i => barcodesFromUser.Contains(i.ProductBarcode)).ToList();
            if (inventoriesToRemove.Count != barcodesFromUser.Length)
            {
                return Json(new OperationResult(OperationResultType.Error, "流水号不一致,无法删除"));
            }
            var productIdGroup = inventoriesToRemove.GroupBy(i => i.ProductId);
            var orderblankItemToDelete = new List<OrderblankItem>();
            var orderblankItemToUpdate = new List<OrderblankItem>();
            foreach (var groupItem in productIdGroup)
            {
                var productId = groupItem.Key;
                var orderblankItem = orderblankItems.First(i => i.ProductId == productId);
                var currentCodes = orderblankItem.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var codesToRemove = groupItem.Select(g => g.ProductBarcode).ToList();

                // 确保流水号在item中存在
                if (codesToRemove.Except(currentCodes).Count() > 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "流水号无效,无法删除"));
                }

                // 从item中移除流水号
                currentCodes.RemoveAll(c => codesToRemove.Contains(c));

                // 判断是否删除了整个item的流水号
                if (currentCodes.Count <= 0)
                {
                    orderblankItemToDelete.Add(orderblankItem);
                }
                else //更新流水号
                {
                    orderblankItem.OrderBlankBarcodes = string.Join(",", currentCodes);
                    orderblankItemToUpdate.Add(orderblankItem);
                }
            }

            using (var transaction = _orderblankContract.GetTransaction())
            {
                // 更新
                if (orderblankItemToUpdate.Count > 0)
                {

                    var updateRes = _orderblankItemContract.Update(orderblankItemToUpdate.ToArray());
                    if (updateRes.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return Json(new OperationResult(OperationResultType.Error, updateRes.Message));
                    }
                }

                // 真。删除
                if (orderblankItemToDelete.Count > 0)
                {
                    var deleteRes = _orderblankItemContract.Delete(orderblankItemToDelete.Select(i => i.Id).ToArray());
                    if (deleteRes.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return Json(new OperationResult(OperationResultType.Error, deleteRes.Message));
                    }
                }


                // 库存解锁
                if (inventoriesToRemove.Count > 0)
                {
                    inventoriesToRemove.Each(i => { i.IsLock = false; i.Status = InventoryStatus.Default; });
                    var res = _inventoryContract.BulkUpdate(inventoriesToRemove);
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return Json(new OperationResult(OperationResultType.Error, res.Message));
                    }
                }

                // 记录解锁日志
                if (inventoriesToRemove.Count > 0)
                {
                    var res = _orderblankContract.LogWhenOrderblankRemove(inventoriesToRemove.ToArray());
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return Json(new OperationResult(OperationResultType.Error, res.Message));
                    }
                }




                // 将删除的条码从缓存校验列表中移除,这样,后面还可以重新加入条码
                validListFromCache.RemoveAll(i => barcodesFromUser.Contains(i.ProductBarcode));
                invalidlistFromCache.RemoveAll(i => barcodesFromUser.Contains(i.ProductBarcode));
                SessionAccess.Set(SESSION_KEY_VALID_LIST + uuid, validListFromCache);
                SessionAccess.Set(SESSION_KEY_INVALID_LIST + uuid, invalidlistFromCache);
                var resul = new OperationResult(OperationResultType.Success, string.Empty);
                resul.Data = new { validCount = validListFromCache.Count, invalidCount = invalidlistFromCache.Count };
                transaction.Commit();
                return Json(resul);
            }



        }


        /// <summary>
        /// 批量导入校验
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nums"></param>
        /// <param name="orderblanknum"></param>
        /// <returns></returns>
        public JsonResult MultitudeVaild(int id, string nums, string orderblanknum, string uuid)
        {
            var dat = nums.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new OperationResult(OperationResultType.Error, "");
            var validListFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_VALID_LIST + uuid) ?? new List<Product_Model>();
            var invalidlistFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_INVALID_LIST + uuid) ?? new List<Product_Model>();
            var orderblankEntity = _orderblankContract.Orderblanks.Where(o => !o.IsDeleted && o.IsEnabled)
                                                                  .Where(o => o.OrderBlankNumber == orderblanknum)
                                                                  .Include(o => o.OrderblankItems)
                                                                  .FirstOrDefault();

            var inventoryListFromDb = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                                                 .Where(i => !i.IsDeleted && i.IsEnabled)
                                                 .Where(i => i.StoreId == orderblankEntity.OutStoreId && i.StorageId == orderblankEntity.OutStorageId)
                                                 .Where(i => dat.Contains(i.ProductBarcode))
                                                 .Include(i => i.Product)
                                                 .Include(i => i.Product.Size)
                                                 .Include(i => i.Product.Color)
                                                 .Include(i => i.Product.ProductOriginNumber)
                                                 .Include(i => i.Product.ProductOriginNumber.Season)
                                                 .Include(i => i.Product.ProductOriginNumber.Category)
                                                 .Include(i => i.Product.ProductOriginNumber.Brand)
                                                 .ToList();


            string strMessage = string.Empty;

            var modelList = dat.Select(barcode => new Product_Model { ProductBarcode = barcode, UUID = Guid.NewGuid().ToString() }).ToList();


            var checkRes = CheckOrderblankEntity(orderblankEntity);
            if (!checkRes.Item1)
            {
                invalidlistFromCache.Add(new Product_Model { ProductBarcode = string.Empty });
            }
            else //批量校验
            {
                var tmpValidModels = new List<Product_Model>();
                //var allValid = true;
                var orderblankItemsFromDb = orderblankEntity.OrderblankItems.ToList();
                foreach (var modelToCheck in modelList)
                {
                    var res = CheckBarcode(modelToCheck, validListFromCache, invalidlistFromCache, orderblankItemsFromDb, inventoryListFromDb);
                    if (!res.Item1)
                    {
                        //allValid = false;
                        modelToCheck.Notes = res.Item2;
                        invalidlistFromCache.Add(modelToCheck);
                    }
                    else
                    {
                        tmpValidModels.Add(modelToCheck);
                        validListFromCache.Add(modelToCheck);
                    }

                }
                if (tmpValidModels.Count > 0)
                {
                    var optRes = BatchAddOrderblankItem(orderblankEntity, inventoryListFromDb, tmpValidModels.ToArray());
                    if (optRes.ResultType != OperationResultType.Success)
                    {
                        invalidlistFromCache.Add(new Product_Model { ProductBarcode = string.Empty, Notes = optRes.Message });
                    }

                }
            }

            SessionAccess.Set(SESSION_KEY_VALID_LIST + uuid, validListFromCache);
            SessionAccess.Set(SESSION_KEY_INVALID_LIST + uuid, invalidlistFromCache);
            result.Data = new { validCount = validListFromCache.Count, invalidCount = invalidlistFromCache.Count };
            result.ResultType = OperationResultType.Success;
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 配货页面扫码校验
        /// </summary>
        /// <returns></returns>
        public ActionResult StartOrderblankValid(string entryId, string barcode, string orderblanknum, string uuid)
        {
            var result = new OperationResult(OperationResultType.Error, "");

            var validListFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_VALID_LIST + uuid) ?? new List<Product_Model>();
            var invalidlistFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_INVALID_LIST + uuid) ?? new List<Product_Model>();
            var orderblankEntity = _orderblankContract.Orderblanks.Where(o => !o.IsDeleted && o.IsEnabled)
                                                                  .Where(o => o.OrderBlankNumber == orderblanknum)
                                                                  .Include(o => o.OrderblankItems)
                                                                  .FirstOrDefault();


            var inventoryListFromDb = _inventoryContract.Inventorys
                                                .Where(i => i.StoreId == orderblankEntity.OutStoreId && i.StorageId == orderblankEntity.OutStorageId)
                                                .Where(i => i.ProductBarcode == barcode).ToList();
            var modelToCheck = new Product_Model() { ProductBarcode = barcode };


            var checkRes = CheckOrderblankEntity(orderblankEntity);
            if (!checkRes.Item1)
            {
                invalidlistFromCache.Add(modelToCheck);
            }
            else //配货单存在
            {
                var orderblankItemsFromDb = orderblankEntity.OrderblankItems.ToList();
                var res = CheckBarcode(modelToCheck, validListFromCache, invalidlistFromCache, orderblankItemsFromDb, inventoryListFromDb);
                if (!res.Item1)
                {
                    modelToCheck.Notes = res.Item2;
                    invalidlistFromCache.Add(modelToCheck);
                }
                else  //有效
                {
                    var optRes = BatchAddOrderblankItem(orderblankEntity, inventoryListFromDb, modelToCheck);
                    if (optRes.ResultType != OperationResultType.Success)
                    {
                        modelToCheck.Notes = optRes.Message;
                        invalidlistFromCache.Add(modelToCheck);
                    }
                    else //保存成功
                    {
                        validListFromCache.Add(modelToCheck);
                    }

                }
            }

            SessionAccess.Set(SESSION_KEY_VALID_LIST + uuid, validListFromCache);
            SessionAccess.Set(SESSION_KEY_INVALID_LIST + uuid, invalidlistFromCache);

            //返回单条校验结果
            //result.Message = modelToCheck.Notes;
            return Json(new OperationResult(OperationResultType.Success, string.Empty, new { validCount = validListFromCache.Count, invalidCount = invalidlistFromCache.Count, entryId = entryId }));
        }




        #region 保存配货单开始配货

        /// <summary>
        /// 保存配货单/保存并开始配货
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Log]
        public ActionResult SaveOrderblankAndSend()
        {

            var resul = new OperationResult(OperationResultType.Error);

            string ordnum = Request["num"];
            bool issend = Request["send"] == "1";//是否配货
            string uuid = Request["uuid"];
            string notes = Request["notes"];

            var orderblankEntity = _orderblankContract.Orderblanks.FirstOrDefault(c => c.OrderBlankNumber == ordnum && !c.IsDeleted && c.IsEnabled);
            if (orderblankEntity == null)
            {
                return Json(OperationResult.Error("未查找到相关的配货单"));
            }

            //配货单状态校验
            if (orderblankEntity.Status != OrderblankStatus.配货中)
            {
                switch (orderblankEntity.Status)
                {
                    case OrderblankStatus.发货中:
                        {
                            resul.Message = "该配货单已发货，不能执行配货操作";
                            break;
                        }
                    case OrderblankStatus.已撤销:
                        {
                            resul.Message = "该配货单已经被撤销，不能执行配货操作";
                            break;
                        }
                    case OrderblankStatus.已完成:
                        {
                            resul.Message = "该配货单已经收货，不能执行配货操作";
                            break;
                        }
                }
                return Json(resul);
            }

            //配货单店铺状态校验
            var checkRes = _orderblankContract.CheckOrderbalnkAction(orderblankEntity, OrderblankAction.Delivery);
            if (checkRes.ResultType != OperationResultType.Success)
            {
                return Json(checkRes);
            }

            orderblankEntity.Notes = notes;
            orderblankEntity.UpdatedTime = DateTime.Now;
            if (!issend) //保存操作
            {
                resul = _orderblankContract.Update(orderblankEntity);
                return Json(resul);
            }


            // 确认发货操作
            var barcodeArr = orderblankEntity.OrderblankItems.ToList().SelectMany(i => i.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            if (barcodeArr.Count <= 0)
            {
                resul.Message = "发货商品不可为空";
                return Json(resul);
            }
            var invents = _inventoryContract.Inventorys
                                    .Where(c => barcodeArr.Contains(c.ProductBarcode) && c.IsEnabled && !c.IsDeleted)
                                    .ToList();
            if (invents.Count <= 0)
            {
                resul.Message = "发货商品流水号不存在";
                return Json(resul);
            }

            var settingEntity = _timeoutSettingContract.GetTimeoutSettingForOrderblank(OrderblankAction.Delivery);
            var timeout = CheckTimeout(orderblankEntity.CreatedTime, settingEntity);
            using (var transaction = _orderblankContract.GetTransaction())
            {
                if (timeout)
                {

                    _orderblankContract.TimeoutProcess(PunishTypeEnum.配货单发货超时, orderblankEntity.OrderBlankNumber, settingEntity.DeductScore);
                }

                //更新配货单状态
                orderblankEntity.Status = OrderblankStatus.发货中;
                orderblankEntity.DeliverAdminId = AuthorityHelper.OperatorId;
                orderblankEntity.DeliveryTime = DateTime.Now;
                resul = _orderblankContract.Update(orderblankEntity);
                if (resul.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return Json(new OperationResult(OperationResultType.Error, "配货单保存失败"));
                }

                invents.Each(c =>
                {
                    c.UpdatedTime = DateTime.Now;
                    c.OperatorId = AuthorityHelper.OperatorId;
                    c.Others = "配货完成，待接收";
                    c.Status = InventoryStatus.UnAccept;
                });
                resul = _inventoryContract.BulkUpdate(invents);
                if (resul.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return Json(new OperationResult(OperationResultType.Error, "配货单保存失败"));
                }

                //记录发货
                resul = _orderblankContract.LogWhenOrderblankDelivery(orderblankEntity, invents.ToArray());
                if (resul.ResultType != OperationResultType.Success)
                {
                    transaction.Rollback();
                    return Json(new OperationResult(OperationResultType.Error, "发货日志记录失败"));
                }

                // 给仓库管理员发送通知
                var notice = new OrderblankNoticeModel()
                {
                    IsReject = false,
                    OrderblankNumer = orderblankEntity.OrderBlankNumber,
                    OutStorageId = orderblankEntity.OutStorageId,
                    ReceiverStorageId = orderblankEntity.ReceiverStorageId
                };
                SendOrderBlankNotification(notice);
                transaction.Commit();
                return Json(OperationResult.OK());
            }
        }

        public ActionResult IsTimeout(string orderblankNum, OrderblankAction action)
        {
            var orderblankEntity = _orderblankContract.Orderblanks.FirstOrDefault(c => c.OrderBlankNumber == orderblankNum);
            var checkRes = CheckOrderblankEntity(orderblankEntity);
            if (!checkRes.Item1)
            {
                return Json(new OperationResult(OperationResultType.Error, checkRes.Item2));
            }
            var settingEntity = _timeoutSettingContract.GetTimeoutSettingForOrderblank(action);
            var timeout = false;
            switch (action)
            {
                case OrderblankAction.Create:
                    break;
                case OrderblankAction.Delete:
                case OrderblankAction.Delivery:
                    {
                        timeout = CheckTimeout(orderblankEntity.CreatedTime, settingEntity);
                    }
                    break;
                case OrderblankAction.Reject:
                case OrderblankAction.Accept:
                    {
                        timeout = CheckTimeout(orderblankEntity.DeliveryTime.Value, settingEntity);
                    }
                    break;
                default:
                    break;
            }
            var deductScore = 0;
            if (timeout)
            {

                deductScore = settingEntity.DeductScore;
                var requestQuery = _timeoutRequestContract.Entities.Where(t => !t.IsDeleted && t.IsEnabled && !t.IsUsed
                                    && t.State == TimeoutRequestState.已通过
                                    && t.RequestAdminId == AuthorityHelper.OperatorId.Value
                                    && t.Number == orderblankNum);
                if (requestQuery.Any())
                {
                    deductScore = 0;
                }
            }

            return Json(new OperationResult(OperationResultType.Success, timeout ? "timeout" : "ok", data: timeout ? deductScore : 0));
        }




        #endregion

        /// <summary>
        /// 是否超时判断
        /// </summary>
        /// <param name="referTimePoint">参照时间点</param>
        /// <param name="settingEntity">超时配置信息</param>
        /// <returns></returns>
        private bool CheckTimeout(DateTime referTimePoint, TimeoutSetting settingEntity)
        {
            if (settingEntity == null)
            {
                return false;
            }
            var timeoutDate = referTimePoint.Add(TimeSpan.FromSeconds(settingEntity.TimeSpan));
            return DateTime.Now > timeoutDate;
        }


        #region 配货单确认收货

        /// <summary>
        /// 配货单确认收货
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ReceptProduct(string num)
        {
            var res = _orderblankContract.ReceptProduct(num);
            return Json(res);
        }

        #endregion

        #region 初始化批量导出界面
        public ActionResult BatchImport()
        {
            return PartialView();
        }
        #endregion

        #region 上传Excel表格
        public JsonResult ExcelFileUpload()
        {
            var res = new OperationResult(OperationResultType.Error);
            if (Request.Files.Count > 0)
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
                {
                    var list = reda.Select(s => s.First()).ToList();
                    res = new OperationResult(OperationResultType.Success, string.Empty, list);
                }
            }
            return Json(res);
        }
        #endregion

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
                }
                return da;
            }
            return null;
        }
        #endregion

        #region 获取仓库
        public JsonResult GetStorages(int storeId)
        {
            var da = _storageContract.Storages.Where(c => c.IsDeleted == false && c.IsEnabled == true && c.StoreId == storeId).Select(c => new { Id = c.Id, Name = c.StorageName, IsDefault = c.IsDefaultStorage });

            return Json(da);
        }

        #endregion

        #region 查看配货列表
        /// <summary>
        /// 获取配货单列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> DetailList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<OrderblankItem, bool>> predicate = FilterHelper.GetExpression<OrderblankItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<OrderblankItem> listOrderblankItem = _orderblankItemContract.OrderblankItems.Where<OrderblankItem, int>(predicate, request.PageCondition, out count);
                IQueryable<Product> listProduct = _productContract.Products;
                var list = (from ob in listOrderblankItem
                            join
                            pr in listProduct
                            on
                            ob.ProductId equals pr.Id
                            select new
                            {
                                ob.OrderBlankBarcodes,
                                pr.ProductOriginNumber.Brand.BrandName,
                                pr.Size.SizeName,
                                pr.ProductOriginNumber.Season.SeasonName,
                                pr.Color.ColorName,
                                pr.ThumbnailPath,
                            }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 初始化配货列表界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult ShowDetail(int Id)
        {
            ViewBag.OrderblankId = Id;
            return PartialView();
        }
        #endregion

        #region 查看无效数据
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        public ActionResult InValid(string uuid)
        {
            ViewBag.uuid = uuid;
            return PartialView();
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public ActionResult InValidList(string uuid)
        {

            var request = new GridRequest(Request);
            int count = 0;
            int pageIndex = request.PageCondition.PageIndex;
            int pageSize = request.PageCondition.PageSize;

            var invalidlistFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_INVALID_LIST + uuid) ?? new List<Product_Model>();
            count = invalidlistFromCache.Count;
            var index = 1;
            var resData = invalidlistFromCache.Skip(pageIndex).Take(pageSize).Select(p => new { Id = index++, ProductBarcode = p.ProductBarcode, Notes = p.Notes }).ToList();
            var data = new GridData<object>(resData, count, request.RequestInfo);
            return Json(data, JsonRequestBehavior.AllowGet);


        }
        #endregion

        #region 查看有效数据
        /// <summary>
        /// 初始化数据界面
        /// </summary>
        /// <returns></returns>
        public ActionResult VaildView(string uuid)
        {
            ViewBag.uuid = uuid;
            return PartialView();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public ActionResult VaildViewList(string uuid)
        {
            GridRequest request = new GridRequest(Request);
            int count = 0;
            int pageIndex = request.PageCondition.PageIndex;
            int pageSize = request.PageCondition.PageSize;

            var validListFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_VALID_LIST + uuid) ?? new List<Product_Model>();
            var invalidlistFromCache = (List<Product_Model>)SessionAccess.Get(SESSION_KEY_INVALID_LIST + uuid) ?? new List<Product_Model>();
            count = validListFromCache.Count;
            var index = 1;
            var resData = validListFromCache.Skip(pageIndex).Take(pageSize).Select(p => new { Id = index++, ProductBarcode = p.ProductBarcode }).ToList();
            var data = new GridData<object>(resData, count, request.RequestInfo);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion





        /// <summary>
        /// 有效
        /// </summary>
        const string CHECK_KEY_VALIDCOUNT = "VaildNums_";

        /// <summary>
        /// 无效
        /// </summary>
        const string CHECK_KEY_INVALIDCOUNT = "InvaildNums_";

        /// <summary>
        /// 缺货
        /// </summary>
        const string CHECK_KEY_MISSINGCOUNT = "MissingCount_";

        /// <summary>
        /// 余货
        /// </summary>
        const string CHECK_KEY_RESIDUECOUNT = "residueCount_";

        /// <summary>
        /// 汇总
        /// </summary>
        const string CHECK_KEY_CHECKEDINFO = "checkedInfo_";

        #region 配货单盘点界面
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IsContinute">可选参数（false表示不继续，true表示继续）</param>
        /// <param name="Id">盘点Id</param>
        /// <returns></returns>
        [Layout]
        public ActionResult CheckInventory(string orderblankNum)
        {
            // 生成一个缓存key
            var cacheKey = CreateCacheKeyGuid();
            ViewBag.guid = cacheKey;
            int adminId = AuthorityHelper.OperatorId ?? 0;
            var orderblankEntity = _orderblankContract.Orderblanks.FirstOrDefault(x => x.OrderBlankNumber == orderblankNum);
            var checkerDto = new CheckerDto();
            var totalBarcodes = orderblankEntity.OrderblankItems.ToList()
                                                                .SelectMany(i => i.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                                                .ToList();
            checkerDto.OrberblankId = orderblankEntity.Id;
            checkerDto.CheckedQuantity = 0;//已盘数量
            checkerDto.MissingQuantity = totalBarcodes.Count;//缺货数量
            checkerDto.CheckQuantity = totalBarcodes.Count;//待盘数量
            checkerDto.ValidQuantity = 0;//有效数量
            checkerDto.InvalidQuantity = 0;//无效数量
            checkerDto.ResidueQuantity = 0;//余货数量

            checkerDto.CheckerName = DateTime.Now.ToString("yyyy年MM月dd日") + "配货盘点";
            checkerDto.CheckGuid = CreateCacheKeyGuid();
            checkerDto.StoreId = orderblankEntity.ReceiverStoreId;
            checkerDto.StorageId = orderblankEntity.ReceiverStorageId;
            checkerDto.CheckerState = CheckerFlag.Checking;
            ViewBag.OrderblankNum = orderblankNum;
            ViewBag.CheckStoreName = orderblankEntity.ReceiverStore.StoreName;
            ViewBag.CheckStorageName = orderblankEntity.ReceiverStorage.StorageName;

            //缓存初始化
            var checkInfo = new CheckedType()
            {
                CheckedQuantity = checkerDto.CheckedQuantity,
                CheckQuantity = checkerDto.CheckQuantity,
                ValidQuantity = checkerDto.ValidQuantity,
                InvalidQuantity = checkerDto.InvalidQuantity,
                MissingQuantity = checkerDto.MissingQuantity,
                ResidueQuantity = checkerDto.ResidueQuantity
            };
            var invalidlist = new List<Product_Model>();
            var validlist = new List<Product_Model>();
            var residueList = new List<Product_Model>();
            var missingList = totalBarcodes.Select(code => new Product_Model() { ProductBarcode = code, Notes = "缺货" }).ToList();
            SessionAccess.Set(CHECK_KEY_CHECKEDINFO + cacheKey, checkInfo);
            SessionAccess.Set(CHECK_KEY_VALIDCOUNT + cacheKey, validlist);
            SessionAccess.Set(CHECK_KEY_INVALIDCOUNT + cacheKey, invalidlist);
            SessionAccess.Set(CHECK_KEY_RESIDUECOUNT + cacheKey, residueList);
            SessionAccess.Set(CHECK_KEY_MISSINGCOUNT + cacheKey, missingList);


            return View(checkerDto);
        }

        #endregion
        #region 生成GUid
        private string CreateCacheKeyGuid()
        {
            string num = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 9);
            return num;
        }
        #endregion




        /// <summary>
        /// 盘点时,扫码单条校验
        /// </summary>
        public ActionResult AddToScan(string uuid, string barcode, string orderblankNumber, string guid)
        {
            var result = new OperationResult(OperationResultType.Success);
            //拿到存在Session中的有效和无效数据
            var validList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_VALIDCOUNT + guid) ?? new List<Product_Model>();
            var invalidList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_INVALIDCOUNT + guid) ?? new List<Product_Model>();
            var residueList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_RESIDUECOUNT + guid) ?? new List<Product_Model>();
            var missingList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_MISSINGCOUNT + guid) ?? new List<Product_Model>();
            var checkInfo = SessionAccess.Get(CHECK_KEY_CHECKEDINFO) as CheckedType ?? new CheckedType() { };
            var orderblankEntity = _orderblankContract.Orderblanks.Where(o => !o.IsDeleted && o.IsEnabled)
                                                                  .Where(o => o.OrderBlankNumber == orderblankNumber)
                                                                  .Include(o => o.OrderblankItems)
                                                                  .FirstOrDefault();

            var model = new Product_Model() { ProductBarcode = barcode, UUID = uuid };
            //校验配货单
            var res = CheckOrderblankEntity(orderblankEntity);
            if (!res.Item1)
            {
                model.Notes = res.Item2;
                invalidList.Add(model);
            }
            else
            {
                //校验流水号
                var listOrderblankItem = orderblankEntity.OrderblankItems.ToList();
                var totalBarcodes = listOrderblankItem.SelectMany(i => i.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList();

                var inventoryList = _inventoryContract.Inventorys.Where(i => i.StoreId == orderblankEntity.OutStoreId && i.StorageId == orderblankEntity.OutStorageId)
                                                .Where(i => totalBarcodes.Contains(i.ProductBarcode))
                                                .Include(i => i.Product)
                                                .Include(i => i.Product.Size)
                                                .Include(i => i.Product.Color)
                                                .Include(i => i.Product.ProductOriginNumber)
                                                .Include(i => i.Product.ProductOriginNumber.Season)
                                                .Include(i => i.Product.ProductOriginNumber.Category)
                                                .Include(i => i.Product.ProductOriginNumber.Brand)
                                                .ToList();
                res = CheckBarcode(model, validList, invalidList, residueList, missingList, listOrderblankItem, inventoryList);
                if (!res.Item1)
                {
                    model.Notes = res.Item2;
                }


                var oper = new OperationResult(OperationResultType.Success);
                checkInfo.UUID = uuid;
                checkInfo.ValidQuantity = validList.Count;
                checkInfo.InvalidQuantity = invalidList.Count;
                checkInfo.MissingQuantity = missingList.Count;
                checkInfo.ResidueQuantity = residueList.Count;


            }

            SessionAccess.Set(CHECK_KEY_CHECKEDINFO + guid, checkInfo);
            SessionAccess.Set(CHECK_KEY_VALIDCOUNT + guid, validList);
            SessionAccess.Set(CHECK_KEY_INVALIDCOUNT + guid, invalidList);
            SessionAccess.Set(CHECK_KEY_RESIDUECOUNT + guid, residueList);

            result.Data = checkInfo;
            result.ResultType = OperationResultType.Success;
            return Json(result, JsonRequestBehavior.AllowGet);


        }


        /// <summary>
        /// 盘点时批量导入校验
        /// </summary>
        public ActionResult CheckerMultitudeVaild(string nums, string orderblankNumber, int checkCount, int checkedCount, string guid)
        {
            var dat = nums.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new OperationResult(OperationResultType.Error, "");
            //拿到存在Session中的有效和无效数据
            var validList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_VALIDCOUNT + guid) ?? new List<Product_Model>();
            var invalidList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_INVALIDCOUNT + guid) ?? new List<Product_Model>();
            var residueList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_RESIDUECOUNT + guid) ?? new List<Product_Model>();
            var missingList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_MISSINGCOUNT + guid) ?? new List<Product_Model>();
            var checkInfo = SessionAccess.Get(CHECK_KEY_CHECKEDINFO) as CheckedType ?? new CheckedType() { };
            var orderblankEntity = _orderblankContract.Orderblanks.Where(o => !o.IsDeleted && o.IsEnabled)
                                                                  .Where(o => o.OrderBlankNumber == orderblankNumber)
                                                                  .Include(o => o.OrderblankItems)
                                                                  .FirstOrDefault();

            var inventoryListFromDb = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                                                 .Where(i => !i.IsDeleted && i.IsEnabled)
                                                 .Where(i => i.StoreId == orderblankEntity.OutStoreId && i.StorageId == orderblankEntity.OutStorageId)
                                                 .Where(i => dat.Contains(i.ProductBarcode))
                                                 .Include(i => i.Product)
                                                 .Include(i => i.Product.Size)
                                                 .Include(i => i.Product.Color)
                                                 .Include(i => i.Product.ProductOriginNumber)
                                                 .Include(i => i.Product.ProductOriginNumber.Season)
                                                 .Include(i => i.Product.ProductOriginNumber.Category)
                                                 .Include(i => i.Product.ProductOriginNumber.Brand)
                                                 .ToList();


            string strMessage = string.Empty;

            var modelList = dat.Select(barcode => new Product_Model { ProductBarcode = barcode, UUID = Guid.NewGuid().ToString() }).ToList();


            var checkRes = CheckOrderblankEntity(orderblankEntity);
            if (!checkRes.Item1)
            {
                invalidList.Add(new Product_Model { ProductBarcode = string.Empty });
            }
            else //批量校验
            {
                var orderblankItemsFromDb = orderblankEntity.OrderblankItems.ToList();
                foreach (var modelToCheck in modelList)
                {
                    var res = CheckBarcode(modelToCheck, validList, invalidList, residueList, missingList, orderblankItemsFromDb, inventoryListFromDb);
                    if (!res.Item1)
                    {
                        modelToCheck.Notes = res.Item2;
                    }
                }
            }

            checkInfo.CheckQuantity = Math.Max(0, checkCount - dat.Length);
            checkInfo.CheckedQuantity = checkedCount + dat.Length;
            checkInfo.ValidQuantity = validList.Count;
            checkInfo.InvalidQuantity = invalidList.Count;
            checkInfo.MissingQuantity = missingList.Count;
            checkInfo.ResidueQuantity = residueList.Count;

            SessionAccess.Set(CHECK_KEY_CHECKEDINFO + guid, checkInfo);
            SessionAccess.Set(CHECK_KEY_VALIDCOUNT + guid, validList);
            SessionAccess.Set(CHECK_KEY_INVALIDCOUNT + guid, invalidList);
            SessionAccess.Set(CHECK_KEY_RESIDUECOUNT + guid, residueList);

            result.Data = checkInfo;
            result.ResultType = OperationResultType.Success;
            return Json(result, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 盘点,有效/无效/缺货/余货列表页面
        /// </summary>
        public ActionResult CheckerDetail(CheckerItemFlag CheckerItemType, string guid)
        {
            ViewBag.guid = guid;
            ViewBag.CheckerItemType = CheckerItemType;
            return PartialView();
        }


        /// <summary>
        /// 获取详细盘点数据列表
        /// </summary>
        public async Task<ActionResult> CheckerDetailList(CheckerItemFlag checkerItemType, string guid)
        {
            GridRequest request = new GridRequest(Request);
            var validList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_VALIDCOUNT + guid) ?? new List<Product_Model>();
            var invalidList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_INVALIDCOUNT + guid) ?? new List<Product_Model>();
            var residueList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_RESIDUECOUNT + guid) ?? new List<Product_Model>();
            var missingList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_MISSINGCOUNT + guid) ?? new List<Product_Model>();

            var data = await Task.Run(() =>
            {
                int index = 0;
                var count = 0;
                switch (checkerItemType)
                {
                    case CheckerItemFlag.Valid:
                        {
                            count = validList.Count;
                            int pageIndex = request.PageCondition.PageIndex;
                            int pageSize = request.PageCondition.PageSize;

                            var list = validList.OrderBy(x => x.ProductBarcode).Skip(pageIndex).Take(pageSize).Select(x => new
                            {
                                Id = index + 1,
                                ProductBarcode = x.ProductBarcode,
                                Notes = x.Notes
                            }).ToList();
                            return new GridData<object>(list, count, request.RequestInfo);
                        }

                    case CheckerItemFlag.Invalid:
                        {
                            count = invalidList.Count;
                            int pageIndex = request.PageCondition.PageIndex;
                            int pageSize = request.PageCondition.PageSize;

                            var list = invalidList.OrderBy(x => x.ProductBarcode).Skip(pageIndex).Take(pageSize).Select(x => new
                            {
                                Id = index + 1,
                                ProductBarcode = x.ProductBarcode,
                                Notes = x.Notes
                            }).ToList();
                            return new GridData<object>(list, count, request.RequestInfo);
                        }

                    case CheckerItemFlag.Lack:
                        {
                            count = missingList.Count;
                            int pageIndex = request.PageCondition.PageIndex;
                            int pageSize = request.PageCondition.PageSize;

                            var list = missingList.OrderBy(x => x.ProductBarcode).Skip(pageIndex).Take(pageSize).Select(x => new
                            {
                                Id = index + 1,
                                ProductBarcode = x.ProductBarcode,
                                Notes = x.Notes
                            }).ToList();
                            return new GridData<object>(list, count, request.RequestInfo);
                        }
                    case CheckerItemFlag.Surplus:
                        {
                            count = residueList.Count;
                            int pageIndex = request.PageCondition.PageIndex;
                            int pageSize = request.PageCondition.PageSize;

                            var list = residueList.OrderBy(x => x.ProductBarcode).Skip(pageIndex).Take(pageSize).Select(x => new
                            {
                                Id = index + 1,
                                ProductBarcode = x.ProductBarcode,
                                Notes = x.Notes
                            }).ToList();
                            return new GridData<object>(list, count, request.RequestInfo);
                        }
                    default:
                        return new GridData<object>(new List<object>(), count, request.RequestInfo);
                }
            });
            return Json(data, JsonRequestBehavior.AllowGet);

        }


        /// <summary>
        /// 盘点过的有效list
        /// </summary>
        /// <returns></returns>
        public ActionResult ViewValidInfo(string guid)
        {
            var request = new GridRequest(Request);
            var validList = (List<Product_Model>)SessionAccess.Get(CHECK_KEY_VALIDCOUNT + guid) ?? new List<Product_Model>();
            var count = 0;
            int pageIndex = request.PageCondition.PageIndex;
            int pageSize = request.PageCondition.PageSize;
            var list = validList.OrderBy(o => o.ProductBarcode).Skip(pageIndex).Take(pageSize).Select(x => new
            {
                Id = x.Id,
                Thumbnail = x.Thumbnail,
                ProductName = x.ProductName,
                ProductNumber = x.ProductNumber,
                ProductBarcode = x.ProductBarcode,
                Season = x.Season,
                Color = x.Color,
                Brand = x.Brand,
                Size = x.Size,
                Category = x.Category,
                WholesalePrice = x.WholesalePrice,
                Amount = 1
            });
            var data = new GridData<object>(list, count, request.RequestInfo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 盘点时对流水号校验
        /// </summary>
        private Tuple<bool, string> CheckBarcode(Product_Model model, List<Product_Model> validList,
                                                List<Product_Model> invalidlist,
                                                List<Product_Model> residueList,
                                                List<Product_Model> missingList,
                                                List<OrderblankItem> orderblankItemsFromDb,
                                                List<Inventory> inventoryListFromOutStorage)
        {
            var result = new OperationResult(OperationResultType.Error, "");
            var number = model.ProductBarcode;
            // 长度校验
            if (number.Length != 14)
            {
                invalidlist.Add(model);
                return Tuple.Create(false, "扫入的商品条码不符合要求");
            }

            // 排重
            if (validList.Any(c => c.ProductBarcode == number))
            {
                invalidlist.Add(model);

                return Tuple.Create(false, "商品条码已经扫入：" + number);
            }

            if (invalidlist.Any(c => c.ProductBarcode == number))
            {
                invalidlist.Add(model);

                return Tuple.Create(false, "商品条码无效，并且已重复出现：" + number);
            }

            #region 校验库存信息
            var inventoryEntity = inventoryListFromOutStorage.FirstOrDefault(c => c.ProductBarcode == number);
            if (inventoryEntity == null)
            {
                if (_inventoryContract.Inventorys.Any(i => i.ProductBarcode == number))
                {
                    residueList.Add(model);
                    return Tuple.Create(false, "商品条码：" + number + ",不属于当前的配货单");
                }
                else
                {
                    invalidlist.Add(model);
                    return Tuple.Create(false, "在库存中未查找到该商品条码：" + number + ",可能是当前用户对该库存不具有操作权限");
                }
            }

            if (inventoryEntity.IsDeleted)
            {
                invalidlist.Add(model);

                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经被移至回收站");
            }
            if (!inventoryEntity.IsEnabled)
            {
                invalidlist.Add(model);

                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存处于禁用状态");
            }

            if (inventoryEntity.Status >= InventoryStatus.PurchasStart &&
                inventoryEntity.Status <= InventoryStatus.PurchasEnd)
            {
                invalidlist.Add(model);

                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经进入采购状态");
            }

            if (inventoryEntity.Status >= InventoryStatus.SaleStart &&
              inventoryEntity.Status <= InventoryStatus.SaleEnd)
            {
                invalidlist.Add(model);

                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存已经进入销售状态");
            }


            #endregion



            if (inventoryEntity.Status == InventoryStatus.UnAccept)
            {

                // 装载数据
                model.Notes = "有效";
                model.Id = inventoryEntity.Id;
                model.ProductId = inventoryEntity.ProductId;
                model.IsValided = true;
                model.ProductNumber = inventoryEntity.Product.ProductNumber;
                model.ProductName = inventoryEntity.Product.ProductOriginNumber.ProductName;
                model.Thumbnail = inventoryEntity.Product.ThumbnailPath;
                model.Brand = inventoryEntity.Product.ProductOriginNumber.Brand.BrandName;
                model.Size = inventoryEntity.Product.Size.SizeName;
                model.Color = inventoryEntity.Product.Color.ColorName;
                model.Season = inventoryEntity.Product.ProductOriginNumber.Season.SeasonName;
                model.Category = inventoryEntity.Product.ProductOriginNumber.Category.CategoryName;
                model.WholesalePrice = inventoryEntity.Product.ProductOriginNumber.WholesalePrice;
                validList.Add(model);
                missingList.RemoveAll(m => m.ProductBarcode == model.ProductBarcode);
                return Tuple.Create(true, string.Empty);
            }
            else
            {
                invalidlist.Add(model);
                return Tuple.Create(false, "在库存中存在该商品条码：" + number + ",但该库存由于其他原因被锁定");
            }

        }
        public ActionResult GetIncompleteOrderbalnkCount()
        {
            var optId = AuthorityHelper.OperatorId;
            if (!optId.HasValue)
            {
                return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, 0));
            }

            var storageIds = _storeContract.QueryManageStorageId(optId.Value);
            if (!storageIds.Any())
            {
                return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, 0));
            }
            var res = _orderblankContract.GetInCompleteOrderblankCount(storageIds.ToArray());
            return Json(res);

        }





        /// <summary>
        /// 导出库存数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpGet]
        public ActionResult ExportOrderblankItem(string orderblankNumber, string barcodes)
        {

            try
            {
                var filterBarcodes = new List<string>();
                if (string.IsNullOrEmpty(orderblankNumber))
                {
                    return Json(OperationResult.Error("参数错误"), JsonRequestBehavior.AllowGet);
                }


                // 导出指定的流水号
                if (!string.IsNullOrEmpty(barcodes))
                {

                    var tempArr = barcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    filterBarcodes.AddRange(tempArr);
                }


                var tempFilePath = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
                var query = _orderblankItemContract.OrderblankItems.Where(i => i.OrderblankNumber == orderblankNumber);

                var originList = query.Select(c => new
                {
                    c.Id,
                    c.OrderblankNumber,
                    c.Orderblank.OutStore.StoreName,
                    c.Orderblank.OutStorage.StorageName,
                    c.OrderBlankBarcodes,
                    c.Product.ProductOriginNumber.Brand.BrandName,
                    c.Product.ProductOriginNumber.Season.SeasonName,
                    c.Product.Size.SizeName,
                    c.Product.Color.ColorName,
                    c.Product.ProductOriginNumber.TagPrice,
                    c.Product.ProductOriginNumber.WholesalePrice,
                    c.CreatedTime,
                    c.Orderblank.Operator.Member.MemberName
                }).ToList();
                var list = new List<object>();

                foreach (var item in originList)
                {
                    var tempbarcodes = item.OrderBlankBarcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    //筛选barcode
                    if (filterBarcodes.Count > 0)
                    {
                        list.AddRange(tempbarcodes.Where(code => filterBarcodes.Contains(code)).Select(code => new ExportData
                        {
                            OrderblankNumber = item.OrderblankNumber,
                            Id = item.Id.ToString(),
                            StoreName = item.StoreName,
                            StorageName = item.StorageName,
                            ProductBarcode = code,
                            BrandName = item.BrandName,
                            SizeName = item.SizeName,
                            ColorName = item.ColorName,
                            SeasonName = item.SeasonName,
                            TagPrice = item.TagPrice.ToString(),
                            WholesalePrice = item.WholesalePrice.ToString(),
                            Quantity = "1",
                            CreatedTime = item.CreatedTime.ToString(),
                            AdminName = item.MemberName
                        }));
                    }
                    else
                    {
                        list.AddRange(tempbarcodes.Select(code => new ExportData
                        {
                            OrderblankNumber = item.OrderblankNumber,
                            Id = item.Id.ToString(),
                            StoreName = item.StoreName,
                            StorageName = item.StorageName,
                            ProductBarcode = code,
                            BrandName = item.BrandName,
                            SizeName = item.SizeName,
                            ColorName = item.ColorName,
                            SeasonName = item.SeasonName,
                            TagPrice = item.TagPrice.ToString(),
                            WholesalePrice = item.WholesalePrice.ToString(),
                            Quantity = "1",
                            CreatedTime = item.CreatedTime.ToString(),
                            AdminName = item.MemberName
                        }));
                    }

                }
                var group = new StringTemplateGroup("all", tempFilePath, typeof(TemplateLexer));
                var st = group.GetInstanceOf("ExporterOrderblankItem");

                st.SetAttribute("list", list);
                var str = st.ToString();
                var buffer = Encoding.UTF8.GetBytes(str);
                var stream = new MemoryStream(buffer);
                return File(stream, "application/ms-excel", "配货记录明细.xls");
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, e.Message), JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Export()
        {
            var gridRequest = new GridRequest(Request);
            Expression<Func<Orderblank, bool>> predicate = FilterHelper.GetExpression<Orderblank>(gridRequest.FilterGroup);
            var orderblankQuery = _orderblankContract.Orderblanks.Where(c => c.IsEnabled).Where(predicate);

            var list = (from s in orderblankQuery
                        let items = s.OrderblankItems.Where(w => w.IsEnabled && !w.IsDeleted).Select(ss => ss.OrderBlankBarcodes)
                        select new
                        {
                            s.OrderBlankNumber,
                            s.Purchase.PurchaseNumber,
                            OutStoreName = s.OutStore.StoreName,
                            OutStorageName = s.OutStorage.StorageName,
                            ReceiverStoreName = s.ReceiverStore.StoreName,
                            ReceiverStorageName = s.ReceiverStorage.StorageName,
                            Status = s.Status + "",
                            OrderblankType = s.OrderblankType + "",
                            s.CreatedTime,
                            OrderblankItems = items
                        }).ToList().Select(s => new
                        {
                            s.OrderBlankNumber,
                            s.PurchaseNumber,
                            s.OutStorageName,
                            s.OutStoreName,
                            s.ReceiverStorageName,
                            s.ReceiverStoreName,
                            s.Status,
                            s.OrderblankType,
                            s.CreatedTime,
                            Quantity = s.OrderblankItems.Any() ? s.OrderblankItems.Aggregate((sum, cur) => { return $"{sum},{cur}"; }).Split(",", true).Count() : 0,
                        }).ToList();

            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "配货管理");
        }

        private class ExportData
        {
            public string OrderblankNumber { get; set; }
            public string Id { get; set; }
            public string StoreName { get; set; }
            public string StorageName { get; set; }
            public string ProductBarcode { get; set; }
            public string BrandName { get; set; }
            public string SizeName { get; set; }
            public string ColorName { get; set; }
            public string SeasonName { get; set; }
            public string TagPrice { get; set; }
            public string WholesalePrice { get; set; }
            public string Quantity { get; set; }
            public string CreatedTime { get; set; }
            public string AdminName { get; set; }
        }
    }
}