using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    /// <summary>
    /// 退货Controller
    /// </summary>
    [CheckStoreIsClosed]
    public class ReturnController : Controller
    {
        protected readonly IRetailContract _retailContract;
        protected readonly IRetailItemContract _retailItemContract;
        protected readonly IProductContract _productContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IReturnedContract _returnedContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly IMemberConsumeContract _memberConsumeContract;
        private readonly IReturnedItemContract _returnedItemContract;
        private readonly ICouponContract _couponContract;
        private readonly IStoreContract _storeContract;
        protected readonly IProductTrackContract _productTrackContract;
        protected readonly ITimeoutSettingContract _timeoutSettingContract;
        private readonly IPunishScoreRecordContract _adminPunishScoreRecordContract;
        private readonly ITimeoutRequestContract _timeoutRequestContract;
        protected readonly ISmsContract _smsContract;

        public ReturnController(IRetailContract retailContract, IProductContract productContract, IRetailItemContract retailItemContract, IAdministratorContract administratorContract, IReturnedContract returnedContract, IInventoryContract inventoryContract, IMemberContract memberContract, IMemberDepositContract memberDepositContract,
            IReturnedItemContract returnedItemContract, ICouponContract couponContract, IStoreContract storeContract,
           IProductTrackContract productTrackContract,
           ITimeoutSettingContract timeoutSettingContract,
           IPunishScoreRecordContract adminPunishScoreRecordContract,
           IMemberConsumeContract memberConsumeContract,
           ISmsContract _smsContract,
           ITimeoutRequestContract timeoutRequestContract)
        {
            _retailContract = retailContract;
            _productContract = productContract;
            _retailItemContract = retailItemContract;
            _administratorContract = administratorContract;
            _returnedContract = returnedContract;
            _inventoryContract = inventoryContract;
            _memberContract = memberContract;
            _memberDepositContract = memberDepositContract;
            _returnedItemContract = returnedItemContract;
            _couponContract = couponContract;
            _storeContract = storeContract;
            _productTrackContract = productTrackContract;
            _timeoutSettingContract = timeoutSettingContract;
            _adminPunishScoreRecordContract = adminPunishScoreRecordContract;
            _memberConsumeContract = memberConsumeContract;
            _timeoutRequestContract = timeoutRequestContract;
            this._smsContract = _smsContract;
        }

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            GridRequest gr = new GridRequest(Request);
            Expression<Func<Retail, bool>> predicate = FilterHelper.GetExpression<Retail>(gr.FilterGroup);
            var retailEntity = _retailContract.Retails.Where(predicate)
                .Include(r => r.ReturnRecordHistory)
                .FirstOrDefault(c => c.IsEnabled && !c.IsDeleted);

            List<object> li = new List<object>();
            if (retailEntity != null)
            {
                //获取retail 信息
                var parent = new
                {
                    Id = retailEntity.Id,
                    ParentId = "",
                    retailEntity.IsDeleted,
                    retailEntity.IsEnabled,
                    retailEntity.RetailNumber,
                    ProductNumber = "",
                    retailEntity.ConsumeCount,
                    retailEntity.ConsumerId,
                    retailEntity.CouponNumber,
                    MemberName = retailEntity.ConsumerId.HasValue ? retailEntity.Consumer.MemberName : string.Empty,
                    StoreActivityId = retailEntity.StoreActivityId ?? 0,  //店铺活动id
                    Operator = retailEntity.Operator.Member.MemberName,
                    ProductCount = (retailEntity.RetailItems.Any()) ? retailEntity.RetailItems.Select(t => t.RetailCount).Sum() : 0,
                    OutStorageTime = retailEntity.OutStorageDatetime,
                    SalesCampaignDiscuss = 10,
                    ProductPic = "",
                    RetailPrice = "",
                    Status = retailEntity.RetailState,
                    StatusText = retailEntity.RetailState.ToString(),
                    StoreName = retailEntity.Store.StoreName,
                };
                li.Add(parent);

                //获取retailItem及inventory信息
                var inventoryList = retailEntity.RetailItems.SelectMany(item => item.RetailInventorys).ToList();

                //获取库存状态的文字描述
                Func<Inventory, string> GetInventoryStatusDesc = inventory =>
                {
                    return retailEntity.ReturnRecordHistory.Count(r => !r.IsDeleted && r.IsEnabled
                                                                 && r.InventoryId == inventory.Id && r.IsReturn) > 0 ? "已退" : "可退";
                };
                var child = inventoryList.Select(t => new
                {
                    StoreName = retailEntity.Store.StoreName,
                    Id = "c" + t.RetailItemId,
                    ParentId = retailEntity.Id,                         // 父项id
                    t.IsDeleted,
                    t.IsEnabled,
                    ConsumeCount = string.Empty,                  // 消费总额,不展示
                    RetailNumber = string.Empty,                  // 订单号,不展示
                    ProductNumber = t.Inventory.ProductBarcode,             // 流水号
                    RetailPrice = t.RetailItem.ProductRetailPrice,// 零售价格
                    t.RetailItem.SalesCampaignDiscount,           // 商品活动折扣
                    ConsumerId = "",                              // 会员名字,不展示
                    CouponNumber = string.Empty,                  // 优惠券号码,不展示
                    StoreActivityId = string.Empty,               // 店铺活动id,不展示
                    Operator = "",                                // 操作人,不展示
                    ProductCount = 1,                             // 购买数量,固定是1
                    OutStorageTime = "",                          // 出库时间,不展示
                    ProductPic = t.Inventory.Product.ThumbnailPath ?? t.Inventory.Product.ProductOriginNumber.ThumbnailPath,         // 商品缩略图
                    Status = string.Empty,                        // 状态,不展示
                    StatusText = GetInventoryStatusDesc(t.Inventory) // 状态描述
                });
                li.AddRange(child);
            }
            GridData<object> gd = new GridData<object>(li, li.Count(), Request);
            return Json(gd);
        }

        /// <summary>
        /// 加载退货视图
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Return(string retailNumber)
        {
            if (string.IsNullOrEmpty(retailNumber))
            {
                return Json(new OperationResult(OperationResultType.Error, "参数错误"));
            }
            var retailEntity = _retailContract.Retails.Where(r => !r.IsDeleted && r.IsEnabled && r.RetailNumber == retailNumber).FirstOrDefault();
            if (retailEntity == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "订单不存在"));
            }

            var isChecking = _storeContract.IsInCheckingStat(retailEntity.StoreId.Value);
            if (isChecking)
            {
                return Json(new OperationResult(OperationResultType.Error, "店铺正在盘点,无法进行退货操作!"));
            }
            //获取订单下可退的商品
            ViewBag.RetailNumber = retailNumber;
            return PartialView();
        }




        public ActionResult CanReturn(string retailNumber)
        {
            var res = _returnedContract.CanReturn(retailNumber);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取零售订单下库存
        /// </summary>
        /// <param name="retailNumber"></param>
        /// <returns></returns>
        public ActionResult GetRetailInventory(string retailNumber)
        {
            if (string.IsNullOrEmpty(retailNumber))
            {
                return Json(new OperationResult(OperationResultType.Error, "参数错误"));
            }
            var retailEntity = _retailContract.Retails.Where(r => !r.IsDeleted && r.IsEnabled && r.RetailNumber == retailNumber)
                .Include(r => r.RetailItems)
                .Include(r => r.ReturnRecordHistory)
                .FirstOrDefault();
            if (retailEntity == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "订单不存在"));
            }
            var levelDiscount = 1.0M;
            if (retailEntity.LevelDiscount.HasValue && retailEntity.LevelDiscountAmount > 0)
            {
                levelDiscount = retailEntity.LevelDiscount.Value;
            }

            Func<Inventory, int> getInventoryReturnStatus = i =>
            {
                return retailEntity.ReturnRecordHistory.Count(r => r.InventoryId == i.Id && r.IsReturn) > 0 ? 1 : 0;
            };
            var inventoryList = retailEntity.RetailItems.SelectMany(i => i.RetailInventorys).ToList();
            var res = new
            {
                InventoryList = inventoryList.Select(i => new
                {
                    InventoryId = i.Id,
                    ProductBarcode = i.Inventory.ProductBarcode,
                    ThumbnailPath = i.Inventory.Product.ThumbnailPath ?? i.Inventory.Product.ProductOriginNumber.ThumbnailPath,
                    ProductRetailPrice = Math.Round(i.RetailItem.ProductRetailPrice * levelDiscount, 2),//退货时,如果订单有等级折扣,退货商品零售价要打折
                    IsReturn = getInventoryReturnStatus(i.Inventory)
                }).ToList(),
                UseCouponOrStoreActivity = retailEntity.UseCouponOrStoreActivity
            };
            return Json(new OperationResult(OperationResultType.Success, string.Empty, res));
        }



       
        public ActionResult Create(FormCollection form)
        {
            OperationResult result = new OperationResult(OperationResultType.Error);
            var jsonData = form["postData"];
            if (string.IsNullOrEmpty(jsonData))
            {
                return Json(new OperationResult(OperationResultType.Error, "参数有误"));
            }

            try
            {
                var resturnInfo = JsonConvert.DeserializeObject<ReturnInfoModel>(jsonData);
                var res = _returnedContract.AddReturn(resturnInfo);
                return Json(res);


            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, "操作失败" + ex.Message));
            }
        }

       

        /// <summary>
        /// 整单退货商品时计算应退各项金额
        /// </summary>
        /// <returns></returns>
        public ActionResult GetWholeReturnCou(string retailNumber, string productBarcodes)
        {
            var res = _returnedContract.GetWholyReturnMoney(retailNumber, productBarcodes);
            return Json(res,JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 部分退货计算退款金额
        /// </summary>
        /// <param name="retailNumber"></param>
        /// <param name="barcodesFromUser"></param>
        /// <returns></returns>
        public ActionResult GetReturnMoney(string retailNumber, string productBarcodes)
        {
            var res = _returnedContract.GetPartialReturnMoney(retailNumber, productBarcodes);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

       
        
    }
}