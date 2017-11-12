
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
    public class ClaimForGoodsService : ServiceBase, IClaimForGoodsContract
    {
        private readonly IRepository<ClaimForGoods, int> _ClaimForGoodsRepository;
        private readonly ICompanyGoodsCategoryContract _companyGoodsCategoryContract;
        private readonly INotificationContract _notificationContract;

        public ClaimForGoodsService(
            IRepository<ClaimForGoods, int> _ClaimForGoodsRepository,
            ICompanyGoodsCategoryContract companyGoodsCategoryContract,
            INotificationContract notificationContract
            ) : base(_ClaimForGoodsRepository.UnitOfWork)
        {
            this._ClaimForGoodsRepository = _ClaimForGoodsRepository;
            this._companyGoodsCategoryContract = companyGoodsCategoryContract;
            this._notificationContract = notificationContract;
        }

        public IQueryable<ClaimForGoods> Entities
        {
            get
            {
                return _ClaimForGoodsRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _ClaimForGoodsRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params ClaimForGoods[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _ClaimForGoodsRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
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
                var entities = _ClaimForGoodsRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params ClaimForGoods[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _ClaimForGoodsRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                });
                return result;
            }, Operation.Update);
        }

        public ClaimForGoods View(int Id)
        {
            return _ClaimForGoodsRepository.GetByKey(Id);
        }

        public OperationResult Insert(params ClaimForGoodsDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _ClaimForGoodsRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }, Operation.Add);
        }

        public OperationResult Update(params ClaimForGoodsDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _ClaimForGoodsRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = entity.OperatorId ?? AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }, Operation.Update);
        }

        public ClaimForGoodsDto Edit(int Id)
        {
            var entity = _ClaimForGoodsRepository.GetByKey(Id);
            Mapper.CreateMap<ClaimForGoods, ClaimForGoodsDto>();
            var dto = Mapper.Map<ClaimForGoods, ClaimForGoodsDto>(entity);
            return dto;
        }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public OperationResult Apply(ClaimForGoodsDto dto)
        {
            var goods = _companyGoodsCategoryContract.Entities.FirstOrDefault(g => g.Id == dto.CompanyGoodsCategoryID && !g.IsDeleted && g.IsEnabled);
            if (goods == null)
            {
                return new OperationResult(OperationResultType.Error, "����Ʒ������");
            }

            if (goods.Parent == null)
            {
                return new OperationResult(OperationResultType.Error, "����ѡ����Ʒ");
            }
            var parent = goods.Parent;

            if (goods.TotalQuantity - goods.UsedQuantity < dto.Quantity)
            {
                return new OperationResult(OperationResultType.Error, "��ѡ�����Ʒ�������ڿ�����Ʒ����");
            }

            if (dto.ReturnTimeLimit && dto.EstimateReturnTime == null)
            {
                return new OperationResult(OperationResultType.Error, "��ѡ��Ԥ�ƹ黹ʱ��");
            }

            if (!dto.ReturnTimeLimit)
            {
                dto.EstimateReturnTime = null;
            }

            goods.UsedQuantity = (parent.UsedQuantity ?? 0) + dto.Quantity;
            if (goods.IsUniqueness)
            {
                goods.Status = 1;
            }

            parent.UsedQuantity = (parent.UsedQuantity ?? 0) + dto.Quantity;
            dto.ReturnStatus = 1;

            try
            {
                UnitOfWork.TransactionEnabled = true;

                _companyGoodsCategoryContract.Update(goods);
                _companyGoodsCategoryContract.Update(parent);
                this.Insert(dto);

                int count = UnitOfWork.SaveChanges();
                return new OperationResult(OperationResultType.Success, "��Ʒ����ɹ�");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "�����쳣�����Ժ�����");
            }
        }

        /// <summary>
        /// ��Ʒ�黹
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public OperationResult ReturnGoods(int[] Ids)
        {

            try
            {
                foreach (int Id in Ids)
                {
                    var entity = _ClaimForGoodsRepository.Entities.FirstOrDefault(f => f.Id == Id && !f.IsDeleted && f.IsEnabled);
                    if (entity == null)
                    {
                        return new OperationResult(OperationResultType.Error, "�����¼������");
                    }
                    var goods = entity.CompanyGoodsCategory;

                    var parent = goods.Parent;

                    goods.UsedQuantity = (parent.UsedQuantity ?? 0) - entity.Quantity;
                    if (goods.IsUniqueness)
                    {
                        goods.Status = 0;
                    }

                    parent.UsedQuantity = (parent.UsedQuantity ?? 0) - entity.Quantity;
                    entity.ReturnStatus = 2;
                    entity.ReturnTime = DateTime.Now;
                    UnitOfWork.TransactionEnabled = true;

                    _companyGoodsCategoryContract.Update(goods);
                    _companyGoodsCategoryContract.Update(parent);
                    this.Update(entity);
                }

                int count = UnitOfWork.SaveChanges();

                return new OperationResult(OperationResultType.Success, "��Ʒ����ɹ�");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "�����쳣�����Ժ�����");
            }
        }

        /// <summary>
        /// �黹����
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public OperationResult ReturnReminder(int[] Ids, Action<List<int>> sendNotificationAction)
        {

            try
            {
                foreach (var Id in Ids)
                {
                    var entity = _ClaimForGoodsRepository.Entities.FirstOrDefault(f => f.Id == Id && !f.IsDeleted && f.IsEnabled);
                    if (entity == null)
                    {
                        return new OperationResult(OperationResultType.Error, "�����¼������");
                    }

                    if (!entity.IsReturn)
                    {
                        return new OperationResult(OperationResultType.Error, "����Ʒ����黹");
                    }

                    if (!entity.ReturnTimeLimit)
                    {
                        return new OperationResult(OperationResultType.Error, "����Ʒ�޹黹ʱ������");
                    }

                    if (entity.ReturnStatus == 1)
                    {
                        return new OperationResult(OperationResultType.Error, "����Ʒ�ѹ黹");
                    }

                    if (entity.IsReturn && entity.ReturnTimeLimit && DateTime.Now < entity.EstimateReturnTime)
                    {
                        return new OperationResult(OperationResultType.Error, "��δ������ʱ��");
                    }

                    var goods = _companyGoodsCategoryContract.Edit(entity.CompanyGoodsCategoryID);
                    _notificationContract.SendNotice(entity.ApplicantId, "�黹����", "������������Ʒ " + goods.CompanyGoodsCategoryName + " ��δ�黹���뼰ʱ�黹", sendNotificationAction);
                }

                return new OperationResult(OperationResultType.Success, "���ѳɹ�");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "�����쳣�����Ժ�����");
            }
        }
    }
}

