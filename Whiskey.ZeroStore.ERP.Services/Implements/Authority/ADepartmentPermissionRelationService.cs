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
    public class ADepartmentPermissionRelationService : ServiceBase, IADepartmentPermissionRelationContract
    {
        private readonly IRepository<ADepartmentPermissionRelation, int> _aDepartmentPermissionRelationRepository;
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(ADepartmentPermissionRelationService));
        public ADepartmentPermissionRelationService(
            IRepository<ADepartmentPermissionRelation, int> _aDepartmentPermissionRelationRepository

            )
            : base(_aDepartmentPermissionRelationRepository.UnitOfWork)
        {
            this._aDepartmentPermissionRelationRepository = _aDepartmentPermissionRelationRepository;
        }
        public ADepartmentPermissionRelation View(int Id)
        {
            var entity = _aDepartmentPermissionRelationRepository.GetByKey(Id);
            return entity;
        }

        public ADepartmentPermissionRelationDto Edit(int Id)
        {
            var entity = _aDepartmentPermissionRelationRepository.GetByKey(Id);
            var dto = Mapper.Map<ADepartmentPermissionRelation, ADepartmentPermissionRelationDto>(entity);
            return dto;
        }

        public System.Linq.IQueryable<ADepartmentPermissionRelation> ADepartmentPermissionRelations
        {
            get { return _aDepartmentPermissionRelationRepository.Entities; }
        }

        public OperationResult Insert(params ADepartmentPermissionRelationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _aDepartmentPermissionRelationRepository.Insert(dtos,
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

        public OperationResult Insert(params ADepartmentPermissionRelation[] adminpers)
        {
            return _aDepartmentPermissionRelationRepository.Insert((IEnumerable<ADepartmentPermissionRelation>)adminpers) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public OperationResult Update(params ADepartmentPermissionRelationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _aDepartmentPermissionRelationRepository.Update(dtos,
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

        public OperationResult Update(params ADepartmentPermissionRelation[] adminpers)
        {
            return _aDepartmentPermissionRelationRepository.Update(adminpers);
        }

        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _aDepartmentPermissionRelationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_administrator_Permission_Relation.Update(entity);
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
                var entities = _aDepartmentPermissionRelationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_administrator_Permission_Relation.Update(entity);
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
                OperationResult result = _aDepartmentPermissionRelationRepository.Delete(ids);
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
                var entities = _aDepartmentPermissionRelationRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_administrator_Permission_Relation.Update(entity);
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
                var entities = _aDepartmentPermissionRelationRepository.Entities.Where(m => ids.Contains(m.Id)).ToList();
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    //_administratorRepository.Update(entity);
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
