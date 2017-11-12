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
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Web.Mvc;


namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class JobPositionService : ServiceBase, IJobPositionContract
    {
        #region 声明数据层操作对象

        private readonly IRepository<JobPosition, int> _jobPositionRepository;

        private readonly IRepository<Department, int> _departmentRepository;
        private readonly IRepository<Administrator, int> _administratoryRepository;
        private readonly IRepository<Designer, int> _designerRepository;
        private readonly IRepository<Factorys, int> _FactorysRepository;
        private readonly IRepository<AppVerManage, int> _appVerManageRepository;

        public JobPositionService(IRepository<JobPosition, int> JobPositionRepository,
            IRepository<Administrator, int> _administratoryRepository,
            IRepository<Designer, int> _designerRepository,
            IRepository<Factorys, int> _FactorysRepository,
            IRepository<AppVerManage, int> _appVerManageRepository,
            IRepository<Department, int> departmentRepository)
            : base(JobPositionRepository.UnitOfWork)
        {
            _jobPositionRepository = JobPositionRepository;
            _departmentRepository = departmentRepository;
            this._administratoryRepository = _administratoryRepository;
            this._designerRepository = _designerRepository;
            this._FactorysRepository = _FactorysRepository;
            this._appVerManageRepository = _appVerManageRepository;
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JobPosition View(int Id)
        {
            var entity = _jobPositionRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JobPositionDto Edit(int Id)
        {
            var entity = _jobPositionRepository.GetByKey(Id);
            List<int> listId = new List<int>();
            if (entity.Departments != null && entity.Departments.Count > 0)
            {
                listId = entity.Departments.Select(x => x.Id).ToList();
            }
            Mapper.CreateMap<JobPosition, JobPositionDto>();
            var dto = Mapper.Map<JobPosition, JobPositionDto>(entity);
            dto.AnnualLeaveName = entity.AnnualLeave.AnnualLeaveName;
            dto.DepartmentName = entity.Department.DepartmentName;
            dto.WorkTimeName = entity.WorkTime != null ? entity.WorkTime.WorkTimeName : "";
            dto.DepartIds = listId;

            dto.FactoryIds = entity.Factorys.Select(s => s.Id).ToList();
            dto.AppVerIds = entity.AppVerManages.Select(s => s.Id).ToList();

            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<JobPosition> JobPositions { get { return _jobPositionRepository.Entities; } }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params JobPositionDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<JobPosition> listJobPosition = JobPositions.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var listDepart = _departmentRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var listFactory = _FactorysRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var listAppVer = _appVerManageRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var dto in dtos)
                {
                    int count = listJobPosition.Where(x => x.JobPositionName == dto.JobPositionName && x.DepartmentId == dto.DepartmentId).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "职位名称重复");
                    }
                    if (dto.IsLeader == true)
                    {
                        JobPosition jobPosition = listJobPosition.FirstOrDefault(x => x.DepartmentId == dto.DepartmentId && x.IsLeader == true);
                        if (jobPosition != null)
                        {
                            jobPosition.IsLeader = false;
                            jobPosition.UpdatedTime = DateTime.Now;
                            _jobPositionRepository.Update(jobPosition);
                        }
                    }
                }
                OperationResult result = _jobPositionRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    List<int> listId = dto.DepartIds;
                    List<int> listFId = dto.FactoryIds;
                    if (listId != null && listId.Count() > 0)
                    {
                        List<Department> departs = listDepart.Where(x => listId.Contains(x.Id)).ToList();
                        entity.Departments = departs;
                    }
                    if (listFId != null && listFId.Count() > 0)
                    {
                        List<Factorys> factorys = listFactory.Where(x => listFId.Contains(x.Id)).ToList();
                        entity.Factorys = factorys;
                    }
                    if (dto.AppVerIds != null && dto.AppVerIds.Count() > 0)
                    {
                        var appvers = listAppVer.Where(x => dto.AppVerIds.Contains(x.Id)).ToList();
                        entity.AppVerManages = appvers;
                    }
                    return entity;
                });
                int index = UnitOfWork.SaveChanges();
                RedisCacheHelper.ResetStoreDepartmentMangeStoreCache();
                return index > 0 ? new OperationResult(OperationResultType.Success, "添加成功") : new OperationResult(OperationResultType.Error, "添加失败");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params JobPositionDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<JobPosition> listJobPosition = JobPositions.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                var listFactory = _FactorysRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var listAppVer = _appVerManageRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var dto in dtos)
                {
                    int count = listJobPosition.Where(x => x.Id != dto.Id && x.JobPositionName == dto.JobPositionName && x.DepartmentId == dto.DepartmentId).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "职位名称重复");
                    }
                    if (dto.IsLeader == true)
                    {
                        JobPosition jobPosition = listJobPosition.FirstOrDefault(x => x.DepartmentId == dto.DepartmentId && x.IsLeader == true && dto.Id != x.Id);
                        if (jobPosition != null)
                        {
                            jobPosition.IsLeader = false;
                            jobPosition.UpdatedTime = DateTime.Now;
                            _jobPositionRepository.Update(jobPosition);
                        }
                    }

                }
                OperationResult result = _jobPositionRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.Departments.Clear();
                        List<int> listId = dto.DepartIds;
                        if (listId != null && listId.Count > 0)
                        {
                            entity.Departments = _departmentRepository.Entities.Where(x => listId.Contains(x.Id)).ToList();
                        }

                        entity.Factorys.Clear();
                        List<int> listFId = dto.FactoryIds;
                        if (listFId != null && listFId.Count() > 0)
                        {
                            List<Factorys> factorys = listFactory.Where(x => listFId.Contains(x.Id)).ToList();
                            entity.Factorys = factorys;
                        }

                        entity.AppVerManages.Clear();
                        if (dto.AppVerIds != null && dto.AppVerIds.Count() > 0)
                        {
                            var appvers = listAppVer.Where(x => dto.AppVerIds.Contains(x.Id)).ToList();
                            entity.AppVerManages = appvers;
                        }

                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int index = UnitOfWork.SaveChanges();
                RedisCacheHelper.ResetStoreDepartmentMangeStoreCache();
                return index > 0 ? new OperationResult(OperationResultType.Success, "修改成功") : new OperationResult(OperationResultType.Error, "修改失败");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 移除数据

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
                var entities = _jobPositionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _jobPositionRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 恢复数据

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
                IQueryable<JobPosition> listJobPosition = _jobPositionRepository.Entities;
                var entities = listJobPosition.Where(m => ids.Contains(m.Id));
                IQueryable<JobPosition> listEntity = listJobPosition.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var entity in entities)
                {
                    int count = listEntity.Where(x => x.JobPositionName == entity.JobPositionName && x.Id != entity.Id).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "恢复数据与正常数据名字出现重复，请先删除重复数据再恢复");
                    }
                    else
                    {
                        entity.IsDeleted = false;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        _jobPositionRepository.Update(entity);
                    }
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 启用数据

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
                IQueryable<JobPosition> listJobPosition = _jobPositionRepository.Entities;
                var entities = listJobPosition.Where(m => ids.Contains(m.Id));
                IQueryable<JobPosition> listEntity = listJobPosition.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var entity in entities)
                {
                    int count = listEntity.Where(x => x.JobPositionName == entity.JobPositionName && x.Id != entity.Id).Count();
                    if (count > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "启用数据与正常数据名字出现重复，请先删除重复数据再恢复");
                    }
                    else
                    {
                        entity.IsEnabled = true;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        _jobPositionRepository.Update(entity);
                    }
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 禁用数据

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
                var entities = _jobPositionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _jobPositionRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取下拉菜单集合
        public List<SelectListItem> SelectList(string title, int departmentId)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<JobPosition> listJob = this.JobPositions.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.DepartmentId == departmentId);
            foreach (var item in listJob)
            {
                list.Add(new SelectListItem() { Text = item.JobPositionName, Value = item.Id.ToString() });
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Text = title, Value = string.Empty });
            }
            return list;
        }

        #endregion


        public List<Designer> GetManagedDesigners(int AdminId)
        {
            List<Designer> listD = new List<Designer>();
            var modA = _administratoryRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).FirstOrDefault(f => f.Id == AdminId);
            if (modA.IsNotNull())
            {
                var list = _jobPositionRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted && w.Id == modA.JobPositionId).SelectMany(s => s.Factorys.Where(w => w.IsEnabled && !w.IsDeleted).SelectMany(ss => ss.Designers.Where(w => w.IsEnabled && !w.IsDeleted))).ToList();
                listD.AddRange(list);

                var modD = _designerRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).FirstOrDefault(f => f.AdminId == AdminId);
                if (modD.IsNotNull())
                {
                    listD.Add(modD);
                }
            }
            return listD.DistinctBy(d => d.Id).ToList();
        }
    }
}
