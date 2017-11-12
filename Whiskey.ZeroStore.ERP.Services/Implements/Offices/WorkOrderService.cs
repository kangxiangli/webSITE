
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
    public class WorkOrderService : ServiceBase, IWorkOrderContract
    {
        private readonly IRepository<WorkOrder, int> _WorkOrderRepository;

        private readonly IRepository<WorkOrderDealtWith, int> _WorkOrderDealtWithRepository;
        public WorkOrderService(
            IRepository<WorkOrder, int> WorkOrderRepository,
            IRepository<WorkOrderDealtWith, int> WorkOrderDealtWithRepository
            ) : base(WorkOrderRepository.UnitOfWork)
        {
            this._WorkOrderRepository = WorkOrderRepository;
            this._WorkOrderDealtWithRepository = WorkOrderDealtWithRepository;
        }

        public IQueryable<WorkOrder> Entities
        {
            get
            {
                return _WorkOrderRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _WorkOrderRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        /// <summary>
        /// 撤销或恢复撤销数据
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public OperationResult CancelOrRecoveryCancel(bool cancel, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _WorkOrderRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.Status = cancel ? -1 : 0;
                    entity.HandlerID = null;
                    entity.DealtTime = null;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, cancel ? Operation.Cancel : Operation.RecoveryCancel);
        }

        public OperationResult Insert(params WorkOrder[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _WorkOrderRepository.Insert(entities,
                entity =>
                {
                    entity.Status = 0;
                    entity.PersonHandleCount = 0;
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
                var entities = _WorkOrderRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params WorkOrder[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _WorkOrderRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public WorkOrder View(int Id)
        {
            return _WorkOrderRepository.GetByKey(Id);
        }

        public OperationResult Insert(params WorkOrderDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _WorkOrderRepository.Insert(dtos, a => { },
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

        public OperationResult Update(params WorkOrderDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _WorkOrderRepository.Update(dtos, a => { },
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

        public WorkOrderDto Edit(int Id)
        {
            var entity = _WorkOrderRepository.GetByKey(Id);
            Mapper.CreateMap<WorkOrder, WorkOrderDto>();
            var dto = Mapper.Map<WorkOrder, WorkOrderDto>(entity);
            return dto;
        }

        /// <summary>
        /// 指派工单
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public OperationResult Assign(int Id, int adminId)
        {
            try
            {
                var workOrder = _WorkOrderRepository.GetByKey(Id);

                if (workOrder == null)
                {
                    return new OperationResult(OperationResultType.Error, "该工单不存在");
                }
                if (workOrder.Status == -1)
                {
                    return new OperationResult(OperationResultType.Error, "该工单已撤销");
                }
                if (workOrder.Status != 0)
                {
                    return new OperationResult(OperationResultType.Error, "该工单已被指派");
                }

                bool wds = _WorkOrderDealtWithRepository.ExistsCheck(d => d.WorkOrderId == Id && (d.Status != -1 && d.Status != 2));
                if (wds)
                {
                    return new OperationResult(OperationResultType.Error, "该工单已指派给您");
                }

                UnitOfWork.TransactionEnabled = true;

                workOrder.HandlerID = adminId;
                workOrder.Status = 1;
                workOrder.UpdatedTime = DateTime.Now;
                workOrder.OperatorId = AuthorityHelper.OperatorId;

                _WorkOrderRepository.Update(workOrder);

                WorkOrderDealtWith dealtwith = new WorkOrderDealtWith();
                dealtwith.WorkOrderId = workOrder.Id;
                dealtwith.HandlerID = adminId;
                dealtwith.Status = 0;
                dealtwith.Notes = "";
                dealtwith.CreatedTime = DateTime.Now;
                dealtwith.UpdatedTime = DateTime.Now;
                dealtwith.IsDeleted = false;
                dealtwith.IsEnabled = true;
                dealtwith.OperatorId = AuthorityHelper.OperatorId;
                _WorkOrderDealtWithRepository.Insert(dealtwith);

                int count = UnitOfWork.SaveChanges();
                return (count > 0) ? new OperationResult(OperationResultType.Success, "指派成功") : new OperationResult(OperationResultType.Error, "指派失败");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "服务器错误");
            }
        }
    }
}

