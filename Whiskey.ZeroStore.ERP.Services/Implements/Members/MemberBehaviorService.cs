using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services
{
    public class MemberBehaviorService : ServiceBase, IMemberBehaviorContract
    {
        private readonly IRepository<MemberBehavior, int> _MemberBehaviorRepository;
        private readonly IRepository<ProductOriginNumber, int> _ProductOriginNumberRepository;
        protected readonly IRepository<OnlinePurchaseProduct,int> _OnlinePurchaseProductRepository;
        protected readonly IRepository<Store,int> _StoreRepository;
        protected readonly IRepository<Storage,int> _StorageRepository;
        protected readonly IRepository<Inventory,int> _InventoryRepository;
        public MemberBehaviorService(
            IRepository<MemberBehavior, int> _MemberBehaviorRepository,
            IRepository<ProductOriginNumber, int> _ProductOriginNumberRepository,
            IRepository<Store, int> _StoreRepository,
            IRepository<Storage, int> _StorageRepository,
            IRepository<Inventory, int> _InventoryRepository,
            IRepository<OnlinePurchaseProduct, int> _OnlinePurchaseProductRepository
            ) : base(_MemberBehaviorRepository.UnitOfWork)
        {
            this._MemberBehaviorRepository = _MemberBehaviorRepository;
            this._ProductOriginNumberRepository = _ProductOriginNumberRepository;
            this._OnlinePurchaseProductRepository = _OnlinePurchaseProductRepository;
            this._StoreRepository = _StoreRepository;
            this._StorageRepository = _StorageRepository;
            this._InventoryRepository = _InventoryRepository;
        }

        public IQueryable<MemberBehavior> Entities
        {
            get
            {
                return _MemberBehaviorRepository.Entities;
            }
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _MemberBehaviorRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _MemberBehaviorRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params MemberBehavior[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _MemberBehaviorRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }

        public OperationResult Update(params MemberBehavior[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _MemberBehaviorRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public MemberBehavior View(int Id)
        {
            return _MemberBehaviorRepository.GetByKey(Id);
        }

        public OperationResult AddBehaviorRecord(int MemberId, string BigProNum)
        {
            return OperationHelper.Try(() =>
             {
                 var modPON = _ProductOriginNumberRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.BigProdNum == BigProNum).Select(s => new { s.Id, s.CategoryId }).FirstOrDefault();
                 if (modPON.IsNull())
                 {
                     return OperationHelper.ReturnOperationResult(false, "商品不存在");
                 }

                 var mod = Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.MemberId == MemberId && w.ProductOriginNumberId == modPON.Id);
                 if (mod.IsNotNull())
                 {
                     mod.PageViews += 1;
                     return Update(mod);
                 }
                 else
                 {
                     mod = new MemberBehavior()
                     {
                         MemberId = MemberId,
                         ProductOriginNumberId = modPON.Id,
                         CategoryId = modPON.CategoryId,
                         PageViews = 1
                     };
                     return Insert(mod);
                 }
             }, ex =>
             {
                 return OperationHelper.ReturnOperationExceptionResult(ex, "操作浏览记录失败", true);
             });
        }

        public OperationResult RelatedRecommend(int MemberId, int StoreId, int Count = 10)
        {
            return OperationHelper.Try(() =>
            {
                var queryB = Entities.Where(w => w.MemberId == MemberId && w.IsEnabled && !w.IsDeleted).Select(s => s.CategoryId).Distinct();

                var queryP = _ProductOriginNumberRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => w.IsRecommend == true);
                var queryStore = _StoreRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted);
                var queryStorage = _StorageRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted);

                #region 筛选推荐款号和来源店铺仓库库存

                var mainStoreId = ConfigurationHelper.GetAppSetting("OnlineStorage").CastTo<int>();
                // 是否是总店铺
                if (StoreId != mainStoreId)
                {
                    queryP = queryP.Where(w => w.RecommendStoreIds.Contains(StoreId + ""));
                    queryStore = queryStore.Where(w => w.Id == StoreId);
                    queryStorage = queryStorage.Where(w => w.StoreId == StoreId);
                }

                #endregion

                var query = from s in _InventoryRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.Status == InventoryStatus.Default && !w.IsLock)
                            where queryStore.Select(ss => ss.Id).Contains(s.StoreId) && queryStorage.Select(ss => ss.Id).Contains(s.StorageId) &&
                            queryP.Select(ss => ss.BigProdNum).Contains(s.Product.BigProdNum)
                            select s.Product;

                var data = (from s in query
                            let pon = s.ProductOriginNumber
                            where queryB.Contains(pon.CategoryId)
                            orderby Guid.NewGuid()
                            select new
                            {
                                ProductId = s.Id,
                                s.BigProdNum,
                                s.ProductName,
                                ThumbnailPath = s.ThumbnailPath != null ? WebUrl + s.ThumbnailPath : pon.ThumbnailPath != null ? WebUrl + pon.ThumbnailPath : string.Empty,
                                pon.TagPrice,
                                pon.Brand.BrandName,
                            }).Skip(0).Take(Count).ToList();

                return OperationHelper.ReturnOperationResult(true, "", data);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取相关推荐失败", true);
            });
        }

        public OperationResult GetBehaviourRecord(int MemberId, int PageIndex = 1, int PageSize = 10)
        {
            return OperationHelper.Try(() =>
            {
                PageIndex = PageIndex > 1 ? PageIndex : 1;
                PageSize = PageSize > 1 ? PageSize : 1;
                var queryB = Entities.Where(w => w.MemberId == MemberId && w.IsEnabled && !w.IsDeleted);

                var list = (from m in queryB
                            orderby new { m.PageViews, m.UpdatedTime } descending
                            let s = m.ProductOriginNumber
                            select new
                            {
                                s.BigProdNum,
                                s.ProductName,
                                ThumbnailPath = s.ThumbnailPath != null ? WebUrl + s.ThumbnailPath : string.Empty,
                                s.TagPrice,
                                s.Brand.BrandName,
                                m.PageViews,
                            }).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();

                var allcount = queryB.Count();
                var allpage = (int)Math.Ceiling((double)allcount / PageSize);

                var data = new
                {
                    TotalCount = allcount,
                    TotalPage = allpage,
                    List = list,
                };

                return OperationHelper.ReturnOperationResult(true, "", data);
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取我的足迹失败", true);
            });
        }
    }
}
