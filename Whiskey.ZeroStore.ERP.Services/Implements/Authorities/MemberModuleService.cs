
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
using Whiskey.Utility.Class;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class MemberModuleService : ServiceBase, IMemberModuleContract
    {
        private readonly IRepository<MemberModule, int> _MemberModuleRepository;
        public MemberModuleService(
            IRepository<MemberModule, int> _MemberModuleRepository
            ) : base(_MemberModuleRepository.UnitOfWork)
        {
            this._MemberModuleRepository = _MemberModuleRepository;
        }

        public IQueryable<MemberModule> Entities
        {
            get
            {
                return _MemberModuleRepository.Entities;
            }
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _MemberModuleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params MemberModule[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");

                foreach (var dto in entities)
                {
                    bool has = Entities.Where(x => x.ModuleName == dto.ModuleName).Any();
                    if (has)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败,该模块名称已经存在");
                    }
                    has = Entities.Where(x => !string.IsNullOrEmpty(dto.OverrideUrl) && dto.OverrideUrl.Equals(x.OverrideUrl, StringComparison.OrdinalIgnoreCase)).Any();
                    if (has)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败,重写路径已经存在");
                    }
                }

                OperationResult result = _MemberModuleRepository.Insert(entities,
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
                var entities = _MemberModuleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params MemberModule[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");

                foreach (var dto in entities)
                {
                    bool has = Entities.Where(x => x.ModuleName == dto.ModuleName && x.Id != dto.Id).Any();
                    if (has)
                    {
                        return new OperationResult(OperationResultType.Error, "更新失败,该模块名称已经存在");
                    }
                    has = Entities.Where(x => !string.IsNullOrEmpty(dto.OverrideUrl) && dto.OverrideUrl.Equals(x.OverrideUrl, StringComparison.OrdinalIgnoreCase) && x.Id != dto.Id).Any();
                    if (has)
                    {
                        return new OperationResult(OperationResultType.Error, "更新失败,重写路径已经存在");
                    }
                }

                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _MemberModuleRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public MemberModule View(int Id)
        {
            return _MemberModuleRepository.GetByKey(Id);
        }

        public OperationResult Insert(params MemberModuleDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");

                foreach (var dto in dtos)
                {
                    bool has = Entities.Where(x => x.ModuleName == dto.ModuleName).Any();
                    if (has)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败,该模块名称已经存在");
                    }
                    has = Entities.Where(x => !string.IsNullOrEmpty(dto.OverrideUrl) && dto.OverrideUrl.Equals(x.OverrideUrl, StringComparison.OrdinalIgnoreCase)).Any();
                    if (has)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败,重写路径已经存在");
                    }
                }

                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _MemberModuleRepository.Insert(dtos, a => { },
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

        public OperationResult Update(params MemberModuleDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");

                foreach (var dto in dtos)
                {
                    bool has = Entities.Where(x => x.ModuleName == dto.ModuleName && x.Id != dto.Id).Any();
                    if (has)
                    {
                        return new OperationResult(OperationResultType.Error, "更新失败,该模块名称已经存在");
                    }
                    has = Entities.Where(x => !string.IsNullOrEmpty(dto.OverrideUrl) && dto.OverrideUrl.Equals(x.OverrideUrl, StringComparison.OrdinalIgnoreCase) && x.Id != dto.Id).Any();
                    if (has)
                    {
                        return new OperationResult(OperationResultType.Error, "更新失败,重写路径已经存在");
                    }
                }

                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _MemberModuleRepository.Update(dtos, a => { },
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

        public MemberModuleDto Edit(int Id)
        {
            var entity = _MemberModuleRepository.GetByKey(Id);
            Mapper.CreateMap<MemberModule, MemberModuleDto>();
            var dto = Mapper.Map<MemberModule, MemberModuleDto>(entity);
            return dto;
        }

        public OperationResult SetSeq(int Id, int SequenceType)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);

            var query = Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            MemberModule module = query.FirstOrDefault(x => x.Id == Id);
            var listModule = query.Where(x => x.ParentId == module.ParentId).ToList();

            List<MemberModule> listEntity = new List<MemberModule>();
            if (listModule.Count == 1)
            {
                oper.Message = "一条数据不支持排序";
                return oper;
            }
            else
            {
                if (SequenceType == (int)SequenceFlag.Up)
                {
                    if (module.Sequence <= 0)
                    {
                        oper.Message = "已经是最高排序了";
                        return oper;
                    }
                    else
                    {
                        module.Sequence = module.Sequence - 1;
                        module.UpdatedTime = DateTime.Now;
                        MemberModule entity = listModule.FirstOrDefault(x => x.Sequence == module.Sequence && x.Id != module.Id);
                        if (entity != null)
                        {
                            entity.Sequence = entity.Sequence + 1;
                            entity.UpdatedTime = DateTime.Now;
                            listEntity.Add(entity);
                        }
                        listEntity.Add(module);

                    }
                }
                else if (SequenceType == (int)SequenceFlag.Down)
                {
                    if (module.Sequence <= 0)
                    {
                        oper.Message = "已经是最低排序了";
                        return oper;
                    }
                    else
                    {
                        module.Sequence = module.Sequence + 1;
                        module.UpdatedTime = DateTime.Now;
                        MemberModule entity = listModule.FirstOrDefault(x => x.Sequence == module.Sequence && x.Id != module.Id);
                        if (entity != null)
                        {
                            entity.Sequence = entity.Sequence - 1;
                            entity.UpdatedTime = DateTime.Now;
                            listEntity.Add(entity);
                        }
                        listEntity.Add(module);

                    }
                }
                else
                {
                    oper.Message = "异常操作";
                    return oper;
                }
            }
            oper = Update(listEntity.ToArray());
            return oper;
        }

        public List<SelectListItem> SelectList(bool hasHit = false)
        {
            var list = this.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == null).Select(s => new SelectListItem
            {
                Value = s.Id + "",
                Text = s.ModuleName,
            }).ToList();
            if (hasHit)
            {
                list.Insert(0, new SelectListItem { Text = "请选择", Value = "" });
            }

            return list;
        }
    }
}

