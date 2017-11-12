using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Notice
{
    public class StoreStatisticsService : ServiceBase, IStoreStatisticsContract
    {
        private readonly IRepository<StoreStatistics, int> _storeStatisticsRepository;
        private readonly IRetailContract _retailContract;
        private readonly IRepository<Returned, int> _returnedRepo;
        private readonly IRepository<Member, int> _MemberRepo;
        private readonly IRepository<MemberDeposit, int> _MemberDepositRepo;
        private readonly IRepository<Orderblank, int> _OrderblankRepo;
        private readonly IRepository<Inventory, int> _InventoryRepo;
        private readonly IRepository<RetailItem, int> _RetailItemRepo;
        private readonly IRepository<Administrator, int> _AdministratorRepo;
        private readonly IRepository<Store, int> _StoreRepo;
        private readonly IRepository<ReturnedItem, int> _ReturnedItemRepo;
        private readonly IRepository<InventoryRecord, int> _inventoryRecordRepo;
        private readonly IStoreContract _storeContract;


        public StoreStatisticsService(IRepository<StoreStatistics, int> storeStatisticsRepository,
                                      IRetailContract retailContract,
                                      IRepository<Returned, int> returnedRepo,
                                      IRepository<Member, int> MemberRepo,
                                      IRepository<MemberDeposit, int> MemberDepositRepo,
                                      IRepository<Orderblank, int> OrderblankRepo,
                                      IRepository<Inventory, int> InventoryRepo,
                                      IRepository<RetailItem, int> RetailItemRepo,
                                      IRepository<Administrator, int> AdministratorRepo,
                                      IRepository<Store, int> StoreRepo,
                                      IRepository<ReturnedItem, int> ReturnedItemRepo,
                                      IRepository<InventoryRecord, int> inventoryRecordRepo,
                                      IStoreContract storeContract


            )
            : base(storeStatisticsRepository.UnitOfWork)
        {
            _storeStatisticsRepository = storeStatisticsRepository;
            _retailContract = retailContract;
            _returnedRepo = returnedRepo;
            _MemberRepo = MemberRepo;
            _MemberDepositRepo = MemberDepositRepo;
            _OrderblankRepo = OrderblankRepo;
            _InventoryRepo = InventoryRepo;
            _RetailItemRepo = RetailItemRepo;
            _AdministratorRepo = AdministratorRepo;
            _StoreRepo = StoreRepo;
            _ReturnedItemRepo = ReturnedItemRepo;
            _inventoryRecordRepo = inventoryRecordRepo;
            _storeContract = storeContract;
        }

        public IQueryable<StoreStatistics> StoreStatistics
        {
            get { return _storeStatisticsRepository.Entities; }
        }

        public OperationResult Insert(params StoreStatistics[] ents)
        {
            ents.Each(c =>
            {
                c.CreatedTime = DateTime.Now;
                c.UpdatedTime = DateTime.Now;
                c.OperatorId = AuthorityHelper.OperatorId;
            });
            return _storeStatisticsRepository.Insert((IEnumerable<StoreStatistics>)ents) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error, "没有数据");
        }

        /// <summary>
        /// 统计店铺某一日的各项数据
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="dateInt">日期,格式:20160608</param>
        /// <returns></returns>
        public StoreStatistics StatData(int storeId, string dateInt)
        {
            var storeEntity = _StoreRepo.Entities.FirstOrDefault(s => s.Id == storeId);
            var startTime = DateTime.ParseExact(dateInt, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            var endTime = startTime.AddDays(1).AddSeconds(-1);
            var model = new StoreStatistics() { StoreId = storeId, StoreName = storeEntity.StoreName, StatDate = int.Parse(dateInt) };

            var retailEntities = _retailContract.Retails.Where(r => r.StoreId == storeId)
                                   .Where(r => !r.IsDeleted && r.IsEnabled)
                                   .Where(r => r.CreatedTime >= startTime && r.CreatedTime <= endTime)
                                   .ToList();
            var retailIds = retailEntities.Select(r => r.Id).ToList();
            var retailItems = _RetailItemRepo.Entities.Where(i => retailIds.Contains(i.RetailId))
                .Select(i => new { Data = i, WholesalePrice = i.Product.ProductOriginNumber.WholesalePrice })
                .ToList();

            var returnedEntities = _returnedRepo.Entities.Where(r => r.StoreId == storeId)
                                   .Where(r => !r.IsDeleted && r.IsEnabled)
                                   .Where(r => r.CreatedTime >= startTime && r.CreatedTime <= endTime)
                                   .Include(r => r.ReturnedItems)
                                   .ToList();
            var returedIds = returnedEntities.Select(r => r.Id).ToList();
            //var returnedItems = returnedEntities.SelectMany(r => r.ReturnedItems).ToList();
            var returnedItems = _ReturnedItemRepo.Entities.Where(i => returedIds.Contains(i.ReturnedId.Value))
                                .Select(i => new
                                {
                                    Data = i,
                                    WholesalePrice = i.Inventory.Product.ProductOriginNumber.WholesalePrice
                                }).ToList();

            // 统计销售
            model.RetailAmount = Math.Round(retailEntities.Sum(r => r.ConsumeCount), 2);
            model.RetailCount = retailItems.Sum(item => item.Data.RetailCount);
            model.RetailOrderCount = retailEntities.Count();
            model.CashConsume = Math.Round(retailEntities.Sum(r => r.CashConsume), 2);
            model.SwipCardConsume = Math.Round(retailEntities.Sum(r => r.SwipeConsume), 2);
            model.BalanceConsume = Math.Round(retailEntities.Sum(r => r.StoredValueConsume), 2);
            model.RealBalanceConsume = Math.Round(retailEntities.Sum(r => r.RealStoredValueConsume), 2);
            model.ScoreConsume = Math.Round(retailEntities.Sum(r => r.ScoreConsume), 2);
            model.Erase = Math.Round(retailEntities.Sum(r => r.EraseConsume), 2);
            model.ReturnMoney = Math.Round(retailEntities.Sum(r => r.ReturnMoney), 2);
            model.RealRetailAmount = Math.Round(retailEntities.Sum(r => r.RealConsumeMoney), 2);



            // 优惠券活动统计
            model.CouponConsumeCount = retailEntities.Count(r => r.CouponConsume > 0);
            model.StoreActivityConsumeCount = retailEntities.Count(r => r.StoreActivityDiscount > 0);
            model.SaleCampaignConsumeCount = retailItems.Count(item => item.Data.SalesCampaignId.HasValue && item.Data.SalesCampaignDiscount > 0);
            model.CouponConsumeMoney = retailEntities.Sum(r => r.CouponConsume);

            // 统计退货
            model.ReturnedCount = returnedItems.Sum(r => r.Data.Quantity);
            model.ReturnedOrderCount = returnedEntities.Count;
            model.ReturnedCash = Math.Round(returnedEntities.Sum(r => r.Cash), 2);
            model.ReturnedSwipCard = Math.Round(returnedEntities.Sum(r => r.SwipCard), 2);
            model.ReturnedBalance = Math.Round(returnedEntities.Sum(r => r.Balance), 2);
            model.RealReturnedBalance = Math.Round(returnedEntities.Sum(r => r.RealBalance), 2);
            model.ReturnedAmount = Math.Round(model.ReturnedCash + model.ReturnedSwipCard + model.ReturnedBalance, 2);
            model.RealReturnedAmount = Math.Round(returnedEntities.Sum(r => r.RealReturnMoney), 2);

            // 统计会员
            model.AddMemberCount = _MemberRepo.Entities.Where(r => r.StoreId.Value == storeId)
                                   .Where(r => !r.IsDeleted && r.IsEnabled)
                                   .Where(r => r.CreatedTime >= startTime && r.CreatedTime <= endTime)
                                   .Distinct()
                                   .Count();
            model.MemberCountFromOrder = retailEntities.Where(r => r.ConsumerId.HasValue).Select(r => r.ConsumerId.Value).Count();
            model.NoMemberCountFromOrder = retailEntities.Count(r => !r.ConsumerId.HasValue);


            var depositQuery = _MemberDepositRepo.Entities.Where(r => r.DepositStoreId.Value == storeId)
                                                          .Where(r => !r.IsDeleted && r.IsEnabled)
                                                          .Where(r => r.CreatedTime >= startTime && r.CreatedTime <= endTime);

            var rechargeBalanceQuery = depositQuery.Where(r => r.MemberActivityType == MemberActivityFlag.Recharge);
            if (rechargeBalanceQuery.Any())
            {
                // 充储值次数
                model.MemberCountFromRechargeBalance = rechargeBalanceQuery.Select(r => r.MemberId).Count();
                // 充储值总额:总金额-赠送部分
                model.MemberRechargeBalanceAmount = rechargeBalanceQuery.Sum(r => r.Price) - rechargeBalanceQuery.Sum(r => r.Coupon);//充值总额减去赠送部分
            }
            else
            {
                model.MemberCountFromRechargeBalance = 0;
                model.MemberRechargeBalanceAmount = 0M;
            }



            // 充积分次数
            var rechargeScoreQuery = depositQuery.Where(r => r.MemberActivityType == MemberActivityFlag.Score);

            if (rechargeScoreQuery.Any())
            {
                model.MemberCountFromRechargeScore = rechargeScoreQuery.Select(r => r.MemberId).Count();
                model.MemberRechargeScoreAmount = rechargeScoreQuery.Sum(r => r.Score);//积分不会有额外赠送
            }
            else
            {
                model.MemberCountFromRechargeScore = 0;
                model.MemberRechargeScoreAmount = 0M;
            }


            // 统计有业绩的员工
            model.EmployeeCountFromOrder = retailEntities.Count(r => r.OperatorId.HasValue);
            model.EmployeeIdsFromOrder = string.Join(",", retailEntities.Where(r => r.OperatorId.HasValue).Select(r => r.OperatorId.Value));

            // 统计可用库存(只能统计当前的库存,历史库存无法统计)
            if (startTime.Date == DateTime.Now.Date)
            {
                model.InventoryCount = _InventoryRepo.Entities.Where(i => i.StoreId == storeId)
                                                         .Where(i => !i.IsDeleted && i.IsEnabled)
                                                         .Where(i => i.Status == (int)InventoryStatus.Default)
                                                         .Select(i => i.ProductBarcode)
                                                         .Distinct()
                                                         .Count();
            }

            // 统计发货,收货
            var orderblankDeliveryQuery = _OrderblankRepo.Entities.Where(r => r.OutStoreId == storeId)
                                                                   .Where(r => !r.IsDeleted && r.IsEnabled)
                                                                   .Where(r => r.DeliveryTime >= startTime && r.DeliveryTime <= endTime);
            if (orderblankDeliveryQuery.Any())
            {
                var subQuery = orderblankDeliveryQuery.SelectMany(o => o.OrderblankItems);
                if (!subQuery.Any())
                {
                    model.OrderblankDeliverCount = 0;
                }
                else
                {
                    model.OrderblankDeliverCount = subQuery.Sum(i => i.Quantity);
                }
            }
            else
            {
                model.OrderblankDeliverCount = 0;
            }
            var orderblankReceiveQuery = _OrderblankRepo.Entities.Where(r => r.ReceiverStoreId == storeId)
                                                                  .Where(r => !r.IsDeleted && r.IsEnabled)
                                                                  .Where(r => r.Status == OrderblankStatus.已完成)
                                                                  .Where(r => r.ReceiveTime >= startTime && r.ReceiveTime <= endTime);
            if (orderblankReceiveQuery.Any())
            {

                model.OrderblankAcceptCount = orderblankReceiveQuery.SelectMany(o => o.OrderblankItems).Sum(i => (int?)i.Quantity) ?? 0;
            }
            else
            {
                model.OrderblankAcceptCount = 0;
            }

            // 统计入库数量
            var invetoryQuery = _inventoryRecordRepo.Entities.Where(r => r.StoreId == storeId)
                                                .Where(r => !r.IsDeleted && r.IsEnabled)
                                                .Where(r => r.CreatedTime >= startTime && r.CreatedTime <= endTime);
            if (invetoryQuery.Any())
            {
                model.InventoryAddCount = invetoryQuery.Sum(r => r.Quantity);
            }
            else
            {
                model.InventoryAddCount = 0;
            }


            // 计算成本 售出商品总进货价-退货商品总进货价
            model.Cost = (decimal)Math.Round(retailItems.Select(i => i.WholesalePrice * i.Data.RetailCount).Sum() - returnedItems.Sum(item => item.WholesalePrice), 2);

            // 计算盈利  实际销售额-退款现金-退款刷卡-成本+换货补差价
            model.Gain = Math.Round(model.RealRetailAmount - model.ReturnedCash - model.ReturnedSwipCard - model.Cost, 2);


            return model;
        }


        /// <summary>
        /// 统计某一天的零售单于退货单数量
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="dateInt">日期</param>
        public OperationResult StatOrderCount()
        {

            var retailGroupData = _retailContract.Retails.Where(r => !r.IsDeleted && r.IsEnabled)
                                    .GroupBy(r => new { StoreId = r.StoreId.Value, Date = DbFunctions.TruncateTime(r.CreatedTime) })
                                    .Select(g => new
                                    {
                                        StoreId = g.Key.StoreId,
                                        Date = g.Key.Date.Value,
                                        Count = g.Count()
                                    }).ToList()
                                    .Select(g => new
                                    {
                                        g.StoreId,
                                        Date = int.Parse(g.Date.ToString("yyyyMMdd")),
                                        g.Count
                                    }).ToList();


            var returnGroupData = _returnedRepo.Entities.Where(r => !r.IsDeleted && r.IsEnabled)
                                   .GroupBy(r => new { StoreId = r.StoreId.Value, Date = DbFunctions.TruncateTime(r.CreatedTime) })
                                   .Select(g => new
                                   {
                                       StoreId = g.Key.StoreId,
                                       Date = g.Key.Date.Value,
                                       Count = g.Count()

                                   }).ToList()
                                   .Select(g => new
                                   {
                                       g.StoreId,
                                       Date = int.Parse(g.Date.ToString("yyyyMMdd")),
                                       g.Count
                                   }).ToList();


            var datas = _storeStatisticsRepository.Entities.Where(s => !s.IsDeleted && s.IsEnabled).ToList();
            var updates = new List<StoreStatistics>();
            foreach (var entity in datas)
            {
                var retailData = retailGroupData.FirstOrDefault(r => r.StoreId == entity.StoreId && r.Date == entity.StatDate);
                var returnData = returnGroupData.FirstOrDefault(r => r.StoreId == entity.StoreId && r.Date == entity.StatDate);
                if (retailData != null)
                {
                    entity.RetailOrderCount = retailData.Count;
                }
                if (returnData != null)
                {
                    entity.ReturnedOrderCount = returnData.Count;
                }
            }

            var res = _storeStatisticsRepository.Update(datas);
            return OperationResult.OK();

        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">要删除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _storeStatisticsRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }

        }

        public DbContextTransaction GetTransaction()
        {
            return _storeStatisticsRepository.GetTransaction();
        }

        public OperationResult StatStoreWhenSignOut(int adminId)
        {
            try
            {
                //判断是否需要统计
                var adminEntity = _AdministratorRepo.Entities.Where(a => a.Id == adminId)
                                                             .Include(a => a.Department)
                                                             .FirstOrDefault();
                if (adminEntity == null)
                {
                    return new OperationResult(OperationResultType.Error, "员工信息未找到");
                }

                // 查找部门下的店铺
                var store = adminEntity.Department.Stores.FirstOrDefault();

                //所在部门没有归属店铺,或店铺已闭店 就不需要统计
                if (store == null)
                {
                    return new OperationResult(OperationResultType.Success, string.Empty);
                }
                else if (store.IsClosed)
                {
                    return new OperationResult(OperationResultType.Success, string.Empty);
                }

                // 统计今日数据
                var date = DateTime.Now.ToString("yyyyMMdd");
                var statData = StatData(store.Id, date);
                var statDataInt = int.Parse(date);
                if (statData == null)
                {
                    return new OperationResult(OperationResultType.Error, "统计信息生成失败");
                }

                using (var transaction = GetTransaction())
                {
                    //删除今日的老数据
                    var oldStatData = StoreStatistics.Where(s => s.StoreId == store.Id && s.StatDate == statDataInt).Select(s => s.Id).ToArray();
                    if (oldStatData.Length > 0)
                    {
                        var deleteRes = Delete(oldStatData);
                        if (deleteRes.ResultType != OperationResultType.Success)
                        {
                            transaction.Rollback();
                            return new OperationResult(OperationResultType.Error, string.Empty);
                        }
                    }

                    //插入新统计的数据
                    var res = Insert(statData);
                    if (res.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                    }
                    transaction.Commit();
                    return res;

                }

            }
            catch (Exception e)
            {
                return new OperationResult(OperationResultType.Error, e.Message);
            }



        }

        public OperationResult SetStoreOpenWhenFirstSignIn(int adminId)
        {
            try
            {
                //判断是否需要统计
                var adminEntity = _AdministratorRepo.Entities.Where(a => a.Id == adminId)
                                                             .Include(a => a.Department)
                                                             .FirstOrDefault();
                if (adminEntity == null)
                {
                    return new OperationResult(OperationResultType.Error, "员工信息未找到");
                }

                // 查找部门下的店铺
                var storeEntity = adminEntity.Department.Stores.FirstOrDefault();

                //所在部门没有归属店铺,或店铺未闭店 就不需要更新
                if (storeEntity == null)
                {
                    return new OperationResult(OperationResultType.Success, string.Empty);
                }

                var res = _storeContract.OpenStore(storeEntity.Id);

                return OperationResult.OK();
            }
            catch (Exception e)
            {
                return OperationResult.Error(e.Message);
            }

        }
    }
}