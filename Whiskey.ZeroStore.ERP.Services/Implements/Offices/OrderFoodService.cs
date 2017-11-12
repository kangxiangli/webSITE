
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
using System.Collections.Generic;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class OrderFoodService : ServiceBase, IOrderFoodContract
    {
        private readonly IRepository<OrderFood, int> _OrderFoodRepository;
        private readonly IRepository<Administrator, int> _AdministratorRepository;
        public OrderFoodService(
            IRepository<OrderFood, int> _OrderFoodRepository,
            IRepository<Administrator, int> _AdministratorRepository
            ) : base(_OrderFoodRepository.UnitOfWork)
        {
            this._OrderFoodRepository = _OrderFoodRepository;
            this._AdministratorRepository = _AdministratorRepository;
        }

        public IQueryable<OrderFood> Entities
        {
            get
            {
                return _OrderFoodRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _OrderFoodRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params OrderFood[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _OrderFoodRepository.Insert(entities,
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
                var entities = _OrderFoodRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params OrderFood[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _OrderFoodRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public OrderFood View(int Id)
        {
            return _OrderFoodRepository.GetByKey(Id);
        }

        public OperationResult Insert(params OrderFoodDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _OrderFoodRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.Admins = _AdministratorRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && dto.AdminIds.Contains(w.Id)).ToList();

                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }

        public OperationResult Update(params OrderFoodDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _OrderFoodRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.Admins.Clear();
                        entity.Admins = _AdministratorRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && dto.AdminIds.Contains(w.Id)).ToList();

                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public OrderFoodDto Edit(int Id)
        {
            var entity = _OrderFoodRepository.GetByKey(Id);
            Mapper.CreateMap<OrderFood, OrderFoodDto>();
            var dto = Mapper.Map<OrderFood, OrderFoodDto>(entity);
            dto.AdminIds = entity.Admins.Where(w => w.IsEnabled && !w.IsDeleted).Select(s => s.Id).ToList();
            return dto;
        }
    }
}

