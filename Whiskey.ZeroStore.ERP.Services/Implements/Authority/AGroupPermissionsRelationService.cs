using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Data;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Extensions;
using AutoMapper;
using Whiskey.Web.Helper;
using Whiskey.Utility.Logging;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AGroupPermissionsRelationService : ServiceBase, IAGroupPermissionsRelationContract
    {
        private readonly IRepository<AGroupPermissionRelation, int> _aGroupPermissionsRelationRepository;
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(AGroupPermissionsRelationService));
        public AGroupPermissionsRelationService(
            IRepository<AGroupPermissionRelation, int> _aGroupPermissionsRelationRepository

            )
            : base(_aGroupPermissionsRelationRepository.UnitOfWork)
        {
            this._aGroupPermissionsRelationRepository = _aGroupPermissionsRelationRepository;
        }
        public AGroupPermissionRelation View(int Id)
        {
            var entity = _aGroupPermissionsRelationRepository.GetByKey(Id);
            return entity;
        }

        public AGroupPermissionsRelationDto Edit(int Id)
        {
            var entity = _aGroupPermissionsRelationRepository.GetByKey(Id);
            var dto = Mapper.Map<AGroupPermissionRelation, AGroupPermissionsRelationDto>(entity);
            return dto;
        }

        public IQueryable<AGroupPermissionRelation> AGroupPermissionsRelations
        {
            get { return _aGroupPermissionsRelationRepository.Entities; }
        }

        public OperationResult Insert(params AGroupPermissionsRelationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _aGroupPermissionsRelationRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Insert(params AGroupPermissionRelation[] adminpers)
        {
            return _aGroupPermissionsRelationRepository.Insert((IEnumerable<AGroupPermissionRelation>)adminpers) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public OperationResult Update(params AGroupPermissionsRelationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _aGroupPermissionsRelationRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Update(params AGroupPermissionRelation[] adminpers)
        {
            return _aGroupPermissionsRelationRepository.Update(adminpers);
        }

        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _aGroupPermissionsRelationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_aGroupPermissionsRelationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _aGroupPermissionsRelationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_aGroupPermissionsRelationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _aGroupPermissionsRelationRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Enable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _aGroupPermissionsRelationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_aGroupPermissionsRelationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _aGroupPermissionsRelationRepository.Entities.Where(m => ids.Contains(m.Id)).ToList();
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_aGroupPermissionsRelationRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }
    }
}
