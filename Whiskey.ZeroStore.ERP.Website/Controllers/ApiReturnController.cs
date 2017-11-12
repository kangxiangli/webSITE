using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Helper;
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

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    /// <summary>
    /// 退货Controller
    /// </summary>
    [CheckStoreIsClosed]
    public class ApiReturnController : Controller
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

        public ApiReturnController(IRetailContract retailContract, IProductContract productContract, IRetailItemContract retailItemContract, IAdministratorContract administratorContract, IReturnedContract returnedContract, IInventoryContract inventoryContract, IMemberContract memberContract, IMemberDepositContract memberDepositContract,
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



        public ActionResult GetOrderInfo(string retailNumber)
        {
            if (string.IsNullOrEmpty(retailNumber))
            {
                throw new ArgumentException("订单号不能为空", nameof(retailNumber));
            }

            var retailEntity = _retailContract.Retails.Where(r => !r.IsDeleted && r.IsEnabled && r.RetailNumber == retailNumber)
                .Include(r => r.ReturnRecordHistory)
                .FirstOrDefault();

            //获取retail 信息
            var orderInfo = new
            {
                Id = retailEntity.Id,
                retailEntity.TradeCredential,
                retailEntity.TradeReferNumber,
                retailEntity.RetailNumber,
                retailEntity.ConsumeCount,
                retailEntity.ConsumerId,
                retailEntity.CouponNumber,
                CouponName = retailEntity.CouponItem?.Coupon?.CouponName,
                MemberName = retailEntity.ConsumerId.HasValue ? retailEntity.Consumer.MemberName : string.Empty,
                StoreActivityId = retailEntity.StoreActivityId,  //店铺活动id
                StoreActivityName = retailEntity.StoreActivity?.ActivityName,
                Operator = retailEntity.Operator.Member.MemberName,
                ProductCount = (retailEntity.RetailItems.Any()) ? retailEntity.RetailItems.Select(t => t.RetailCount).Sum() : 0,
                retailEntity.CreatedTime,
                Status = retailEntity.RetailState.ToString(),
                StoreName = retailEntity.Store.StoreName,
                SwipeCardType = retailEntity.SwipeCardType
            };

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

                ProductBarcode = t.Inventory.ProductBarcode,             // 流水号
                RetailPrice = t.RetailItem.ProductRetailPrice,// 零售价格
                ThumbnailPath = t.Inventory.Product.ThumbnailPath ?? t.Inventory.Product.ProductOriginNumber.ThumbnailPath,         // 商品缩略图                        // 状态,不展示
                Status = GetInventoryStatusDesc(t.Inventory) // 状态描述
            }).ToList().Select(t => new
            {
                t.ProductBarcode,
                t.RetailPrice,
                ThumbnailPath = string.IsNullOrEmpty(t.ThumbnailPath) ? string.Empty : ConfigurationHelper.WebUrl + t.ThumbnailPath,
                t.Status
            }).ToList();
            var resData = new
            {
                OrderInfo = new
                {
                    orderInfo.Id,
                    orderInfo.StoreName,
                    orderInfo.RetailNumber,
                    orderInfo.ConsumeCount,
                    orderInfo.ConsumerId,
                    orderInfo.CouponNumber,
                    orderInfo.CouponName,
                    orderInfo.MemberName,
                    orderInfo.StoreActivityId,
                    orderInfo.StoreActivityName,
                    orderInfo.Operator,
                    orderInfo.ProductCount,
                    CreatedTime = orderInfo.CreatedTime.ToUnixTime(),
                    orderInfo.Status,
                    orderInfo.TradeCredential,
                    orderInfo.TradeReferNumber,
                    orderInfo.SwipeCardType
                },
                OrderDetail = child
            };


            return Json(new OperationResult(OperationResultType.Success, string.Empty, resData), JsonRequestBehavior.AllowGet);
        }


        public ActionResult CanReturn(string retailNumber)
        {
            var res = _returnedContract.CanReturn(retailNumber);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        
        [HttpPost]
        public ActionResult Add(string payload)
        {
            var result = new OperationResult(OperationResultType.Error);

            if (string.IsNullOrEmpty(payload))
            {
                return Json(new OperationResult(OperationResultType.Error, "参数有误"));
            }

            try
            {
                var resturnInfo = JsonConvert.DeserializeObject<ReturnInfoModel>(payload);
                var res = _returnedContract.AddReturn(resturnInfo);
                return Json(res);
            }
            catch (Exception ex)
            {
                return Json(new OperationResult(OperationResultType.Error, "操作失败" + ex.Message));
            }
        }

        public ActionResult GetRetailBarcodes(string retailNumber)
        {
            var res = _returnedContract.GetRetailBarcodes(retailNumber);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取退货金额
        /// </summary>
        /// <param name="retailNumber">退货单</param>
        /// <param name="barcodes">退货流水号</param>
        /// <returns></returns>
        public ActionResult GetReturnMoney(string retailNumber, string barcodes)
        {
            var res = _returnedContract.GetReturnMoney(retailNumber, barcodes);
            return Json(res, JsonRequestBehavior.AllowGet);
        }


    }
}