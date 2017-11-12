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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using System.Xml;
using System.Xml.Linq;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class LeaveInfoService : ServiceBase, ILeaveInfoContract
    {
        #region 声明数据层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(LeaveInfoService));

        protected readonly IRepository<LeaveInfo, int> _leaveInfoRepository;

        private readonly IRepository<Administrator, int> _adminRepository;

        private readonly IRepository<Rest, int> _restRepository;

        private readonly IRepository<Attendance, int> _attendanceRepository;
        private readonly IRepository<WorkTimeDetaile, int> _workTimeDetaileRepository;
        private readonly IRepository<Holiday, int> _holidayRepository;
        private readonly IRepository<Member, int> _memberRepository;
        private readonly IMemberContract _memberContract;
        private readonly IMemberConsumeContract _memberConsumeContract;
        public LeaveInfoService(IRepository<LeaveInfo, int> leaveInfoRepository,
            IRepository<Administrator, int> adminRepository,
            IRepository<Rest, int> restRepository,
            IRepository<Attendance, int> attendanceRepository,
              IRepository<WorkTimeDetaile, int> workTimeDetaileRepository,
                IRepository<Holiday, int> holidayRepository,
                IRepository<Member, int> memberRepository,
                IMemberContract memberContract,
                IMemberConsumeContract memberConsumeContract)
            : base(leaveInfoRepository.UnitOfWork)
        {
            _leaveInfoRepository = leaveInfoRepository;
            _adminRepository = adminRepository;
            _restRepository = restRepository;
            _attendanceRepository = attendanceRepository;
            _workTimeDetaileRepository = workTimeDetaileRepository;
            _holidayRepository = holidayRepository;
            _memberRepository = memberRepository;
            _memberConsumeContract = memberConsumeContract;
            _memberContract = memberContract;
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public LeaveInfo View(int Id)
        {
            var entity = _leaveInfoRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public LeaveInfoDto Edit(int Id)
        {
            var entity = _leaveInfoRepository.GetByKey(Id);
            Mapper.CreateMap<LeaveInfo, LeaveInfoDto>();
            var dto = Mapper.Map<LeaveInfo, LeaveInfoDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<LeaveInfo> LeaveInfos { get { return _leaveInfoRepository.Entities; } }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params LeaveInfoDto[] dtos)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍候访问");
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<LeaveInfo> listLeaveInfo = this.LeaveInfos.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType != (int)VerifyFlag.NoPass);
                IQueryable<Rest> listRest = _restRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                //作为请假日期是否重复的判断
                bool isHave = false;
                LeaveInfoDto[] dtos_new = new LeaveInfoDto[] { };
                UnitOfWork.TransactionEnabled = true;
                foreach (LeaveInfoDto dto in dtos)
                {
                    Administrator admin = _adminRepository.Entities.FirstOrDefault(a => a.Id == dto.AdminId);
                    if (admin == null)
                    {
                        continue;
                    }
                    if (dto.LeaveMethod != 1)
                    {
                        if (admin.IsPersonalTime)
                        {
                            var workDeatile = admin.WorkTime == null ? null : _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.Year == dto.StartTime.Year && x.Month == dto.StartTime.Month
                  && x.WorkDay == dto.StartTime.Day && x.WorkTimeId == admin.WorkTime.Id);
                            if (workDeatile == null)
                            {
                                continue;
                            }
                            if (workDeatile.WorkTimeType == 2)
                            {
                                continue;
                            }
                            if (dto.AmOrPm == 2)
                            {
                                dto.StartTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmStartTime);
                            }
                            else
                            {
                                dto.StartTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + workDeatile.AmStartTime);
                            }
                            if (dto.AmOrPm == 1)
                            {
                                dto.EndTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + workDeatile.AmEndTime);
                            }
                            else
                            {
                                dto.EndTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + workDeatile.PmEndTime);
                            }
                        }
                        else
                        {
                            if (admin.JobPosition == null || admin.JobPosition.WorkTime == null)
                            {
                                continue;
                            }
                            if (dto.AmOrPm == 2)
                            {
                                dto.StartTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmStartTime);
                            }
                            else
                            {
                                dto.StartTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmStartTime);
                            }
                            if (dto.AmOrPm == 1)
                            {
                                dto.EndTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmEndTime);
                            }
                            else
                            {
                                dto.EndTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmEndTime);
                            }
                        }
                    }
                    IQueryable<LeaveInfo> listAtt = listLeaveInfo.Where(x => x.AdminId == dto.AdminId);
                    isHave = listAtt.Any(x => (x.StartTime <= dto.StartTime && x.EndTime >= dto.StartTime) || (x.StartTime <= dto.EndTime && x.EndTime >= dto.EndTime));
                    if (isHave == true)
                    {
                        oper.Message = "请假时间重复！";
                        return oper;
                    }
                    if (dto.LeaveMethod == 1)
                    {
                        oper = _memberContract.CheckLeavePointsInfo(Convert.ToDecimal(dto.LeaveDays), dto.AdminId, "leave");
                        if (oper.ResultType == OperationResultType.Error)
                        {
                            //普通请假验证积分
                            return oper;
                        }
                        dto.DeductionLeavePoints = Convert.ToDecimal(oper.Data);
                        Administrator adm = _adminRepository.Entities.Single(a => a.Id == dto.AdminId);
                        //扣除修改后需要的积分
                        _memberContract.ReturnPoints(adm.Member == null ? 0 : adm.Member.Id, -dto.DeductionLeavePoints);
                    }

                    Rest rest = listRest.Where(x => x.AdminId == dto.AdminId).FirstOrDefault();
                    if (rest != null)
                    {
                        if (dto.AnnualLeaveDays < 0 || dto.PaidLeaveDays < 0 || dto.ChangeRestDays < 0)
                        {
                            oper.Message = "数据异常";
                            return oper;
                        }
                        double annDays = rest.AnnualLeaveDays - dto.AnnualLeaveDays;
                        if (annDays >= 0)
                        {
                            rest.AnnualLeaveDays = annDays;
                        }
                        else
                        {
                            oper.Message = "选择的年假天数超出了你可使用的天数！";
                            return oper;
                        }
                        double paidDays = rest.PaidLeaveDays - dto.PaidLeaveDays;
                        if (paidDays >= 0)
                        {
                            rest.PaidLeaveDays = paidDays;
                        }
                        else
                        {
                            oper.Message = "选择的带薪休假年假天数超出了你可使用的天数！";
                            return oper;
                        }
                        rest.UpdatedTime = DateTime.Now;
                        rest.OperatorId = dto.AdminId;
                        _restRepository.Update(rest);
                    }
                }
                OperationResult result = _leaveInfoRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "添加成功") : new OperationResult(OperationResultType.Error, "添加失败");
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return oper;
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert_API(params LeaveInfoDto[] dtos)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍候访问");
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<LeaveInfo> listLeaveInfo = this.LeaveInfos.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType != (int)VerifyFlag.NoPass);
                IQueryable<Rest> listRest = _restRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                //作为请假日期是否重复的判断
                bool isHave = false;
                foreach (LeaveInfoDto dto in dtos)
                {
                    Administrator admin = _adminRepository.Entities.FirstOrDefault(a => a.Id == dto.AdminId);
                    if (admin == null)
                    {
                        continue;
                    }
                    if (dto.LeaveMethod != 1)
                    {
                        if (admin.IsPersonalTime)
                        {
                            var workDeatile = admin.WorkTime == null ? null : _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.Year == dto.StartTime.Year && x.Month == dto.StartTime.Month
                  && x.WorkDay == dto.StartTime.Day && x.WorkTimeId == admin.WorkTime.Id);
                            if (workDeatile == null)
                            {
                                continue;
                            }
                            if (workDeatile.WorkTimeType == 2)
                            {
                                continue;
                            }
                            if (dto.AmOrPm == 2)
                            {
                                dto.StartTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmStartTime);
                            }
                            else
                            {
                                dto.StartTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + workDeatile.AmStartTime);
                            }
                            if (dto.AmOrPm == 1)
                            {
                                dto.EndTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + workDeatile.AmEndTime);
                            }
                            else
                            {
                                dto.EndTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + workDeatile.PmEndTime);
                            }
                        }
                        else
                        {
                            if (admin.JobPosition == null || admin.JobPosition.WorkTime == null)
                            {
                                continue;
                            }
                            if (dto.AmOrPm == 2)
                            {
                                dto.StartTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmStartTime);
                            }
                            else
                            {
                                dto.StartTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmStartTime);
                            }
                            if (dto.AmOrPm == 1)
                            {
                                dto.EndTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.AmEndTime);
                            }
                            else
                            {
                                dto.EndTime = DateTime.Parse(dto.StartTime.ToShortDateString() + " " + admin.JobPosition.WorkTime.PmEndTime);
                            }
                        }
                    }

                    IQueryable<LeaveInfo> listAtt = listLeaveInfo.Where(x => x.AdminId == dto.AdminId);
                    isHave = listAtt.Any(x => (x.StartTime <= dto.StartTime && x.EndTime > dto.StartTime) || (x.StartTime < dto.EndTime && x.EndTime >= dto.EndTime));
                    if (isHave == true)
                    {
                        oper.Message = "请假时间重复！";
                        return oper;
                    }
                    if (dto.LeaveMethod == 1)
                    {
                        oper = _memberContract.CheckLeavePointsInfo(Convert.ToDecimal(dto.LeaveDays), dto.AdminId, "leave");
                        if (oper.ResultType == OperationResultType.Error)
                        {
                            //普通请假验证积分
                            return oper;
                        }
                        dto.DeductionLeavePoints = Convert.ToDecimal(oper.Data);
                        Administrator adm = _adminRepository.Entities.Single(a => a.Id == dto.AdminId);
                        //扣除修改后需要的积分
                        _memberContract.ReturnPoints(adm.Member == null ? 0 : adm.Member.Id, -dto.DeductionLeavePoints);
                    }

                    Rest rest = listRest.Where(x => x.AdminId == dto.AdminId).FirstOrDefault();
                    if (rest != null)
                    {
                        if (dto.AnnualLeaveDays < 0 || dto.PaidLeaveDays < 0 || dto.ChangeRestDays < 0)
                        {
                            oper.Message = "数据异常";
                            return oper;
                        }
                        double annDays = rest.AnnualLeaveDays - dto.AnnualLeaveDays;
                        if (annDays >= 0)
                        {
                            rest.AnnualLeaveDays = annDays;
                        }
                        else
                        {
                            oper.Message = "选择的年假天数超出了你可使用的天数！";
                            return oper;
                        }
                        double paidDays = rest.PaidLeaveDays - dto.PaidLeaveDays;
                        if (paidDays >= 0)
                        {
                            rest.PaidLeaveDays = paidDays;
                        }
                        else
                        {
                            oper.Message = "选择的带薪休假年假天数超出了你可使用的天数！";
                            return oper;
                        }
                        rest.UpdatedTime = DateTime.Now;
                        rest.OperatorId = dto.AdminId;
                        _restRepository.Update(rest);
                    }
                }
                OperationResult result = _leaveInfoRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                if (result.ResultType != OperationResultType.Success)
                {
                    return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "添加成功") : new OperationResult(OperationResultType.Error, "添加失败");
                }
                else
                {
                    result.Message = result.ResultType == OperationResultType.Success ? "添加成功" : "添加失败";
                    return result;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return oper;
            }
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params LeaveInfoDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<LeaveInfo> list = this.LeaveInfos;
                IQueryable<Rest> listRest = _restRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var dto in dtos)
                {
                    LeaveInfo leaveInfo = list.Where(x => x.Id == dto.Id && x.AdminId == dto.AdminId).FirstOrDefault();
                    if (leaveInfo == null)
                    {
                        return new OperationResult(OperationResultType.Error, "修改失败，数据不存在");
                    }
                    else
                    {
                        if (leaveInfo.VerifyType == (int)VerifyFlag.Pass)
                        {
                            return new OperationResult(OperationResultType.Error, "审核通过，不能修改数据");
                        }
                        else
                        {
                            IQueryable<LeaveInfo> listAtt = list.Where(x => x.AdminId == dto.AdminId);
                            foreach (LeaveInfo att in listAtt)
                            {
                                if (att.Id != dto.Id && ((att.StartTime <= dto.StartTime && att.EndTime >= dto.StartTime) || (att.StartTime <= dto.EndTime && att.EndTime >= dto.EndTime)))
                                {
                                    return new OperationResult(OperationResultType.Error, "请假时间段有重叠，请重新选择时间段！");
                                }
                            }
                            if (dto.LeaveMethod == 1)
                            {
                                var oper = _memberContract.CheckLeavePointsInfo(Convert.ToDecimal(dto.LeaveDays), dto.AdminId, "leave");
                                if (oper.ResultType == OperationResultType.Error)
                                {
                                    //普通请假验证积分
                                    return oper;
                                }
                                Administrator adm = _adminRepository.Entities.Single(a => a.Id == dto.AdminId);
                                //将上次扣除积分返还
                                _memberContract.ReturnPoints(adm.Member == null ? 0 : adm.Member == null ? 0 : adm.Member.Id, leaveInfo.DeductionLeavePoints);

                                dto.DeductionLeavePoints = Convert.ToDecimal(oper.Data);
                                //扣除修改后需要的积分
                                _memberContract.ReturnPoints(adm.Member.Id, -dto.DeductionLeavePoints);
                            }
                            Rest rest = listRest.Where(x => x.AdminId == dto.AdminId).FirstOrDefault();
                            if (rest != null)
                            {
                                double annDays = rest.AnnualLeaveDays + leaveInfo.AnnualLeaveDays - dto.AnnualLeaveDays;
                                if (annDays >= 0)
                                {
                                    rest.AnnualLeaveDays = annDays;
                                }
                                else
                                {
                                    return new OperationResult(OperationResultType.Error, "请选择的年假天数超出了你可使用的天数！");
                                }
                                double chanDays = rest.ChangeRestDays + leaveInfo.ChangeRestDays - dto.ChangeRestDays;
                                if (chanDays >= 0)
                                {
                                    rest.ChangeRestDays = chanDays;
                                }
                                else
                                {
                                    return new OperationResult(OperationResultType.Error, "请选择的调休假天数超出了你可使用的天数！");
                                }
                                rest.UpdatedTime = DateTime.Now;
                                rest.OperatorId = dto.AdminId;
                                _restRepository.Update(rest);
                            }
                        }
                    }
                }

                OperationResult result = _leaveInfoRepository.Update(dtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "更新成功") : new OperationResult(OperationResultType.Error, "更新失败");
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
                var entities = _leaveInfoRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _leaveInfoRepository.Update(entity);
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
                var entities = _leaveInfoRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _leaveInfoRepository.Update(entity);
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
                var entities = _leaveInfoRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _leaveInfoRepository.Update(entity);
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
                var entities = _leaveInfoRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _leaveInfoRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 审核数据
        /// <summary>
        /// 审核数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public OperationResult Verify(LeaveInfoDto dto)
        {
            try
            {
                int adminId = dto.AdminId;
                OperationResult oper = new OperationResult(OperationResultType.Error, "审核数据不存在");
                LeaveInfo leaveInfo = _leaveInfoRepository.Entities.Where(x => x.AdminId == adminId && x.Id == dto.Id).FirstOrDefault();
                if (leaveInfo == null)
                {
                    return oper;
                }
                else
                {
                    if (leaveInfo.VerifyType == (int)VerifyFlag.Pass)
                    {
                        if (dto.VerifyType == (int)VerifyFlag.NoPass)
                        {
                            oper = VerifyNoPass(leaveInfo);
                            return oper;
                        }
                    }
                    else
                    {
                        if (dto.VerifyType == (int)VerifyFlag.Pass)
                        {
                            oper = VerifyPass(leaveInfo, adminId);
                            return oper;
                        }
                        else if (dto.VerifyType == (int)VerifyFlag.NoPass)
                        {
                            oper = VerifyNoPass(leaveInfo);
                            return oper;
                        }
                    }
                    return oper;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后访问");
            }
        }
        #endregion

        #region 审核不通过
        /// <summary>
        /// 审核不通过
        /// </summary>
        /// <param name="leaveInfo"></param>
        /// <returns></returns>
        private OperationResult VerifyNoPass(LeaveInfo leaveInfo)
        {
            UnitOfWork.TransactionEnabled = true;
            leaveInfo.VerifyType = (int)VerifyFlag.NoPass;
            leaveInfo.UpdatedTime = DateTime.Now;
            leaveInfo.OperatorId = AuthorityHelper.OperatorId;
            int num = _leaveInfoRepository.Update(leaveInfo);
            Administrator admin = _adminRepository.Entities.Where(x => x.Id == leaveInfo.AdminId).FirstOrDefault();
            _memberContract.ReturnPoints(admin.Member == null ? 0 : admin.Member.Id, leaveInfo.DeductionLeavePoints);
            return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "审核成功") : new OperationResult(OperationResultType.Error, "审核失败");
        }
        #endregion

        #region 审核通过
        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="leaveInfo"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private OperationResult VerifyPass(LeaveInfo leaveInfo, int adminId)
        {
            Administrator admin = _adminRepository.Entities.Where(x => x.Id == adminId).FirstOrDefault();
            var member = admin.Member;
            if (admin == null) return new OperationResult(OperationResultType.Error, "审核的员工不存在");
            if (admin.JobPosition == null) return new OperationResult(OperationResultType.Error, "该员工没有工作职位，添加数据失败");
            //List<Attendance> listAtten = new List<Attendance>();
            leaveInfo.VerifyType = (int)VerifyFlag.Pass;
            leaveInfo.UpdatedTime = DateTime.Now;
            leaveInfo.OperatorId = AuthorityHelper.OperatorId;
            if (_leaveInfoRepository.Update(leaveInfo) <= 0)
            {
                _memberContract.ReturnPoints(member.Id, leaveInfo.DeductionLeavePoints);
                return new OperationResult(OperationResultType.Error, "审核失败");
            }
            _memberConsumeContract.LogScoreWhenLeave((int)admin.MemberId, leaveInfo.DeductionLeavePoints);
            //审核通过后 处理
            var _rest = _restRepository.Entities.FirstOrDefault(x => x.AdminId == adminId);
            if (_rest == null)
            {
                return new OperationResult(OperationResultType.Error, "审核成功");
            }
            if (leaveInfo.LeaveMethod == 1)
            {
                return new OperationResult(OperationResultType.Success, "审核成功");
            }
            switch (leaveInfo.LeaveMethod)
            {
                case 0:
                    _rest.AnnualLeaveDays = _rest.AnnualLeaveDays - leaveInfo.UseAnnualLeaveDay;
                    break;
                case 2:
                    _rest.PaidLeaveDays = _rest.PaidLeaveDays - leaveInfo.UseAnnualLeaveDay;
                    break;
            }
            if (_restRepository.Update(_rest) <= 0)
            {
                //ReturnPoints(member, leaveInfo.DeductionLeavePoints);
                return new OperationResult(OperationResultType.Error, "审核失败");
            }
            return new OperationResult(OperationResultType.Success, "审核成功");
        }
        #endregion
    }
}
