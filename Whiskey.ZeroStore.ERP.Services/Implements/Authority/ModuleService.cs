




using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using System.Data.Entity;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{

    public class ModuleService : ServiceBase, IModuleContract
    {
        #region ModuleService


        private readonly IRepository<Module, int> _moduleRepository;
        private readonly IRepository<Administrator, int> _adminRepo;

        public ModuleService(
            IRepository<Module, int> moduleRepository,
            IRepository<Administrator,int> adminRepo
            ) : base(moduleRepository.UnitOfWork)
        {
            _moduleRepository = moduleRepository;
            _adminRepo = adminRepo;
        }


        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		public Module View(int Id)
        {
            var entity = _moduleRepository.GetByKey(Id);
            return entity;
        }


        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		public ModuleDto Edit(int Id)
        {
            var entity = _moduleRepository.GetByKey(Id);
            Mapper.CreateMap<Module, ModuleDto>();
            var dto = Mapper.Map<Module, ModuleDto>(entity);
            return dto;
        }


        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Module> Modules { get { return _moduleRepository.Entities; } }



        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Module, bool>> predicate, int id = 0)
        {
            return _moduleRepository.ExistsCheck(predicate, id);
        }


        #region 添加数据        
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params ModuleDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Module> listModule = _moduleRepository.Entities;
                foreach (var dto in dtos)
                {
                    int count = listModule.Where(x => x.ModuleName == dto.ModuleName).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败,该模块名称已经存在");
                    }
                    count = listModule.Where(x => !string.IsNullOrEmpty(dto.OverrideUrl) && dto.OverrideUrl.Equals(x.OverrideUrl, StringComparison.OrdinalIgnoreCase)).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败,重写路径已经存在");
                    }

                    if (!dto.ParentId.HasValue)
                    {
                        dto.Sequence = listModule.Where(w => w.ParentId == null).Count();
                    }
                    else
                    {
                        dto.Sequence = listModule.Where(w => w.ParentId == dto.ParentId).Count();
                    }
                }
                OperationResult result = _moduleRepository.Insert(dtos,
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

        #endregion

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params ModuleDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                var listModule = this.Modules;
                foreach (var dto in dtos)
                {
                    int count = listModule.Where(x => x.ModuleName == dto.ModuleName && x.Id != dto.Id).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败,该模块名称已经存在");
                    }
                    count = listModule.Where(x => !string.IsNullOrEmpty(dto.OverrideUrl) && dto.OverrideUrl.Equals(x.OverrideUrl, StringComparison.OrdinalIgnoreCase) && x.Id != dto.Id).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "添加失败,重写路径已经存在");
                    }
                }
                OperationResult result = _moduleRepository.Update(dtos,
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



        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">要移除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _moduleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _moduleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">要恢复的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _moduleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _moduleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">要删除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _moduleRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }

        }


        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">要启用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Enable(params int[] ids)
        {

            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _moduleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _moduleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }


        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">要禁用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _moduleRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _moduleRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }




        #endregion

        #region 获取模块键值对集合
        /// <summary>
        /// 获取模块键值对集合
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public List<KeyValue<string, string>> SelectList(string title)
        {
            List<KeyValue<string, string>> list = new List<KeyValue<string, string>>();
            IQueryable<Module> listModule = _moduleRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == null);
            foreach (var module in listModule)
            {
                list.Add(new KeyValue<string, string> { Key = module.Id.ToString(), Value = module.ModuleName });
            }
            list.Insert(0, new KeyValue<string, string> { Key = "0", Value = title });
            return list;
        }
        #endregion


        public OperationResult CompleteRule(int Id)
        {

            var modu = _moduleRepository.GetByKey(Id);
            modu.IsCompleteRule = true;
            return _moduleRepository.Update(modu) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public OperationResult NoCompleteRule(int Id)
        {
            var modu = _moduleRepository.GetByKey(Id);
            modu.IsCompleteRule = false;
            return _moduleRepository.Update(modu) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }


        #region 排序设置
        public OperationResult SetSeq(int Id, int SequenceType)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Module module = this.Modules.FirstOrDefault(x => x.Id == Id);
            var listModule = this.Modules.Where(x => x.ParentId == module.ParentId).ToList();

            UnitOfWork.TransactionEnabled = true;

            List<Module> listEntity = new List<Module>();
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
                        module.OperatorId = AuthorityHelper.OperatorId;
                        Module entity = listModule.FirstOrDefault(x => x.Sequence == module.Sequence && x.Id != module.Id);
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
                    if (module.Sequence >= listModule.Count - 1)
                    {
                        oper.Message = "已经是最低排序了";
                        return oper;
                    }
                    else
                    {
                        module.Sequence = module.Sequence + 1;
                        module.UpdatedTime = DateTime.Now;
                        module.OperatorId = AuthorityHelper.OperatorId;
                        Module entity = listModule.FirstOrDefault(x => x.Sequence == module.Sequence && x.Id != module.Id);
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
            _moduleRepository.Update(listEntity);
            return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "操作成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
        }
        #endregion



        public List<int> GetPermissionedAdminIds(string controllerName, string areaName, int? departmentId, string[] filterFlags)
        {

            // module search
            var query = _moduleRepository.Entities.Where(c => !c.IsDeleted && c.IsEnabled);
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                throw new ArgumentException(nameof(controllerName));
            }
            query = query.Where(m => m.PageController == controllerName);
            if (!string.IsNullOrWhiteSpace(areaName))
            {
                query = query.Where(m => m.PageArea == areaName);
            }


            // filter permissions under module 
            var flagQuery = query.SelectMany(m => m.Permissions
                                                  .Where(p => !p.IsDeleted && p.IsEnabled));
            if (filterFlags != null && filterFlags.Length > 0)
            {
                flagQuery = flagQuery.Where(p => filterFlags.Contains(p.OnlyFlag));
            }

            var permissionIds =  flagQuery.Select(p => p.Id).ToList();


            // search adminid by permissionid
            var adminQuery = _adminRepo.Entities.Where(a => !a.IsDeleted && a.IsEnabled)
                .Where(a => a.Roles.Any(r => permissionIds.All(id => r.ARolePermissionRelations.Any(ar => ar.PermissionsId == id))));
            if (departmentId.HasValue)
            {
                adminQuery = adminQuery.Where(a => a.JobPosition.Departments.Any(d => d.Id == departmentId));
            }
            var adminIds = adminQuery.Select(a => a.Id).ToList();
            return adminIds;
        }


    }
}
