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
    public class AAdministratorPermissionRelationService : ServiceBase, IAdministratorPermissionRelationContract
    {
        private readonly IRepository<AAdministratorPermissionRelation, int> _aadministratorPermissionRelation;
        private readonly IRepository<Administrator, int> _administratorRepository;
        private readonly IRepository<Permission, int> _permissionRepository;
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(AAdministratorPermissionRelationService));
        public AAdministratorPermissionRelationService(
            IRepository<Administrator, int> _administratorRepository
            , IRepository<Permission, int> _permissionRepository
            , IRepository<AAdministratorPermissionRelation, int> _aadministratorPermissionRelationRepository
            )
            : base(_aadministratorPermissionRelationRepository.UnitOfWork)
        {
            this._aadministratorPermissionRelation = _aadministratorPermissionRelationRepository;
            this._administratorRepository = _administratorRepository;
            this._permissionRepository = _permissionRepository;
        }
        public AAdministratorPermissionRelation View(int Id)
        {
            var entity = _aadministratorPermissionRelation.GetByKey(Id);
            return entity;
        }

        public AAdministratorPermissionRelationDto Edit(int Id)
        {
            var entity = _aadministratorPermissionRelation.GetByKey(Id);
            var dto = Mapper.Map<AAdministratorPermissionRelation, AAdministratorPermissionRelationDto>(entity);
            return dto;
        }

        public IQueryable<AAdministratorPermissionRelation> AAdministratorPermissionRelations
        {
            get { return _aadministratorPermissionRelation.Entities; }
        }


        public OperationResult Insert(params AAdministratorPermissionRelationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _aadministratorPermissionRelation.Insert(dtos,
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

        public OperationResult Insert(params AAdministratorPermissionRelation[] adminpers)
        {
            return _aadministratorPermissionRelation.Insert((IEnumerable<AAdministratorPermissionRelation>)adminpers) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public OperationResult Update(params AAdministratorPermissionRelationDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _aadministratorPermissionRelation.Update(dtos,
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

        public OperationResult Update(params AAdministratorPermissionRelation[] adminpers)
        {
            return _aadministratorPermissionRelation.Update(adminpers);
        }

        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _aadministratorPermissionRelation.Entities.Where(m => ids.Contains(m.Id));
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
                var entities = _aadministratorPermissionRelation.Entities.Where(m => ids.Contains(m.Id));
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
                OperationResult result = _aadministratorPermissionRelation.Delete(ids);
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
                var entities = _aadministratorPermissionRelation.Entities.Where(m => ids.Contains(m.Id));
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
                var entities = _aadministratorPermissionRelation.Entities.Where(m => ids.Contains(m.Id)).ToList();
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
