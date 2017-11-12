
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
    public class ProductCrowdService : ServiceBase, IProductCrowdContract
    {
        private readonly IRepository<ProductCrowd, int> _ProductCrowdRepository;
        public ProductCrowdService(
            IRepository<ProductCrowd, int> _ProductCrowdRepository
            ) : base(_ProductCrowdRepository.UnitOfWork)
        {
            this._ProductCrowdRepository = _ProductCrowdRepository;
        }

        public IQueryable<ProductCrowd> Entities
        {
            get
            {
                return _ProductCrowdRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _ProductCrowdRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params ProductCrowd[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _ProductCrowdRepository.Insert(entities,
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
                var entities = _ProductCrowdRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params ProductCrowd[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _ProductCrowdRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public ProductCrowd View(int Id)
        {
            return _ProductCrowdRepository.GetByKey(Id);
        }

        public OperationResult Insert(params ProductCrowdDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");

                foreach (var dto in dtos)
                {
                    if (string.IsNullOrEmpty(dto.CrowdCode))
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败，编码不能为空！");
                    }
                    int hasCount = Entities.Where(x => x.CrowdName == dto.CrowdName).Count();
                    if (hasCount > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败，名称已经存在！");
                    }
                    int index = Entities.Where(x => x.CrowdCode == dto.CrowdCode).Count();
                    if (index > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败，编码已经存在！");
                    }
                }

                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _ProductCrowdRepository.Insert(dtos, a => { },
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

        public OperationResult Update(params ProductCrowdDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");

                foreach (var dto in dtos)
                {
                    if (string.IsNullOrEmpty(dto.CrowdCode))
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败，编码不能为空！");
                    }
                    var color = Entities.Where(x => x.CrowdName == dto.CrowdName).FirstOrDefault();
                    if (color != null && color.Id != dto.Id)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败，名称已经存在！");
                    }
                    var entity = Entities.Where(x => x.CrowdCode == dto.CrowdCode).FirstOrDefault();
                    if (entity != null && entity.Id != dto.Id)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败，编码已经存在！");
                    }
                }

                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _ProductCrowdRepository.Update(dtos, a => { },
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

        public ProductCrowdDto Edit(int Id)
        {
            var entity = _ProductCrowdRepository.GetByKey(Id);
            Mapper.CreateMap<ProductCrowd, ProductCrowdDto>();
            var dto = Mapper.Map<ProductCrowd, ProductCrowdDto>(entity);
            return dto;
        }
    }
}

