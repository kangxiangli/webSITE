
using AutoMapper;
using System;
using System.Collections.Generic;
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
    public class WorkOrderDealtWithService : ServiceBase, IWorkOrderDealtWithContract
    {
        private readonly IRepository<WorkOrderDealtWith, int> _WorkOrderDealtWithRepository;
        private readonly INotificationContract _notificationContract;

        private readonly IRepository<WorkOrder, int> _WorkOrderRepository;
        public WorkOrderDealtWithService(
            IRepository<WorkOrderDealtWith, int> WorkOrderDealtWithRepository,
            IRepository<WorkOrder, int> WorkOrderRepository,
            INotificationContract notificationContract
            ) : base(WorkOrderDealtWithRepository.UnitOfWork)
        {
            this._WorkOrderDealtWithRepository = WorkOrderDealtWithRepository;
            this._WorkOrderRepository = WorkOrderRepository;
            this._notificationContract = notificationContract;
        }

        public IQueryable<WorkOrderDealtWith> Entities
        {
            get
            {
                return _WorkOrderDealtWithRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _WorkOrderDealtWithRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params WorkOrderDealtWith[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _WorkOrderDealtWithRepository.Insert(entities,
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
                var entities = _WorkOrderDealtWithRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params WorkOrderDealtWith[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _WorkOrderDealtWithRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public WorkOrderDealtWith View(int Id)
        {
            return _WorkOrderDealtWithRepository.GetByKey(Id);
        }

        public OperationResult Insert(params WorkOrderDealtWithDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _WorkOrderDealtWithRepository.Insert(dtos, a => { },
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

        public OperationResult Update(params WorkOrderDealtWithDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _WorkOrderDealtWithRepository.Update(dtos, a => { },
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

        public WorkOrderDealtWithDto Edit(int Id)
        {
            var entity = _WorkOrderDealtWithRepository.GetByKey(Id);
            Mapper.CreateMap<WorkOrderDealtWith, WorkOrderDealtWithDto>();
            var dto = Mapper.Map<WorkOrderDealtWith, WorkOrderDealtWithDto>(entity);
            return dto;
        }

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="status">����״̬��-1���Ѿܾ� 1���ѽ��ܣ�2������ɣ�</param>
        /// <returns></returns>
        public OperationResult DealtWith(int Id, int status, string Notes)
        {
            try
            {
                var workOrderDealtWith = _WorkOrderDealtWithRepository.GetByKey(Id);

                if (workOrderDealtWith == null)
                {
                    return new OperationResult(OperationResultType.Error, "�ù���δָ�ɸ���");
                }

                UnitOfWork.TransactionEnabled = true;

                workOrderDealtWith.Status = status;
                workOrderDealtWith.Notes = Notes;
                workOrderDealtWith.UpdatedTime = DateTime.Now;
                workOrderDealtWith.OperatorId = AuthorityHelper.OperatorId;

                var workOrder = _WorkOrderRepository.GetByKey(workOrderDealtWith.WorkOrderId);
                string title = "��������֪ͨ";
                string content = "";
                switch (status)
                {
                    case -1:
                        workOrder.Status = 0;
                        workOrder.HandlerID = null;
                        title = "��������ܾ�֪ͨ";
                        content = "������" + workOrder.WorkOrderTitle + " �ѱ�" + workOrderDealtWith.Handler.Member.RealName + "�ܾ�,�ܾ�ԭ��" + workOrderDealtWith.Notes;
                        break;
                    case 1:
                        workOrder.Status = 2;
                        workOrder.DealtTime = DateTime.Now;
                        workOrderDealtWith.DealtTime = DateTime.Now;
                        title = "�����������֪ͨ";
                        content = "������" + workOrder.WorkOrderTitle + " �ѱ�" + workOrderDealtWith.Handler.Member.RealName + "����";
                        break;
                    case 2:
                        workOrder.Status = 3;
                        workOrder.FinishTime = DateTime.Now;
                        workOrderDealtWith.FinishTime = DateTime.Now;
                        title = "�����������֪ͨ";
                        content = "������" + workOrder.WorkOrderTitle + " �ѱ�" + workOrderDealtWith.Handler.Member.RealName + "���";
                        break;
                    default:
                        break;

                }
                workOrder.UpdatedTime = DateTime.Now;

                _WorkOrderDealtWithRepository.Update(workOrderDealtWith);
                _WorkOrderRepository.Update(workOrder);
                
               // _notificationContract.SendNotice(workOrder.ApplicantId, title, content, sendNotificationAction);

                int count = UnitOfWork.SaveChanges();
                return (count > 0) ? new OperationResult(OperationResultType.Success, "�����ɹ�") : new OperationResult(OperationResultType.Error, "����ʧ��");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "����������");
            }
        }
    }
}

