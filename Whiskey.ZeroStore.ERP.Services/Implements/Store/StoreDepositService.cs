using AutoMapper;
using System;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class StoreDepositService : ServiceBase, IStoreDepositContract
    {
        private readonly IRepository<StoreDeposit, int> _storeDepositRepository;
        private readonly IRepository<Store, int> _storeRepository;
        public StoreDepositService(
            IRepository<StoreDeposit, int> _storeDepositRepository,
            IRepository<Store, int> _storeRepository
            ) : base(_storeDepositRepository.UnitOfWork)
        {
            this._storeDepositRepository = _storeDepositRepository;
            this._storeRepository = _storeRepository;
        }

        public IQueryable<StoreDeposit> Entities
        {
            get
            {
                return _storeDepositRepository.Entities;
            }
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _storeDepositRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params StoreDeposit[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _storeDepositRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _storeDepositRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params StoreDeposit[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _storeDepositRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public StoreDeposit View(int Id)
        {
            return _storeDepositRepository.GetByKey(Id);
        }

        public OperationResult Insert(params StoreDepositDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;

                var sids = dtos.Select(s => s.StoreId);

                var stores = _storeRepository.Entities.Where(w => sids.Contains(w.Id)).ToList();

                foreach (var item in dtos)
                {
                    var store = stores.FirstOrDefault(w => w.Id == item.StoreId);
                    if (store == null) return new OperationResult(OperationResultType.Error, "店铺不存在！");

                    store.Balance += (item.Card + item.Cash + item.Remit);
                    store.UpdatedTime = DateTime.Now;
                    _storeRepository.Update(store);
                }

                OperationResult result = _storeDepositRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.Price = dto.Cash + dto.Card + dto.Remit;
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }

        public OperationResult Update(params StoreDepositDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _storeDepositRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.Price = dto.Cash + dto.Card + dto.Remit;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public StoreDepositDto Edit(int Id)
        {
            var entity = _storeDepositRepository.GetByKey(Id);
            Mapper.CreateMap<StoreDeposit, StoreDepositDto>();
            var dto = Mapper.Map<StoreDeposit, StoreDepositDto>(entity);
            return dto;
        }
    }
}
