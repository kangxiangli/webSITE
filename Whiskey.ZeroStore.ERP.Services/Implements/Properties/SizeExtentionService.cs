
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
using System.Web.Mvc;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class SizeExtentionService : ServiceBase, ISizeExtentionContract
    {
        private readonly IRepository<SizeExtention, int> _SizeExtentionRepository;
        public SizeExtentionService(
            IRepository<SizeExtention, int> _SizeExtentionRepository
            ) : base(_SizeExtentionRepository.UnitOfWork)
        {
            this._SizeExtentionRepository = _SizeExtentionRepository;
        }

        public IQueryable<SizeExtention> Entities
        {
            get
            {
                return _SizeExtentionRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _SizeExtentionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params SizeExtention[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                OperationResult result = _SizeExtentionRepository.Insert(entities,
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
                var entities = _SizeExtentionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params SizeExtention[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _SizeExtentionRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public SizeExtention View(int Id)
        {
            return _SizeExtentionRepository.GetByKey(Id);
        }

        public OperationResult Insert(params SizeExtentionDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _SizeExtentionRepository.Insert(dtos, a => { },
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

        public OperationResult Update(params SizeExtentionDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _SizeExtentionRepository.Update(dtos, a => { },
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

        public SizeExtentionDto Edit(int Id)
        {
            var entity = _SizeExtentionRepository.GetByKey(Id);
            Mapper.CreateMap<SizeExtention, SizeExtentionDto>();
            var dto = Mapper.Map<SizeExtention, SizeExtentionDto>(entity);
            return dto;
        }

        public List<SelectListItem> SelectListItem(bool hasHit = false)
        {
            var res = new List<SelectListItem>();
            var list = Entities.Where(w => w.IsEnabled && !w.IsDeleted).Select(s => new SelectListItem()
            {
                Text = s.Name,
                Value = s.Id + "",
            }).ToList();
            if (hasHit)
            {
                res.Add(new SelectListItem() { Text = "«Î—°‘Ò", Value = "" });
            }
            if (list.IsNotNull())
            {
                res.AddRange(list);
            }
            return res;
        }
    }
}

