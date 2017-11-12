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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using System.Web.Mvc;
using Whiskey.Utility.Logging;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AnnualLeaveService : ServiceBase, IAnnualLeaveContract
    {
        #region 声明数据层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(AnnualLeaveService));

        private readonly IRepository<AnnualLeave, int> _annualLeaveRepository;

        private readonly IRepository<Administrator, int> _adminRepository;

        private readonly IRepository<Rest, int> _restRepository;

        public AnnualLeaveService(IRepository<AnnualLeave, int> AnnualLeaveRepository,
            IRepository<Administrator, int> adminRepository,
            IRepository<Rest, int> restRepository)
            : base(AnnualLeaveRepository.UnitOfWork)
		{
            _annualLeaveRepository = AnnualLeaveRepository;
            _adminRepository = adminRepository;
            _restRepository = restRepository;
		}
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AnnualLeave View(int Id)
        {
            var entity = _annualLeaveRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象
                
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AnnualLeaveDto Edit(int Id)
        {
            var entity = _annualLeaveRepository.GetByKey(Id);
            Mapper.CreateMap<AnnualLeave, AnnualLeaveDto>();
            var dto = Mapper.Map<AnnualLeave, AnnualLeaveDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
               
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<AnnualLeave> AnnualLeaves { get { return _annualLeaveRepository.Entities; } }
        #endregion

        #region 添加数据
               
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params AnnualLeaveDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                List<Rest> listAddRest = new List<Rest>();
                List<Rest> listUpdateRest = new List<Rest>();
                IQueryable<AnnualLeave> listAnnualLeave = this.AnnualLeaves.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                IQueryable<Administrator> listAdmin= _adminRepository.Entities.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                List<Rest> listRest = _restRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                foreach (var dto in dtos)
                {

                    int index  = listAnnualLeave.Where(x => x.AnnualLeaveName == dto.AnnualLeaveName).Count();
                    if (index > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在！");
                    }
                    
                    if (dto.AnnualLeaveType==(int)AnnualLeaveFlag.Department)
                    {
                        //int count = listAnnualLeave.Where(x => x.DepartmentId == dto.DepartmentId).Count();
                        //if (count>0)
                        //{
                        //    return new OperationResult(OperationResultType.Error, "已经对该部门添加年假！");
                        //}
                    }
                    else if (dto.AnnualLeaveType == (int)AnnualLeaveFlag.JobPosition)
                    {
                        IQueryable<AnnualLeave> listAnn = listAnnualLeave.Where(x => (x.StartYear <= dto.StartYear && x.EndYear >= dto.StartYear) || (x.StartYear <= dto.EndYear && x.EndYear >= dto.EndYear));
                        //int count = listAnn.Where(x => x.JobPositionId == dto.JobPositionId).Count();
                        //if(count>0)
                        //{
                        //    return new OperationResult(OperationResultType.Error, "该职位的年限出现重叠！");
                        //}
                        bool res = this.AddOrUpdateRest(dto, listAdmin, listRest, listAddRest, listUpdateRest);
                        if (!res) return new OperationResult(OperationResultType.Error, "添加失败！");
                    }
                }
                if (listAddRest.Count()>0)
                {
                    _restRepository.Insert(listAddRest.AsEnumerable());
                }
                if (listUpdateRest.Count()>0)
                {
                    _restRepository.Update(listUpdateRest);
                }
                OperationResult result = _annualLeaveRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                int resCount = UnitOfWork.SaveChanges();
                return resCount>0?new OperationResult(OperationResultType.Success,"添加成功！")
                    : new OperationResult(OperationResultType.Error,"添加失败！");
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！");
            }
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params AnnualLeaveDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                List<Rest> listAddRest = new List<Rest>();
                List<Rest> listUpdateRest = new List<Rest>();
                IQueryable<AnnualLeave> listAnnualLeave = this.AnnualLeaves.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                IQueryable<Administrator> listAdmin = _adminRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                List<Rest> listRest = _restRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                foreach (var dto in dtos)
                {
                    AnnualLeave annLeave = listAnnualLeave.Where(x => x.AnnualLeaveName == dto.AnnualLeaveName).FirstOrDefault();
                    if (annLeave != null && annLeave.Id!=dto.Id)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在！");
                    }
                    if (dto.AnnualLeaveType == (int)AnnualLeaveFlag.Department)
                    {                        
                        //int count = listAnnualLeave.Where(x => x.AnnualLeaveType == dto.AnnualLeaveType && x.DepartmentId == dto.DepartmentId && x.Id != dto.Id).Count();
                        //if (count > 0)
                        //{
                        //    return new OperationResult(OperationResultType.Error, "已经对该部门添加年假！");
                        //}
                    }
                    else if (dto.AnnualLeaveType == (int)AnnualLeaveFlag.JobPosition)
                    {
                        IQueryable<AnnualLeave> listAnn = listAnnualLeave.Where(x => (x.StartYear <= dto.StartYear && x.EndYear >= dto.StartYear) || (x.StartYear <= dto.EndYear && x.EndYear >= dto.EndYear));
                        //int count = listAnn.Where(x => x.AnnualLeaveType == dto.AnnualLeaveType && x.JobPositionId == dto.JobPositionId).Count();
                        //if (count > 0)
                        //{
                        //    return new OperationResult(OperationResultType.Error, "该职位的年限出现重叠！");
                        //}
                        bool res = this.AddOrUpdateRest(dto, listAdmin, listRest, listAddRest, listUpdateRest);
                        if (!res) return new OperationResult(OperationResultType.Error, "更新失败失败！");
                    }
                }
                if (listAddRest.Count() > 0)
                {
                    _restRepository.Insert(listAddRest.AsEnumerable());
                }
                if (listUpdateRest.Count() > 0)
                {
                    _restRepository.Update(listUpdateRest);
                }

                OperationResult result = _annualLeaveRepository.Update(dtos,
                 dto =>
                 {
                 
                 },
                 (dto, entity) =>
                 {
                     entity.UpdatedTime = DateTime.Now;
                     entity.OperatorId = AuthorityHelper.OperatorId;
                     return entity;
                 });
                int resCount = UnitOfWork.SaveChanges();
                return resCount > 0 ? new OperationResult(OperationResultType.Success, "更新成功！")
                    : new OperationResult(OperationResultType.Error, "更新失败！");
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！" );
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
                var entities = _annualLeaveRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _annualLeaveRepository.Update(entity);
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
                var entities = _annualLeaveRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _annualLeaveRepository.Update(entity);
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
                var entities = _annualLeaveRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _annualLeaveRepository.Update(entity);
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
                var entities = _annualLeaveRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _annualLeaveRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取数据下拉选项列表
        /// <summary>
        /// 获取数据下拉选项列表
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public List<SelectListItem> SelectList(string title)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<AnnualLeave> listAnn = this.AnnualLeaves.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId==null);
            foreach (var item in listAnn)
            {
                list.Add(new SelectListItem() { Text = item.AnnualLeaveName, Value = item.Id.ToString() });
            }
            if (!String.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Text = title, Value =string.Empty });
            }
            return list;
        }
        #endregion

        #region 添加或者更新员工休息档案
        /// <summary>
        /// 添加或者更新员工休息档案
        /// </summary>
        /// <param name="departmentId"></param>
        private bool AddOrUpdateRest(AnnualLeaveDto dto, IQueryable<Administrator> listAdmin, List<Rest> listRest, List<Rest> listAddRest, List<Rest> listUpdateRest)
        {
            try
            {
                DateTime startTime = DateTime.Now.AddYears(dto.EndYear * -1);
                DateTime endTime = DateTime.Now.AddYears(dto.StartYear * -1);
                IQueryable<int> listAdminId = listAdmin.Where(x => x.CreatedTime.CompareTo(startTime) > 0 && x.CreatedTime.CompareTo(endTime) <= 0).Select(x => x.Id);
                if (listAdminId.Count() > 0)
                {
                    foreach (int admindId in listAdminId)
                    {
                        //这个职位的员工是否已经存在的休假
                        Rest rest = listRest.Where(x => x.AdminId == admindId).FirstOrDefault();
                        if (rest == null)
                        {
                            rest = new Rest()
                            {
                                AdminId = admindId,
                                AnnualLeaveDays = dto.Days,
                            };
                            listAddRest.Add(rest);
                        }
                        else
                        {
                            rest.AnnualLeaveDays = dto.Days;
                            listUpdateRest.Add(rest);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return false;
            }
        }
        #endregion

    }
}
