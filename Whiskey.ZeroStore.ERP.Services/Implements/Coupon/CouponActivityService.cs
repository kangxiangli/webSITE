using System;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Logging;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class CouponActivityService : ServiceBase, ICouponActivityContract
    {


        private readonly IRepository<CouponActivity, int> _repo;

        private readonly IRepository<LBSCouponEntity, int> _couponItemRepository;

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(CouponService));

        public CouponActivityService(IRepository<CouponActivity, int> couponRepository,
            IRepository<LBSCouponEntity, int> couponItemRepository) : base(couponRepository.UnitOfWork)
        {
            _repo = couponRepository;
            _couponItemRepository = couponItemRepository;
        }

        public IQueryable<CouponActivity> Entities
        {
            get
            {
                return _repo.Entities;
            }
        }

        public IQueryable<LBSCouponEntity> LBSCouponEntities {
            get
            {
                return _couponItemRepository.Entities;
            }
        }
       

        public CouponActivity View(int Id)
        {
            return _repo.Entities.Where(e => !e.IsDeleted && e.IsEnabled && e.Id == Id).FirstOrDefault();
        }

        public OperationResult Delete(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    _repo.Delete(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "删除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Disable(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id)).ToList();
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Enable(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _repo.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Insert(params CouponActivity[] entities)
        {
            return _repo.Insert(entities, e => e.OperatorId = AuthorityHelper.OperatorId) ;
        }


        public OperationResult Recovery(int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _repo.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Update(params CouponActivity[] entities)
        {
            try
            {
                return _repo.Update(entities, entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "设置失败！错误如下：" + ex.Message, ex.ToString());
            }

        }

        public CouponActivity Edit(int Id)
        {
            return _repo.Entities.FirstOrDefault(e => !e.IsDeleted && e.IsEnabled && e.Id == Id);
        }

        public OperationResult Remove(params int[] ids)
        {
            throw new NotImplementedException();
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult UpdateCoupon(LBSCouponEntity couponEntity)
        {
            return _couponItemRepository.Update(new LBSCouponEntity[] { couponEntity }, c =>
            {
                c.UpdatedTime = DateTime.Now;
                c.OperatorId = AuthorityHelper.OperatorId;
            });
        }
    }
}
