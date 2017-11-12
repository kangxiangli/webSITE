using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Antlr3.ST.Language;
using AutoMapper;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;
using Whiskey.ZeroStore.ERP.Services;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using System.Data.Entity;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.Web.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Notices.Controllers
{
    public class PrintDataModel
    {

        public int StoreId { get; set; }
        public string StoreName { get; set; }
        //有业绩员工
        public int EmployeeCoun { get; set; }
        //成本
        public decimal Cost { get; set; }
        //盈利
        public decimal Gain { get; set; }
        //库存
        public string InventoryCount { get; set; }
        //销售数量
        public int RetailCount { get; set; }
        //实际销售额
        public decimal RealRetailAmount { get; set; }


        public decimal RetailAmount { get; set; }
        //现金
        public decimal CashConsume { get; set; }
        //刷卡消费
        public decimal SwiptCardConsume { get; set; }
        // 积分消费
        public decimal ScoreConsume { get; set; }
        // 储值消费
        public decimal BalanceConsume { get; set; }
        // 抹去总计
        public decimal EraseAmount { get; set; }
        // 找零总计
        public decimal ReturnSmallMoneyTotal { get; set; }
        // 换入商品数量
        public decimal ExchangeOrderNewProductQuantity { get; set; }
        // 换出商品数量
        public decimal ExchangeOrderOriginProductQuantity { get; set; }
        // 补差金额
        public decimal ExchangeOrderPayAmount { get; set; }
        // 退货数量
        public decimal ReturnedCount { get; set; }
        // 退货金额
        public decimal ReturnedAmount { get; set; }
        // 实际退货金额
        public decimal RealReturnedAmount { get; set; }
        // 会员新增数量
        public int AddMembCoun { get; set; }
        // 充储值次数
        public int RechargeBalanceCoun { get; set; }

        /// <summary>
        /// 储值总额(现金+刷卡,不包含赠送)
        /// </summary>
        public decimal MemberRechargeBalanceAmount { get; set; }

        /// <summary>
        /// 现金
        /// </summary>
        public decimal CashAmountFromRecharge { get; set; }

        /// <summary>
        /// 刷卡
        /// </summary>
        public decimal CardAmountFromRecharge { get; set; }

        /// <summary>
        /// 赠送
        /// </summary>
        public decimal CouponAmountFromRecharge { get; set; }

        // 充积分次数
        public int RechargeScoreCoun { get; set; }

        /// <summary>
        /// 充积分总额
        /// </summary>
        public decimal MemberRechargeScoreAmount { get; set; }

    }
    public class StoreStatisticsController : Controller
    {
        protected readonly IRetailContract _retailContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IStoreStatisticsContract _storeStatisticsContract;
        protected readonly IReturnedContract _ReturnedContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IOrderblankContract _orderblankContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IOrderblankItemContract _orderblankItemContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IModuleContract _moduleContract;
        protected readonly IPermissionContract _permissionContract;
        private const string DATE_FORMAT = "yyyyMMdd";

        //店铺统计
        // GET: /Notices/StoreStatistics/
        public StoreStatisticsController(IRetailContract retailContract,
            IStorageContract storageContract,
             IAdministratorContract administratorContract,
            IBrandContract brandContract,
            IStoreStatisticsContract storeStatisticsContract,
            IReturnedContract returnedContract,
            IMemberContract memberContract,
            IMemberDepositContract memberDepositContract,
            IInventoryContract inventoryContract,
            IOrderblankContract orderblankContract,
            IOrderblankItemContract orderblankItemContract,
            IStoreContract storeContract,
            IModuleContract moduleContract,
            IPermissionContract permissionContract)
        {
            _retailContract = retailContract;
            _storageContract = storageContract;
            _brandContract = brandContract;
            _storeStatisticsContract = storeStatisticsContract;
            _ReturnedContract = returnedContract;
            _memberContract = memberContract;
            _memberDepositContract = memberDepositContract;
            _inventoryContract = inventoryContract;
            _administratorContract = administratorContract;
            _orderblankContract = orderblankContract;
            _storeContract = storeContract;
            _orderblankItemContract = orderblankItemContract;
            _moduleContract = moduleContract;
            _permissionContract = permissionContract;
        }
        [Layout]
        public ActionResult Index()
        {
            ViewBag.DefaultDate = DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd");
            ViewBag.StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1).ToString("yyyy-MM-dd");
            ViewBag.EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
            ViewBag.Flags = JsonHelper.ToJson(PageFlag());
            return View();
        }

        /// <summary>
        /// 库存统计界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult InventoryStat()
        {
            ViewBag.DefaultDate = DateTime.Now.Date.AddDays(-1).ToString("yyyy-MM-dd");
            return View();
        }


        private List<string> PageFlag()
        {
            var area = RouteData.DataTokens.ContainsKey("area") ? RouteData.DataTokens["area"].ToString() : string.Empty;
            var controller = RouteData.Values["controller"].ToString();

            var pageUrl = string.Format("{0}/{1}/Index", area, controller);

            try
            {
                var listpers = PermissionHelper.GetCurrentUserPagePermission(pageUrl, _administratorContract, _moduleContract, _permissionContract)
                    .ToList();

                return listpers.Where(p => !string.IsNullOrEmpty(p.OnlyFlag))
                    .Select(p => p.OnlyFlag)
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<string>();
            }

        }

        public ActionResult List(int? storeId, int? startDate, int? endDate)
        {
            var rq = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<StoreStatistics>(rq.FilterGroup);
            var storeids = new List<int>();
            var currentDate = int.Parse(DateTime.Now.ToString(DATE_FORMAT));
            if (storeId.HasValue)
            {
                storeids.Add(storeId.Value);
            }
            else
            {
                storeids.AddRange(_storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value));
            }

            var query = _storeStatisticsContract.StoreStatistics
                                    .Where(c => storeids.Contains(c.StoreId) && (!c.Store.IsDeleted && c.Store.IsEnabled));
            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(c => c.StatDate >= startDate.Value && c.StatDate <= endDate.Value);
            }

            //根据店铺分组
            var groupedStatList = query.GroupBy(c => c.StoreId).OrderBy(c => c.Key);
            Func<int, int, string> getInventory = (sid, date) =>
            {
                var entity = _storeStatisticsContract.StoreStatistics.FirstOrDefault(s => s.StatDate == date && s.StoreId == sid);

                return entity.InventoryCount.HasValue ? entity.InventoryCount.Value.ToString() : "无信息";
            };
            var resd = groupedStatList.Skip(rq.PageCondition.PageIndex).Take(rq.PageCondition.PageSize).ToList().Select(c => new
            {
                // 基础信息
                c.Key,
                c.FirstOrDefault().Store.Id,
                c.FirstOrDefault().StoreName,
                c.FirstOrDefault().Store.IsClosed,
                // 销售
                RetailOrderCount = c.Sum(g => g.RetailOrderCount),
                RetailCount = c.Sum(g => g.RetailCount),
                RetailAmount = c.Sum(g => g.RetailAmount),
                RealRetailAmount = c.Sum(g => g.RealRetailAmount),
                // 退货
                ReturnedCount = c.Sum(g => g.ReturnedCount),
                ReturnedAmount = c.Sum(g => g.ReturnedAmount),
                ReturnedOrderCount = c.Sum(g => g.ReturnedOrderCount),
                RealReturnedAmount = (float)Math.Round(c.Sum(g => g.RealReturnedAmount), 2),
                EmployeeCoun = c.SelectMany(g => g.EmployeeIdsFromOrder.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).Distinct().Count(),
                AddMembCoun = c.Sum(g => g.AddMemberCount),

                // 换货
                ExchangeOrderNewProductQuantity = c.Sum(g => g.ExchangeOrderNewProductQuantity),
                ExchangeOrderOriginProductQuantity = c.Sum(g => g.ExchangeOrderOriginProductQuantity),
                ExchangeOrderPayAmount = c.Sum(g => g.ExchangeOrderPayAmount),

                // 储值,积分
                RechargeBalanceCoun = c.Sum(g => g.MemberCountFromRechargeBalance),
                RechargeScoreCoun = c.Sum(g => g.MemberCountFromRechargeScore),
                MemberRechargeBalanceAmount = c.Sum(g => g.MemberRechargeBalanceAmount),
                MemberRechargeScoreAmount = c.Sum(g => g.MemberRechargeScoreAmount),

                // 盈利成本
                Cost = c.Sum(g => g.Cost),
                Gain = c.Sum(g => g.Gain),

                // 配货单发货,收货
                OrderblankDeliverCount = c.Sum(g => g.OrderblankDeliverCount),
                OrderblankAcceptCount = c.Sum(g => g.OrderblankAcceptCount),


                // 库存
                InventoryCount = (startDate.HasValue && endDate.HasValue && startDate == endDate) ? getInventory(storeId.HasValue ? storeId.Value : c.FirstOrDefault().Store.Id, startDate.Value) : "无信息",


            }).ToList();


            int cou = groupedStatList.Count();
            GridData<object> data = new GridData<object>(resd, cou, Request);
            return Json(data);

        }

        /// <summary>
        /// 获取合计数据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ActionResult GetTotalStatData(int? storeId, int? startDate, int? endDate)
        {
            var rq = new GridRequest(Request);


            // storeid为空,则总计所有店铺的数据
            var storeids = new List<int>();
            var storeName = "所有店铺";
            if (storeId.HasValue)
            {
                storeids.Add(storeId.Value);
                storeName = _storeContract.Stores.FirstOrDefault(s => s.Id == storeId.Value).StoreName;
            }
            else
            {
                storeids.AddRange(_storeContract.Stores.Where(s => !s.IsDeleted && s.IsEnabled).Select(s => s.Id).ToList());
            }

            var query = _storeStatisticsContract.StoreStatistics
                                    .Where(c => storeids.Contains(c.StoreId) && (!c.Store.IsDeleted && c.Store.IsEnabled));


            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(c => c.StatDate >= startDate.Value && c.StatDate <= endDate.Value);
            }
            object res;
            if (!query.Any())
            {
                res = new
                {

                };
            }
            else
            {
                res = new
                {
                    StoreName = storeName,
                    // 销售
                    RetailOrderCount = query.Sum(g => g.RetailOrderCount),
                    RetailCount = query.Sum(g => g.RetailCount),
                    RetailAmount = query.Sum(g => g.RetailAmount),
                    RealRetailAmount = query.Sum(g => g.RealRetailAmount),
                    // 退货
                    ReturnedCount = query.Sum(g => g.ReturnedCount),
                    ReturnedOrderCount = query.Sum(g => g.ReturnedOrderCount),
                    ReturnedAmount = query.Sum(g => g.ReturnedAmount),
                    RealReturnedAmount = (float)Math.Round(query.Sum(g => g.RealReturnedAmount), 2),

                    // 新增会员
                    AddMembCoun = query.Sum(g => g.AddMemberCount),


                    // 储值,积分
                    RechargeBalanceCoun = query.Sum(g => g.MemberCountFromRechargeBalance),
                    RechargeScoreCoun = query.Sum(g => g.MemberCountFromRechargeScore),
                    MemberRechargeBalanceAmount = query.Sum(g => g.MemberRechargeBalanceAmount),
                    MemberRechargeScoreAmount = query.Sum(g => g.MemberRechargeScoreAmount),

                };
            }


            return Json(new OperationResult(OperationResultType.Success, string.Empty, res));
        }

        /// <summary>
        /// 实时店铺统计
        /// </summary>
        /// <param name="starttime"></param>
        /// <param name="endtime"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public ActionResult RealTimeData(DateTime starttime, DateTime endtime, int storeId, bool forceReGenerate = false)
        {
            // 日期范围校验
            if (starttime.Date > DateTime.Now.Date || endtime.Date > DateTime.Now.Date)
            {
                return Json(new OperationResult(OperationResultType.Error, "统计日期不可超过今日"));
            }

            if (endtime.Subtract(starttime).Days > 30)
            {
                return Json(new OperationResult(OperationResultType.Error, "日期范围过长"));
            }

            // 生成int型日期
            var startDateInt = starttime.ToString(DATE_FORMAT);
            var endDateInt = endtime.ToString(DATE_FORMAT);
            var startNum = int.Parse(startDateInt);
            List<int> dates = new List<int>();
            for (int i = 0; i <= endtime.Subtract(starttime).Days; i++)
            {
                dates.Add(int.Parse((starttime.AddDays(i).ToString(DATE_FORMAT))));
            }

            //查询历史数据
            var oldData = _storeStatisticsContract.StoreStatistics.Where(s => s.StoreId == storeId && dates.Contains(s.StatDate)).ToList();

            // 生成统计数据
            var list = new List<StoreStatistics>();
            var currentDate = int.Parse(DateTime.Now.ToString(DATE_FORMAT));
            foreach (var date in dates)
            {
                //如果统计的是今日数据,无论是否已有统计数据,都要重新统计
                if (date == currentDate)
                {
                    var model = _storeStatisticsContract.StatData(storeId, date.ToString());
                    list.Add(model);
                }
                else  //对小于今天的日期,如果历史数据存在,则不用统计,如果不存在,那么需要统计出来,但是结果中不包含库存的统计信息
                {
                    if (date < currentDate && oldData.Any(s => s.StatDate == date) && !forceReGenerate)
                    {
                        continue;
                    }
                    if (
                        (date < currentDate && !oldData.Any(s => s.StatDate == date)) ||
                        (date < currentDate && forceReGenerate)
                        )
                    {
                        var model = _storeStatisticsContract.StatData(storeId, date.ToString());
                        list.Add(model);
                    }
                }
            }

            using (var transaction = _storeStatisticsContract.GetTransaction())
            {
                // 删除今日
                if (dates.Contains(currentDate))
                {

                    if (oldData.Any(s => s.StatDate == currentDate))
                    {
                        var ids = oldData.Where(s => s.StatDate == currentDate).Select(s => s.Id).ToArray();
                        var deleteRes = _storeStatisticsContract.Delete(ids);
                        if (deleteRes.ResultType != OperationResultType.Success)
                        {
                            transaction.Rollback();
                            return Json(deleteRes);
                        }
                        oldData.RemoveAll(s => s.StatDate == currentDate);
                    }

                }

                // 强制生成时,删除老数据
                if (forceReGenerate && oldData.Any())
                {
                    _storeStatisticsContract.Delete(oldData.Select(s => s.Id).ToArray());
                }

                var res = new OperationResult(OperationResultType.Success);
                //插入新统计的数据
                if (list.Any())
                {
                    var inventoryCountDic = oldData.Where(o => o.InventoryCount.HasValue)
                           .ToDictionary(o => o.StatDate, o => o.InventoryCount.Value);
                    // 保留老数据的库存数记录
                    foreach (var item in list)
                    {
                        if (inventoryCountDic.ContainsKey(item.StatDate))
                        {
                            item.InventoryCount = inventoryCountDic[item.StatDate];
                        }
                    }
                    res = _storeStatisticsContract.Insert(list.ToArray());
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return Json(res);
                    }
                }
                transaction.Commit();

                return Json(res);

            }

        }


        public ActionResult StatOrderCount()
        {
            var res = _storeStatisticsContract.StatOrderCount();
            return Json(res);
        }

        /// <summary>
        /// 收货详情
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <param name="type">0:发货,1:收货</param>
        /// <returns></returns>
        public ActionResult GetOrderblankDetail(int storeId, DateTime startDate, DateTime endDate, int type)
        {
            if (endDate == startDate)
            {
                endDate = startDate.AddDays(1).AddSeconds(-1);
            }

            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");
            ViewBag.StoreId = storeId;
            ViewBag.Type = type;
            ViewBag.Brands = CacheAccess.GetBrand(_brandContract, true, false);
            return PartialView(new Orderblank());
        }


        /// <summary>
        /// 获取配货单明细
        /// </summary>
        /// <returns></returns>
        public ActionResult OrderblankViewList(DateTime startDate, DateTime endDate, int storeId, int? brandId, string barcode, int type)
        {
            if (endDate == startDate)
            {
                endDate = startDate.AddDays(1).AddSeconds(-1);
            }
            var req = new GridRequest(Request);

            var query = _orderblankContract.Orderblanks.Where(r => !r.IsDeleted && r.IsEnabled);

            if (type == 0)
            {
                query = query.Where(r => r.OutStoreId == storeId && r.DeliveryTime.Value >= startDate && r.DeliveryTime.Value <= endDate);
            }
            else
            {
                query = query.Where(r => r.ReceiverStoreId == storeId && r.ReceiveTime.Value >= startDate && r.ReceiveTime.Value <= endDate);
            }
            var orderblankNumbers = query.Select(o => o.OrderBlankNumber).ToList();


            var orderblankIemQuery = _orderblankItemContract.OrderblankItems.Where(c => orderblankNumbers.Contains(c.OrderblankNumber))
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
                                                                                Childs = c.OrderBlankBarcodes,
                                                                                SendStorageName = c.Orderblank.OutStorage.StorageName,
                                                                                ReceiveStorageName = c.Orderblank.ReceiverStorage.StorageName
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
                        SendStorageName = item.SendStorageName,
                        ReceiveStorageName = item.ReceiveStorageName
                    }));
                }
            }
            GridData<object> dat = new GridData<object>(dataList, orderblankIemQuery.Count(), Request);
            return Json(dat);

        }

        public ActionResult CloseStore(int? storeId)
        {
            if (!storeId.HasValue)
            {
                return Json(OperationResult.Error("参数错误"));
            }
            var res = _storeContract.CloseStore(storeId.Value);
            if (res.ResultType != OperationResultType.Success)
            {
                return Json(res);
            }
            var statRes = RealTimeData(DateTime.Now.Date, DateTime.Now.Date, storeId.Value);

            return Json(res);
        }

        public ActionResult OpenStore(params int[] storeId)
        {
            if (storeId == null || storeId.Length <= 0)
            {
                return Json(OperationResult.Error("参数错误"));
            }
            var res = _storeContract.OpenStore(storeId);
            return Json(res);
        }


        [HttpGet]
        /// <summary>
        /// 获取指定店铺指定时间段的销售信息
        /// </summary>
        public ActionResult GetRetailInfos(int id, DateTime startDate, DateTime endDate)
        {
            ViewBag.id = id;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate.AddDays(1).AddSeconds(-1);
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetRetailInfos()
        {
            GridRequest grq = new GridRequest(Request);

            var pre = FilterHelper.GetExpression<Retail>(grq.FilterGroup);
            var allda = _retailContract.Retails.Where(pre).Where(r => !r.IsDeleted && r.IsEnabled);
            var da = allda.OrderByDescending(c => c.CreatedTime)
                 .Skip(grq.PageCondition.PageIndex).Take(grq.PageCondition.PageSize)
                 .Select(c => new
                 {
                     c.Id,
                     c.Store.StoreName,
                     c.RetailNumber,
                     RetailCount = c.RetailItems.Sum(g => g.RetailCount),
                     c.ConsumeCount,
                     c.CashConsume,
                     c.SwipeConsume,
                     c.CouponConsume,
                     c.StoredValueConsume,
                     c.ScoreConsume,
                     c.EraseConsume,
                     c.CreatedTime,
                     c.StoreActivityId,
                     c.CouponNumber,
                     LevelDiscount = c.LevelDiscount ?? 0,
                     LevelDiscountAmount = c.LevelDiscountAmount,
                     MemberName = c.Consumer == null ? "" : c.Consumer.MemberName,
                     AdminName = c.Operator.Member.MemberName,
                     Child = c.RetailItems.SelectMany(i => i.RetailInventorys).Select(i => new
                     {
                         ThumbnailPath = i.Inventory.Product.ThumbnailPath ?? i.Inventory.Product.ProductOriginNumber.ThumbnailPath ?? string.Empty,
                         i.ProductBarcode,
                         i.Inventory.Storage.StorageName,
                         ProductName = i.Inventory.Product.ProductName ?? i.Inventory.Product.ProductOriginNumber.ProductName ?? string.Empty,
                         i.Inventory.Product.ProductOriginNumber.Brand.BrandName,
                         i.RetailItem.ProductTagPrice,
                         i.RetailItem.ProductRetailPrice
                     })
                 }).ToList();
            GridData<object> data = new GridData<object>(da, allda.Count(), Request);
            return Json(data);
        }
        /// <summary>
        /// 店铺在指定时间段的销售金额
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ActionResult GetRealAmountInfo(int id, DateTime startDate, DateTime endDate)
        {
            // 生成int型日期
            var startDateInt = int.Parse(startDate.ToString(DATE_FORMAT));
            var endDateInt = int.Parse(endDate.ToString(DATE_FORMAT));

            var data = _storeStatisticsContract.StoreStatistics
                 .Where(c => c.StoreId == id && c.StatDate >= startDateInt && c.StatDate <= endDateInt)
                 .GroupBy(c => c.StoreId).ToList()
                 .Select(c => new StoreStatistics()
                 {
                     StoreName = c.FirstOrDefault().StoreName,
                     CashConsume = c.Sum(g => g.CashConsume),
                     SwipCardConsume = c.Sum(g => g.SwipCardConsume),
                     BalanceConsume = c.Sum(g => g.BalanceConsume),
                     ScoreConsume = c.Sum(g => g.ScoreConsume),
                     Erase = c.Sum(g => g.Erase),
                     ReturnMoney = c.Sum(g => g.ReturnMoney),
                     RealBalanceConsume = c.Sum(g => g.RealBalanceConsume),
                     RealRetailAmount = c.Sum(g => g.RealRetailAmount),
                     CouponConsumeCount = c.Sum(g => g.CouponConsumeCount),
                     CouponConsumeMoney = c.Sum(g => g.CouponConsumeMoney),
                 }).FirstOrDefault();

            return PartialView(data);
        }
        /// <summary>
        /// 店铺在指定时间段的退货信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ActionResult GetReturnedInfos(int id, DateTime startDate, DateTime endDate)
        {
            ViewBag.id = id;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate.AddDays(1).AddSeconds(-1);
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetReturnedInfos(DateTime? startDate,DateTime? endDate)
        {
            GridRequest gr = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Returned>(gr.FilterGroup);
            
            var alldat = _ReturnedContract.Returneds.Where(pred);
            if (startDate.HasValue && endDate.HasValue)
            {
                alldat = alldat.Where(r => r.CreatedTime >= startDate.Value && r.CreatedTime < endDate.Value);
            }
            var da = alldat.OrderByDescending(c => c.CreatedTime)
                    .Skip(gr.PageCondition.PageIndex)
                    .Take(gr.PageCondition.PageSize)
                    .Select(c => new
                    {
                        c.Id,
                        c.ReturnedNumber,
                        c.Status,
                        c.Store.StoreName,
                        Coun = c.ReturnedItems.Sum(g => g.Quantity),
                        c.CreatedTime,
                        AdminName = c.Operator.Member.MemberName,
                        Child = c.ReturnedItems.Select(i => new
                        {
                            ThumbnailPath = i.Inventory.Product.ThumbnailPath ?? i.Inventory.Product.ProductOriginNumber.ThumbnailPath ?? string.Empty,
                            i.ProductBarcode,
                            i.Inventory.Storage.StorageName,
                            i.Inventory.Product.ProductOriginNumber.Brand.BrandName,
                            ProductName = i.Inventory.Product.ProductName ?? i.Inventory.Product.ProductOriginNumber.ProductName ?? string.Empty,
                            ProductTagPrice = i.Inventory.Product.ProductOriginNumber.TagPrice,
                            ProductRetailPrice = i.RetailPrice,
                        })
                    }).ToList();
            GridData<object> data = new GridData<object>(da, alldat.Count(), Request);
            return Json(data);
        }
        /// <summary>
        /// 店铺在指定时间段的退货金额
        /// </summary>
        /// <param name="id"></param>
        /// <param name="starTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public ActionResult GetReturnedMoneInfos(int id, DateTime startDate, DateTime endDate)
        {
            // 生成int型日期
            var startDateInt = int.Parse(startDate.ToString(DATE_FORMAT));
            var endDateInt = int.Parse(endDate.ToString(DATE_FORMAT));


            var data = _storeStatisticsContract.StoreStatistics
                  .Where(c => c.StoreId == id && c.IsEnabled && !c.IsDeleted && c.StatDate >= startDateInt &&
                          c.StatDate <= endDateInt)
                  .GroupBy(c => c.StoreId).ToList()
                  .Select(c => new StoreStatistics()
                  {
                      StoreName = c.FirstOrDefault().StoreName,
                      RealReturnedAmount = c.Sum(g => g.RealReturnedAmount),
                      ReturnedCount = c.Sum(g => g.ReturnedCount),
                      ReturnedCash = c.Sum(g => g.ReturnedCash),
                      ReturnedSwipCard = c.Sum(g => g.ReturnedSwipCard),
                      ReturnedBalance = c.Sum(g => g.ReturnedBalance),
                      RealReturnedBalance = c.Sum(g => g.RealReturnedBalance),
                      ReturnedScore = c.Sum(g => g.ReturnedScore),

                  }).FirstOrDefault();
            return PartialView(data);
        }
        /// <summary>
        /// 指定店铺指定时间段新增会员
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ActionResult GetAddMemInfo(int id, DateTime startDate, DateTime endDate)
        {
            ViewBag.id = id;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate.AddDays(1).AddSeconds(-1);
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetAddMemInfo()
        {
            GridRequest req = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Member>(req.FilterGroup);
            var alldata = _memberContract.Members.Where(pred);
            var data = alldata.OrderByDescending(c => c.CreatedTime)
                    .Skip(req.PageCondition.PageIndex)
                    .Take(req.PageCondition.PageSize)
                    .Select(c => new
                    {
                        c.Id,
                        c.MemberName,
                        c.Gender,
                        c.CreatedTime,
                        c.Store.StoreName,
                        c.UserPhoto,
                        c.MobilePhone,
                        AdminName = c.Operator.Member.MemberName,
                    }).ToList();
            GridData<object> grdata = new GridData<object>(data, alldata.Count(), Request);
            return Json(grdata);
        }
        /// <summary>
        /// 指定店铺，指定时间段内，新充值储值的会员
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ActionResult GetBalanceRecharge(int id, DateTime startDate, DateTime endDate)
        {
            ViewBag.id = id;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate.AddDays(1).AddSeconds(-1);
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetBalanceRecharge()
        {
            GridRequest req = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<MemberDeposit>(req.FilterGroup);
            var allda = _memberDepositContract.MemberDeposits.Where(pred).Where(c => c.MemberActivityType == MemberActivityFlag.Recharge);
            var data = allda.OrderByDescending(c => c.CreatedTime)
                    .Skip(req.PageCondition.PageIndex)
                    .Take(req.PageCondition.PageSize)
                    .Select(c => new
                    {
                        c.Id,
                        c.Cash,
                        c.Card,
                        c.Member.MemberName,
                        c.Price,
                        c.Coupon,
                        c.BeforeBalance,
                        c.AfterBalance,
                        AdminName = c.Operator.Member.MemberName,
                        DepositContext = c.DepositContext.ToString(),
                        c.CreatedTime
                    }).ToList();
            GridData<object> grdata = new GridData<object>(data, allda.Count(), Request);
            return Json(grdata);
        }
        public ActionResult GetScorRechang(int id, DateTime startDate, DateTime endDate)
        {
            ViewBag.id = id;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate.AddDays(1).AddSeconds(-1);
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetScorRechang()
        {
            GridRequest req = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<MemberDeposit>(req.FilterGroup);
            var allda = _memberDepositContract.MemberDeposits.Where(pred).Where(c => c.MemberActivityType == MemberActivityFlag.Score);
            var data = allda.OrderByDescending(c => c.CreatedTime)
                    .Skip(req.PageCondition.PageIndex)
                    .Take(req.PageCondition.PageSize)
                    .Select(c => new
                    {
                        c.Id,
                        c.Member.MemberName,
                        Score = c.Score,
                        c.Coupon,
                        c.BeforeScore,
                        c.AfterScore,
                        AdminName = c.Operator.Member.MemberName,
                        c.CreatedTime,
                        DepositContext = c.DepositContext.ToString()
                    }).ToList();
            GridData<object> grdata = new GridData<object>(data, allda.Count(), Request);
            return Json(grdata);
        }
        /// <summary>
        /// 有销售业绩的员工统计
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ActionResult GetEmployeInfo(int id, DateTime startDate, DateTime endDate)
        {
            ViewBag.id = id;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate.AddDays(1).AddSeconds(-1);
            return PartialView();

        }
        [HttpPost]
        public ActionResult GetEmployeInfo()
        {
            GridRequest req = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Retail>(req.FilterGroup);
            var allda = _retailContract.Retails.Where(pred).GroupBy(c => c.OperatorId);
            var data = allda.OrderBy(c => c.Key)
                    .Skip(req.PageCondition.PageIndex)
                    .Take(req.PageCondition.PageSize)
                    .Select(c => new
                    {
                        Id = c.Key,
                        c.FirstOrDefault().Store.StoreName,
                        AdminName = c.FirstOrDefault().Operator.Member.MemberName,
                        ConsumeCount = c.Sum(g => g.ConsumeCount),
                        Count = c.Count(),
                    }).ToList();
            GridData<object> grdata = new GridData<object>(data, allda.Count(), Request);
            return Json(grdata);

        }

        private string getStoreInventory(int storeId, int date)
        {
            var entity = _storeStatisticsContract.StoreStatistics.FirstOrDefault(s => s.StatDate == date && s.StoreId == storeId);
            if (entity == null)
            {
                return "无信息";
            }
            return entity.InventoryCount.HasValue ? entity.InventoryCount.Value.ToString() : "无信息";
        }

        /// <summary>
        /// 打印统计数据
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ActionResult PrintReceipt(int? storeId, DateTime? startDate, DateTime? endDate)
        {
            //日期格式化为int
            var dateIntStart = int.Parse(startDate.Value.ToString(DATE_FORMAT));
            var dateIntEnd = int.Parse(endDate.Value.ToString(DATE_FORMAT));

            var data = _storeStatisticsContract.StoreStatistics
                                  .Where(c => c.StoreId == storeId.Value && c.StatDate >= dateIntStart && c.StatDate <= dateIntEnd)
                                  .ToList();

            var rechareDetail = this.QueryCashScoreFromRecharge(storeId.Value, startDate.Value, endDate.Value);
            //根据店铺分组
            var groupedStatList = data.GroupBy(c => c.StoreId).OrderBy(c => c.Key);

            var result = groupedStatList.Select(c => new PrintDataModel
            {
                CashAmountFromRecharge = rechareDetail.Cash,
                CardAmountFromRecharge = rechareDetail.Card,
                CouponAmountFromRecharge = rechareDetail.Coupon,

                StoreId = c.FirstOrDefault().Store.Id,
                StoreName = c.FirstOrDefault().StoreName,
                //有业绩员工
                EmployeeCoun = c.SelectMany(g => g.EmployeeIdsFromOrder.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).Distinct().Count(),
                //成本
                Cost = c.Sum(g => g.Cost),
                //盈利
                Gain = c.Sum(g => g.Gain),
                //库存
                InventoryCount = (startDate.HasValue && endDate.HasValue && startDate == endDate) ? getStoreInventory(storeId.HasValue ? storeId.Value : c.FirstOrDefault().Store.Id, dateIntStart) : "无信息",

                //销售数量
                RetailCount = c.Sum(g => g.RetailCount),
                //实际销售额
                RealRetailAmount = c.Sum(g => g.RealRetailAmount),

                RetailAmount = c.Sum(g => g.RetailAmount),
                //现金
                CashConsume = c.Sum(g => g.CashConsume),
                //刷卡消费
                SwiptCardConsume = c.Sum(g => g.SwipCardConsume),

                // 积分消费
                ScoreConsume = c.Sum(g => g.ScoreConsume),

                // 储值消费
                BalanceConsume = c.Sum(g => g.BalanceConsume),

                // 抹去总计
                EraseAmount = c.Sum(g => g.Erase),

                // 找零总计
                ReturnSmallMoneyTotal = c.Sum(g => g.ReturnMoney),

                // 换入商品数量
                ExchangeOrderNewProductQuantity = c.Sum(g => g.ExchangeOrderNewProductQuantity),

                // 换出商品数量
                ExchangeOrderOriginProductQuantity = c.Sum(g => g.ExchangeOrderOriginProductQuantity),

                // 补差金额
                ExchangeOrderPayAmount = c.Sum(g => g.ExchangeOrderPayAmount),

                // 退货数量
                ReturnedCount = c.Sum(g => g.ReturnedCount),

                // 退货金额
                ReturnedAmount = c.Sum(g => g.ReturnedAmount),

                // 实际退货金额
                RealReturnedAmount = c.Sum(g => g.RealReturnedAmount),

                // 会员新增数量
                AddMembCoun = c.Sum(g => g.AddMemberCount),

                // 充储值
                RechargeBalanceCoun = c.Sum(g => g.MemberCountFromRechargeBalance),
                MemberRechargeBalanceAmount = c.Sum(g => g.MemberRechargeBalanceAmount),

                // 充积分
                RechargeScoreCoun = c.Sum(g => g.MemberCountFromRechargeScore),
                MemberRechargeScoreAmount = c.Sum(g => g.MemberRechargeScoreAmount),
            }).Where(g => g.StoreId == storeId.Value).FirstOrDefault();
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
            if (result == null)
            {
                result = new PrintDataModel();
            }
            return PartialView(result);
        }

        private DepositDetail QueryCashScoreFromRecharge(int storeId, DateTime startDate, DateTime endDate)
        {
            var startTime = startDate.Date;
            var endTime = endDate.Date.AddDays(1).AddSeconds(-1);
            // 统计时间段内储值的构成(现金,刷卡,赠送)
            var depositQuery = _memberDepositContract.MemberDeposits
                                                          .Where(r => !r.IsDeleted && r.IsEnabled)
                                                          .Where(r => r.Member.StoreId.Value == storeId)
                                                          .Where(r => r.CreatedTime >= startTime && r.CreatedTime <= endTime);
            var rechargeBalanceQuery = depositQuery.Where(r => r.MemberActivityType == MemberActivityFlag.Recharge);
            var data = rechargeBalanceQuery.Select(r => new
            {
                r.Cash,
                r.Card,
                r.Coupon
            }).ToList();
            if (data == null || data.Count <= 0)
            {
                return new DepositDetail();
            }

            var res = new DepositDetail()
            {
                Cash = data.Sum(r => r.Cash),
                Card = data.Sum(r => r.Card),
                Coupon = data.Sum(r => r.Coupon)
            };

            return res;
        }

        private class DepositDetail
        {
            public decimal Cash { get; set; }
            public decimal Card { get; set; }
            public decimal Coupon { get; set; }
        }


        public ActionResult ValidPrintDate(int? storeId, DateTime? startDate, DateTime? endDate)
        {
            if (!storeId.HasValue || !startDate.HasValue || !endDate.HasValue)
            {
                return Json(OperationResult.Error("参数错误"));
            }

            //如果打印日期包含今日,需要店铺闭店
            if (endDate.Value.Date > DateTime.Now.Date)
            {
                return Json(OperationResult.Error("打印日期不可超过今日"));
            }
            else if (endDate.Value.Date == DateTime.Now.Date)
            {
                if (!_storeContract.IsInClosedStat(storeId.Value))
                {
                    return Json(OperationResult.Error("打印日期包含今日,店铺需要闭店后才能打印"));
                }
            }
            return Json(OperationResult.OK());
        }





    }
}