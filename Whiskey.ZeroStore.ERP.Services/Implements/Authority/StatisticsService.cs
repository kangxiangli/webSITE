using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Caching;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class StatisticsService : ServiceBase, IStatisticsContract
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StatisticsService));

        private readonly IRepository<Administrator, int> _administratorRepository;
        private readonly IRepository<Brand, int> _brandRepository;
        private readonly IRepository<Category, int> _categoryRepository;
        private readonly IRepository<Color, int> _colorRepo;
        private readonly IRepository<JobPosition, int> _jobPositionRepository;
        private readonly IRepository<Member, int> _memberRepository;
        private readonly IRepository<MemberType, int> _memberTypeRepository;
        private readonly IRepository<MemberLevel, int> _memberLevelRepository;
        private readonly IRepository<OpenCloseRecord, int> _openCloseRecordRepository;
        private readonly IRepository<Product, int> _productRepository;
        private readonly IRepository<ProductOriginNumber, int> _productOriginNumberRepository;
        private readonly IRepository<Purchase, int> _purchaseRepository;
        private readonly IRepository<Retail, int> _retailRepository;
        private readonly IRepository<RetailItem, int> _retailItemRepository;
        private readonly IRepository<ReturnedItem, int> _returnedItemRepository;
        private readonly IRepository<Season, int> _seasonRepo;
        private readonly IRepository<Size, int> _sizeRepository;
        private readonly IRepository<Store, int> _storeRepository;
        private readonly IRepository<Inventory, int> _inventoryRepository;
        private readonly IRepository<MemberFigure, int> _memberFigureRepository;
        private readonly IStoreContract _storeContract;
        private readonly IProductAttributeContract _productAttributeContract;
        private readonly IAdministratorContract _adminContract;


        private Tuple<bool, string> ChkStatParam(SaleStatReq req, IAdministratorContract adminContract, bool requirStoreId = true)
        {
            if (!req.AdminId.HasValue)
            {
                return Tuple.Create(false, "参数错误");
            }
            if (!req.StatType.HasValue)
            {
                return Tuple.Create(false, "参数错误");
            }

            if (requirStoreId)
            {
                if (!req.StoreId.HasValue)
                {
                    return Tuple.Create(false, "参数错误");
                }
                //店铺权限校验

                var storeIds = _storeContract.QueryManageStoreId(req.AdminId.Value);

                if (!storeIds.Contains(req.StoreId.Value))
                {
                    return Tuple.Create(false, "权限不足");
                }
            }


            return Tuple.Create(true, string.Empty);
        }

        private Tuple<bool, string> ChkMemberStatParam(MemberStatReq req, IAdministratorContract adminContract)
        {
            if (!req.AdminId.HasValue)
            {
                return Tuple.Create(false, "参数错误");
            }
            if (!req.MemberStatType.HasValue)
            {
                return Tuple.Create(false, "参数错误");
            }

            if (!req.StoreId.HasValue)
            {
                return Tuple.Create(false, "参数错误");
            }
            //店铺权限校验

            var storeId = _storeContract.QueryManageStoreId(req.AdminId.Value);
            if (!storeId.Contains(req.StoreId.Value))
            {
                return Tuple.Create(false, "权限不足");
            }


            return Tuple.Create(true, string.Empty);
        }

        /// <summary>
        /// 计算统计时间范围
        /// </summary>
        private Tuple<bool, string, DateTime?, DateTime?> CalcStatRange(int? days, DateTime? startDate, DateTime? endDate)
        {
            DateTime? startTime = DateTime.Now;
            DateTime? endTime = DateTime.Now;
            if (days.HasValue)
            {
                if (days.Value > 365)
                {
                    return Tuple.Create<bool, string, DateTime?, DateTime?>(false, "日期范围过大", null, null);
                }

                // 起始日期为前n天
                startTime = DateTime.Now.Date.AddDays(-1 * days.Value);

                //结束时间不包含今日
                endTime = DateTime.Now.Date.AddSeconds(-1);
            }
            else
            {
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    return Tuple.Create<bool, string, DateTime?, DateTime?>(false, "日期参数错误", null, null);
                }

                if (startDate.Value.Date > DateTime.Now.Date)
                {
                    return Tuple.Create<bool, string, DateTime?, DateTime?>(false, "起始日期不可超过今日", null, null);
                }

                if (startDate.Value > endDate.Value)
                {
                    return Tuple.Create<bool, string, DateTime?, DateTime?>(false, "起始不可大于结束日期", null, null);
                }

                if ((DateTime.Now.Date - startDate.Value).TotalDays > 365)
                {
                    return Tuple.Create<bool, string, DateTime?, DateTime?>(false, "日期范围过大", null, null);
                }

                //起始日期当天0：00
                startTime = startDate.Value.Date;
                //截止到结束日期当天23：59：59
                endTime = endDate.Value.Date.AddDays(1).AddSeconds(-1);
            }
            return Tuple.Create(true, string.Empty, startTime, endTime);
        }

        /// <summary>
        /// 销售统计信息返回结构
        /// </summary>
        private Func<RetailInventory, RetailStatEntry> func = i => new RetailStatEntry
        {
            InventoryId = i.InventoryId.Value,
            StoreId = i.RetailItem.Retail.StoreId.Value,
            BrandId = i.Inventory.Product.ProductOriginNumber.BrandId,
            BrandName = i.Inventory.Product.ProductOriginNumber.Brand.BrandName,
            CategoryId = i.Inventory.Product.ProductOriginNumber.CategoryId,
            ColorId = i.Inventory.Product.ColorId,
            ColorName = i.Inventory.Product.Color.ColorName,
            SeasonId = i.Inventory.Product.ProductOriginNumber.SeasonId,
            SizeId = i.Inventory.Product.SizeId,
            SizeName = i.Inventory.Product.Size.SizeName
        };

        private Func<ReturnedItem, RetailStatEntry> returnFunc = i => new RetailStatEntry
        {
            InventoryId = i.InventoryId.Value,
            StoreId = i.Returned.StoreId.Value,
            BrandId = i.Inventory.Product.ProductOriginNumber.BrandId,
            CategoryId = i.Inventory.Product.ProductOriginNumber.Category.ParentId.Value,
            ColorId = i.Inventory.Product.ColorId,
            SeasonId = i.Inventory.Product.ProductOriginNumber.SeasonId,
            SizeId = i.Inventory.Product.SizeId
        };

        public StatisticsService(
               IRepository<OpenCloseRecord, int> _openCloseRecordRepository,
               IRepository<Member, int> _memberRepository,
               IRepository<Category, int> _categoryRepository,
               IRepository<Purchase, int> _purchaseRepository,
               IRepository<Retail, int> _retailRepository,
               IRepository<RetailItem, int> _retailItemRepository,
               IRepository<ReturnedItem, int> _returnedItemRepository,
               IRepository<Store, int> _storeRepository,
               IRepository<Administrator, int> _administratorRepository,
               IRepository<Brand, int> brandRepository,
               IRepository<Color, int> colorRepo,
               IRepository<JobPosition, int> _jobPositionRepository,
               IRepository<Product, int> productRepository,
               IRepository<ProductOriginNumber, int> productOriginNumberRepository,
               IRepository<Season, int> seasonRepo,
               IRepository<Size, int> sizeRepository,
               IRepository<Inventory, int> inventoryRepository,
               IRepository<MemberType, int> memberTypeRepository,
               IRepository<MemberFigure, int> memberFigureRepository,
               IRepository<MemberLevel, int> memberLevelRepository,
               IStoreContract storeContract,
               IAdministratorContract _adminContract,
            IProductAttributeContract productAttributeContract
            ) : base(_administratorRepository.UnitOfWork)
        {
            this._administratorRepository = _administratorRepository;
            this._storeRepository = _storeRepository;
            this._memberRepository = _memberRepository;
            this._retailRepository = _retailRepository;
            this._retailItemRepository = _retailItemRepository;
            this._returnedItemRepository = _returnedItemRepository;
            this._categoryRepository = _categoryRepository;
            this._purchaseRepository = _purchaseRepository;
            this._openCloseRecordRepository = _openCloseRecordRepository;
            this._jobPositionRepository = _jobPositionRepository;
            _brandRepository = brandRepository;
            _colorRepo = colorRepo;
            _seasonRepo = seasonRepo;
            _productOriginNumberRepository = productOriginNumberRepository;
            _sizeRepository = sizeRepository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _memberTypeRepository = memberTypeRepository;
            _memberFigureRepository = memberFigureRepository;
            _memberLevelRepository = memberLevelRepository;
            _storeContract = storeContract;
            _productAttributeContract = productAttributeContract;
            this._adminContract = _adminContract;
        }



        public OperationResult GetSomeCount(int? FrontDay, int TopCount = 6)
        {
            return OperationHelper.Try((opera) =>
            {
                string _key = string.Format("170313_statistics_01_{0}_{1}", FrontDay, TopCount);
                var rdata = CacheHelper.GetCache(_key);
                if (rdata.IsNull())
                {
                    var dateNow = DateTime.Now.Date;
                    TopCount = Math.Abs(TopCount);

                    var member = _memberRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled);
                    var store = _storeRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled);
                    var admin = _administratorRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled);
                    var retailItem = _retailItemRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled);
                    //.Where(w => w.RetailItemState != RetailStatus.整单退货 && w.RetailItemState != RetailStatus.删除 && w.RetailItemState != RetailStatus.禁用)//这个状态现在没用着
                    var returnedItem = _returnedItemRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled);
                    var retail = _retailRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled).Where(w => w.StoreId != null)
                                  .Where(w => w.RetailState != RetailStatus.整单退货 && w.RetailState != RetailStatus.删除 && w.RetailState != RetailStatus.禁用);

                    if (FrontDay.HasValue)
                    {
                        var dateFront = dateNow.AddDays(-FrontDay.Value);
                        retail = retail.Where(w => w.CreatedTime >= dateFront && w.CreatedTime < dateNow);
                    }

                    //销售额前TopCount名
                    var retailTop = retail.GroupBy(g => new { g.StoreId, g.Store.StoreName })
                                 .Select(s => new { StoreId = s.Key.StoreId ?? 0, s.Key.StoreName, SaleMoney = s.Sum(ss => ss.CashConsume + ss.SwipeConsume + ss.RealStoredValueConsume) })
                                 .OrderByDescending(o => o.SaleMoney).Skip(0).Take(TopCount).ToList();

                    var loseCount = TopCount - retailTop.Count;

                    if (loseCount > 0)
                    {
                        var hasSid = retailTop.Select(s => s.StoreId).ToList();
                        var float0 = (decimal)(0);
                        var loseRetail = _storeRepository.Entities.Where(w => !hasSid.Contains(w.Id)).OrderBy(o => o.Id).Skip(0).Take(loseCount).Select(s => new { StoreId = s.Id, s.StoreName, SaleMoney = float0 }).ToList();
                        retailTop.AddRange(loseRetail);
                    }

                    rdata = new
                    {
                        AdminCount = admin.Count(),
                        StoreCount = store.Count(),
                        MemberCount = member.Count(),
                        SaleCount = retailItem.LongCount() - returnedItem.LongCount(),
                        StoreSale = retailTop
                    };

                    CacheHelper.SetCache(_key, rdata, new TimeSpan(0, 5, 0));
                }
                return OperationHelper.ReturnOperationResult(true, opera, rdata);
            }, "获取数据");
        }
        /// <summary>
        /// 用户掌管的店铺列表，空时 返回 0-FASHION
        /// </summary>
        /// <param name="HitName"></param>
        /// <returns></returns>
        public OperationResult GetManagedStores(int AdminId, string HitName = "0-FASHION")
        {
            return OperationHelper.Try((opera) =>
            {
                string _key = string.Format("170313_statistics_06_{0}_{1}", AdminId, HitName);
                var cdata = CacheHelper.GetCache(_key);
                if (cdata.IsNull())
                {
                    var rdata = _storeContract.QueryManageStore(AdminId).Select(s => new { StoreId = s.Id, StoreName = s.StoreName }).ToList();
                    if (rdata.Count == 0)
                    {
                        rdata.Insert(0, new { StoreId = 0, StoreName = HitName });
                    }

                    cdata = rdata;

                    AggregateCacheDependency deps = new AggregateCacheDependency();
                    deps.Add(new SqlCacheDependency(CacheAccess.DBName, "A_Department"));
                    deps.Add(new SqlCacheDependency(CacheAccess.DBName, "S_Store"));
                    deps.Add(new SqlCacheDependency(CacheAccess.DBName, "O_JobPosition"));

                    CacheHelper.SetCache(_key, rdata, deps, new TimeSpan(24, 0, 0));
                }

                return OperationHelper.ReturnOperationResult(true, opera, cdata);
            }, "获取掌管店铺列表");
        }
        /// <summary>
        /// 获取销售统计信息
        /// </summary>
        /// <returns></returns>
        public OperationResult GetSaleInfo(int AdminId, int StoreId, int? CategoryId, int FrontDay = 7)
        {
            return OperationHelper.Try((opera) =>
            {
                string _key = string.Format("170313_statistics_02_{0}_{1}_{2}_{3}", AdminId, StoreId, CategoryId, FrontDay);
                var rdata = CacheHelper.GetCache(_key);
                if (rdata.IsNull())
                {
                    var now = DateTime.Now;
                    var nowYear = DateTime.Parse(now.ToString("yyyy-01-01"));//今年
                    var NowDay = now.Date;//今天
                    var Day7 = NowDay.AddDays(-FrontDay);//昨天算第1天的前7天时间
                    var float0 = (decimal)(0);

                    #region 销售额

                    var retailAll = _retailRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled).Where(w => w.StoreId == StoreId && w.RetailState != RetailStatus.删除 && w.RetailState != RetailStatus.禁用);
                    if (CategoryId.HasValue)
                    {
                        retailAll = retailAll.Where(w => w.RetailItems.Count(ww => ww.Product.ProductOriginNumber.CategoryId == CategoryId) > 0);
                    }
                    var returnedItem = _returnedItemRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled);

                    var reailYearAll = retailAll.Where(w => w.CreatedTime > nowYear);
                    var retailYear = reailYearAll.Where(w => w.RetailState != RetailStatus.整单退货);//今年

                    var retailDay7All = retailAll.Where(w => w.CreatedTime >= Day7 && w.CreatedTime < NowDay).ToList();//前7天
                    var retailDay7 = retailDay7All.Where(w => w.RetailState != RetailStatus.整单退货);


                    #region 12个月

                    var retailYearMoney = (from retail in reailYearAll
                                           group retail by retail.CreatedTime.Month into g
                                           let reailyear = retailYear.Where(w => w.CreatedTime.Month == g.Key)
                                           let reailIds = g.Select(s => s.Id)
                                           let SaleCount = g.Sum(ss => ss.RetailItems.Count(c => !c.IsDeleted && c.IsEnabled))
                                           let returnedCount = returnedItem.Count(c => reailIds.Contains(c.RetailId.Value))
                                           let SaleRealCount = SaleCount - returnedCount
                                           let RealMoney = reailyear.Count() > 0 ? reailyear.Sum(ss => ss.CashConsume + ss.SwipeConsume + ss.RealStoredValueConsume) : float0
                                           let Money = g.Sum(ss => ss.CashConsume + ss.SwipeConsume + ss.RealStoredValueConsume)
                                           select new MonthMoney
                                           {
                                               Month = g.Key,
                                               Money = (float)Money,
                                               RealMoney = (float)RealMoney,
                                               SaleCount = SaleCount,
                                               SaleRealCount = SaleRealCount,
                                               Rate = SaleCount == 0 ? 0 : Math.Round((double)SaleRealCount / SaleCount * 100, 2)
                                           }).OrderBy(o => o.Month).ToList();

                    Enumerable.Range(1, 12).ToList().ForEach((month) =>
                    {
                        if (retailYearMoney.Count(c => c.Month == month) == 0)
                        {
                            retailYearMoney.Insert(month - 1, new MonthMoney()
                            {
                                Month = month,
                            });
                        }
                    });

                    #endregion

                    #region 前FrontDay天

                    var retailDay7Money = (from retail in retailDay7All
                                           group retail by retail.CreatedTime.Date into g
                                           let retailDay = retailDay7.Where(w => w.CreatedTime.Date == g.Key)
                                           let reailIds = g.Select(s => s.Id)
                                           let saleCount = g.Sum(ss => ss.RetailItems.Count(c => !c.IsDeleted && c.IsEnabled))
                                           let returnedCount = returnedItem.Count(c => reailIds.Contains(c.RetailId.Value))
                                           let saleRealCount = saleCount - returnedCount
                                           select new DayMoney
                                           {
                                               Date = g.Key,
                                               Money = (float)g.Sum(ss => ss.CashConsume + ss.SwipeConsume + ss.RealStoredValueConsume),
                                               RealMoney = (float)retailDay.Sum(ss => ss.CashConsume + ss.SwipeConsume + ss.RealStoredValueConsume),
                                               SaleCount = saleCount,
                                               SaleRealCount = saleRealCount,
                                               Rate = saleCount == 0 ? 0 : Math.Round((double)saleRealCount / saleCount * 100, 2)
                                           }).OrderBy(o => o.Date).ToList();


                    Enumerable.Range(0, FrontDay).ToList().ForEach((day) =>
                    {
                        var date = Day7.AddDays(day);
                        if (retailDay7Money.Count(c => c.Date == date) == 0)
                        {
                            retailDay7Money.Insert(day, new DayMoney
                            {
                                Date = date,
                            });
                        }
                    });

                    #endregion

                    #endregion

                    var yearSaleCount = retailYearMoney.Sum(s => s.SaleCount);
                    var daySaleCount = retailDay7Money.Sum(s => s.SaleCount);
                    rdata = new
                    {
                        SaleYear = retailYearMoney,
                        SaleYearRate = yearSaleCount != 0 ? Math.Round(retailYearMoney.Sum(s => s.SaleRealCount) / yearSaleCount * 100, 2) : 0,
                        SaleDay = retailDay7Money,
                        SaleDayRate = daySaleCount != 0 ? Math.Round(retailDay7Money.Sum(s => s.SaleRealCount) / daySaleCount * 100, 2) : 0,
                    };

                    CacheHelper.SetCache(_key, rdata, new TimeSpan(3, 0, 0));
                }

                return OperationHelper.ReturnOperationResult(true, opera, rdata);
            }, "获取销售统计信息");
        }
        /// <summary>
        /// 获取库存品类信息
        /// </summary>
        /// <param name="StoreId"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public OperationResult GetCategoryInfo(int AdminId, int StoreId, int? CategoryId)
        {
            return OperationHelper.Try((opera) =>
            {
                string _key = string.Format("170313_statistics_05_{0}_{1}_{2}", AdminId, StoreId, CategoryId);
                var cdata = CacheHelper.GetCache(_key);
                if (cdata.IsNull())
                {
                    var Inventories = PermissionHelper.ManagedStorages<Inventory>(AdminId, _adminContract, s => s.Inventories, w => w.StoreId == StoreId);

                    var categoryAll = (from inv in Inventories
                                       let cate = inv.Product.ProductOriginNumber.Category
                                       group inv by new { cate.Id, cate.ParentId, cate.CategoryName } into g
                                       select new
                                       {
                                           g.Key.Id,
                                           g.Key.ParentId,
                                           g.Key.CategoryName,
                                           Count = g.Count()
                                       }).ToList();
                    var category = _categoryRepository.Entities.Where(w => w.ParentId == null).Select(s => new { s.Id, s.CategoryName });

                    List<dynamic> rdata = new List<dynamic>();

                    if (CategoryId.HasValue)
                    {
                        category = category.Where(w => w.Id == CategoryId);
                    }

                    var cateIds = category.ToList();
                    var allCount = categoryAll.Sum(s => s.Count);
                    foreach (var item in cateIds)
                    {
                        var count = categoryAll.Where(w => w.Id == item.Id || w.ParentId == item.Id).Sum(s => s.Count);
                        rdata.Add(new
                        {
                            Id = item.Id,
                            CategoryName = item.CategoryName,
                            Count = count,
                            Rate = allCount > 0 ? Math.Round((double)count / allCount * 100.0, 2) : 0
                        });
                    }

                    cdata = rdata;

                    CacheHelper.SetCache(_key, rdata, new TimeSpan(0, 5, 0));
                }

                return OperationHelper.ReturnOperationResult(true, opera, cdata);
            }, "获取库存品类信息");
        }

        public OperationResult GetCategorySaleStatInfo(int adminId, int storeId)
        {
            string _key = string.Format("170313_statistics_06_{0}_{1}", adminId, storeId);
            var cacheData = CacheHelper.GetCache(_key);
            if (cacheData != null)
            {
                return new OperationResult(OperationResultType.Success, string.Empty, cacheData);
            }

            var storeIds = _storeContract.QueryManageStoreId(adminId);
            if (!storeIds.Contains(storeId) || storeId <= 0)
            {
                return OperationResult.Error("storeId无效");
            }

            var baseCategory = _categoryRepository.Entities.Where(c => !c.IsDeleted && c.IsEnabled && !c.ParentId.HasValue).Select(c => new { CategoryId = c.Id, CategoryName = c.CategoryName }).ToList();

            var query = _retailRepository.Entities.Where(r => !r.IsDeleted && r.IsEnabled)
                                                  .Where(r => r.StoreId == storeId);
            var endTime = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            var startTimeByWeek = DateTime.Now.Date.AddDays(-7);
            var startTimeByYear = new DateTime(DateTime.Now.Year, 1, 1);//1月1号

            //startTime =
            var weekCount = query.Where(r => r.CreatedTime >= startTimeByWeek && r.CreatedTime <= endTime)
                                    .SelectMany(r => r.RetailItems)
                                    .SelectMany(i => i.RetailInventorys)
                                    .Select(i => i.Inventory)
                                    .GroupBy(i => i.Product.ProductOriginNumber.Category.ParentId.Value)
                                    .Select(g => new CatetorySaleQuantity
                                    {
                                        CatetoryId = g.Key,
                                        RetailQuantity = g.Count()
                                    });
            var yearCount = query.Where(r => r.CreatedTime >= startTimeByYear && r.CreatedTime <= endTime)
                                    .SelectMany(r => r.RetailItems)
                                    .SelectMany(i => i.RetailInventorys)
                                    .Select(i => i.Inventory)
                                    .GroupBy(i => i.Product.ProductOriginNumber.Category.ParentId.Value)
                                    .Select(g => new CatetorySaleQuantity
                                    {
                                        CatetoryId = g.Key,
                                        RetailQuantity = g.Count()
                                    });
            var weekDict = new Dictionary<string, int>();
            var yearDict = new Dictionary<string, int>();
            foreach (var category in baseCategory)
            {
                if (!weekDict.ContainsKey(category.CategoryName))
                {
                    var entry = weekCount.FirstOrDefault(w => w.CatetoryId == category.CategoryId);
                    if (entry == null)
                    {
                        weekDict.Add(category.CategoryName, 0);
                    }
                    else
                    {
                        weekDict.Add(category.CategoryName, entry.RetailQuantity);
                    }
                }

                if (!yearDict.ContainsKey(category.CategoryName))
                {
                    var entry = yearCount.FirstOrDefault(w => w.CatetoryId == category.CategoryId);
                    if (entry == null)
                    {
                        yearDict.Add(category.CategoryName, 0);
                    }
                    else
                    {
                        yearDict.Add(category.CategoryName, entry.RetailQuantity);
                    }
                }
            }
            cacheData = new
            {
                WeekCount = weekDict,
                YearCount = yearDict
            };
            CacheHelper.SetCache(_key, cacheData, new TimeSpan(0, 5, 0));
            return new OperationResult(OperationResultType.Success, string.Empty, cacheData);

        }






        /// <summary>
        /// 获取库存信息
        /// </summary>
        /// <param name="StoreId"></param>
        /// <returns></returns>
        public OperationResult GetStorageInfo(int AdminId, int StoreId, int FrontDay = 7)
        {
            return OperationHelper.Try((opera) =>
            {
                string _key = string.Format("170313_statistics_03_{0}_{1}_{2}", AdminId, StoreId, FrontDay);
                var rdata = CacheHelper.GetCache(_key);
                if (rdata.IsNull())
                {
                    var listStorageIds = PermissionHelper.ManagedStorages(AdminId, _adminContract, s => s.Id, w => w.StoreId == StoreId);

                    var now = DateTime.Now;
                    var nowYear = DateTime.Parse(now.ToString("yyyy-01-01"));//今年
                    var NowDay = now.Date;//今天
                    var Day7 = NowDay.AddDays(-FrontDay);//昨天算第1天的前7天时间

                    var listPur = _purchaseRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => listStorageIds.Contains(w.StorageId.Value) || listStorageIds.Contains(w.ReceiverStorageId.Value));
                    var returnedItem = _returnedItemRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled);
                    var purYear = listPur.Where(w => w.CreatedTime > nowYear);//今年
                    var purDay = listPur.Where(w => w.CreatedTime >= Day7 && w.CreatedTime < NowDay).ToList();//前7天

                    #region 进货和配货

                    #region 12个月

                    var listPurYear = (from pur in purYear
                                       group pur by pur.CreatedTime.Month into g
                                       select new
                                       {
                                           Month = g.Key,
                                           PurchaseCount = g.Count(c => listStorageIds.Contains(c.StorageId.Value)),
                                           AllocateCount = g.Count(c => listStorageIds.Contains(c.ReceiverStorageId.Value)),
                                       }).ToList();

                    Enumerable.Range(1, 12).ToList().ForEach((month) =>
                    {
                        if (listPurYear.Count(c => c.Month == month) == 0)
                        {
                            listPurYear.Insert(month - 1, new
                            {
                                Month = month,
                                PurchaseCount = 0,
                                AllocateCount = 0,
                            });
                        }
                    });

                    #endregion

                    #region 前FrontDay天

                    var listPur7 = (from pur in purDay
                                    group pur by pur.CreatedTime.Date into g
                                    select new
                                    {
                                        Date = g.Key,
                                        Week = g.Key.ToString("dddd", new System.Globalization.CultureInfo("zh-CN")),
                                        PurchaseCount = g.Count(c => listStorageIds.Contains(c.StorageId.Value)),
                                        AllocateCount = g.Count(c => listStorageIds.Contains(c.ReceiverStorageId.Value)),
                                    }).ToList();

                    Enumerable.Range(0, FrontDay).ToList().ForEach((day) =>
                    {
                        var dayday = Day7.AddDays(day);
                        if (listPur7.Count(c => c.Date == dayday) == 0)
                        {
                            listPur7.Insert(day, new
                            {
                                Date = Day7.AddDays(day),
                                Week = Day7.AddDays(day).ToString("dddd", new System.Globalization.CultureInfo("zh-CN")),
                                PurchaseCount = 0,
                                AllocateCount = 0,
                            });
                        }
                    });

                    #endregion

                    #endregion

                    #region 退货

                    #region 12个月

                    var listOverYear = (from over in returnedItem.Where(w => w.CreatedTime > nowYear)
                                        group over by over.CreatedTime.Month into g
                                        select new
                                        {
                                            Month = g.Key,
                                            Count = g.Count(w => w.Retail.StoreId == StoreId)
                                        }).ToList();

                    Enumerable.Range(1, 12).ToList().ForEach((month) =>
                    {
                        if (listOverYear.Count(c => c.Month == month) == 0)
                        {
                            listOverYear.Insert(month - 1, new
                            {
                                Month = month,
                                Count = 0
                            });
                        }
                    });

                    #endregion

                    #region 前FrontDay天

                    var listOverYear7 = (from over in returnedItem.Where(w => w.CreatedTime >= Day7 && w.CreatedTime < NowDay).ToList()
                                         group over by over.CreatedTime.Date into g
                                         select new
                                         {
                                             Date = g.Key,
                                             Week = g.Key.ToString("dddd", new System.Globalization.CultureInfo("zh-CN")),
                                             Count = g.Count(w => w.Retail.StoreId == StoreId)
                                         }).ToList();

                    Enumerable.Range(0, FrontDay).ToList().ForEach((day) =>
                    {
                        var dayday = Day7.AddDays(day);
                        if (listOverYear7.Count(c => c.Date == dayday) == 0)
                        {
                            listOverYear7.Insert(day, new
                            {
                                Date = dayday,
                                Week = dayday.ToString("dddd", new System.Globalization.CultureInfo("zh-CN")),
                                Count = 0,
                            });
                        }
                    });

                    #endregion

                    #endregion

                    rdata = new
                    {
                        YearPurchaseAllocateCount = listPurYear,
                        DayPurchaseAllocateCount = listPur7,
                        YearReturnedCount = listOverYear,
                        DayReturnedCount = listOverYear7
                    };

                    CacheHelper.SetCache(_key, rdata, new TimeSpan(3, 0, 0));
                }

                return OperationHelper.ReturnOperationResult(true, opera, rdata);
            }, "获取库存信息");
        }

        public OperationResult GetStoreLocationInfo(int? StoreId, int TopCount = 3)
        {
            return OperationHelper.Try((opera) =>
            {
                var query = _storeRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted);
                var queryRetail = _retailRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled);
                var queryAdmin = _administratorRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled);
                var queryJobPosition = _jobPositionRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled);

                if (StoreId.HasValue)
                {
                    var selStore = (from s in query.Where(w => w.Id == StoreId)
                                    let star = queryRetail.Where(w => w.StoreId == s.Id).Select(ss => ss.RatePoints).ToList()
                                    let starLevel = star.Count > 0 ? (int)Math.Ceiling((double)star.Sum() / (star.Count * 5)) : 0
                                    let adminCount = queryAdmin.Count(c => c.DepartmentId == s.DepartmentId)
                                    let jobid = queryJobPosition.Where(w => w.DepartmentId == s.DepartmentId && w.IsLeader).Select(s => s.Id).FirstOrDefault()
                                    let modAdminLeader = queryAdmin.Where(w => w.JobPositionId == jobid).Select(x => new { x.Member.MemberName, x.Member.MobilePhone }).FirstOrDefault()
                                    select new
                                    {
                                        s.Id,
                                        s.StoreName,
                                        s.Longitude,
                                        s.Latitude,
                                        s.StorePhoto,
                                        StoreType = s.StoreType.TypeName,
                                        s.Telephone,
                                        s.IsClosed,
                                        ContactPerson = modAdminLeader != null ? modAdminLeader.MemberName : "",// s.ContactPerson,
                                        AdminCount = adminCount,
                                        StarLevel = starLevel
                                    }).FirstOrDefault();

                    return OperationHelper.ReturnOperationResult(selStore.IsNotNull(), "获取店铺信息", selStore);
                }
                else
                {
                    var listStores = (from s in query.Where(w => w.Longitude > 0 && w.Latitude > 0)
                                      let star = queryRetail.Where(w => w.StoreId == s.Id).Select(ss => ss.RatePoints).ToList()
                                      let starLevel = star.Count > 0 ? (int)Math.Ceiling((double)star.Sum() / (star.Count * 5)) : 0
                                      select new
                                      {
                                          s.Id,
                                          s.StoreName,
                                          s.Longitude,
                                          s.Latitude,
                                          s.IsClosed,
                                          StarLevel = starLevel,
                                      }).ToList();
                    var dateNow = DateTime.Now.Date;
                    var listTop = _openCloseRecordRepository.Entities.Where(w => !w.IsDeleted && w.IsEnabled).GroupBy(g => g.StoreId)
                                  .SelectMany(ss => ss.Where(w => w.OpenOrClose == OpenCloseFlag.Open && w.CreatedTime >= dateNow).OrderByDescending(o => o.CreatedTime).Skip(0).Take(1))
                                  .OrderBy(o => o.CreatedTime)
                                  .Select(ss => new
                                  {
                                      ss.StoreId,
                                      ss.Store.StoreName,
                                      ss.CreatedTime,
                                  }).Skip(0).Take(TopCount).ToList();
                    var rdata = new
                    {
                        Stores = listStores,
                        TopOpendStores = listTop
                    };
                    return OperationHelper.ReturnOperationResult(true, opera, rdata);
                }

            }, "获取店铺地图");
        }


        /// <summary>
        /// 获取统计options
        /// </summary>
        public OperationResult QueryOptions()
        {
            var dict = new Dictionary<string, object>();
            // 获取所有基础品类

            //Children = c.Children.Select(child=> new QueryOptionEntryCategory() { Id=child.Id,Name=child.CategoryName,Children=null}).ToList()
            var cateList = _categoryRepository.Entities.Where(c => !c.IsDeleted && c.IsEnabled)
                .Include(c => c.Children)
                .Include(c => c.Sizes)
                .Select(c => new { c.Id, c.ParentId, c.CategoryName, Sizes = c.Sizes.Select(s => new { s.Id, s.SizeName }).ToList() })
                .ToList();
            var parentCategoryList = cateList.Where(c => !c.ParentId.HasValue)
                .Select(c => new QueryOptionEntryCategory()
                {
                    Id = c.Id,
                    Name = c.CategoryName,
                    Children = new List<QueryOptionEntryCategory>()
                }).ToList();

            foreach (var parentItem in parentCategoryList)
            {
                parentItem.Children.AddRange(
                     cateList.Where(c => c.ParentId.HasValue && c.ParentId.Value == parentItem.Id)
                            .Select(c => new QueryOptionEntryCategory()
                            {
                                Id = c.Id,
                                Name = c.CategoryName,
                                Sizes = c.Sizes.Select(s => new QueryOptionEntry() { Id = s.Id, Name = s.SizeName }).ToList(),
                            }).ToList()
                    );

            }

            var brandList = _brandRepository.Entities.Where(b => !b.IsDeleted && b.IsEnabled && b.ParentId.HasValue).Select(b => new QueryOptionEntry { Id = b.Id, Name = b.BrandName }).ToList();
            var colorList = _colorRepo.Entities.Where(c => !c.IsDeleted && c.IsEnabled).Select(c => new QueryOptionEntry { Id = c.Id, Name = c.ColorName }).ToList();
            var seasonList = _seasonRepo.Entities.Where(c => !c.IsDeleted && c.IsEnabled).Select(c => new QueryOptionEntry { Id = c.Id, Name = c.SeasonName }).ToList();
            var sizeList = _sizeRepository.Entities.Where(c => !c.IsDeleted && c.IsEnabled).Select(c => new QueryOptionEntry { Id = c.Id, Name = c.SizeName }).ToList();
            dict.Add(QueryOptionEnum.Brand.ToString(), brandList);
            dict.Add(QueryOptionEnum.Category.ToString(), parentCategoryList);
            dict.Add(QueryOptionEnum.Color.ToString(), colorList);
            dict.Add(QueryOptionEnum.Season.ToString(), seasonList);
            //dict.Add(QueryOptionEnum.Size.ToString(), sizeList);
            return new OperationResult(OperationResultType.Success, string.Empty, dict);
        }



        /// <summary>
        /// 获取款号下可供查询的颜色和尺码
        /// </summary>
        public OperationResult QueryOptions(int? storeId, string bigProductNum)
        {
            // 参数校验
            if (!storeId.HasValue || storeId.Value <= 0)
            {
                return OperationResult.Error("storeId不能为空");
            }

            if (string.IsNullOrEmpty(bigProductNum) || bigProductNum.Length <= 0)
            {
                return OperationResult.Error("款号不能为空");
            }
            if (bigProductNum.Length > 7)
            {
                bigProductNum = bigProductNum.Substring(0, 7);
            }
            var query = _inventoryRepository.Entities.Where(p => !p.IsDeleted && p.IsEnabled)
                                                     .Where(p => p.Product.BigProdNum == bigProductNum && p.StoreId == storeId.Value);

            //查询款号下所有的颜色和尺码
            var colors = query.DistinctBy(p => p.Product.ColorId)
                                .Select(p => new QueryOptionEntry { Id = p.Product.ColorId, Name = p.Product.Color.ColorName })
                                .ToList();

            var sizes = query.DistinctBy(p => p.Product.SizeId)
                              .Select(p => new QueryOptionEntry { Id = p.Product.SizeId, Name = p.Product.Size.SizeName })
                              .ToList();
            var imgPath = query.Where(i => !string.IsNullOrEmpty(i.Product.ProductOriginNumber.ThumbnailPath)).Select(i => i.Product.ProductOriginNumber.ThumbnailPath).FirstOrDefault();
            var res = new BigProductNumQueryOptionsRes()
            {
                Color = colors,
                Size = sizes,
                StoreId = storeId.Value,
                BigProductNum = bigProductNum,
                ThumbnailPath = imgPath
            };
            return new OperationResult(OperationResultType.Success, string.Empty, res);
        }



        /// <summary>
        /// 销售统计
        /// </summary>
        public OperationResult SaleStat(SaleStatReq req)
        {
            // 参数校验
            var chkParam = ChkStatParam(req, _adminContract);
            if (!chkParam.Item1)
            {
                return OperationResult.Error(chkParam.Item2);
            }


            // 确定时间范围
            var chkRes = CalcStatRange(req.Days, req.StartDate, req.EndDate);

            if (!chkRes.Item1)
            {
                return OperationResult.Error(chkRes.Item2);
            }

            var startTime = chkRes.Item3;
            var endTime = chkRes.Item4;


            var res = new SaleStatRes()
            {
                StartTime = startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                EndTime = endTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
            };
            //统计总销售/退货数量

            switch (req.StatType.Value)
            {
                case StatTypeEnum.SaleStat:
                    {
                        var allStoreQuery = _retailItemRepository.Entities
                                                            .Where(i => !i.IsDeleted && i.IsEnabled)
                                                            .Where(i => i.CreatedTime >= startTime && i.CreatedTime <= endTime)
                                                            .SelectMany(i => i.RetailInventorys)
                                                            .Select(func);

                        if (req.BrandId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.BrandId == req.BrandId.Value);
                        }

                        if (req.CategoryId.HasValue)
                        {
                            var childCategories = _categoryRepository.Entities.Where(c => c.ParentId.Value == req.CategoryId.Value).Select(c => c.Id).ToList();
                            if (childCategories.Count > 0)
                            {
                                allStoreQuery = allStoreQuery.Where(q => childCategories.Contains(q.CategoryId));
                            }
                            else
                            {
                                allStoreQuery = allStoreQuery.Where(q => q.CategoryId == req.CategoryId.Value);
                            }
                        }

                        if (req.ColorId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.ColorId == req.ColorId);
                        }

                        if (req.SeasonId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.SeasonId == req.SeasonId.Value);
                        }

                        if (req.SizeId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.SizeId == req.SizeId.Value);
                        }

                        var currentStoreQuery = allStoreQuery.Where(i => i.StoreId == req.StoreId.Value);

                        res.CountFromAllStore = allStoreQuery.Count();
                        res.CountFromCurrentStore = currentStoreQuery.Count();

                        break;
                    }
                case StatTypeEnum.ReturnStat:
                    {
                        var allStoreQuery = _returnedItemRepository.Entities
                                                            .Where(i => !i.IsDeleted && i.IsEnabled)
                                                            .Where(i => i.CreatedTime >= startTime && i.CreatedTime <= endTime)
                                                            .Select(returnFunc);
                        if (req.BrandId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.BrandId == req.BrandId.Value);
                        }

                        if (req.CategoryId.HasValue)
                        {
                            var childCategories = _categoryRepository.Entities.Where(c => c.ParentId.Value == req.CategoryId.Value).Select(c => c.Id).ToList();
                            if (childCategories.Count > 0)
                            {
                                allStoreQuery = allStoreQuery.Where(q => childCategories.Contains(q.CategoryId));
                            }
                            else
                            {
                                allStoreQuery = allStoreQuery.Where(q => q.CategoryId == req.CategoryId.Value);
                            }
                        }

                        if (req.ColorId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.ColorId == req.ColorId);
                        }

                        if (req.SeasonId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.SeasonId == req.SeasonId.Value);
                        }
                        if (req.SizeId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.SizeId == req.SizeId.Value);
                        }
                        var currentStoreQuery = allStoreQuery.Where(i => i.StoreId == req.StoreId.Value);

                        res.CountFromAllStore = allStoreQuery.Count();
                        res.CountFromCurrentStore = currentStoreQuery.Count();
                        break;
                    }
                default:
                    break;
            }
            return new OperationResult(OperationResultType.Success, string.Empty, res);
        }





        /// <summary>
        /// 品牌统计
        /// </summary>
        public OperationResult BrandStat(BrandStatReq req)
        {
            // 参数校验
            var chkParam = ChkStatParam(req, _adminContract, false);
            if (!chkParam.Item1)
            {
                return OperationResult.Error(chkParam.Item2);
            }

            // 计算统计时间范围
            var chkRes = CalcStatRange(req.Days, req.StartDate, req.EndDate);

            if (!chkRes.Item1)
            {
                return OperationResult.Error(chkRes.Item2);
            }
            var startTime = chkRes.Item3;
            var endTime = chkRes.Item4;


            var res = new BrandStatRes()
            {
                StartTime = startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                EndTime = endTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 统计
            switch (req.StatType.Value)
            {
                case StatTypeEnum.SaleStat:
                    {
                        var filterQuery = _retailItemRepository.Entities
                                                            .Where(i => !i.IsDeleted && i.IsEnabled)
                                                            .Where(i => i.CreatedTime >= startTime && i.CreatedTime <= endTime)
                                                            .SelectMany(i => i.RetailInventorys)
                                                            .Select(func);

                        if (req.BrandId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.BrandId == req.BrandId.Value);
                        }

                        if (req.CategoryId.HasValue)
                        {
                            var childCategories = _categoryRepository.Entities.Where(c => c.ParentId.Value == req.CategoryId.Value).Select(c => c.Id).ToList();
                            if (childCategories.Count > 0)
                            {
                                filterQuery = filterQuery.Where(q => childCategories.Contains(q.CategoryId));

                            }
                            else
                            {
                                filterQuery = filterQuery.Where(q => q.CategoryId == req.CategoryId.Value);
                            }
                        }

                        if (req.ColorId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.ColorId == req.ColorId);
                        }

                        if (req.SeasonId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.SeasonId == req.SeasonId.Value);
                        }
                        if (req.SizeId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.SizeId == req.SizeId.Value);
                        }

                        //按品牌分组,根据品牌销量取前top条
                        if (req.StoreId.HasValue)
                        {
                            // 针对当前店铺查询品牌排行榜
                            var topBrands = filterQuery.Where(q => q.StoreId == req.StoreId.Value)
                                                        .GroupBy(i => i.BrandId)
                                                        .OrderByDescending(g => g.Count())
                                                        .Take(req.PageSize)
                                                        .Select(g => new BrandStatEntry
                                                        {
                                                            BrandId = g.Key,
                                                            BrandName = g.FirstOrDefault().BrandName,
                                                            Quantity = g.Count()
                                                        }).ToList();
                            res.TopBrands.AddRange(topBrands);
                        }
                        else
                        {
                            // 针对所有店铺查询品牌排行榜
                            var topBrands = filterQuery
                                .GroupBy(i => i.BrandId)
                                .OrderByDescending(g => g.Count())
                                .Take(req.PageSize)
                                .Select(g => new BrandStatEntry
                                {
                                    BrandId = g.Key,
                                    BrandName = g.FirstOrDefault().BrandName,
                                    Quantity = g.Count()
                                }).ToList();
                            res.TopBrands.AddRange(topBrands);
                        }

                        break;
                    }
                case StatTypeEnum.ReturnStat:
                    {
                        var filterQuery = _returnedItemRepository.Entities
                                                            .Where(i => !i.IsDeleted && i.IsEnabled)
                                                            .Where(i => i.CreatedTime >= startTime && i.CreatedTime <= endTime)
                                                            .Select(returnFunc);
                        if (req.BrandId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.BrandId == req.BrandId.Value);
                        }

                        if (req.CategoryId.HasValue)
                        {
                            var childCategories = _categoryRepository.Entities.Where(c => c.ParentId.Value == req.CategoryId.Value).Select(c => c.Id).ToList();
                            if (childCategories.Count > 0)
                            {
                                filterQuery = filterQuery.Where(q => childCategories.Contains(q.CategoryId));

                            }
                            else
                            {
                                filterQuery = filterQuery.Where(q => q.CategoryId == req.CategoryId.Value);
                            }
                        }

                        if (req.ColorId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.ColorId == req.ColorId);
                        }

                        if (req.SeasonId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.SeasonId == req.SeasonId.Value);
                        }

                        if (req.SizeId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.SizeId == req.SizeId.Value);
                        }


                        //按品牌分组,根据品牌销量取前top条
                        if (req.StoreId.HasValue)
                        {
                            // 针对当前店铺查询品牌排行榜
                            var topBrands = filterQuery.Where(q => q.StoreId == req.StoreId.Value)
                                                        .GroupBy(i => i.BrandId)
                                                        .OrderByDescending(g => g.Count())
                                                        .Take(req.PageSize)
                                                        .Select(g => new BrandStatEntry
                                                        {
                                                            BrandId = g.Key,
                                                            BrandName = g.FirstOrDefault().BrandName,
                                                            Quantity = g.Count()
                                                        }).ToList();
                            res.TopBrands.AddRange(topBrands);
                        }
                        else
                        {
                            // 针对所有店铺查询品牌排行榜
                            var topBrands = filterQuery.GroupBy(i => i.BrandId)
                                .OrderByDescending(g => g.Count())
                                .Take(req.PageSize)
                                .Select(g => new BrandStatEntry
                                {
                                    BrandId = g.Key,
                                    BrandName = g.FirstOrDefault().BrandName,
                                    Quantity = g.Count()
                                }).ToList();
                            res.TopBrands.AddRange(topBrands);
                        }

                        break;
                    }
                default:
                    break;
            }
            return new OperationResult(OperationResultType.Success, string.Empty, res);
        }

        /// <summary>
        /// 品类统计
        /// </summary>
        public OperationResult CategoryStat(SaleStatReq req)
        {
            // 参数校验
            var chkParam = ChkStatParam(req, _adminContract, false);
            if (!chkParam.Item1)
            {
                return OperationResult.Error(chkParam.Item2);
            }

            // 计算统计时间范围
            var chkRes = CalcStatRange(req.Days, req.StartDate, req.EndDate);

            if (!chkRes.Item1)
            {
                return OperationResult.Error(chkRes.Item2);
            }
            var startTime = chkRes.Item3;
            var endTime = chkRes.Item4;


            var res = new CategoryStatRes()
            {
                StartTime = startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                EndTime = endTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 统计
            switch (req.StatType.Value)
            {
                case StatTypeEnum.SaleStat:
                    {
                        var filterQuery = _retailItemRepository.Entities
                                                            .Where(i => !i.IsDeleted && i.IsEnabled)
                                                            .Where(i => i.CreatedTime >= startTime && i.CreatedTime <= endTime)
                                                            .SelectMany(i => i.RetailInventorys)
                                                            .Select(i => new
                                                            {
                                                                InventoryId = i.InventoryId,
                                                                StoreId = i.RetailItem.Retail.StoreId.Value,
                                                                BrandId = i.Inventory.Product.ProductOriginNumber.BrandId,
                                                                BaseCategoryId = i.Inventory.Product.ProductOriginNumber.Category.ParentId.Value,
                                                                BaseCategoryName = i.Inventory.Product.ProductOriginNumber.Category.Parent.CategoryName,
                                                                ColorId = i.Inventory.Product.ColorId,
                                                                SeasonId = i.Inventory.Product.ProductOriginNumber.SeasonId
                                                            });

                        if (req.BrandId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.BrandId == req.BrandId.Value);
                        }

                        if (req.CategoryId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.BaseCategoryId == req.CategoryId.Value);
                        }

                        if (req.ColorId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.ColorId == req.ColorId);
                        }

                        if (req.SeasonId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.SeasonId == req.SeasonId.Value);
                        }

                        //按品类分组,根据类销量取
                        if (req.StoreId.HasValue)
                        {
                            // 针对当前店铺查询品类排行榜
                            var topCategories = filterQuery.Where(q => q.StoreId == req.StoreId.Value)
                                                        .GroupBy(i => i.BaseCategoryId)
                                                        .OrderByDescending(g => g.Count())

                                                        .Select(g => new CategoryStatEntry
                                                        {
                                                            CategoryId = g.Key,
                                                            CategoryName = g.FirstOrDefault().BaseCategoryName,
                                                            Quantity = g.Count()
                                                        }).ToList();
                            res.TopCategories.AddRange(topCategories);
                        }
                        else
                        {
                            // 针对所有店铺查询品类排行榜
                            var topCategories = filterQuery
                                .GroupBy(i => i.BaseCategoryId)
                                .OrderByDescending(g => g.Count())
                                .Select(g => new CategoryStatEntry
                                {
                                    CategoryId = g.Key,
                                    CategoryName = g.FirstOrDefault().BaseCategoryName,
                                    Quantity = g.Count()
                                }).ToList();
                            res.TopCategories.AddRange(topCategories);
                        }

                        break;
                    }
                case StatTypeEnum.ReturnStat:
                    {
                        var filterQuery = _returnedItemRepository.Entities
                                                            .Where(i => !i.IsDeleted && i.IsEnabled)
                                                            .Where(i => i.CreatedTime >= startTime && i.CreatedTime <= endTime)
                                                            .Select(i => new
                                                            {
                                                                InventoryId = i.InventoryId,
                                                                StoreId = i.Returned.StoreId,
                                                                BrandId = i.Inventory.Product.ProductOriginNumber.BrandId,
                                                                BaseCategoryId = i.Inventory.Product.ProductOriginNumber.Category.ParentId.Value,
                                                                BaseCategoryName = i.Inventory.Product.ProductOriginNumber.Category.Parent.CategoryName,
                                                                ColorId = i.Inventory.Product.ColorId,
                                                                SeasonId = i.Inventory.Product.ProductOriginNumber.SeasonId
                                                            });
                        if (req.BrandId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.BrandId == req.BrandId.Value);
                        }

                        if (req.CategoryId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.BaseCategoryId == req.CategoryId.Value);
                        }

                        if (req.ColorId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.ColorId == req.ColorId);
                        }

                        if (req.SeasonId.HasValue)
                        {
                            filterQuery = filterQuery.Where(q => q.SeasonId == req.SeasonId.Value);
                        }

                        //按品牌分组,根据品牌销量取前top条
                        if (req.StoreId.HasValue)
                        {
                            // 针对当前店铺查询品牌排行榜
                            var topCategories = filterQuery.Where(q => q.StoreId == req.StoreId.Value)
                                                        .GroupBy(i => i.BaseCategoryId)
                                                        .OrderByDescending(g => g.Count())
                                                        .Select(g => new CategoryStatEntry
                                                        {
                                                            CategoryId = g.Key,
                                                            CategoryName = g.FirstOrDefault().BaseCategoryName,
                                                            Quantity = g.Count()
                                                        }).ToList();
                            res.TopCategories.AddRange(topCategories);
                        }
                        else
                        {
                            // 针对所有店铺查询品类排行榜
                            var topCategories = filterQuery
                                .GroupBy(i => i.BaseCategoryId)
                                .OrderByDescending(g => g.Count())
                                .Select(g => new CategoryStatEntry
                                {
                                    CategoryId = g.Key,
                                    CategoryName = g.FirstOrDefault().BaseCategoryName,
                                    Quantity = g.Count()
                                }).ToList();
                            res.TopCategories.AddRange(topCategories);
                        }

                        break;
                    }
                default:
                    break;
            }

            // 取出所有基础品类,用于对没有数据的品类补0
            var baseCategoryList = _categoryRepository.Entities.Where(c => !c.IsDeleted && c.IsEnabled && !c.ParentId.HasValue).Select(s => new CategoryStatEntry { CategoryId = s.Id, CategoryName = s.CategoryName, Quantity = 0 }).ToList();
            var missingCategoryIds = baseCategoryList.Select(c => c.CategoryId).Except(res.TopCategories.Select(c => c.CategoryId));
            res.TopCategories.AddRange(baseCategoryList.Where(c => missingCategoryIds.Contains(c.CategoryId)));
            return new OperationResult(OperationResultType.Success, string.Empty, res);
        }

        /// <summary>
        /// 款号统计
        /// </summary>
        public OperationResult BigProductNumStat(BigProductNumStatReq req)
        {
            // 参数校验
            var chkParam = ChkStatParam(req, _adminContract, true);
            if (!chkParam.Item1)
            {
                return OperationResult.Error(chkParam.Item2);
            }
            if (req.ColorId.HasValue && req.SizeId.HasValue)
            {
                return OperationResult.Error("颜色尺码不可同时查询");
            }

            if (req.BigProductNum.Length > 7)
            {
                req.BigProductNum = req.BigProductNum.Substring(0, 7);
            }

            // 计算统计时间范围
            var chkRes = CalcStatRange(req.Days, req.StartDate, req.EndDate);

            if (!chkRes.Item1)
            {
                return OperationResult.Error(chkRes.Item2);
            }
            var startTime = chkRes.Item3;
            var endTime = chkRes.Item4;


            var res = new BigProductNumStatRes()
            {
                StartTime = startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                EndTime = endTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // 统计
            switch (req.StatType.Value)
            {
                case StatTypeEnum.SaleStat:
                    {
                        var allStoreQuery = _retailItemRepository.Entities
                                                            .Where(i => !i.IsDeleted && i.IsEnabled)
                                                            .Where(i => i.CreatedTime >= startTime && i.CreatedTime <= endTime)
                                                            .Where(i => i.Product.BigProdNum == req.BigProductNum)
                                                            .SelectMany(i => i.RetailInventorys)
                                                            .Select(func);

                        if (req.BrandId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.BrandId == req.BrandId.Value);
                        }

                        if (req.CategoryId.HasValue)
                        {
                            var childCategories = _categoryRepository.Entities.Where(c => c.ParentId.Value == req.CategoryId.Value).Select(c => c.Id).ToList();
                            if (childCategories.Count > 0)
                            {
                                allStoreQuery = allStoreQuery.Where(q => childCategories.Contains(q.CategoryId));
                            }
                            else
                            {
                                allStoreQuery = allStoreQuery.Where(q => q.CategoryId == req.CategoryId.Value);
                            }
                        }

                        if (req.ColorId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.ColorId == req.ColorId);
                        }

                        if (req.SeasonId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.SeasonId == req.SeasonId.Value);
                        }

                        if (req.SizeId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.SizeId == req.SizeId.Value);
                        }
                        var currentStoreQuery = allStoreQuery.Where(i => i.StoreId == req.StoreId.Value);

                        var allStoreData = allStoreQuery.ToList();
                        var currentStoreData = currentStoreQuery.ToList();
                        // 根据店铺，款号获得要统计的尺码/颜色的范围
                        var queryRes = QueryOptions(req.StoreId.Value, req.BigProductNum).Data as BigProductNumQueryOptionsRes;

                        if (req.ColorId.HasValue)
                        {
                            // 返回该颜色下所有尺码的销售数量
                            foreach (var sizeItem in queryRes.Size)
                            {
                                //统计出每个尺码对应的全网和当前店铺的销售数量
                                var entryToAdd = new BigProductNumStatEntry()
                                {
                                    Id = sizeItem.Id,
                                    Name = sizeItem.Name
                                };
                                entryToAdd.CountFromAllStore = allStoreData.Count(q => q.SizeId == sizeItem.Id);
                                entryToAdd.CountFromCurrentStore = currentStoreData.Count(q => q.SizeId == sizeItem.Id);
                                res.Entries.Add(entryToAdd);
                            }

                        }
                        else // 返回该尺码下所有颜色的统计信息
                        {

                            foreach (var colorItem in queryRes.Color)
                            {
                                //统计出每个尺码对应的全网和当前店铺的销售数量
                                var entryToAdd = new BigProductNumStatEntry()
                                {
                                    Id = colorItem.Id,
                                    Name = colorItem.Name
                                };
                                entryToAdd.CountFromAllStore = allStoreQuery.Count(q => q.ColorId == colorItem.Id);
                                entryToAdd.CountFromCurrentStore = currentStoreQuery.Count(q => q.ColorId == colorItem.Id);
                                res.Entries.Add(entryToAdd);

                            }
                        }

                        break;
                    }
                case StatTypeEnum.ReturnStat:
                    {
                        var allStoreQuery = _returnedItemRepository.Entities
                                                            .Where(i => !i.IsDeleted && i.IsEnabled)
                                                            .Where(i => i.CreatedTime >= startTime && i.CreatedTime <= endTime)
                                                            .Where(i => i.Inventory.Product.BigProdNum == req.BigProductNum)
                                                            .Select(returnFunc);
                        if (req.BrandId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.BrandId == req.BrandId.Value);
                        }

                        if (req.CategoryId.HasValue)
                        {
                            var childCategories = _categoryRepository.Entities.Where(c => c.ParentId.Value == req.CategoryId.Value).Select(c => c.Id).ToList();
                            if (childCategories.Count > 0)
                            {
                                allStoreQuery = allStoreQuery.Where(q => childCategories.Contains(q.CategoryId));
                            }
                            else
                            {
                                allStoreQuery = allStoreQuery.Where(q => q.CategoryId == req.CategoryId.Value);
                            }
                        }

                        if (req.ColorId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.ColorId == req.ColorId);
                        }

                        if (req.SeasonId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.SeasonId == req.SeasonId.Value);
                        }

                        if (req.SizeId.HasValue)
                        {
                            allStoreQuery = allStoreQuery.Where(q => q.SizeId == req.SizeId.Value);
                        }
                        var currentStoreQuery = allStoreQuery.Where(i => i.StoreId == req.StoreId.Value);
                        var allStoreData = allStoreQuery.ToList();
                        var currentStoreData = currentStoreQuery.ToList();

                        // 根据店铺，款号获得要统计的尺码/颜色的范围
                        var queryRes = QueryOptions(req.StoreId.Value, req.BigProductNum).Data as BigProductNumQueryOptionsRes;

                        if (req.ColorId.HasValue)
                        {
                            // 返回该颜色下所有尺码的销售数量
                            foreach (var sizeItem in queryRes.Size)
                            {
                                //统计出每个尺码对应的全网和当前店铺的销售数量
                                var entryToAdd = new BigProductNumStatEntry()
                                {
                                    Id = sizeItem.Id,
                                    Name = sizeItem.Name
                                };
                                entryToAdd.CountFromAllStore = allStoreData.Count(q => q.SizeId == sizeItem.Id);
                                entryToAdd.CountFromCurrentStore = currentStoreData.Count(q => q.SizeId == sizeItem.Id);
                                res.Entries.Add(entryToAdd);
                            }

                        }
                        else // 返回该尺码下所有颜色的统计信息
                        {
                            foreach (var colorItem in queryRes.Color)
                            {
                                //统计出每个尺码对应的全网和当前店铺的销售数量
                                var entryToAdd = new BigProductNumStatEntry()
                                {
                                    Id = colorItem.Id,
                                    Name = colorItem.Name
                                };
                                entryToAdd.CountFromAllStore = allStoreData.Count(q => q.ColorId == colorItem.Id);
                                entryToAdd.CountFromCurrentStore = currentStoreData.Count(q => q.ColorId == colorItem.Id);
                                res.Entries.Add(entryToAdd);
                            }
                        }
                        break;
                    }
                default:
                    break;
            }
            return new OperationResult(OperationResultType.Success, string.Empty, res);
        }



        public OperationResult MemberStat(MemberStatReq req)
        {
            var chkParam = ChkMemberStatParam(req, _adminContract);
            if (!chkParam.Item1)
            {
                return OperationResult.Error(chkParam.Item2);
            }
            OperationResult res = null;
            switch (req.MemberStatType.Value)
            {
                case MemberStatTypeEnum.会员类型:
                    res = MemberTypeStat(req.StoreId.Value);
                    break;
                case MemberStatTypeEnum.会员热度:
                    break;
                case MemberStatTypeEnum.会员性别:
                    res = MemberGenderStat(req.StoreId.Value);
                    break;
                case MemberStatTypeEnum.尺码:
                    res = MemberSizeStat(req.StoreId.Value);
                    break;
                case MemberStatTypeEnum.偏好颜色:
                    {

                        res = MemberColorStat(req.StoreId.Value);
                        break;
                    }
                case MemberStatTypeEnum.身高:
                case MemberStatTypeEnum.体重:
                case MemberStatTypeEnum.肩宽:
                case MemberStatTypeEnum.胸围:
                case MemberStatTypeEnum.腰围:
                case MemberStatTypeEnum.臀围:
                    {
                        res = MemberFigureCommonStat(req);
                        break;
                    }
                case MemberStatTypeEnum.等级:
                    {
                        res = MemberLevelStat(req.StoreId.Value);
                        break;
                    }
                case MemberStatTypeEnum.体型:
                    {
                        res = MemberFigureStat(req.StoreId.Value);
                        break;
                    }
                default:
                    break;
            }
            return res;
        }

        private OperationResult MemberFigureStat(int storeId)
        {
            var allStoreQuery = _memberFigureRepository.Entities.Where(m => !m.IsDeleted && m.IsEnabled);
            var currentStoreQuery = allStoreQuery.Where(m => m.Member.StoreId.Value == storeId);


            var memberFigures = _memberFigureRepository.Entities.Where(t => !t.IsDeleted && t.IsEnabled).Where(t => !string.IsNullOrEmpty(t.FigureType))
                .Select(t => new BigProductNumStatEntry
                {
                    Id = 0,
                    Name = t.FigureType,
                    CountFromAllStore = 0,
                    CountFromCurrentStore = 0
                }).Distinct().ToList();

            foreach (var item in memberFigures)
            {
                item.CountFromAllStore = allStoreQuery.Count(m => m.FigureType == item.Name);
                item.CountFromCurrentStore = currentStoreQuery.Count(m => m.FigureType==item.Name);
            }
            return new OperationResult(OperationResultType.Success, string.Empty, memberFigures);
        }

        private OperationResult MemberTypeStat(int storeId)
        {
            var allStoreQuery = _memberRepository.Entities.Where(m => !m.IsDeleted && m.IsEnabled);
            var currentStoreQuery = allStoreQuery.Where(m => m.StoreId.Value == storeId);
            var MemberStatEntries = _memberTypeRepository.Entities.Where(t => !t.IsDeleted && t.IsEnabled)
                .Select(t => new MemberStatEntry { Id = t.Id, Name = t.MemberTypeName, CountFromAllStore = 0, CountFromCurrentStore = 0 })
                .ToList();
            foreach (var item in MemberStatEntries)
            {
                item.CountFromAllStore = allStoreQuery.Count(m => m.MemberTypeId == item.Id);
                item.CountFromCurrentStore = currentStoreQuery.Count(m => m.MemberTypeId == item.Id);



            }
            return new OperationResult(OperationResultType.Success, string.Empty, MemberStatEntries);
        }

        private OperationResult MemberLevelStat(int storeId)
        {
            var allStoreQuery = _memberRepository.Entities.Where(m => !m.IsDeleted && m.IsEnabled && m.LevelId.HasValue);
            var currentStoreQuery = allStoreQuery.Where(m => m.StoreId.Value == storeId);

            var allDict = allStoreQuery.GroupBy(m => m.LevelId.Value).ToDictionary(m => m.Key);
            var currentStoreDict = currentStoreQuery.GroupBy(m => m.LevelId.Value).ToDictionary(m => m.Key);


            var MemberStatEntries = _memberLevelRepository.Entities.Where(t => !t.IsDeleted && t.IsEnabled)
                .Select(t => new MemberStatEntry { Id = t.Id, Name = t.LevelName, CountFromAllStore = 0, CountFromCurrentStore = 0 })
                .ToList();
            foreach (var item in MemberStatEntries)
            {
                if (currentStoreDict.ContainsKey(item.Id))
                {
                    item.CountFromCurrentStore = currentStoreDict[item.Id].Count();
                }
                if (allDict.ContainsKey(item.Id))
                {
                    item.CountFromAllStore = allDict[item.Id].Count();
                }
            }
            return new OperationResult(OperationResultType.Success, string.Empty, MemberStatEntries);
        }

        private OperationResult MemberGenderStat(int storeId)
        {
            var allStoreQuery = _memberRepository.Entities.Where(m => !m.IsDeleted && m.IsEnabled);
            var currentStoreQuery = allStoreQuery.Where(m => m.StoreId.Value == storeId);
            var allDict = allStoreQuery.GroupBy(m => m.Gender).ToDictionary(m => m.Key);

            var currentStoreDict = currentStoreQuery.GroupBy(m => m.Gender).ToDictionary(m => m.Key);

            var res = new List<MemberStatEntry>();

            res.Add(new MemberStatEntry { Id = 0, Name = "女", CountFromAllStore = allStoreQuery.Count(m => m.Gender == 0), CountFromCurrentStore = currentStoreQuery.Count(m => m.Gender == 0) });
            res.Add(new MemberStatEntry { Id = 1, Name = "男", CountFromAllStore = allStoreQuery.Count(m => m.Gender == 1), CountFromCurrentStore = currentStoreQuery.Count(m => m.Gender == 1) });
            return new OperationResult(OperationResultType.Success, string.Empty, res);
        }

        private OperationResult MemberSizeStat(int storeId)
        {

            var allStoreQuery = GetMemberSizes();

            var sizeToGroup = new List<string>()
            {
                "S","M","L","XL","2XL"
            };

            // 初始化尺码分布人数字典
            var topSizeDict = sizeToGroup.ToDictionary(s => s, s => new MemberSizeStatEntry { Id = 0, Name = "上装", CountFromAllStore = 0, CountFromCurrentStore = 0 });
            var bottonmSizeDict = sizeToGroup.ToDictionary(s => s, s => new MemberSizeStatEntry { Id = 1, Name = "下装", CountFromAllStore = 0, CountFromCurrentStore = 0 });


            foreach (var item in topSizeDict)
            {
                item.Value.CountFromAllStore = allStoreQuery.Where(q => q.TopSize == item.Key).Count();
                item.Value.CountFromCurrentStore = allStoreQuery.Where(q => q.StoreId == storeId && q.TopSize == item.Key).Count();
            }

            foreach (var item in bottonmSizeDict)
            {
                item.Value.CountFromAllStore = allStoreQuery.Where(q => q.BottomSize == item.Key).Count();
                item.Value.CountFromCurrentStore = allStoreQuery.Where(q => q.StoreId == storeId && q.BottomSize == item.Key).Count();
            }

            return new OperationResult(OperationResultType.Success, string.Empty, new MemberSizeStatRes { StoreId = storeId, TopSizeDict = topSizeDict, BottomSizeDict = bottonmSizeDict });


        }


        private OperationResult MemberColorStat(int storeId)
        {

            var allStoreQuery = GetMemberColorPreference();

            var colorDict = allStoreQuery
                .SelectMany(c => c.PerferColors).Distinct()
                .ToDictionary(c => c, c => new MemberColorPreferenceStatEntry { Name = c });

            foreach (var item in colorDict)
            {
                item.Value.CountFromAllStore = allStoreQuery.Where(c => c.PerferColors.Contains(item.Key)).Count();
                item.Value.CountFromCurrentStore = allStoreQuery.Where(c => c.StoreId == storeId && c.PerferColors.Contains(item.Key)).Count();
            }
            return new OperationResult(OperationResultType.Success, string.Empty, new MemberColorPreferenceStatRes { StoreId = storeId, ColorPreference = colorDict });

        }

        private OperationResult MemberFigureCommonStat(MemberStatReq req)
        {
            var allStoreQuery = GetMemberFigureCommon();
            var res = new MemberFigureCommonStatRes() { StoreId = req.StoreId.Value, FigureStatType = req.MemberStatType.Value.ToString() };
            switch (req.MemberStatType.Value)
            {
                case MemberStatTypeEnum.会员类型:

                case MemberStatTypeEnum.会员热度:

                case MemberStatTypeEnum.会员性别:

                case MemberStatTypeEnum.尺码:

                case MemberStatTypeEnum.偏好颜色:
                    return OperationResult.Error("调用错误");

                case MemberStatTypeEnum.身高:
                    {

                        var HeightRanges = JsonHelper.FromJson<RangeEntry[]>(req.HeightRanges);
                        if (HeightRanges == null || HeightRanges.Length <= 0)
                        {
                            return OperationResult.Error("范围不能为空");
                        }

                        foreach (var item in HeightRanges)
                        {
                            res.StatEntries.Add(new MemberFigureCommonStatEntry()
                            {
                                MinValue = item.MinValue,
                                MaxValue = item.MaxValue,
                                CountFromAllStore = allStoreQuery.Count(f => f.Height >= item.MinValue && f.Height <= item.MaxValue),
                                CountFromCurrentStore = allStoreQuery.Count(f => f.StoreId == req.StoreId.Value && f.Height >= item.MinValue && f.Height <= item.MaxValue)
                            });
                        }

                    }
                    break;
                case MemberStatTypeEnum.体重:
                    {

                        var WeightRanges = JsonHelper.FromJson<RangeEntry[]>(req.WeightRanges);
                        if (WeightRanges == null || WeightRanges.Length <= 0)
                        {
                            return OperationResult.Error("范围不能为空");
                        }

                        foreach (var item in WeightRanges)
                        {
                            res.StatEntries.Add(new MemberFigureCommonStatEntry()
                            {
                                MinValue = item.MinValue,
                                MaxValue = item.MaxValue,
                                CountFromAllStore = allStoreQuery.Count(f => f.Weight >= item.MinValue && f.Weight <= item.MaxValue),
                                CountFromCurrentStore = allStoreQuery.Count(f => f.StoreId == req.StoreId.Value && f.Weight >= item.MinValue && f.Weight <= item.MaxValue)
                            });
                        }

                    }
                    break;
                case MemberStatTypeEnum.肩宽:
                    {

                        var shoulderRanges = JsonHelper.FromJson<RangeEntry[]>(req.ShoudlerRanges);
                        if (shoulderRanges == null || shoulderRanges.Length <= 0)
                        {
                            return OperationResult.Error("范围不能为空");
                        }

                        foreach (var item in shoulderRanges)
                        {
                            res.StatEntries.Add(new MemberFigureCommonStatEntry()
                            {
                                MinValue = item.MinValue,
                                MaxValue = item.MaxValue,
                                CountFromAllStore = allStoreQuery.Count(f => f.Shoulder >= item.MinValue && f.Shoulder <= item.MaxValue),
                                CountFromCurrentStore = allStoreQuery.Count(f => f.StoreId == req.StoreId.Value && f.Shoulder >= item.MinValue && f.Shoulder <= item.MaxValue)
                            });
                        }

                    }
                    break;
                case MemberStatTypeEnum.胸围:
                    {

                        var bustRanges = JsonHelper.FromJson<RangeEntry[]>(req.BustRanges);
                        if (bustRanges == null || bustRanges.Length <= 0)
                        {
                            return OperationResult.Error("范围不能为空");
                        }

                        foreach (var item in bustRanges)
                        {
                            res.StatEntries.Add(new MemberFigureCommonStatEntry()
                            {
                                MinValue = item.MinValue,
                                MaxValue = item.MaxValue,
                                CountFromAllStore = allStoreQuery.Count(f => f.Bust >= item.MinValue && f.Bust <= item.MaxValue),
                                CountFromCurrentStore = allStoreQuery.Count(f => f.StoreId == req.StoreId.Value && f.Bust >= item.MinValue && f.Bust <= item.MaxValue)
                            });
                        }

                    }
                    break;
                case MemberStatTypeEnum.腰围:
                    {

                        var waistRanges = JsonHelper.FromJson<RangeEntry[]>(req.WaistLineRanges);
                        if (waistRanges == null || waistRanges.Length <= 0)
                        {
                            return OperationResult.Error("范围不能为空");
                        }

                        foreach (var item in waistRanges)
                        {
                            res.StatEntries.Add(new MemberFigureCommonStatEntry()
                            {
                                MinValue = item.MinValue,
                                MaxValue = item.MaxValue,
                                CountFromAllStore = allStoreQuery.Count(f => f.WaistLine >= item.MinValue && f.WaistLine <= item.MaxValue),
                                CountFromCurrentStore = allStoreQuery.Count(f => f.StoreId == req.StoreId.Value && f.WaistLine >= item.MinValue && f.WaistLine <= item.MaxValue)
                            });
                        }

                    }
                    break;
                case MemberStatTypeEnum.臀围:
                    {

                        var hipRanges = JsonHelper.FromJson<RangeEntry[]>(req.HipRanges);
                        if (hipRanges == null || hipRanges.Length <= 0)
                        {
                            return OperationResult.Error("范围不能为空");
                        }

                        foreach (var item in hipRanges)
                        {
                            res.StatEntries.Add(new MemberFigureCommonStatEntry()
                            {
                                MinValue = item.MinValue,
                                MaxValue = item.MaxValue,
                                CountFromAllStore = allStoreQuery.Count(f => f.Hip >= item.MinValue && f.Hip <= item.MaxValue),
                                CountFromCurrentStore = allStoreQuery.Count(f => f.StoreId == req.StoreId.Value && f.Hip >= item.MinValue && f.Hip <= item.MaxValue)
                            });
                        }

                    }
                    break;
                default:
                    return OperationResult.Error("调用错误");
            }
            return new OperationResult(OperationResultType.Success, string.Empty, res);
        }




        private List<MemberSizeEntry> GetMemberSizes()
        {

            var data = _memberFigureRepository.Entities
                        .Where(m => !m.IsDeleted && m.IsEnabled)
                        .Where(m => m.Member.StoreId.HasValue) //过滤无归属店铺的数据
                        .Where(f => !string.IsNullOrEmpty(f.ApparelSize) && f.ApparelSize.Length > 1)  //过滤无效数据
                        .Select(f => new { f.MemberId, ApparelSize = f.ApparelSize, StoreId = f.Member.StoreId.Value })
                        .ToList();

            // 将size分解为两部分
            var list = new List<MemberSizeEntry>();
            foreach (var item in data)
            {
                var sizeArr = item.ApparelSize.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                list.Add(new MemberSizeEntry()
                {
                    MemberId = item.MemberId,
                    StoreId = item.StoreId,
                    TopSize = sizeArr.Length > 0 ? sizeArr[0] : string.Empty,
                    BottomSize = sizeArr.Length > 1 ? sizeArr[1] : string.Empty
                });
            }

            return list;

        }

        private List<MemberColorPreferenceEntry> GetMemberColorPreference()
        {

            var data = _memberFigureRepository.Entities
                        .Where(m => !m.IsDeleted && m.IsEnabled)
                        .Where(m => m.Member.StoreId.HasValue) //过滤无归属店铺的数据
                        .Where(f => !string.IsNullOrEmpty(f.PreferenceColor) && f.PreferenceColor.Length > 1)  //过滤无效数据
                        .Select(f => new { f.MemberId, PreferenceColor = f.PreferenceColor, StoreId = f.Member.StoreId.Value })
                        .ToList();

            // 将size分解为两部分
            var list = new List<MemberColorPreferenceEntry>();
            foreach (var item in data)
            {
                var sizeArr = item.PreferenceColor.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                list.Add(new MemberColorPreferenceEntry()
                {
                    MemberId = item.MemberId,
                    StoreId = item.StoreId,
                    PerferColors = sizeArr.ToList()
                });
            }

            return list;

        }

        private List<MemberFigureCommonEntry> GetMemberFigureCommon()
        {

            var groupQuery = _memberFigureRepository.Entities
                        .Where(m => !m.IsDeleted && m.IsEnabled)
                        .Where(m => m.Member.StoreId.HasValue) //过滤无归属店铺的数据
                        .GroupBy(m => m.MemberId);


            var data = from g in groupQuery
                       let memberLastUpdateEntry = g.OrderByDescending(f => f.UpdatedTime).FirstOrDefault()  //每个会员最新的数据
                       select new MemberFigureCommonEntry
                       {
                           MemberId = memberLastUpdateEntry.MemberId,
                           StoreId = memberLastUpdateEntry.Member.StoreId.Value,
                           Height = memberLastUpdateEntry.Height,
                           Weight = memberLastUpdateEntry.Weight,
                           Shoulder = memberLastUpdateEntry.Shoulder,
                           Bust = memberLastUpdateEntry.Bust,
                           WaistLine = memberLastUpdateEntry.Waistline,
                           Hip = memberLastUpdateEntry.Hips
                       };

            return data.ToList();
        }

        private class MemberSizeEntry
        {
            public int MemberId { get; set; }

            public int StoreId { get; set; }

            /// <summary>
            /// 上装尺码
            /// </summary>
            public string TopSize { get; set; }

            /// <summary>
            /// 下装尺码
            /// </summary>
            public string BottomSize { get; set; }
        }

        private class MemberFigureCommonEntry
        {
            public int MemberId { get; set; }

            public int StoreId { get; set; }

            public int Height { get; set; }
            public int Weight { get; set; }
            public int Shoulder { get; set; }
            public int Bust { get; set; }
            public int WaistLine { get; set; }
            public int Hip { get; set; }
        }

        /// <summary>
        /// 会员颜色偏好
        /// </summary>
        private class MemberColorPreferenceEntry
        {
            public MemberColorPreferenceEntry()
            {
                PerferColors = new List<string>();
            }
            public int MemberId { get; set; }

            public int StoreId { get; set; }

            public List<string> PerferColors { get; set; }
        }


    }


    /// <summary>
    /// 统计项
    /// </summary>
    public enum QueryOptionEnum
    {
        Brand = 0,
        Color = 1,
        Category = 2,
        Season = 3,
        Size = 4
    }

    public class CatetorySaleQuantity
    {
        public int CatetoryId { get; set; }
        public int RetailQuantity { get; set; }
    }
    public class RetailStatEntry
    {

        public int InventoryId { get; set; }
        public int StoreId { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
        public int CategoryId { get; set; }
        public int ColorId { get; set; }
        public string ColorName { get; set; }
        public int SeasonId { get; set; }
        public int SizeId { get; set; }
        public string SizeName { get; set; }
    }
    public class QueryOptionEntry
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class QueryOptionEntryCategory : QueryOptionEntry
    {
        public QueryOptionEntryCategory()
        {
            Sizes = new List<QueryOptionEntry>();
            Children = new List<QueryOptionEntryCategory>();
        }
        /// <summary>
        /// 品类下的子分类
        /// </summary>
        public List<QueryOptionEntryCategory> Children { get; set; }

        /// <summary>
        /// 品类下的所有尺码
        /// </summary>
        public List<QueryOptionEntry> Sizes { get; set; }
    }
    public class BigProductNumQueryOptionsRes
    {
        public BigProductNumQueryOptionsRes()
        {
            Color = new List<QueryOptionEntry>();
            Size = new List<QueryOptionEntry>();
        }
        public int StoreId { get; set; }
        public string BigProductNum { get; set; }
        public string ThumbnailPath { get; set; }
        public List<QueryOptionEntry> Color { get; set; }
        public List<QueryOptionEntry> Size { get; set; }
    }


    public class MonthMoney
    {
        public int Month { get; set; }
        public float Money { get; set; }
        public float RealMoney { get; set; }
        public double SaleCount { get; set; }
        public double SaleRealCount { get; set; }
        public double Rate { get; set; }
    }

    public class DayMoney
    {
        public DateTime Date { get; set; }
        public float Money { get; set; }
        public float RealMoney { get; set; }
        public string Week
        {
            get
            {
                return Date.ToString("dddd", new System.Globalization.CultureInfo("zh-CN"));
            }
        }
        public double SaleCount { get; set; }
        public double SaleRealCount { get; set; }
        public double Rate { get; set; }
    }
}

