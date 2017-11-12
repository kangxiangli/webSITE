using AutoMapper;
using System;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class PartnerManageService : ServiceBase, IPartnerManageContract
    {
        private readonly IRepository<PartnerManage, int> _PartnerManageRepository;
        public PartnerManageService(
            IRepository<PartnerManage, int> _PartnerManageRepository
            ) : base(_PartnerManageRepository.UnitOfWork)
        {
            this._PartnerManageRepository = _PartnerManageRepository;
        }

        public IQueryable<PartnerManage> Entities
        {
            get
            {
                return _PartnerManageRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _PartnerManageRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params PartnerManage[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _PartnerManageRepository.Insert(entities,
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
                var entities = _PartnerManageRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params PartnerManage[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _PartnerManageRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public PartnerManage View(int Id)
        {
            return _PartnerManageRepository.GetByKey(Id);
        }

        public OperationResult Insert(params PartnerManageDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _PartnerManageRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        entity.CreatedTime = dto.CreateTime ?? DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Add);
        }

        public OperationResult Update(params PartnerManageDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _PartnerManageRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        if (dto.CreateTime.HasValue)
                        {
                            entity.CreatedTime = dto.CreateTime.Value;
                        }
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public PartnerManageDto Edit(int Id)
        {
            var entity = _PartnerManageRepository.GetByKey(Id);
            Mapper.CreateMap<PartnerManage, PartnerManageDto>();
            var dto = Mapper.Map<PartnerManage, PartnerManageDto>(entity);
            return dto;
        }

        public OperationResult JoinUs(PartnerManageDto dto)
        {
            var data = OperationHelper.Try((opera) =>
            {
                #region 补充会员信息

                var result = new OperationResult(OperationResultType.Error);

                dto.CheckStatus = CheckStatusFlag.待审核;
                dto.IsRead = false;
                dto.MemberId = null;
                dto.CheckNotes = null;
                #endregion

                var modPar = _PartnerManageRepository.Entities.FirstOrDefault(w => w.IsEnabled && !w.IsDeleted && w.Id == dto.Id);
                if (modPar.IsNull())
                {
                    result = Insert(dto);
                }
                else
                {
                    if (modPar.CheckStatus == CheckStatusFlag.待审核)
                    {
                        result.Message = "资料正在审核中,请勿重复提交";
                        return result;
                    }
                    else if (modPar.CheckStatus == CheckStatusFlag.通过)
                    {
                        result.Message = "资料审核已通过,不能做修改";
                        return result;
                    }

                    modPar.Email = dto.Email;
                    modPar.MemberName = dto.MemberName;
                    modPar.Gender = dto.Gender;
                    modPar.MobilePhone = dto.MobilePhone;
                    modPar.IDCard_Front = dto.IDCard_Front;
                    modPar.IDCard_Reverse = dto.IDCard_Reverse;
                    modPar.Province = dto.Province;
                    modPar.City = dto.City;
                    modPar.Address = dto.Address;
                    modPar.LicencePhoto = dto.LicencePhoto;
                    modPar.StorePhoto = dto.StorePhoto;
                    modPar.ZipCode = dto.ZipCode;
                    modPar.CheckStatus = dto.CheckStatus;
                    modPar.IsRead = dto.IsRead;
                    modPar.CheckNotes = dto.CheckNotes;
                    modPar.MemberId = dto.MemberId;
                    modPar.MemberPass = dto.MemberPass;
                    modPar.ProposerId = dto.ProposerId;

                    result = Update(dto);
                }

                return OperationHelper.ReturnOperationResult(result.ResultType == OperationResultType.Success, opera);

            }, "提交资料");
            return data;
        }
    }
}
