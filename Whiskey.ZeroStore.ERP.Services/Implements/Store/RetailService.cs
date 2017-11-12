using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Models;
using System.Web;
using System.Web.Security;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class RetailService : ServiceBase, IRetailContract
    {
        private readonly IRepository<Retail, int> _retailRepository;
        private readonly IMemberContract _memberContract;
        protected readonly ISalesCampaignContract _salesCampaignContract;
        private readonly ICouponContract _couponContract;
        protected readonly IStoreActivityContract _storeActivityContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IRepository<Inventory, int> _inventoryRepository;
        protected readonly IProductContract _productContract;


        public RetailService(IRepository<Retail, int> retailRepository,
             IMemberContract memberContract,
             ISalesCampaignContract salesCampaignContract,
             IStoreActivityContract storeActivityContract,
             ICouponContract couponContract,
             IStoreContract storeContract, IRepository<Inventory, int> inventoryRepository,
             IProductContract productContract) : base(retailRepository.UnitOfWork)
        {
            _retailRepository = retailRepository;
            _memberContract = memberContract;
            _salesCampaignContract = salesCampaignContract;
            _couponContract = couponContract;
            _storeActivityContract = storeActivityContract;
            _storeContract = storeContract;
            _inventoryRepository = inventoryRepository;
            _productContract = productContract;
        }
        public IQueryable<Retail> Retails
        {
            get { return _retailRepository.Entities; }
        }

        public Utility.Data.OperationResult Insert(List<Retail> retails)
        {
            int res = _retailRepository.Insert((IEnumerable<Retail>)retails);
            return res > 0
                ? new OperationResult(OperationResultType.Success)
                : new OperationResult(OperationResultType.Error);
        }


        public OperationResult Insert(bool isTrans = false, params Retail[] retails)
        {
            //throw new NotImplementedException();
            _retailRepository.UnitOfWork.TransactionEnabled = isTrans;
            int res = _retailRepository.Insert((IEnumerable<Retail>)retails);
            return res > 0
                ? new OperationResult(OperationResultType.Success)
                : new OperationResult(OperationResultType.Error);
        }


        public OperationResult Update(params Retail[] retails)
        {
            retails.Each(c =>
            {
                c.UpdatedTime = DateTime.Now;
                c.OperatorId = AuthorityHelper.OperatorId;
            });
            return _retailRepository.Update((ICollection<Retail>)retails);
        }


        public OperationResult Remove(int id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error, "数据不存在");
            var ent = _retailRepository.Entities.FirstOrDefault(c => c.Id == id);
            if (ent != null)
            {
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsDeleted = true;
                ent.Note = "删除销售单";
                //ent.RetailState = 3;
                ent.RetailItems.Each(c =>
                {
                    c.IsDeleted = true;
                    //c.RetailItemState = 3;
                });

                resul.ResultType = _retailRepository.Update(ent) > 0 ? OperationResultType.Success : OperationResultType.Error;
            }
            return resul;
        }

        public OperationResult Recovery(int id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error, "");
            var ent = _retailRepository.Entities.FirstOrDefault(c => c.Id == id);
            if (ent != null)
            {
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsDeleted = false;
                ent.Note = "从删除状态恢复销售单";
                ent.RetailState = 0;
                ent.RetailItems.Each(c =>
                {
                    c.IsDeleted = false;
                    c.RetailItemState = 0;
                });
                resul.ResultType = _retailRepository.Update(ent) > 0 ? OperationResultType.Success : OperationResultType.Error;
            }
            return resul;
        }

        public OperationResult Enable(int id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error, "数据不存在");
            var ent = _retailRepository.Entities.FirstOrDefault(c => c.Id == id);
            if (ent != null)
            {
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsEnabled = true;
                ent.Note = "从禁用状态启用销售单";
                ent.RetailState = 0;
                ent.RetailItems.Each(c =>
                {
                    c.IsEnabled = true;
                    c.RetailItemState = 0;
                });
                resul.ResultType = _retailRepository.Update(ent) > 0 ? OperationResultType.Success : OperationResultType.Error;
            }
            return resul;
        }

        public OperationResult Disable(int id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error, "数据不存在");
            var ent = _retailRepository.Entities.FirstOrDefault(c => c.Id == id);
            if (ent != null)
            {
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsEnabled = false;
                ent.Note = "禁用销售单";
                //ent.RetailState = 4;
                ent.RetailItems.Each(c =>
                {
                    c.IsEnabled = false;
                    //c.RetailItemState = 4;
                });
                resul.ResultType = _retailRepository.Update(ent) > 0 ? OperationResultType.Success : OperationResultType.Error;
            }
            return resul;
        }

        public OperationResult Delete(int id)
        {
            throw new NotImplementedException();
        }

        public DbContextTransaction GetTransaction()
        {
            return _retailRepository.GetTransaction();
        }



        public bool ClearMemberDTOInfo(int memberId)
        {
            var key = RedisCacheHelper.KEY_MEMBER_RETAIL_LOGIN_PREFIX + memberId.ToString();
            return RedisCacheHelper.Remove(key);
        }


        public MemberLoginPassDTO GetMemberInfo(int memberId)
        {
            var key = RedisCacheHelper.KEY_MEMBER_RETAIL_LOGIN_PREFIX + memberId.ToString();
            return RedisCacheHelper.Get<MemberLoginPassDTO>(key);
        }


        /// <summary>
        /// 获取会员可用的优惠活动(等级折扣,店铺活动,优惠券)
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="memberId">会员id</param>
        /// <param name="saleCampIds">已使用的商品活动</param>
        /// <returns></returns>
        public OperationResult GetEnableCoupon(int storeId, string memberCard, params int[] saleCampIds)
        {
            var canUseCoupon = true; //是否允许使用优惠券
            var canUseStoreActivity = true;//是否允许使用店铺活动
            var memberLevelDiscount = 1.0M;

            var couponList = new List<MemberCoupon>();
            var storeActivityList = new List<StoreActivityEntry>();

            if (storeId <= 0)
            {
                throw new Exception("参数错误");
            }

            // 如果参与了商品活动,判断商品活动中是否包含禁用优惠券和其他活动的活动
            if (saleCampIds != null && saleCampIds.Length > 0)
            {

                var campQuery = _salesCampaignContract.SalesCampaigns.Where(camp => saleCampIds.Contains(camp.Id));
                if (campQuery.Any(c => !c.OtherCampaign))
                {
                    canUseStoreActivity = false;
                }
                if (campQuery.Any(c => !c.OtherCashCoupon))
                {
                    canUseCoupon = false;
                }
            }


            if (string.IsNullOrEmpty(memberCard))  // 非会员
            {
                // 不需要查询优惠券
                var activities = GetStoreActivities(storeId, null, null);
                storeActivityList.AddRange(activities);


                return new OperationResult(OperationResultType.Success, string.Empty, new GetEnableCouponRes
                {
                    LevelDiscount = memberLevelDiscount,
                    Coupons = couponList,
                    StoreActivities = storeActivityList
                });
            }
            else  //会员
            {
                // 会员折扣查询
                var memberProfile = _memberContract.Members.Where(m => !m.IsDeleted && m.IsEnabled && m.UniquelyIdentifies == memberCard)
                                                            .Select(m => new
                                                            {
                                                                Id = m.Id,
                                                                Discount = m.LevelId.HasValue ? m.MemberLevel.Discount : 1,
                                                                MemberTypeId = m.MemberTypeId
                                                            }).FirstOrDefault();

                if (memberProfile == null)
                {
                    throw new Exception("会员信息未找到");
                }
                if (memberProfile.Discount > 0 && memberProfile.Discount <= 1)
                {
                    memberLevelDiscount = (decimal)memberProfile.Discount;
                }


                // 可用优惠券查询
                if (canUseCoupon)
                {
                    var time = DateTime.Now;
                    var coupons = _couponContract.CouponItems
                                                            .Where(c => c.IsEnabled && !c.IsDeleted)
                                                            .Where(c => c.MemberId == memberProfile.Id && !c.IsUsed)
                                                            .Where(c => c.Coupon.IsForever || (c.Coupon.StartDate <= time && time <= c.Coupon.EndDate))
                                                            .Select(c => new MemberCoupon
                                                            {
                                                                Id = c.Id,
                                                                CouponNumb = c.CouponNumber,
                                                                CouponName = c.Coupon.CouponName,
                                                                DiscountAmount = c.Coupon.CouponPrice
                                                            })
                                                            .ToList();
                    couponList.AddRange(coupons);
                }


                // 店铺活动查询
                if (canUseStoreActivity)
                {
                    var activities = GetStoreActivities(storeId, memberProfile.Id, memberProfile.MemberTypeId);
                    storeActivityList.AddRange(activities);
                }

                return new OperationResult(OperationResultType.Success, string.Empty, new GetEnableCouponRes
                {
                    LevelDiscount = memberLevelDiscount,
                    Coupons = couponList,
                    StoreActivities = storeActivityList
                });
            }







        }


        private StoreActivityEntry[] GetStoreActivities(int storeId, int? memberId, int? memberTypeId)
        {
            var activities = _storeActivityContract.StoreActivities.Where(s => !s.IsDeleted && s.IsEnabled)
                .Where(s => s.StoreIds.Contains(storeId.ToString()))
                .Where(s => s.StartDate <= DateTime.Now && s.EndDate > DateTime.Now)
                .ToList();
            if (!memberId.HasValue)
            {
                activities = activities.Where(a => a.MemberTypes.Contains("-1")).ToList();
            }
            else
            {
                if (!memberTypeId.HasValue)
                {
                    throw new Exception("memberTypeId不能为空");
                }
                // 会员类型筛选
                activities = activities.Where(a => a.MemberTypes.Contains(memberTypeId.ToString())).ToList();


                // 获取之前已参与过的不可重复参与的活动id
                var historyActivitiesFromOrder = _retailRepository.Entities.Where(r => r.StoreActivityId.HasValue
                                                                                    && r.StoreActivity.OnlyOncePerMember.Value == true
                                                                                    && r.ConsumerId.Value == memberId)
                                                                        .Select(r => r.StoreActivityId.Value)
                                                                        .ToList();
                // 过滤掉已参与的有限制次数的活动
                activities.RemoveAll(a => historyActivitiesFromOrder.Contains(a.Id));

            }


            var res = activities.Select(s => new StoreActivityEntry
            {
                ActivityId = s.Id,
                ActivityName = s.ActivityName,
                MinConsume = s.MinConsume,
                DiscountMoney = s.DiscountMoney
            }).ToArray();
            return res;
        }


        /// <summary>
        /// 获取商品信息
        /// </summary>
        /// <param name="storeId">店铺Id</param>
        /// <param name="adminId">操作人</param>
        /// <param name="isMember">是否会员</param>
        /// <param name="barcodes">流水号</param>
        /// <returns></returns>
        public OperationResult GetProductsInfo(int storeId, int adminId, string memberCard, string[] barcodes, bool isFirstQuery, List<CustomSaleCampsEntry> selectedSaleCamps)
        {
            // 店铺权限
            _storeContract.CheckStoreId(storeId, adminId);

            // 会员校验
            var isMember = false;
            if (!string.IsNullOrEmpty(memberCard) && memberCard.Length > 0)
            {
                if (!_memberContract.CheckExists(m => !m.IsDeleted && m.IsEnabled && m.UniquelyIdentifies == memberCard))
                {
                    throw new Exception("会员不存在");
                }
                else
                {
                    isMember = true;
                }

            }


            var salesCampaignType = isMember ? SalesCampaignType.MemberOnly : SalesCampaignType.NoMemberOnly;


            // barocdes-productnumber字典

            var lookup = _inventoryRepository.Entities.Where(i => !i.IsDeleted && i.IsEnabled
                                                                && i.StoreId == storeId
                                                                && barcodes.Contains(i.ProductBarcode))
                                          .ToLookup(i => i.ProductNumber, i => i.ProductBarcode);

            var data = _inventoryRepository.Entities.Where(i => !i.IsDeleted && i.IsEnabled
                                                                && i.StoreId == storeId
                                                                && barcodes.Contains(i.ProductBarcode))
                .GroupBy(i => i.ProductNumber)
                .Select(g => new ItemEntry
                {
                    ThumbnailPath = g.FirstOrDefault().Product.ThumbnailPath ?? g.FirstOrDefault().Product.ProductOriginNumber.ThumbnailPath,
                    ProductNumber = g.FirstOrDefault().ProductNumber,
                    BrandName = g.FirstOrDefault().Product.ProductOriginNumber.Brand.BrandName,
                    CategoryName = g.FirstOrDefault().Product.ProductOriginNumber.Category.CategoryName,
                    TagPrice = g.FirstOrDefault().Product.ProductOriginNumber.TagPrice,
                    SizeName = g.FirstOrDefault().Product.Size.SizeName,
                    Quantity = g.Count(),
                    BrandDiscount = g.FirstOrDefault().Product.ProductOriginNumber.Brand.DefaultDiscount, // 品牌折扣
                    SaleCampaigns = g.FirstOrDefault().Product.ProductOriginNumber.SalesCampaigns         // 商品活动折扣
                                    .Where(s => !s.IsDeleted && s.IsEnabled)
                                    .Where(s => s.StoresIds.Contains(storeId.ToString()))
                                    .Where(s => s.CampaignStartTime <= DateTime.Now && s.CampaignEndTime >= DateTime.Now)
                                    .Where(s => s.SalesCampaignType == SalesCampaignType.EveryOne || s.SalesCampaignType == salesCampaignType)
                                    .Select(s => new SaleCampEntry
                                    {
                                        Id = s.Id,
                                        CampaignName = s.CampaignName,
                                        MemberDiscount = (float)(s.MemberDiscount / 10.0),
                                        NoMmebDiscount = (float)(s.NoMmebDiscount / 10.0),
                                        OtherCampaign = s.OtherCampaign,
                                        OtherCashCoupon = s.OtherCashCoupon
                                    }),
                    Barcodes = g.Select(i=>i.ProductBarcode)
                }).ToList();
           
            if (data.Any(g => g.BrandDiscount < 0.1 || g.BrandDiscount > 1))
            {
                throw new Exception($"商品品牌折扣异常,货号:{data.Where(g => g.BrandDiscount < 1).Select(g => g.ProductNumber)}");
            }
            var productSaleCampaigns = data.ToDictionary(d => d.ProductNumber, d => d.SaleCampaigns);

            var res = new GetProductInfoRes() { IsFirstQuery = isFirstQuery, IsMember = isMember };
            var customSelects = selectedSaleCamps.ToDictionary(c => c.ProductNumber, c => c.SaleCampId);
            foreach (var item in data)
            {
                var entry = new ProductEntry
                {
                    ThumbnailPath = string.IsNullOrEmpty(item.ThumbnailPath) ? string.Empty : ConfigurationHelper.WebUrl + item.ThumbnailPath,
                    BrandName = item.BrandName,
                    CategoryName = item.CategoryName,
                    ProductNumber = item.ProductNumber,
                    Quantity = (uint)item.Quantity,
                    Barcodes = item.Barcodes.ToArray(),
                    SizeName = item.SizeName,
                    TagPrice = (decimal)item.TagPrice,
                };

                if (!item.SaleCampaigns.Any())// 没有的话使用默认的品牌折扣
                {
                    ApplyBrandDiscount(entry, (decimal)item.BrandDiscount);
                }
                else
                {
                    if (isFirstQuery) // 由系统选择
                    {
                        var saleCamp = item.SaleCampaigns.OrderBy(s => s.NoMmebDiscount).FirstOrDefault();
                        var discount = isMember ? saleCamp.MemberDiscount : saleCamp.NoMmebDiscount;
                        saleCamp.IsSelected = true;
                        ApplySaleCampDiscount(entry, (decimal)discount, item.SaleCampaigns.ToList());
                    }
                    else  // 由用户选择
                    {
                        ApplyUserSelectSaleCamp(isMember, item, entry, productSaleCampaigns, customSelects);

                    }
                }

                if (entry.RetailPrice < 0 || entry.RetailPrice > entry.TagPrice)
                {
                    throw new Exception($"零售价格异常,商品货号:{entry.ProductNumber},吊牌价:{entry.TagPrice},零售价:{entry.RetailPrice}");
                }

                res.Products.Add(entry);

            }
            return new OperationResult(OperationResultType.Success, string.Empty, res);
        }


        private void ApplyUserSelectSaleCamp(bool isMember, ItemEntry item, ProductEntry entry, Dictionary<string, IEnumerable<SaleCampEntry>> productAllSaleCamps, Dictionary<string, int?> customSaleCamps)
        {
            if (customSaleCamps == null)
            {
                throw new Exception($"商品活动参数错误,customSaleCamps不能为null");
            }
            if (!customSaleCamps.ContainsKey(item.ProductNumber)) // 放弃商品活动,使用默认的品牌折扣
            {
                ApplyBrandDiscount(entry, (decimal)item.BrandDiscount);
                return;
            }

            //  校验商品活动是否存在
            if (productAllSaleCamps == null || !productAllSaleCamps.ContainsKey(item.ProductNumber))
            {
                throw new Exception($"商品活动不存在,货号:{item.ProductNumber},活动id:{customSaleCamps[item.ProductNumber].ToString()}");
            }


            var campId = customSaleCamps[item.ProductNumber];
            if (!campId.HasValue)
            {
                ApplyBrandDiscount(entry, (decimal)item.BrandDiscount);
                return;
            }

            var saleCampInfo = productAllSaleCamps[item.ProductNumber].FirstOrDefault(s => s.Id == campId);
            if (saleCampInfo == null)
            {
                throw new Exception($"商品活动不存在,货号:{item.ProductNumber},活动id:{customSaleCamps[item.ProductNumber].ToString()}");
            }

            // 采纳用户选择的商品活动折扣
            var discount = isMember ? saleCampInfo.MemberDiscount : saleCampInfo.NoMmebDiscount;
            saleCampInfo.IsSelected = true;
            ApplySaleCampDiscount(entry, (decimal)discount, item.SaleCampaigns.ToList());


        }


        private void ApplyBrandDiscount(ProductEntry entry, decimal discount)
        {
            entry.SaleCampaignDiscount = null;
            entry.SaleCampaigns = new List<SaleCampEntry>();
            entry.BrandDiscount = discount;
            entry.RetailPrice = entry.TagPrice * entry.BrandDiscount.Value;
        }

        private void ApplySaleCampDiscount(ProductEntry entry, decimal discount, List<SaleCampEntry> saleCamps)
        {

            entry.SaleCampaignDiscount = discount;
            entry.SaleCampaigns = saleCamps;
            entry.BrandDiscount = null;
            entry.RetailPrice = entry.TagPrice * entry.SaleCampaignDiscount.Value;
        }

        private class ItemEntry
        {
            /// <summary>
            /// 商品货号
            /// </summary>
            public string ProductNumber { get; set; }

            /// <summary>
            /// 商品图
            /// </summary>
            public string ThumbnailPath { get; set; }


            /// <summary>
            /// 品牌名
            /// </summary>
            public string BrandName { get; set; }

            /// <summary>
            /// 品类
            /// </summary>
            public string CategoryName { get; set; }
            /// <summary>
            /// 吊牌价
            /// </summary>
            public float TagPrice { get; set; }
            /// <summary>
            /// 尺码
            /// </summary>
            public string SizeName { get; set; }

            /// <summary>
            /// 数量
            /// </summary>
            public int Quantity { get; set; }
            /// <summary>
            /// 品牌折扣
            /// </summary>
            public float BrandDiscount { get; set; }

            public IEnumerable<string> Barcodes { get; set; }


            public IEnumerable<SaleCampEntry> SaleCampaigns { get; set; }     // 商品活动折扣
        }

        public class ProductEntry
        {
            public ProductEntry()
            {
                SaleCampaigns = new List<SaleCampEntry>();
                BrandDiscount = null;
                SaleCampaignDiscount = null;
            }
            /// <summary>
            /// 商品货号
            /// </summary>
            public string ProductNumber { get; set; }

            /// <summary>
            /// 商品图
            /// </summary>
            public string ThumbnailPath { get; set; }


            /// <summary>
            /// 品牌名
            /// </summary>
            public string BrandName { get; set; }

            /// <summary>
            /// 品类
            /// </summary>
            public string CategoryName { get; set; }

            /// <summary>
            /// 吊牌价
            /// </summary>
            public decimal TagPrice { get; set; }

            /// <summary>
            /// 尺码
            /// </summary>
            public string SizeName { get; set; }

            /// <summary>
            /// 数量
            /// </summary>
            public uint Quantity { get; set; }

            public string[] Barcodes { get; set; }

            /// <summary>
            /// 商品活动
            /// </summary>
            public List<SaleCampEntry> SaleCampaigns { get; set; }

            /// <summary>
            /// 品牌折扣
            /// </summary>
            public decimal? BrandDiscount { get; set; }

            /// <summary>
            /// 商品活动折扣
            /// </summary>
            public decimal? SaleCampaignDiscount { get; set; }

            public decimal RetailPrice { get; set; }

            public decimal SubTotal { get { return RetailPrice * Quantity; } }
        }



        public class GetProductInfoRes
        {
            public GetProductInfoRes()
            {
                Products = new List<ProductEntry>();
            }
            public bool IsFirstQuery { get; set; }

            public bool IsMember { get; set; }

            /// <summary>
            /// 商品信息
            /// </summary>
            public List<ProductEntry> Products;


            /// <summary>
            /// 总金额
            /// </summary>
            public decimal Total { get { return Products.Sum(p => p.SubTotal); } }
        }
    }

    public class GetEnableCouponRes
    {

        public decimal LevelDiscount { get; set; }

        public List<MemberCoupon> Coupons { get; set; }
        public List<StoreActivityEntry> StoreActivities { get; set; }
    }

    public class StoreActivityEntry
    {
        public int ActivityId { get; set; }
        public string ActivityName { get; set; }
        public decimal MinConsume { get; set; }
        public decimal DiscountMoney { get; set; }
    }
    public class SaleCampEntry
    {
        public SaleCampEntry()
        {
            IsSelected = false;
        }
        public bool IsSelected { get; set; }
        public int Id { get; set; }


        /// <summary>
        /// 活动名称
        /// </summary>
        public string CampaignName { get; set; }


        /// <summary>
        /// 会员专享折扣
        /// </summary>
        public float MemberDiscount { get; set; }

        /// <summary>
        /// 非会员商品折扣
        /// </summary>
        public float NoMmebDiscount { get; set; }

        /// <summary>
        /// 非否可再参与店铺活动
        /// </summary>
        public bool OtherCampaign { get; set; }


        /// <summary>
        /// 是否可再使用优惠券
        /// </summary>
        public bool OtherCashCoupon { get; set; }
    }



}
