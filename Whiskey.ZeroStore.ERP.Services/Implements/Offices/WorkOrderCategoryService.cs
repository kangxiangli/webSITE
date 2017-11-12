
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
    public class WorkOrderCategoryService : ServiceBase, IWorkOrderCategoryContract
    {
        private readonly IRepository<WorkOrderCategory, int> _WorkOrderCategoryRepository;
        public WorkOrderCategoryService(
            IRepository<WorkOrderCategory, int> _WorkOrderCategoryRepository
            ) : base(_WorkOrderCategoryRepository.UnitOfWork)
        {
            this._WorkOrderCategoryRepository = _WorkOrderCategoryRepository;
        }

        public IQueryable<WorkOrderCategory> Entities
        {
            get
            {
                return _WorkOrderCategoryRepository.Entities;
            }
        }

        public IQueryable<WorkOrderCategory> WorkOrderCategorys
        {
            get
            {
                return _WorkOrderCategoryRepository.Entities;
            }
        }

        /// <summary>
        /// 启用或禁用数据
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _WorkOrderCategoryRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params WorkOrderCategory[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _WorkOrderCategoryRepository.Insert(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Add);
        }

        /// <summary>
        /// 移除或还原数据
        /// </summary>
        /// <param name="delete"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _WorkOrderCategoryRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params WorkOrderCategory[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _WorkOrderCategoryRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public WorkOrderCategory View(int Id)
        {
            return _WorkOrderCategoryRepository.GetByKey(Id);
        }

        public OperationResult Insert(params WorkOrderCategoryDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _WorkOrderCategoryRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }

        public OperationResult Update(params WorkOrderCategoryDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _WorkOrderCategoryRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public WorkOrderCategoryDto Edit(int Id)
        {
            var entity = _WorkOrderCategoryRepository.GetByKey(Id);
            Mapper.CreateMap<WorkOrderCategory, WorkOrderCategoryDto>();
            var dto = Mapper.Map<WorkOrderCategory, WorkOrderCategoryDto>(entity);
            return dto;
        }
    }
}

