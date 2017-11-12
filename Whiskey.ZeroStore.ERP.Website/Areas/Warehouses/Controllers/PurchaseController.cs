




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
using System.Web.Caching;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Website.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Content;
using System.Web.Script.Serialization;
using XKMath36;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Stores;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{

    [License(CheckMode.Verify)]
    [CheckStoreIsClosed]
    public class PurchaseController : BaseController
    {
        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(PurchaseController));

        protected readonly IPurchaseContract _purchaseContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IPurchaseItemContract _purchaseItemContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IProductContract _productContract;
        protected readonly IOrderblankContract _orderblankContract;
        protected readonly IPurchaseAuditContract _purchaseAuditContract;
        protected readonly IStoreTypeContract _storeTypeContract;

        public PurchaseController(IPurchaseContract purchaseContract, IPurchaseItemContract purchaseItemContract,
            IStoreTypeContract _storeTypeContract,
            IStorageContract storageContract, IStoreContract storeContract, IAdministratorContract administratorContract,
            IInventoryContract inventoryContract, IProductContract productContract, IOrderblankContract orderblankContract,
            IPurchaseAuditContract purchaseAuditContract)
        {
            _purchaseContract = purchaseContract;
            _purchaseItemContract = purchaseItemContract;
            _storageContract = storageContract;
            _storeContract = storeContract;
            _administratorContract = administratorContract;
            _inventoryContract = inventoryContract;
            _productContract = productContract;
            _orderblankContract = orderblankContract;
            _purchaseAuditContract = purchaseAuditContract;
            this._storeTypeContract = _storeTypeContract;
        }
        #endregion

        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            var listoutStorages = new List<SelectListItem>();
            var listinStores = new List<SelectListItem>();
            var listinStorages = new List<SelectListItem>();
            var data = _administratorContract.GetDesignerStoreStorageList(AuthorityHelper.OperatorId.Value);
            if (data.Item1)
            {
                listoutStorages = data.Item3;
                var daStorage = _storageContract.Storages.Where(w => w.IsForAddInventory && w.IsEnabled && !w.IsDeleted).FirstOrDefault();
                if (daStorage.IsNotNull())
                {
                    listinStores.Add(new SelectListItem() { Text = daStorage.Store.StoreName, Value = daStorage.StoreId + "" });
                    listinStorages.Add(new SelectListItem() { Text = daStorage.StorageName, Value = daStorage.Id + "" });
                }
            }
            else
            {
                listoutStorages = _storageContract.SelectList("请选择仓库", c => c.IsDeleted == false && c.IsEnabled == true).Select(c => new SelectListItem()
                {
                    Text = c.Value,
                    Value = c.Key
                }).ToList();
            }
            ViewBag.IsDesigner = data.Item1;
            ViewBag.outStorages = listoutStorages;

            ViewBag.inStores = listinStores;
            ViewBag.inStorages = listinStorages;

            return View("PurchaseOrder");
        }

        /// <summary>
        /// 创建采购单
        /// </summary>
        public ActionResult PurchaseOrder()
        {
            var listoutStorages = new List<SelectListItem>();
            var listinStores = new List<SelectListItem>();
            var listinStorages = new List<SelectListItem>();
            var data = _administratorContract.GetDesignerStoreStorageList(AuthorityHelper.OperatorId.Value);
            if (data.Item1)
            {
                listoutStorages = data.Item3;
                var daStorage = _storageContract.Storages.Where(w => w.IsForAddInventory && w.IsEnabled && !w.IsDeleted).FirstOrDefault();
                if (daStorage.IsNotNull())
                {
                    listinStores.Add(new SelectListItem() { Text = daStorage.Store.StoreName, Value = daStorage.StoreId + "" });
                    listinStorages.Add(new SelectListItem() { Text = daStorage.StorageName, Value = daStorage.Id + "" });
                }
            }
            else
            {
                listoutStorages = _storageContract.SelectList("请选择仓库", c => c.IsDeleted == false && c.IsEnabled == true).Select(c => new SelectListItem()
                {
                    Text = c.Value,
                    Value = c.Key
                }).ToList();
            }
            ViewBag.IsDesigner = data.Item1;
            ViewBag.outStorages = listoutStorages;
            
            ViewBag.inStores = listinStores;
            ViewBag.inStorages = listinStorages;

            return PartialView();
        }
        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
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
        public ActionResult Create(PurchaseDto dto)
        {
            var result = _purchaseContract.Insert(dto);
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
        public ActionResult Update(PurchaseDto dto)
        {
            var result = _purchaseContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult UpdateItem(int Id)
        {

            //ViewBag.ProductName = _productContract.Products.Where(c => c.Id == Id).Select(c => c.ProductName).FirstOrDefault();
            var result = _purchaseItemContract.PurchaseItems.Where(c => c.Id == Id).FirstOrDefault();
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult PurView(int Id)
        {
            //string num = InputHelper.SafeInput(Id);
            //if (!string.IsNullOrEmpty(num))
            //{
            var resul = _purchaseContract.Purchases.Where(c => c.Id == Id && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
            if (resul != null)
            {
                var storag = _storageContract.Storages.Where(c => c.Id == resul.StorageId).FirstOrDefault();
                if (storag != null)
                {
                    TempData["storageOut"] = storag.StorageName;
                }
                var receStore = _storeContract.Stores.Where(c => c.Id == resul.ReceiverId).FirstOrDefault();
                if (receStore != null)
                    TempData["ReceiverId"] = receStore.StoreName;
                var receStorage = _storageContract.Storages.Where(c => c.Id == resul.ReceiverStorageId).FirstOrDefault();
                if (receStorage != null)
                    TempData["ReceiverStorageId"] = receStorage.StorageName;

            }
            return PartialView(resul);
            //}
            //else {
            //    return Json(new OperationResult(OperationResultType.Error));
            //}
        }

        public ActionResult PurItemView(int Id)
        {
            var resul = _purchaseItemContract.PurchaseItems.Where(c => c.Id == Id && c.IsEnabled == true && c.IsDeleted == false).FirstOrDefault();
            //TempData["img"] = _purchaseItemContract.PurchaseItems.Where(c => c.Id == Id && c.IsEnabled == true && c.IsDeleted == false).Select(c => c.Product.ThumbnailPath);
            return PartialView(resul);
        }

        //yxk 2015-9-
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Purchase, bool>> predicate = FilterHelper.GetExpression<Purchase>(request.FilterGroup);
            var availableStorageIds = CacheAccess.GetManagedStorage(_storageContract, _administratorContract).Select(i => i.Id).ToList();
            var data = await Task.Run(() =>
            {
                var count = 0;

                var list = (from m in _purchaseContract.Purchases.Where(c => !c.IsDeleted && c.IsEnabled)
                            .Where(c => availableStorageIds.Contains(c.ReceiverStorageId.Value) || availableStorageIds.Contains(c.StorageId.Value))
                            .Where<Purchase, int>(predicate, request.PageCondition, out count)
                            select new
                            {
                                ParentId = "",
                                m.PurchaseNumber,
                                FactoryName = m.StoreCartId.HasValue && m.OriginFlag == StoreCardOriginFlag.工厂 ? m.StoreCart.Factory.FactoryName:"",
                                StorageOut = _storageContract.Storages.Where(c => c.Id == m.StorageId).FirstOrDefault().StorageName,
                                // m.StorageId,
                                ReceiverStore = _storeContract.Stores.Where(c => c.Id == m.ReceiverId).FirstOrDefault().StoreName,
                                ReceiverStoreId = _storeContract.Stores.Where(c => c.Id == m.ReceiverId).FirstOrDefault().Id,
                                // m.ReceiverId,
                                ReceiverStorage = _storageContract.Storages.Where(c => c.Id == m.ReceiverStorageId).FirstOrDefault().StorageName,
                                m.PurchaseStatus,
                                m.Notes,
                                ProductNum = "",
                                m.Id,
                                m.IsDeleted,
                                m.IsEnabled,
                                m.Sequence,
                                m.UpdatedTime,
                                m.CreatedTime,
                                m.Operator.Member.MemberName,
                                Quantity = -1,
                                OrderBlankNumber = m.Orderblanks.Where(x => x.IsEnabled && !x.IsDeleted).OrderByDescending(s => s.CreatedTime).FirstOrDefault().OrderBlankNumber ?? "",
                                StyleCount = m.PurchaseItems.Where(w => !w.IsNewAdded).Select(s => s.Product.BigProdNum).Distinct().Count(),
                                StyleCountReal = m.PurchaseItems.Select(s => s.Product.BigProdNum).Distinct().Count(),
                                StyleCountRealed = m.PurchaseItems.Where(w => w.PurchaseItemProducts.Any()).Select(s => s.Product.BigProdNum).Distinct().Count(),
                                PieceCount = m.PurchaseItems.Where(w => !w.IsNewAdded).Sum(s => s.Quantity),
                                PieceCountReal = m.PurchaseItems.Sum(s => s.Quantity),
                                PieceCountRealed = m.PurchaseItems.SelectMany(s => s.PurchaseItemProducts).Count(),
                                //OriginFlag = m.StoreCart != null ? (m.StoreCart.OriginFlag + "") : "未知"
                                OriginFlag = m.OriginFlag + "",
                            }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        //yxk 2015-10-8
        /// <summary>
        /// 采购单审核不通过
        /// </summary>
        /// <returns></returns>
        public ActionResult AuditNo()
        {
            var id = Request["Id"];
            if (id != null && id != "")
                ViewBag.purchaseNum = Request["Id"].ToString();
            return PartialView();
        }
        //审核不通过
        [HttpPost]
        public JsonResult AuditNoa()
        {
            OperationResult res = new OperationResult(OperationResultType.Error, "未找到该盘点记录或者不通过理由为空");
            var _num = Request["id"];
            var mes = Request["mes"];
            _num = InputHelper.SafeInput(_num);
            mes = InputHelper.SafeInput(mes);
            if (_num != null && _num != "" && mes != null && mes != "")
            {
                var purenti = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == _num).FirstOrDefault();
                if (purenti != null)
                {
                    purenti.PurchaseStatus = 2;
                    purenti.AuditMessage = mes;
                    PurchaseDto dto = AutoMapper.Mapper.Map<PurchaseDto>(purenti);
                    res = _purchaseContract.Update(dto);

                    PurchaseAudit puraudit = new PurchaseAudit()
                    {
                        IsEnabled = true,
                        IsDeleted = false,
                        Status = 2,
                        PurchaseId = purenti.Id,
                        UpdatedTime = DateTime.Now,
                        CreatedTime = DateTime.Now,
                        OperatorId = AuthorityHelper.OperatorId,
                        Note = mes,
                    };
                    _purchaseAuditContract.Insert(puraudit);
                }
            }
            return Json(res);


        }
        //yxk 2015-11
        /// <summary>
        /// 撤消不通过审核
        /// </summary>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult AuditNoReset(string num)
        {
            OperationResult res = new OperationResult(OperationResultType.Error);
            string _num = InputHelper.SafeInput(num);
            if (!string.IsNullOrEmpty(_num))
            {
                var pur = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == num && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
                if (pur != null)
                {
                    pur.PurchaseStatus = 0;
                    pur.UpdatedTime = DateTime.Now;
                    res = _purchaseContract.Update(pur);
                }
                //采购单审核记录
                PurchaseAudit purchaseAudit = new PurchaseAudit()
                {
                    Status = 5,
                    PurchaseId = pur.Id,
                    IsDeleted = false,
                    IsEnabled = true,
                    OperatorId = AuthorityHelper.OperatorId,
                    CreatedTime = DateTime.Now,
                    UpdatedTime = DateTime.Now,
                    Note = "撤消不通过审核记录",
                };
                _purchaseAuditContract.Insert(purchaseAudit);
            }
            return Json(res);
        }
        //yxk 2015-11-
        /// <summary>
        /// 废除采购单
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public JsonResult KillPur(string num)
        {
            OperationResult resu = new OperationResult(OperationResultType.Error);
            string _num = InputHelper.SafeInput(num);
            if (!string.IsNullOrEmpty(_num))
            {
                var pur = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == _num && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
                if (pur != null)
                {
                    pur.PurchaseStatus = 3;
                    pur.UpdatedTime = DateTime.Now;
                    resu = _purchaseContract.Update(pur);
                    //取消库存锁定
                    List<Inventory> li = new List<Inventory>();
                    foreach (var item in pur.PurchaseItems)
                    {
                        var invet = _inventoryContract.Inventorys.Where(c => c.ProductId == item.ProductId).FirstOrDefault();
                        if (invet != null)
                        {
                            //invet.LockCoun -= item.Quantity;
                            //if (invet.LockCoun <= 0)
                            //{
                            //    invet.LockCoun = 0;
                            //    invet.IsLock = false;
                            //    invet.UpdatedTime = DateTime.Now;
                            //}
                            li.Add(invet);
                        }
                    }
                    List<InventoryDto> dtoli = AutoMapper.Mapper.Map<List<InventoryDto>>(li);
                    resu = _inventoryContract.Update(dtoli.ToArray());

                    //采购单审核记录
                    PurchaseAudit purcaudit = new PurchaseAudit()
                    {
                        IsDeleted = false,
                        IsEnabled = true,
                        Status = 3,
                        Note = "采购单废除",
                        UpdatedTime = DateTime.Now,
                        CreatedTime = DateTime.Now,
                        OperatorId = AuthorityHelper.OperatorId,
                        PurchaseId = pur.Id,
                    };
                    _purchaseAuditContract.Insert(purcaudit);
                }
            }
            return Json(resu);

        }
        //yxk 2015-10
        /// <summary>
        /// 采购单审核成功
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AuditOk()
        {
            OperationResult res = new OperationResult(OperationResultType.Error, "操作失败");
            var purnum = Request["id"];
            int purId;
            if (purnum != null && purnum != "")
            {

                var purenti = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == purnum).FirstOrDefault();
                if (purenti != null)
                {
                    purenti.PurchaseStatus = 1;
                    purenti.AuditMessage = "";
                    purenti.UpdatedTime = DateTime.Now;
                    string OrderBlankNum = GetOnlyNumb().ToUpper();
                    purenti.Orderblanks = new List<Orderblank>()
                    {
                      new Orderblank()
                      {
                          OutStorageId=purenti.StorageId.Value,
                          UpdatedTime=DateTime.Now,
                          Status=0,
                          CreatedTime=DateTime.Now,
                          IsDeleted=false,
                          IsEnabled=true,
                          OperatorId=AuthorityHelper.OperatorId,
                          OrderBlankNumber=OrderBlankNum,
                          PurchaseId=purenti.Id,
                          ReceiverStoreId=purenti.ReceiverId.Value,
                          ReceiverStorageId=purenti.ReceiverStorageId??0,
                          OrderblankItems=GetOrderItems(purenti.PurchaseItems,OrderBlankNum)

                      }
                    };
                    //采购单审核记录
                    PurchaseAudit puraudit = new PurchaseAudit()
                    {
                        IsDeleted = false,
                        IsEnabled = true,
                        CreatedTime = DateTime.Now,
                        UpdatedTime = DateTime.Now,
                        PurchaseId = purenti.Id,
                        Status = 1,
                        OperatorId = AuthorityHelper.OperatorId,

                    };
                    res = _purchaseContract.Update(purenti);
                    if (res.ResultType == OperationResultType.Success)
                    {
                        _purchaseAuditContract.Insert(puraudit);//插入审核记录
                        Orderblank ord = purenti.Orderblanks.Where(c => c.IsEnabled == true).OrderByDescending(c => c.CreatedTime).FirstOrDefault();

                        res = new OperationResult(OperationResultType.Success) { Data = ord.OrderBlankNumber.ToUpper() };
                    }
                }
            }
            return Json(res);
        }
        /// <summary>
        /// 获取配货明细
        /// </summary>
        /// <param name="puritemColl"></param>
        /// <returns></returns>
        private List<OrderblankItem> GetOrderItems(ICollection<PurchaseItem> puritemColl, string OrderBlankNum)
        {
            return puritemColl.Select(c => new OrderblankItem()
            {
                CreatedTime = DateTime.Now,
                IsDeleted = false,
                IsEnabled = true,
                OperatorId = AuthorityHelper.OperatorId,
                ProductId = c.ProductId ?? 0,
                OrderblankNumber = OrderBlankNum,
                //PurchasePrice = c.PurchasePrice,
                Quantity = c.Quantity,
                //TagPrice = c.TagPrice,
                //WholesalePrice = c.WholesalePrice,
                //OrderBlankBarcodes = c.Barcodes
            }).ToList();


        }
        //yxk 2015-11
        /// <summary>
        /// 撤消审核通过
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Log]
        //public ActionResult RecallAudit(string num)
        //{
        //    num = InputHelper.SafeInput(num);
        //    OperationResult resul = new OperationResult(OperationResultType.Error);
        //    if (!string.IsNullOrEmpty(num))
        //    {
        //        var pur = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == num && c.IsEnabled == true).FirstOrDefault();
        //        if (pur != null)
        //        {
        //            //配货单未配货或者拒绝配货的情况下才能允许撤消
        //            var order = pur.Orderblanks.Where(c => c.IsDeleted == false).OrderByDescending(c => c.CreatedTime).FirstOrDefault();
        //            if (order != null && order.Status ==  OrderblankStatus.配货中 || order.Status ==  OrderblankStatus.NoDelivery)
        //            {

        //                pur.PurchaseStatus = 0;
        //                pur.UpdatedTime = DateTime.Now;
        //                pur.Notes = "将该条审核通过记录撤消";

        //                var purdto = AutoMapper.Mapper.Map<PurchaseDto>(pur);

        //                //废除配货单
        //                order.IsEnabled = false;
        //                order.Status =  OrderblankStatus.已撤销;
        //                order.Notes = "配货单废除";
        //                order.UpdatedTime = DateTime.Now;
        //                resul = _purchaseContract.Update(pur);
        //            }
        //            else if (order == null)
        //            {
        //                resul = new OperationResult(OperationResultType.Error, "未找到配货记录，撤消失败");
        //            }
        //            else
        //            {
        //                resul = new OperationResult(OperationResultType.Error, "已经开始配货，撤消失败");
        //            }
        //            //采购单审核记录
        //            PurchaseAudit puraudit = new PurchaseAudit()
        //            {
        //                IsDeleted = false,
        //                IsEnabled = true,
        //                Note = "撤消审核通过",
        //                CreatedTime = DateTime.Now,
        //                UpdatedTime = DateTime.Now,
        //                OperatorId = AuthorityHelper.OperatorId,
        //                PurchaseId = pur.Id,
        //                Status = 4,
        //            };
        //            _purchaseAuditContract.Insert(puraudit);
        //        }
        //    }
        //    else
        //    {
        //        resul = new OperationResult(OperationResultType.Error, "未查找到该采购记录");
        //    }
        //    return Json(resul);

        //}
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _purchaseContract.Remove(Id);
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
            var result = _purchaseContract.Delete(Id);
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
            var result = _purchaseContract.Recovery(Id);
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
            var result = _purchaseContract.Enable(Id);
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
            var result = _purchaseContract.Disable(Id);
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
            var list = _purchaseContract.Purchases.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _purchaseContract.Purchases.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出采购单数据
        /// </summary>
        [Log]
        [HttpGet]
        public ActionResult ExportPurchaseOrder(string purchaseNumber)
        {
            if (string.IsNullOrEmpty(purchaseNumber))
            {
                return Json(OperationResult.Error("参数错误"));
            }

            Purchase purchaseEntity = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == purchaseNumber && !c.IsDeleted && c.IsEnabled).FirstOrDefault();
            if (purchaseEntity == null)
            {
                return Json(OperationResult.Error("为找到采购单信息"));
            }
            var li = purchaseEntity.PurchaseItems.Select(c => new Product_Model()
            {
                ProductNumber = c.Product.ProductNumber,
                ProductName = c.Product.ProductName ?? c.Product.ProductOriginNumber.ProductName,
                Brand = c.Product.ProductOriginNumber.Brand.BrandName,
                Size = c.Product.Size.SizeName,
                Season = c.Product.ProductOriginNumber.Season.SeasonName,
                Color = c.Product.Color.ColorName,
                TagPrice = c.Product.ProductOriginNumber.TagPrice,
                PurchaseQuantity = c.Quantity
            }).ToList();
            var tempFilePath = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var group = new StringTemplateGroup("all", tempFilePath, typeof(TemplateLexer));
            var st = group.GetInstanceOf("ExportPurchaseOrder");

            st.SetAttribute("list", li);
            var str = st.ToString();
            var buffer = Encoding.UTF8.GetBytes(str);
            var stream = new MemoryStream(buffer);
            return File(stream, "application/ms-excel", $"采购单[{purchaseEntity.PurchaseNumber}]详情.xls");
        }


        #region 注释代码


        //public JsonResult UpdateWord(int id, string fieldName)
        //{
        //    OperationResult res = new OperationResult(OperationResultType.Error);
        //    switch (fieldName)
        //    {

        //        case "IsDelivered":  //确认发货
        //            {
        //                //更改采购单信息
        //                var pur = _purchaseContract.Purchases.Where(c => c.Id == id).FirstOrDefault();
        //                pur.IsDelivered = true;
        //                pur.DeliverName = _administratorContract.Administrators.Where(c => c.Id == AuthorityHelper.OperatorId).FirstOrDefault().AdminName;
        //                pur.UpdatedTime = DateTime.Now;
        //                PurchaseDto purdt = Mapper.Map<PurchaseDto>(pur);
        //                res = _purchaseContract.Update(purdt);
        //                //将原库存的数量减少
        //                var purchLis = pur.PurchaseItems;
        //                List<InventoryDto> li = new List<InventoryDto>();
        //                foreach (var e in purchLis)
        //                {
        //                    var t = _inventoryContract.Inventorys.Where(c => c.ProductId == e.ProductId).FirstOrDefault();
        //                    t.Quantity = t.Quantity - e.Quantity;
        //                    InventoryDto dto = Mapper.Map<InventoryDto>(t);
        //                    li.Add(dto);
        //                }
        //                res = _inventoryContract.Update(li.ToArray());

        //                break;
        //            }
        //        case "IsReceived":
        //            {
        //                var pur = _purchaseContract.Purchases.Where(c => c.Id == id && c.IsDelivered == true).FirstOrDefault();
        //                if (pur == null)
        //                {
        //                    res = new OperationResult(OperationResultType.Error, "确认收货前需要由出库方确认出库");
        //                    break;
        //                }
        //                pur.IsReceived = true;
        //                pur.UpdatedTime = DateTime.Now;
        //                pur.ReceiverName = _administratorContract.Administrators.Where(c => c.Id == AuthorityHelper.OperatorId).FirstOrDefault().AdminName;
        //                PurchaseDto purdt = Mapper.Map<PurchaseDto>(pur);
        //                res = _purchaseContract.Update(purdt);
        //                break;

        //            }
        //        case "IsStoraged":
        //            {
        //                //修改采购单信息
        //                var pur = _purchaseContract.Purchases.Where(c => c.Id == id && c.IsReceived == true).FirstOrDefault();
        //                if (pur == null)
        //                {
        //                    res = new OperationResult(OperationResultType.Error, "确认入库前必须要先确认收货");
        //                    break;
        //                }
        //                pur.IsStoraged = true;
        //                pur.StoragerName = _administratorContract.Administrators.Where(c => c.Id == AuthorityHelper.OperatorId).FirstOrDefault().AdminName;
        //                pur.UpdatedTime = DateTime.Now;
        //                PurchaseDto purdt = Mapper.Map<PurchaseDto>(pur);
        //                res = _purchaseContract.Update(purdt);
        //                //修改入库的仓库信息
        //                var purch = _purchaseContract.Purchases.Where(c => c.Id == id).FirstOrDefault();
        //                var purchItemList = pur.PurchaseItems;
        //                List<InventoryDto> li = new List<InventoryDto>();
        //                foreach (var e in purchItemList)
        //                {
        //                    li.Add(new InventoryDto()
        //                    {
        //                        StoreId = (int)purch.ReceiverId,//收货店铺
        //                        StorageId = purch.ReceiverStorageId, //收货仓库
        //                        ProductId = e.ProductId,
        //                        Quantity = e.Quantity,
        //                        TagPrice = e.TagPrice,
        //                        RetailPrice = e.RetailPrice,
        //                        WholesalePrice = e.WholesalePrice,
        //                        PurchasePrice = e.PurchasePrice,
        //                        //LocationCode 库位编码
        //                        Description = purch.Notes

        //                    });
        //                }
        //                res = _inventoryContract.Insert(li.ToArray());
        //                break;
        //            }


        //    }
        //    return Json(res);
        //}

        //yxk 2015-9
        #endregion

        /// <summary>
        /// 返回一个不重复单号
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

            var maxid = CacheAccess.GetOrderblankMaxId(_orderblankContract);
            XKMath36.Math36 math = new Math36();
            var newNum = math.To36(maxid);
            var num = _num.Substring(0, 6) + newNum.PadLeft(4, '0');
            return num.ToUpper();
        }
        /// <summary>
        /// 根据采购单号获取采购明细
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPruListByNum(string productNumber)
        {
            string num = Request["num"];
            string _start = Request["iDisplayStart"] ?? "0";
            int start = Convert.ToInt32(_start);
            string _count = Request["iDisplayLength"] ?? "10";
            int count = Convert.ToInt32(_count);
            GridData<object> da = new GridData<object>(new List<object>(), 0, Request);

            if (!string.IsNullOrEmpty(num))
            {
                Purchase pur = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == num && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
                if (pur != null)
                {
                    var li = pur.PurchaseItems.Where(w => w.IsEnabled && !w.IsDeleted).Select(c => new
                    {
                        Id = c.Id,//+ "|" + c.ProductId,//采购明细ID+商品ID
                        Brand = c.Product.ProductOriginNumber.Brand.BrandName,
                        Amount = GetAmount(c.ProductId ?? 0),
                        Category = c.Product.ProductOriginNumber.Category.CategoryName,
                        Color = c.Product.Color.ColorName,
                        ProductName = c.Product.ProductName ?? c.Product.ProductOriginNumber.ProductName,
                        ProductNumber = c.Product.ProductNumber,
                        Season = c.Product.ProductOriginNumber.Season.SeasonName,
                        Size = c.Product.Size.SizeName,
                        TagPrice = c.Product.ProductOriginNumber.TagPrice,
                        ValidCoun = 1,
                        Thumbnail = c.Product.ThumbnailPath ?? c.Product.ProductOriginNumber.ThumbnailPath,
                        UUID = "",
                        WholesalePrice = c.Product.ProductOriginNumber.WholesalePrice,
                        Other = c.Quantity.ToString(),
                        Quantity = c.Quantity,
                        PurchaseQuantity = c.PurchaseItemProducts.Count(),
                        c.IsNewAdded,
                        OriginFlag = c.Purchase.OriginFlag + "",
                    }).ToList();

                    if (!string.IsNullOrEmpty(productNumber))
                    {
                        li = li.Where(p => p.ProductNumber == productNumber).ToList();
                    }
                    var resli = li.Skip(start).Take(count);
                    da = new GridData<object>(resli, li.Count, Request);
                }
            }
            return Json(da, JsonRequestBehavior.AllowGet);
        }

        private int GetAmount(int proid)
        {

            var te = _inventoryContract.Inventorys.Where(c => c.ProductId == proid && c.IsDeleted == false && c.IsEnabled == true && !c.IsLock).ToList();
            if (te != null)
            {
                return te.Count;
            }
            return 0;
        }
        /// <summary>
        /// 根据店铺id获取仓库
        /// </summary>
        /// <returns></returns>
        public ActionResult GetStorageByStoreId()
        {
            var storeId = Request["stId"];
            if (!string.IsNullOrEmpty(storeId))
            {
                int _sid = Convert.ToInt32(storeId);
                var resul = CacheAccess.GetManagedStorageByStoreId(_storageContract, _administratorContract, _sid, true);
                var obj = resul.Select(c => new { text = c.Text, value = c.Value });
                return Json(obj);
            }
            else
            {
                return Json(new OperationResult(OperationResultType.Error));

            }
        }

        #region 拒绝配货
        /// <summary>
        /// 拒绝配货
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Refuse(int Id)
        {
            PurchaseDto dto = _purchaseContract.Edit(Id);
            dto.PurchaseStatus = (int)PurchaseStatusFlag.RefusePurchase;
            OperationResult oper = _purchaseContract.Update(dto);
            return Json(oper);
        }
        #endregion

        [HttpPost]
        public JsonResult SendPurchase(int Id)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "操作失败");
            PurchaseDto dto = _purchaseContract.Edit(Id);
            if (dto.IsNotNull())
            {
                var result = _orderblankContract.SaveOrderblankFromPurchaseOrder(dto.Id);
                if (result.ResultType == OperationResultType.Success)
                {
                    oper.Message = "";
                    dto.PurchaseStatus = (int)PurchaseStatusFlag.Purchased;
                    oper = _purchaseContract.Update(dto);
                }
                else
                {
                    oper.Message = result.Message;
                }
            }
            return Json(oper);
        }

        public ActionResult Payment(string PurchaseNumber, int Id)
        {
            ViewBag.PurchaseNumber = PurchaseNumber;
            ViewBag.PurchaserId = Id;
            var mod = _purchaseContract.Purchases.Where(c => c.PurchaseNumber == PurchaseNumber && c.IsDeleted == false && c.IsEnabled == true && c.PurchaseStatus == (int)PurchaseStatusFlag.待付款).FirstOrDefault();
            if (mod.IsNotNull())
            {
                ViewBag.ReceiptStoreId = mod.ReceiverId;
                ViewBag.ReceiptStorageId = mod.ReceiverStorageId;

                #region 计算价格

                var listprices = mod.PurchaseItems.Where(w => w.PurchaseItemProducts.Any()).Select(s => new
                {
                    TagPrices = s.Product.ProductOriginNumber.TagPrice * s.PurchaseItemProducts.Count,
                    PurchasePrices = s.Product.ProductOriginNumber.PurchasePrice * s.PurchaseItemProducts.Count,
                    WholesalePrices = s.Product.ProductOriginNumber.WholesalePrice * s.PurchaseItemProducts.Count,
                }).ToList();

                ViewBag.TagPrices = listprices.Sum(s => s.TagPrices);
                ViewBag.PurchasePrices = listprices.Sum(s => s.PurchasePrices);
                ViewBag.WholesalePrices = listprices.Sum(s => s.WholesalePrices);

                #endregion
            }

            ViewBag.StoreTypes = CacheAccess.GetStoreType(_storeTypeContract, true);
            return PartialView();
        }
        /// <summary>
        /// 采购单支付，支付完成后生成配货单
        /// </summary>
        /// <param name="PurchaseNumber">采购单号</param>
        /// <param name="WithoutMoney">true无需支付</param>
        /// <param name="discountNumber">折扣，0-1之间</param>
        /// <param name="ReceiptStoreId">采购店铺Id</param>
        /// <param name="ReceiptStorageId">采购仓库Id</param>
        /// <param name="discountType">折扣方案 0吊牌价、1采购价、2进货价</param>
        /// <returns></returns>int purchaserId, int ReceiverStorageId, int ReceiverStoreId, PaymentPurchaseType payType, float Discount
        [HttpPost]
        public ActionResult Payment(string PurchaseNumber, int purchaserId, float discountNumber, int ReceiptStoreId, int ReceiptStorageId, PaymentPurchaseType discountType)
        {
            var data = _purchaseContract.Payment(purchaserId, ReceiptStorageId, ReceiptStoreId, discountType, discountNumber);
            return Json(data);
        }
    }
}
