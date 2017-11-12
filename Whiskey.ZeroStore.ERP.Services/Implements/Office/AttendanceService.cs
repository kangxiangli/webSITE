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
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.OfficeInfo;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using System.Data.Entity;
using System.Xml;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AttendanceService : ServiceBase, IAttendanceContract
    {
        #region 声明数据层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceService));

        private readonly IRepository<Attendance, int> _attendanceRepository;

        private readonly IRepository<LeaveInfo, int> _leaveInfoRepository;

        private readonly IRepository<WorkTime, int> _workTimeRepository;

        private readonly IRepository<Administrator, int> _adminRepository;

        private readonly IRepository<Overtime, int> _overtimeRepository;

        private readonly IRepository<Holiday, int> _holidayRepository;

        private readonly IRepository<AttendanceStatistics, int> _attenStatisticsRepository;
        private readonly IRepository<WorkTimeDetaile, int> _workTimeDetaileRepository;
        private readonly IRepository<AttendanceRepair, int> _attendanceRepairRepository;
        private readonly IStoreStatisticsContract _statContract;
        private readonly IConfigureContract _configureContract;
        public AttendanceService(IRepository<Attendance, int> attendanceRepository,
            IRepository<LeaveInfo, int> leaveInfoRepository,
            IRepository<WorkTime, int> workTimeRepository,
            IRepository<Administrator, int> adminRepository,
            IRepository<Overtime, int> overtimeRepository,
            IRepository<Holiday, int> holidayRepository,
            IRepository<AttendanceStatistics, int> attenStatisticsRepository,
            IRepository<AttendanceRepair, int> attendanceRepairRepository,
            IRepository<WorkTimeDetaile, int> workTimeDetaileRepository,
            IStoreStatisticsContract statContract,
            IConfigureContract configureContract)
            : base(attendanceRepository.UnitOfWork)
        {
            _attendanceRepository = attendanceRepository;
            _leaveInfoRepository = leaveInfoRepository;
            _workTimeRepository = workTimeRepository;
            _adminRepository = adminRepository;
            _overtimeRepository = overtimeRepository;
            _holidayRepository = holidayRepository;
            _attenStatisticsRepository = attenStatisticsRepository;
            _attendanceRepairRepository = attendanceRepairRepository;
            _workTimeDetaileRepository = workTimeDetaileRepository;
            _statContract = statContract;
            _configureContract = configureContract;
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Attendance View(int Id)
        {
            var entity = _attendanceRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AttendanceDto Edit(int Id)
        {
            var entity = _attendanceRepository.GetByKey(Id);
            Mapper.CreateMap<Attendance, AttendanceDto>();
            var dto = Mapper.Map<Attendance, AttendanceDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Attendance> Attendances { get { return _attendanceRepository.Entities; } }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params AttendanceDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Attendance> listAttendance = this.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (AttendanceDto dto in dtos)
                {
                    IQueryable<Attendance> listAtt = listAttendance.Where(x => x.AdminId == dto.AdminId);
                    foreach (Attendance att in listAtt)
                    {
                        //if ((att.StartTime<=dto.StartTime || att.EndTime>=dto.StartTime) || (att.EndTime<=dto.EndTime || att.EndTime>=dto.EndTime))
                        //{
                        //    return new OperationResult(OperationResultType.Error, "请假时间段有重叠，请重写选择时间段！");
                        //}
                    }
                }
                OperationResult result = _attendanceRepository.Insert(dtos,
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
        public OperationResult Update(params AttendanceDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _attendanceRepository.Update(dtos,
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
                var entities = _attendanceRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRepository.Update(entity);
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
                var entities = _attendanceRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRepository.Update(entity);
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
                var entities = _attendanceRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRepository.Update(entity);
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
                var entities = _attendanceRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 签到
        /// <summary>
        /// 签到
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="jmpDep">跳过部门参数</param>
        /// <returns></returns>
        public OperationResult LoginIn(int adminId, Department department, bool jmpDep = false)
        {
            try
            {
                OperationResult oper = CheckWorkTime(adminId);
                if (oper.ResultType == OperationResultType.Success)
                {
                    WorkTime workTime = oper.Data as WorkTime;
                    Attendance entity;
                    oper = CheckLoginIn(workTime, adminId);
                    if (oper.ResultType == OperationResultType.Success)
                    {
                        entity = oper.Data as Attendance;
                        //entity.IsAbsence = 1;
                        if (!jmpDep) entity.Departments.Add(department);
                        oper.Data = null;
                        int count = _attendanceRepository.Insert(entity);

                        var res = _statContract.SetStoreOpenWhenFirstSignIn(adminId);
                        if (res.ResultType != OperationResultType.Success)
                        {
                            _Logger.Error(res.Message);
                        }
                        return count > 0 ? new OperationResult(OperationResultType.Success, "签到成功", entity.AmStartTime.ToString()) : new OperationResult(OperationResultType.Error, "签到失败");
                    }
                    else
                    {
                        oper.Data = null;
                        return oper;
                    }
                }
                else
                {
                    oper.Data = null;
                    return oper;
                }

            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            }
        }
        #endregion

        #region 打卡校验

        /// <summary>
        /// 登录校验
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="nowTime"></param>
        /// <returns></returns>

        private Attendance CheckLogin(int adminId, DateTime nowTime)
        {
            IQueryable<Attendance> listAttendance = this.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId);
            Attendance entity = listAttendance.Where(x => nowTime.Year == x.AttendanceTime.Year && nowTime.Month == x.AttendanceTime.Month && nowTime.Day == x.AttendanceTime.Day).FirstOrDefault();
            return entity;
        }
        /// <summary>
        /// 签到校验
        /// </summary>
        /// <param name="workTime"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private OperationResult CheckLoginIn(WorkTime workTime, int adminId)
        {
            DateTime nowTime = DateTime.Now;
            OperationResult oper = new OperationResult(OperationResultType.Error, "");
            Attendance entity = CheckLogin(adminId, nowTime);
            Dictionary<string, int> dic = this.GetHolidays();
            var admin = _adminRepository.Entities.FirstOrDefault(x => x.Id == adminId);
            if (dic == null || dic.Count == 0)
            {
                oper.Message = "请先添加公休假";
                return oper;
            }
            if (admin.IsPersonalTime)
            {
                var _wd = _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId && x.Year == nowTime.Year &&
                x.Month == nowTime.Month && x.WorkDay == nowTime.Day);
                if (_wd != null)
                {
                    workTime.AmStartTime = _wd.AmStartTime;
                    workTime.PmEndTime = _wd.PmEndTime;
                    workTime.WorkTimeType = _wd.WorkTimeType;
                }

                if (nowTime.CompareTo(DateTime.Parse(nowTime.ToShortDateString() + " " + workTime.PmEndTime)) > 0 && workTime.IsFlexibleWork == false)
                {
                    oper.Message = "大于下班时间，禁止签到！";
                    return oper;
                }
            }
            else
            {
                workTime = admin.JobPosition.WorkTime;

                bool isWork = IsWorkDay(nowTime, workTime, dic, admin);
                bool isOverTime = IsOverTime(nowTime, adminId);
                if (isWork == false && isOverTime == false)
                {
                    oper.Message = "不是工作日，禁止签到";
                    return oper;
                }

                if (nowTime.CompareTo(DateTime.Parse(nowTime.ToShortDateString() + " " + workTime.PmEndTime)) > 0 && isOverTime == false && workTime.IsFlexibleWork == false)
                {
                    oper.Message = "大于下班时间，禁止签到！";
                    return oper;
                }
            }
            workTime.IsPersonalTime = admin.IsPersonalTime;
            if (entity == null)
            {
                string strStartTime = nowTime.ToString("HH:mm:ss");
                //string[] strWeek = workTime.WorkWeek.Split(',');

                entity = new Attendance()
                {
                    AdminId = adminId,
                    IsLeaveEarly = 0,
                    IsLate = 0,
                    IsAbsence = 0,
                    IsNoSignOut = -1,
                    CreatedTime = nowTime,
                    AmStartTime = strStartTime,
                    AttendanceTime = nowTime,
                    OperatorId = AuthorityHelper.OperatorId,
                };
            }
            else
            {

                if (entity.IsAbsence == -1 && entity.AbsenceType == (int)AttendanceFlag.DayOfAbsence)
                {
                    oper.ResultType = OperationResultType.Error;
                    oper.Message = "今日全天缺勤，禁止签到";
                    return oper;
                }

                if (entity.OvertimeId == null)
                {
                    oper.ResultType = OperationResultType.Error;
                    oper.Message = "禁止重复签到";
                    return oper;
                }
            }
            oper = IsFlexibleWork(workTime, nowTime, entity);
            return oper;
        }



        /// <summary>
        /// 校验是否为弹性工作时间
        /// </summary>
        /// <param name="workTime"></param>
        /// <param name="nowTime"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private OperationResult IsFlexibleWork(WorkTime workTime, DateTime nowTime, Attendance entity)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "");
            //当签到员工不是弹性工作时间时，计算签到时间
            if (workTime.IsFlexibleWork == false)
            {
                DateTime amStartTime = DateTime.Parse(workTime.AmStartTime);
                DateTime pmEndTime;
                #region 全天班
                pmEndTime = DateTime.Parse(workTime.PmEndTime);
                entity.IsAbsence = 0;
                if (nowTime.CompareTo(amStartTime) <= 0)
                {
                    entity.IsLate = 0;
                    TimeSpan timeSpan = nowTime - amStartTime;
                    entity.ArrivalEarlyMinutes = (int)Math.Floor(timeSpan.TotalMinutes);
                }
                else if (nowTime.CompareTo(amStartTime) > 0 && nowTime.CompareTo(pmEndTime) < 0)
                {
                    entity.IsLate = -1;
                    TimeSpan timeSpan = nowTime - amStartTime;
                    entity.LateMinutes = (int)Math.Floor(timeSpan.TotalMinutes);
                }
                oper.ResultType = OperationResultType.Success;
                oper.Message = string.Empty;
                oper.Data = entity;
                #endregion
            }
            else
            {
                oper.ResultType = OperationResultType.Success;
                oper.Message = string.Empty;
                oper.Data = entity;
            }
            return oper;
        }
        #endregion

        #region 签退
        /// <summary>
        /// 签退
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public OperationResult LoginOut(int adminId, Department department, bool jmpDep = false)
        {
            try
            {
                var admin = _adminRepository.Entities.FirstOrDefault(x => x.Id == adminId);
                OperationResult oper = CheckWorkTime(adminId);
                if (oper.ResultType == OperationResultType.Success)
                {
                    WorkTime workTime = oper.Data as WorkTime;
                    oper.Data = null;
                    DateTime nowTime = DateTime.Now;
                    IQueryable<Attendance> listAttendance = this.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId);
                    Attendance entity = listAttendance.Where(x => nowTime.Year == x.AttendanceTime.Year && nowTime.Month == x.AttendanceTime.Month && nowTime.Day == x.AttendanceTime.Day).FirstOrDefault();
                    if (entity != null)
                    {
                        if (!jmpDep) entity.Departments.Add(department);
                        int PunchOut = Convert.ToInt32(GetXmlNodeByXpath("PunchOut"));
                        if ((nowTime - DateTime.Parse(nowTime.ToShortDateString() + " " + entity.AmStartTime)).TotalMinutes <= PunchOut)
                        {
                            oper.ResultType = OperationResultType.Error;
                            oper.Message = "签退和签到时间间隔必须大于" + PunchOut + "分钟";
                            return oper;
                        }
                        if (workTime.IsFlexibleWork == false)
                        {
                            DateTime dtEntTime;
                            if (admin.IsPersonalTime)
                            {
                                int currentYear = DateTime.Now.Year;
                                int currentMonth = DateTime.Now.Month;
                                var WtdArry = new List<WorkTimeDetaile>();
                                var monthList = _workTimeDetaileRepository.Entities.Where(x => x.WorkTimeId == workTime.Id).GroupBy(x => x.Month).Select(x => x.Key).ToList();
                                if (monthList.Count() < 2)
                                {
                                    return new OperationResult(OperationResultType.Error, "没有排班信息");
                                }
                                int minMonth = monthList.Min();
                                int minYear = _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.WorkTimeId == workTime.Id && x.Month == minMonth).Year;
                                int maxMonth = monthList.Max();
                                int maxYear = _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.WorkTimeId == workTime.Id && x.Month == maxMonth).Year;

                                if (!(minMonth == currentMonth && minYear == currentYear) && !(maxMonth == currentMonth && maxYear == currentYear))
                                {
                                    if (minYear > maxYear)
                                    {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                                        currentMonth = maxMonth;
                                        currentYear = maxYear;
                                    }
                                    else
                                    {
                                        currentMonth = minMonth;
                                        currentYear = minYear;
                                    }
                                }

                                var _wd = _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.WorkTimeId == workTime.Id && x.Year == currentYear &&
                                x.Month == currentMonth && x.WorkDay == nowTime.Day);
                                dtEntTime = DateTime.Parse(nowTime.ToShortDateString() + " " + _wd.PmEndTime);
                            }
                            else
                            {
                                dtEntTime = DateTime.Parse(nowTime.ToShortDateString() + " " + workTime.PmEndTime);
                            }
                            if (nowTime.Hour < dtEntTime.Hour)
                            {
                                entity.IsLeaveEarly = -1;
                                TimeSpan timeSpan = dtEntTime - nowTime;
                                entity.LeaveEarlyMinutes = Math.Floor(timeSpan.TotalMinutes);
                            }
                            else
                            {
                                TimeSpan timeSpan = dtEntTime - nowTime;
                                entity.IsLeaveEarly = 0;
                                entity.LeaveLateMinutes = Math.Floor(timeSpan.TotalMinutes);
                            }
                        }
                        entity.PmEndTime = nowTime.ToString("HH:mm:ss");
                        entity.IsNoSignOut = 0;
                        entity.UpdatedTime = nowTime;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        int count = _attendanceRepository.Update(entity);
                        return count > 0 ? new OperationResult(OperationResultType.Success, "签退成功", entity.PmEndTime.ToString()) : new OperationResult(OperationResultType.Error, "签到失败");


                        //else
                        //{
                        //    return new OperationResult(OperationResultType.Error, "禁止重复签退");
                        //}
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Error, "请先签到在进行签退！");
                    }
                }
                else
                {
                    return oper;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            }
        }
        #endregion

        #region 校验工作时间
        /// <summary>
        /// 校验工作时间
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private OperationResult CheckWorkTime(int adminId)
        {
            Administrator admin = _adminRepository.Entities.Where(x => x.Id == adminId).Include(a => a.JobPosition).FirstOrDefault();
            if (admin == null)
            {
                return new OperationResult(OperationResultType.Error, "员工不存在");
            }
            if (admin.JobPosition == null)
            {
                return new OperationResult(OperationResultType.Error, "请先给该员工添加工作职位");
            }
            WorkTime workTime = new WorkTime();
            if (admin.IsPersonalTime)
            {
                workTime = admin.WorkTime;
                if (workTime == null)
                {
                    return new OperationResult(OperationResultType.Error, "没有排班信息");
                }
                int currentYear = DateTime.Now.Year;
                int currentMonth = DateTime.Now.Month;
                var WtdArry = new List<WorkTimeDetaile>();
                var monthList = _workTimeDetaileRepository.Entities.Where(x => x.WorkTimeId == admin.WorkTimeId).GroupBy(x => x.Month).Select(x => x.Key).ToList();
                if (monthList.Count() < 2)
                {
                    return new OperationResult(OperationResultType.Error, "没有排班信息");
                }
                int minMonth = monthList.Min();
                int minYear = _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId && x.Month == minMonth).Year;
                int maxMonth = monthList.Max();
                int maxYear = _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId && x.Month == maxMonth).Year;

                if (!(minMonth == currentMonth && minYear == currentYear) && !(maxMonth == currentMonth && maxYear == currentYear))
                {
                    if (minYear > maxYear)
                    {//如果最小月的年份儿大于最大月的年份儿，那么最大月与最小月反过来（即最大月为最小月，最小月为最大月）
                        currentMonth = maxMonth;
                        currentYear = maxYear;
                    }
                    else
                    {
                        currentMonth = minMonth;
                        currentYear = minYear;
                    }
                }

                var _wd = _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId && x.Year == currentYear &&
x.Month == currentMonth && x.WorkDay == DateTime.Now.Day);
                workTime.AmStartTime = _wd.AmStartTime;
                workTime.PmEndTime = _wd.PmEndTime;
            }
            else
            {
                workTime = admin.JobPosition.WorkTime;
            }
            workTime.IsPersonalTime = admin.IsPersonalTime;
            return new OperationResult(OperationResultType.Success, string.Empty, workTime);
        }
        #endregion

        #region 忘记打卡
        public OperationResult Pardon(DateTime currentDate)
        {
            int adminId = AuthorityHelper.OperatorId ?? 0;

            IQueryable<Attendance> listAtten = Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId);
            Attendance atten = listAtten.Where(x => x.AttendanceTime.Year == currentDate.Year && x.AttendanceTime.Month == currentDate.Month && x.AttendanceTime.Day == currentDate.Day).FirstOrDefault();
            WorkTime workTime = new WorkTime();
            var admin = _adminRepository.Entities.Where(x => x.Id == adminId).FirstOrDefault();
            if (admin != null && admin.JobPosition != null && admin.JobPosition.WorkTime != null)
            {
                workTime = admin.JobPosition.WorkTime;
            }
            if (atten == null)
            {
                atten = new Attendance()
                {
                    IsAbsence = 1,
                    IsPardon = true,
                    AttendanceTime = currentDate,
                    AdminId = adminId,
                    AmStartTime = workTime != null ? workTime.AmStartTime : "00:00",
                    PmEndTime = workTime != null ? workTime.PmEndTime : "00:00"
                };
                int count = _attendanceRepository.Insert(atten);
                return count > 0 ? new OperationResult(OperationResultType.Success, "添加成功") : new OperationResult(OperationResultType.Error, "添加失败");
            }
            else
            {
                if (atten.IsPardon == true)
                {
                    return new OperationResult(OperationResultType.Error, "已经使用过了忘记打卡");
                }
                else
                {
                    if (atten.IsAbsence == -1)
                    {
                        atten.IsAbsence = 1;
                        atten.IsPardon = true;
                        int count = _attendanceRepository.Update(atten);
                        return count > 0 ? new OperationResult(OperationResultType.Success, "添加成功") : new OperationResult(OperationResultType.Error, "添加失败");
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Error, "正常上班数据");
                    }
                }
            }
        }
        #endregion

        #region 获取签到记录
        public OperationResult GetList(int adminId, int days)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Administrator admin = _adminRepository.GetByKey(adminId);
            if (admin.JobPosition == null)
            {
                oper.Message = "请联系管理员添加工作职位";
                return oper;
            }
            WorkTime workTime = admin.JobPosition.WorkTime;
            if (admin.IsPersonalTime)
            {
                workTime = admin.WorkTime;
                var _wd = _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId && x.Year == DateTime.Now.Year &&
x.Month == DateTime.Now.Month && x.WorkDay == DateTime.Now.Day);
                if (_wd != null)
                {
                    workTime.AmStartTime = _wd.AmStartTime;
                    workTime.PmEndTime = _wd.PmEndTime;
                    workTime.WorkTimeType = _wd.WorkTimeType;
                }
            }
            workTime.IsPersonalTime = admin.IsPersonalTime;
            string strWorkTime = workTime.WorkWeek;
            string[] arrWorkTime = strWorkTime.Split(",");
            DateTime nowTime = DateTime.Now;
            List<Attendance> listAttendance = this.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId)
                .OrderByDescending(x => x.AttendanceTime).Take(7).ToList();
            List<AttendanceInfo> listAtt = new List<AttendanceInfo>();
            Dictionary<string, int> dic = this.GetHolidays();
            if (dic == null || dic.Count == 0)
            {
                oper.Message = "请先添加公休假";
                return oper;
            }
            int i = 0;
            while (true)
            {
                if (i > 15)
                {
                    break;
                }
                if (days == 0)
                {
                    break;
                }
                bool isWork = IsWorkDay(nowTime, workTime, dic, admin);
                bool isOverTime = IsOverTime(nowTime, adminId);
                if (isWork == true || isOverTime == true)
                {
                    Attendance attendance = listAttendance.FirstOrDefault(x => x.AttendanceTime.Year == nowTime.Year && x.AttendanceTime.Month == nowTime.Month && x.AttendanceTime.Day == nowTime.Day);
                    if (attendance == null)
                    {
                        listAtt.Add(new AttendanceInfo()
                        {
                            Date = nowTime.ToString("yyyy/MM/dd"),
                            IsLate = 0,
                            IsLeaveEarly = 0,
                            IsSign = -1,
                            IsSignOut = -1,
                            DepartmentName = "未打卡",
                        });
                    }
                    else
                    {
                        AttendanceInfo attInfo = new AttendanceInfo();
                        attInfo.IsLate = attendance.IsLate;
                        attInfo.IsLeaveEarly = attendance.IsLeaveEarly;
                        attInfo.IsSign = attendance.IsAbsence;
                        attInfo.IsSignOut = attendance.IsNoSignOut;
                        //if (string.IsNullOrEmpty(attendance.PmEndTime))
                        //{
                        //    attInfo.IsSignOut = -1;
                        //}
                        attInfo.IsSignOut = attendance.IsNoSignOut;
                        attInfo.Date = attendance.AttendanceTime.ToString("yyyy/MM/dd");
                        if (attendance.Departments != null && attendance.Departments.Count() > 0)
                        {
                            attInfo.DepartmentName = attendance.Departments.LastOrDefault().DepartmentName;
                        }
                        else
                        {
                            attInfo.DepartmentName = "未知打卡环境";
                        }
                        listAtt.Add(attInfo);
                    }
                    days -= 1;
                }

                nowTime = nowTime.AddDays(-1);
                i++;
            }
            var entity = listAtt.Select(x => new
            {
                admin.Member.RealName,
                x.DepartmentName,
                x.Date,
                x.IsLate,
                x.IsLeaveEarly,
                x.IsSign,
                x.IsSignOut
            }).ToList();
            oper.ResultType = OperationResultType.Success;
            oper.Message = "获取成功";
            oper.Data = entity;
            return oper;
        }
        #endregion
        /// <summary>
        /// 获取当月未签到次数
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public OperationResult GetNoLoginCount(int adminId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Administrator admin = _adminRepository.GetByKey(adminId);
            int current_year = DateTime.Now.Year;
            int current_month = DateTime.Now.Month;
            int current_days = DateTime.DaysInMonth(current_year, current_month);
            DateTime start_datetime = DateTime.Parse(current_year.ToString() + "-" + current_month.ToString().PadLeft(2, '0') + "-01 0:00");
            DateTime end_datetime = DateTime.Parse(current_year.ToString() + "-" + current_month.ToString().PadLeft(2, '0') + "-" + current_days.ToString() + " 23:59");
            if (admin.JobPosition == null)
            {
                oper.Message = "请联系管理员添加工作职位";
                return oper;
            }
            else
            {
                WorkTime workTime = admin.JobPosition.WorkTime;
                DateTime entry_time = admin.EntryTime;
                if (admin.IsPersonalTime)
                {
                    workTime = admin.WorkTime;
                    var _wd = _workTimeDetaileRepository.Entities.FirstOrDefault(x => x.WorkTimeId == admin.WorkTimeId && x.Year == DateTime.Now.Year &&
    x.Month == DateTime.Now.Month && x.WorkDay == DateTime.Now.Day);
                    if (_wd != null)
                    {
                        workTime.AmStartTime = _wd.AmStartTime;
                        workTime.PmEndTime = _wd.PmEndTime;
                        workTime.WorkTimeType = _wd.WorkTimeType;
                    }
                }
                workTime.IsPersonalTime = admin.IsPersonalTime;
                string strWorkTime = workTime.WorkWeek;
                string[] arrWorkTime = strWorkTime.Split(",");
                List<Attendance> listAttendance = this.Attendances.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == adminId
                && x.AttendanceTime >= start_datetime && x.AttendanceTime <= end_datetime).ToList();
                List<AttendanceInfo> listAtt = new List<AttendanceInfo>();
                Dictionary<string, int> dic = this.GetHolidays();
                if (dic == null || dic.Count == 0)
                {
                    oper.Message = "请先添加公休假";
                    return oper;
                }
                //每月1号默认0
                if (DateTime.Now.Day > 1)
                {
                    end_datetime = DateTime.Parse(DateTime.Now.AddDays(-1).ToShortDateString() + " 23:59:59");
                    if (entry_time.Year == start_datetime.Year && entry_time.Month == start_datetime.Month)
                    {
                        //入职当月未签到打卡处理
                        start_datetime = entry_time;
                    }
                    for (DateTime nowTime = start_datetime; nowTime <= end_datetime;)
                    {
                        bool isWork = IsWorkDay(nowTime, workTime, dic, admin);
                        bool isOverTime = IsOverTime(nowTime, adminId);
                        if (isWork == true || isOverTime == true)
                        {
                            Attendance attendance = listAttendance.FirstOrDefault(x => x.AttendanceTime.Year == nowTime.Year && x.AttendanceTime.Month == nowTime.Month && x.AttendanceTime.Day == nowTime.Day);
                            if (attendance == null)
                            {
                                listAtt.Add(new AttendanceInfo()
                                {
                                    Id = 0,
                                    Date = nowTime.ToString("yyyy/MM/dd"),
                                    IsLate = 0,
                                    IsLeaveEarly = 0,
                                    IsSign = -1,
                                    IsSignOut = -1,
                                    DepartmentName = "未打卡",
                                });
                            }
                            else
                            {
                                if (attendance.AmStartTime == null)
                                {
                                    listAtt.Add(new AttendanceInfo()
                                    {
                                        Id = attendance.Id,
                                        Date = nowTime.ToString("yyyy/MM/dd"),
                                        IsLate = 0,
                                        IsLeaveEarly = 0,
                                        IsSign = -1,
                                        IsSignOut = -1,
                                        DepartmentName = "审核中",
                                    });
                                }
                            }
                        }
                        nowTime = nowTime.AddDays(1);
                    }
                }
                oper = new OperationResult(OperationResultType.Success, "获取成功！");
                oper.Data = listAtt;
                return oper;
            }
        }

        #region 将假期放入到缓存中
        private Dictionary<string, int> GetHolidays()
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            string _key = OfficeHelper._holidayKey;
            if (HttpRuntime.Cache[_key] == null)
            {
                List<Holiday> list = _holidayRepository.Entities.Where(x => x.IsEnabled == true && x.IsDeleted == false).ToList();
                dic = OfficeHelper.InsertHolidayCache(list);
            }
            else
            {
                dic = HttpRuntime.Cache[_key] as Dictionary<string, int>;
            }
            return dic;

        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="strAdminId"></param>
        /// <returns></returns>
        public OperationResult Create(AttendanceDto dto, List<int> adminIds)
        {
            if (adminIds == null || adminIds.Count <= 0)
            {
                return new OperationResult(OperationResultType.Error, "请选择员工");
            }

            List<Administrator> listAdmin = _adminRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && adminIds.Contains(x.Id)).ToList();
            List<Attendance> listAtten = _attendanceRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.AttendanceTime.Year == dto.AttendanceTime.Year && x.AttendanceTime.Month == dto.AttendanceTime.Month && x.AttendanceTime.Day == dto.AttendanceTime.Day).ToList();
            List<AttendanceDto> listAdd = new List<AttendanceDto>();
            int error = 0;
            foreach (int id in adminIds)
            {
                Administrator admin = listAdmin.FirstOrDefault(x => x.Id == x.Id);
                if (admin == null)
                {
                    error++;
                    continue;
                }
                Attendance atten = listAtten.FirstOrDefault(x => x.Id == id);
                if (atten != null)
                {
                    error++;
                    continue;
                }
                WorkTime workTime = new WorkTime();
                if (admin.WorkTime != null && admin.WorkTime.IsEnabled == true)
                {
                    workTime = admin.WorkTime;
                }
                else if (admin.JobPosition != null)
                {
                    workTime = admin.JobPosition.WorkTime;
                }
                AttendanceDto entity = dto.DeepClone();
                entity.AdminId = id;
                entity.IsAbsence = 0;
                entity.IsLate = 0;
                entity.IsLeaveEarly = 0;
                entity.IsNoSignOut = 0;
                entity.NormalAMStartTime = workTime != null ? workTime.AmStartTime : "00:00";
                entity.NormalPMEndTime = workTime != null ? workTime.PmEndTime : "00:00";
                entity.AmStartTime = dto.AmStartTime;
                entity.PmEndTime = dto.PmEndTime;
                listAdd.Add(entity);
            }
            UnitOfWork.TransactionEnabled = true;
            this.Insert(listAdd.ToArray());
            int count = UnitOfWork.SaveChanges();
            error += listAdd.Count - count;
            OperationResult oper = count <= 0 ? new OperationResult(OperationResultType.Error, "添加失败") : count > 0 && error > 0 ? new OperationResult(OperationResultType.Success, "添加成功" + count + "条,失败" + error + "条") : new OperationResult(OperationResultType.Success, "添加成功");
            return oper;
        }
        #endregion

        #region 添加或者更新数据
        /// <summary>
        /// 添加或者更新数据
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="strAdminId"></param>
        /// <returns></returns>
        public OperationResult CreateOrUpdate(AttendanceDto dto, string strAdminId)
        {
            string[] arrId = strAdminId.Split(",");
            List<int> listId = new List<int>();
            foreach (string id in arrId)
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }
                else
                {
                    listId.Add(int.Parse(id));
                }
            }
            List<Administrator> listAdmin = _adminRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && listId.Contains(x.Id)).ToList();
            IQueryable<Attendance> listAttendance = _attendanceRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            List<Attendance> listAtten = listAttendance.Where(x => x.AttendanceTime.Year == dto.AttendanceTime.Year && x.AttendanceTime.Month == dto.AttendanceTime.Month && x.AttendanceTime.Day == dto.AttendanceTime.Day).ToList();
            List<AttendanceDto> listAdd = new List<AttendanceDto>();
            List<AttendanceDto> listUpdate = new List<AttendanceDto>();
            foreach (int id in listId)
            {
                Administrator admin = listAdmin.FirstOrDefault(x => x.Id == x.Id);
                Attendance atten = listAtten.FirstOrDefault(x => x.Id == id);
                WorkTime workTime = new WorkTime();
                if (admin.WorkTime != null && admin.WorkTime.IsEnabled == true)
                {
                    workTime = admin.WorkTime;
                }
                else if (admin.JobPosition != null)
                {
                    workTime = admin.JobPosition.WorkTime;
                }
                if (atten == null)
                {
                    AttendanceDto entity = dto.DeepClone();
                    entity.AdminId = id;
                    //entity.AmStartTime = workTime.AmStartTime;
                    //entity.PmEndTime = workTime.PmEndTime;
                    listAdd.Add(entity);
                }
                else
                {
                    Mapper.CreateMap<Attendance, AttendanceDto>();
                    AttendanceDto entitty = Mapper.Map<Attendance, AttendanceDto>(atten);
                    //entitty.IsLate = 0;
                    //entitty.IsLeaveEarly = 0;
                    //entitty.LateMinutes = 0;
                    //entitty.LeaveEarlyMinutes = 0;
                    //entitty.IsAbsence = 0;
                    //entitty.AmStartTime = workTime.AmStartTime;
                    //entitty.PmEndTime = workTime.PmEndTime;
                    listUpdate.Add(entitty);
                }
            }
            UnitOfWork.TransactionEnabled = true;
            this.Insert(listAdd.ToArray());
            this.Update(listUpdate.ToArray());
            int count = UnitOfWork.SaveChanges();
            return count > 0 ? new OperationResult(OperationResultType.Success, "添加成功") : new OperationResult(OperationResultType.Error, "添加失败");
        }
        #endregion

        #region 校验是否加班
        private bool IsOverTime(DateTime attenTime, int adminId)
        {
            bool isOverTime = false;
            int count = _overtimeRepository.Entities.Where(x => x.AdminId == adminId && x.VerifyType == (int)VerifyFlag.Pass && x.StartTime.CompareTo(attenTime) <= 0 && x.EndTime.CompareTo(attenTime) >= 0).Count();
            if (count > 0)
            {
                isOverTime = true;
            }
            return isOverTime;
        }
        #endregion

        #region 添加缺勤数据
        public void Add(List<Attendance> listEntity, List<AttendanceStatistics> listStatisticsAdd, List<AttendanceStatistics> listStatisticsUpdate)
        {
            UnitOfWork.TransactionEnabled = true;
            if (listEntity.Count > 0)
            {
                _attendanceRepository.Insert(listEntity.AsEnumerable());
            }
            if (listStatisticsAdd.Count() > 0)
            {
                _attenStatisticsRepository.Insert(listStatisticsAdd.AsEnumerable());
            }
            if (listStatisticsUpdate.Count() > 0)
            {
                _attenStatisticsRepository.Update(listStatisticsUpdate);
            }
            UnitOfWork.SaveChanges();
        }
        #endregion

        #region 补卡
        public OperationResult Repair(DateTime Date, int AdminId, int Id)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                UnitOfWork.TransactionEnabled = true;
                Administrator admin = _adminRepository.GetByKey(AdminId);
                WorkTime workTime = new WorkTime();
                if (admin.WorkTimeId != null)
                {
                    workTime = admin.WorkTime;
                }
                else if (admin.JobPosition != null)
                {
                    workTime = admin.JobPosition.WorkTime;
                }
                if (admin == null)
                {
                    oper.Message = "会员不存在";
                    return oper;
                }
                IQueryable<Attendance> listAtten = _attendanceRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                Attendance atten = listAtten.FirstOrDefault(x => x.AdminId == AdminId && x.Id == Id);
                if (atten == null)
                {
                    oper.Message = "数据不存在";
                    return oper;
                }
                if (atten.IsPardon == true)
                {
                    oper.Message = "已补卡";
                    return oper;
                }
                AttendanceRepair attenReapair = new AttendanceRepair()
                {
                    AdminId = AdminId,
                    AttendanceId = atten.Id,
                    VerifyType = (int)VerifyFlag.Verifing,
                };
                _attendanceRepairRepository.Insert(attenReapair);
                DateTime current = DateTime.Now;
                DateTime attenTime = atten.AttendanceTime;
                if (attenTime.Year != current.Year && attenTime.Month != current.Month && attenTime.Day != current.Day)
                {
                    AttendanceStatistics attenSta = _attenStatisticsRepository.Entities.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.AdminId == AdminId);

                    if (atten.IsAbsence == -1)
                    {
                        if (atten.AbsenceType == (int)AttendanceFlag.AmAbsence || atten.AbsenceType == (int)AttendanceFlag.PmAbsence)
                        {
                            attenSta.AbsenceDays -= 0.5;
                        }
                        else
                        {
                            attenSta.AbsenceDays -= 1;
                        }
                    }
                    if (atten.IsLate == -1)
                    {
                        attenSta.NegativeMinutes -= (int)atten.LateMinutes;
                    }
                    if (atten.IsLeaveEarly == -1)
                    {
                        attenSta.NegativeMinutes -= (int)atten.LeaveEarlyMinutes;
                    }
                    attenSta.UpdatedTime = current;
                    _attenStatisticsRepository.Update(attenSta);
                }
                //atten.IsPardon = true;
                //atten.AmStartTime = workTime.AmStartTime;
                //atten.PmEndTime = workTime.PmEndTime;
                //atten.LateMinutes = 0;
                //atten.LeaveEarlyMinutes = 0;
                atten.UpdatedTime = current;
                _attendanceRepository.Update(atten);


                int count = UnitOfWork.SaveChanges();
                if (count > 0)
                {
                    oper.Message = "提交成功";
                    oper.ResultType = OperationResultType.Success;
                }
                else
                {
                    oper.Message = "提交失败";
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后";
            }
            return oper;

        }
        #endregion

        #region 校验工作时间
        /// <summary>
        /// 校验是否为工作时间
        /// </summary>
        /// <param name="attenTime"></param>
        /// <param name="workTime"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public bool IsWorkDay(DateTime attenTime, WorkTime workTime, Dictionary<string, int> dic, Administrator admin)
        {
            string strCurrentWeek = attenTime.DayOfWeek.ToString("d");
            string strCurrentDate = attenTime.ToString("yyyyMMdd");
            string[] arrWeek = workTime.WorkWeek.Split(',');
            int count = 0;
            bool isPersonalTime = admin.IsPersonalTime;
            if (admin.whetherChange != null && admin.whetherChange.Value && DateTime.Compare(admin.whetherDateTime.Value, attenTime) > 0)
            {
                //工作时间类型变更 例如 职位时间变更个人时间 必须在上次由个人时间变更职位时间 大于7天  不然 此处获取数据会有问题
                //whetherDateTime 是工作时间类型变更的时候添加
                //whetherChange  是工作时间类型变更的时候 值为true
                //if (admin.whetherDateTime != null)
                //{
                //    if (DateTime.Compare(attenTime, admin.whetherDateTime.Value) < 0)
                //    {
                //        isPersonalTime = !isPersonalTime;
                //    }
                //}
            }
            if (isPersonalTime)
            {
                var _wd = _workTimeDetaileRepository.Entities.Where(x => x.WorkTimeId == workTime.Id && x.Year == attenTime.Year &&
                x.Month == attenTime.Month && x.WorkDay == attenTime.Day).FirstOrDefault();
                if (_wd != null)
                {
                    if (_wd.WorkTimeType == 2)
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (admin.JobPosition != null)
            {
                arrWeek = admin.JobPosition.WorkTime.WorkWeek.Split(',');
            }
            if (arrWeek.Contains(strCurrentWeek))
            {
                if (workTime.IsVacations == true)
                {
                    if (dic != null)
                    {
                        count = dic.Where(x => x.Key == strCurrentDate && x.Value == (int)WorkDateFlag.Holidays).Count();
                    }
                    if (count > 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    var holiday_list = _holidayRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled).ToList();
                    return holiday_list.Where(x => attenTime >= x.StartTime && attenTime <= x.EndTime.AddHours(23)).Count() > 0 ? false : true;
                }
            }
            else
            {
                if (workTime.IsVacations == true)
                {
                    if (dic != null)
                    {
                        count = dic.Where(x => x.Key == strCurrentDate && x.Value == (int)WorkDateFlag.WorkDay).Count();
                    }
                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        public string GetXmlNodeByXpath(string xmlFileName)
        {
            //string xpath = System.Web.HttpContext.Current.Server.MapPath("/Content/Config/PunchOut/PunchOutconfig.xml");
            //XmlDocument xmlDoc = new XmlDocument();
            try
            {
                //xmlDoc.Load(xpath); //加载XML文档
                //XmlNode xmlNode = xmlDoc.SelectSingleNode("User").SelectSingleNode(xmlFileName);
                //return string.IsNullOrEmpty(xmlNode.InnerText) ? "0" : xmlNode.InnerText;
                return _configureContract.GetConfigureValue("PunchOut", "PunchOutconfig", xmlFileName);
            }
            catch (Exception ex)
            {
                return "0";
                //throw ex; //这里可以定义你自己的异常处理
            }
        }

        /// <summary>
        /// 是否需要扣除双倍积分
        /// </summary>
        /// <param name="attenId">要判断的考勤Id</param>
        /// <param name="attenFlag">要判断的考勤标识</param>
        /// <returns></returns>
        public bool IsDeductionDoubleScore(int attenId, int attenFlag)
        {

            int mounth = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            DateTime DevelopmentTime = new DateTime(2017, 8, 1, 0, 0, 0);       //开发签到提醒补卡功能的当月，此日期之前的补卡统统给予一月提醒机会
            DateTime RemindNextMonthTime = (new DateTime(year, mounth, 1, 0, 0, 0));       //下月警告日期

            //该数据是否未补卡需要扣除双倍积分
            int RemindNextMonth = 0;

            if (attenFlag == (int)ApiAttenFlag.Late)
            {
                RemindNextMonth = this.Attendances.Count(a => a.Id == attenId && !a.IsDeleted && a.IsEnabled && a.IsLate == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.Late) == 0 && a.CreatedTime < RemindNextMonthTime);
            }
            else if (attenFlag == (int)ApiAttenFlag.LeaveEarly)
            {
                RemindNextMonth = this.Attendances.Count(a => a.Id == attenId && !a.IsDeleted && a.IsEnabled && a.IsLeaveEarly == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly) == 0 && a.CreatedTime < RemindNextMonthTime);
            }
            else if (attenFlag == (int)ApiAttenFlag.NoSignOut)
            {
                RemindNextMonth = this.Attendances.Count(a => a.Id == attenId && !a.IsDeleted && a.IsEnabled && a.IsNoSignOut == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut) == 0 && a.CreatedTime < RemindNextMonthTime);
            }

            return DevelopmentTime < RemindNextMonthTime && RemindNextMonth > 0;
        }

        /// <summary>
        /// 扣除双倍积分签到系统
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public IDictionary<string, bool> DoubleScoreReminderBySign(int adminId)
        {

            int mounth = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            DateTime DevelopmentTime = new DateTime(2017, 8, 1, 0, 0, 0);       //开发签到提醒补卡功能的当月，此日期之前的补卡统统给予一月提醒机会
            DateTime ReminderOfTheMonthTime = (new DateTime(year, mounth + 1, 1, 0, 0, 0)).AddDays(-6);       //当月提醒开始日期
            DateTime RemindNextMonthTime = (new DateTime(year, mounth, 1, 0, 0, 0));       //下月警告日期

            //截至目前为止未进行补卡的数量（当月最后五天提醒需判断该值）
            int ReminderOfTheMonth = this.Attendances.Count(a => a.AdminId == adminId && !a.IsDeleted && a.IsEnabled && ((a.IsLate == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.Late) == 0) || (a.IsLeaveEarly == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly) == 0) || (a.IsNoSignOut == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut) == 0)));

            //截至上月为止未进行补卡的数量（下月不进行提醒且要扣双倍积分需判断该值）
            int RemindNextMonth = this.Attendances.Count(a => a.AdminId == adminId && !a.IsDeleted && a.IsEnabled && ((a.IsLate == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.Late) == 0) || (a.IsLeaveEarly == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly) == 0) || (a.IsNoSignOut == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut) == 0)) && a.CreatedTime < RemindNextMonthTime);

            RemindNextMonth += _attendanceRepairRepository.Entities.Count(x => x.AdminId == adminId && x.IsDeleted == false && x.IsEnabled == true && x.VerifyType == (int)VerifyFlag.Waitting && x.IsDoubleScore);

            IDictionary<string, bool> dic = new Dictionary<string, bool>();

            //是否需要扣除双倍积分
            dic.Add("DeductionDoubleScore", DevelopmentTime != RemindNextMonthTime && RemindNextMonth > 0);
            //是否需要提醒
            dic.Add("IsReminder", ReminderOfTheMonthTime <= DateTime.Now && ReminderOfTheMonth > 0);

            return dic;
        }

        /// <summary>
        /// 扣除双倍积分的数据
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public List<DoubleScore> GetDoubleScoreReminderList(int adminId)
        {

            int mounth = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            DateTime DevelopmentTime = new DateTime(2017, 8, 1, 0, 0, 0);       //开发签到提醒补卡功能的当月，此日期之前的补卡统统给予一月提醒机会
            DateTime ReminderOfTheMonthTime = (new DateTime(year, mounth + 1, 1, 0, 0, 0)).AddDays(-6);       //当月提醒开始日期
            DateTime RemindNextMonthTime = (new DateTime(year, mounth, 1, 0, 0, 0));       //下月警告日期

            if (DevelopmentTime == RemindNextMonthTime)
            {
                return null;
            }

            //截至上月为止未进行补卡的数量（下月不进行提醒且要扣双倍积分需判断该值）
            var list = this.Attendances.Where(a => a.AdminId == adminId && !a.IsDeleted && a.IsEnabled && ((a.IsLate == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.Late) == 0) || (a.IsLeaveEarly == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly) == 0) || (a.IsNoSignOut == -1 && a.AttendanceRepairs.Count(r => r.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut) == 0)) && a.CreatedTime < RemindNextMonthTime).ToList();

            List<DoubleScore> dslist = new List<DoubleScore>();
            foreach (var atten in list)
            {

                if (atten.IsLate == -1 && atten.AttendanceRepairs.Count(x => x.IsDeleted == false && x.IsEnabled == true && x.ApiAttenFlag == (int)ApiAttenFlag.Late) == 0)
                {
                    dslist.Add(new DoubleScore(atten.Id, (int)ApiAttenFlag.Late));
                }
                if (atten.IsLeaveEarly == -1 && atten.AttendanceRepairs.Count(x => x.IsDeleted == false && x.IsEnabled == true && x.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly) == 0)
                {
                    dslist.Add(new DoubleScore(atten.Id, (int)ApiAttenFlag.LeaveEarly));
                }
                if (atten.IsNoSignOut == -1 && atten.AttendanceRepairs.Count(x => x.IsDeleted == false && x.IsEnabled == true && x.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut) == 0)
                {
                    dslist.Add(new DoubleScore(atten.Id, (int)ApiAttenFlag.NoSignOut));
                }
            }
            return dslist;
        }
    }
}
