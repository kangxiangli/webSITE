using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AttendanceRepairService : ServiceBase, IAttendanceRepairContract
    {
        #region 声明数据层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceRepairService));

        private readonly IRepository<AttendanceRepair, int> _attendanceRepairRepository;

        private readonly IRepository<Administrator, int> _adminRepository;

        private readonly IRepository<Attendance, int> _attendanceRepository;

        private readonly IRepository<AttendanceStatistics, int> _attendanceStatisticsRepository;
        private readonly IRepository<WorkTimeDetaile, int> _workTimeDetaile;
        private readonly IMemberConsumeContract _memberConsumeContract;
        private readonly IAttendanceContract _attendanceContract;
        private readonly IConfigureContract _configureContract;
        public AttendanceRepairService(IRepository<AttendanceRepair, int> attendanceRepairRepository,
            IRepository<Administrator, int> adminRepository,
            IRepository<Attendance, int> attendanceRepository,
            IRepository<AttendanceStatistics, int> attendanceStatisticsRepository,
             IRepository<WorkTimeDetaile, int> workTimeDetaile,
             IMemberConsumeContract memberConsumeContract,
             IAttendanceContract attendanceContract,
            IConfigureContract configureContract) : base(attendanceRepairRepository.UnitOfWork)
        {
            _attendanceRepairRepository = attendanceRepairRepository;
            _adminRepository = adminRepository;
            _attendanceRepository = attendanceRepository;
            _attendanceStatisticsRepository = attendanceStatisticsRepository;
            _workTimeDetaile = workTimeDetaile;
            _memberConsumeContract = memberConsumeContract;
            _attendanceContract = attendanceContract;
            _configureContract = configureContract;
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AttendanceRepair View(int Id)
        {
            var entity = _attendanceRepairRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AttendanceRepairDto Edit(int Id)
        {
            var entity = _attendanceRepairRepository.GetByKey(Id);
            Mapper.CreateMap<AttendanceRepair, AttendanceRepairDto>();
            var dto = Mapper.Map<AttendanceRepair, AttendanceRepairDto>(entity);
            dto.RealName = entity.Administrator.Member.RealName;
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<AttendanceRepair> AttendanceRepairs { get { return _attendanceRepairRepository.Entities; } }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params AttendanceRepairDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _attendanceRepairRepository.Insert(dtos,
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
        public OperationResult Update(params AttendanceRepairDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _attendanceRepairRepository.Update(dtos,
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
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params AttendanceRepair[] entitys)
        {
            try
            {
                return OperationHelper.Try((opera) =>
                {
                    entitys.CheckNotNull("entities");
                    OperationResult result = _attendanceRepairRepository.Update(entitys,
                     entity =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                    });
                    return result;
                }, Operation.Update);
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
                var entities = _attendanceRepairRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRepairRepository.Update(entity);
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
                var entities = _attendanceRepairRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRepairRepository.Update(entity);
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
                var entities = _attendanceRepairRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRepairRepository.Update(entity);
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
                var entities = _attendanceRepairRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRepairRepository.Update(entity);
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
        public OperationResult Verify(AttendanceRepairDto dto)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "操作失败");
            AttendanceRepair attendanceRepair = this.AttendanceRepairs.FirstOrDefault(x => x.Id == dto.Id && x.ApiAttenFlag == dto.ApiAttenFlag);
            if (attendanceRepair == null)
            {
                oper.Message = "数据不存在";
                return oper;
            }
            if (attendanceRepair.VerifyType != (int)VerifyFlag.Verifing)
            {
                oper.Message = "数据已经被审核过了";
                return oper;
            }
            UnitOfWork.TransactionEnabled = true;

            dto.VerifyAdminId = AuthorityHelper.OperatorId;
            dto.AdminId = attendanceRepair.AdminId;
            dto.AttendanceId = attendanceRepair.AttendanceId;
            dto.ApiAttenFlag = attendanceRepair.ApiAttenFlag;
            if (dto.VerifyType == (int)VerifyFlag.Pass)
            {
                #region 更新考勤统计
                var atten = _attendanceRepository.GetByKey(dto.AttendanceId ?? 0);
                if (atten.IsNotNull())
                {
                    DateTime current = DateTime.Now;
                    DateTime attenTime = atten.AttendanceTime;
                    Administrator admin;
                    var goper = this.GetWorkTime(dto.AdminId ?? 0, atten.AttendanceTime, out admin);
                    if (goper.ResultType == OperationResultType.Success)
                    {
                        WorkTime workTime = goper.Data as WorkTime;
                        //更新考勤
                        if (dto.ApiAttenFlag == (int)ApiAttenFlag.Absence)
                        {
                            atten.IsAbsence = 1;
                            atten.AmStartTime = workTime.AmStartTime;
                        }
                        else if (dto.ApiAttenFlag == (int)ApiAttenFlag.Late)
                        {
                            atten.IsLate = 1; ;
                            atten.AmStartTime = workTime.AmStartTime;
                            atten.LateMinutes = 0;
                        }
                        else if (dto.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly)
                        {
                            atten.IsLeaveEarly = 1; ;
                            atten.LeaveEarlyMinutes = 0;
                            atten.PmEndTime = workTime.PmEndTime;
                        }
                        else if (dto.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut)
                        {
                            atten.IsNoSignOut = 1; ;
                        }
                        //else
                        //{
                        //    atten.IsAbsence = 0;
                        //    atten.AmStartTime = workTime.AmStartTime;
                        //    atten.IsLate = 0;
                        //    atten.LateMinutes = 0;
                        //    atten.IsLeaveEarly = 0;
                        //    atten.IsNoSignOut = 0;
                        //    atten.LeaveEarlyMinutes = 0;
                        //    atten.PmEndTime = workTime.PmEndTime;
                        //}
                        atten.IsPardon = true;
                        atten.UpdatedTime = current;
                        _attendanceRepository.Update(atten);
                    }
                }
                #endregion
            }
            if (dto.VerifyType != (int)VerifyFlag.Waitting)
            {
                dto.IsPardon = true;
            }
            this.Update(dto);

            int count = UnitOfWork.SaveChanges();
            if (count > 0)
            {
                oper.Message = "操作成功";
                oper.ResultType = OperationResultType.Success;
            }
            return oper;
        }
        #endregion
        /// <summary>
        /// 未签到补卡
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="id"></param>
        /// <param name="AttenFlag"></param>
        /// <returns></returns>
        public OperationResult ApplyNoLoginRepair(int adminId, string nowTime, int? AttenFlag)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                DateTime atten_time = DateTime.Parse(nowTime);
                var _oper = _attendanceRepository.Entities.Where(x => !x.IsDeleted && x.IsEnabled && x.AdminId == adminId && x.AttendanceTime.Year == atten_time.Year &&
                 x.AttendanceTime.Month == atten_time.Month && x.AttendanceTime.Day == atten_time.Day).Count();
                if (_oper == 0)
                {
                    _oper = _attendanceRepository.Insert(new Attendance
                    {
                        OperatorId = adminId,
                        AdminId = adminId,
                        AttendanceTime = atten_time,
                        AmStartTime = null,
                        IsDeleted = false,
                        IsEnabled = true
                    });
                }
                if (_oper > 0)
                {
                    IQueryable<Attendance> listAtten = _attendanceRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                    Attendance atten = _attendanceRepository.Entities.FirstOrDefault(x => x.AdminId == adminId && !x.IsDeleted && x.IsEnabled &&
                    x.AttendanceTime.Year == atten_time.Year &&
                 x.AttendanceTime.Month == atten_time.Month && x.AttendanceTime.Day == atten_time.Day);
                    if (atten == null || !AttenFlag.HasValue)
                    {
                        oper.Message = "数据不存在";
                    }
                    else
                    {
                        var curRepair = _attendanceRepairRepository.Entities.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.ApiAttenFlag == AttenFlag.Value
                        && x.AttendanceId == atten.Id);
                        if (curRepair.IsNotNull())
                        {
                            if (curRepair.IsPardon)
                            {
                                oper.Message = "已补卡";
                            }
                            else
                            {
                                if (curRepair.VerifyType == (int)VerifyFlag.Verifing)
                                {
                                    oper.Message = "正在审核中";
                                }
                                else if (curRepair.VerifyType == (int)VerifyFlag.Waitting)
                                {
                                    oper.Message = "等待用户确认中";
                                }
                                else
                                {
                                    oper.Message = "已补卡";
                                }
                            }
                        }
                        else
                        {
                            AttendanceRepairDto attenReapair = new AttendanceRepairDto()
                            {
                                AdminId = adminId,
                                AttendanceId = atten.Id,
                                ApiAttenFlag = AttenFlag.Value,
                                VerifyType = (int)VerifyFlag.Verifing,
                                IsPardon = false
                            };
                            oper = this.Insert(attenReapair);
                        }
                    }
                }
                else
                {
                    oper.Message = "补卡失败";
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后";
            }
            return oper;
        }

        #region 申请补卡
        public OperationResult ApplyRepair(int adminId, int id, int? AttenFlag, string Reasons = "")
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                IQueryable<Attendance> listAtten = _attendanceRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                Attendance atten = listAtten.FirstOrDefault(x => x.AdminId == adminId && x.Id == id);
                if (atten == null || !AttenFlag.HasValue)
                {
                    oper.Message = "数据不存在";
                }
                else
                {
                    var curRepair = atten.AttendanceRepairs.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.ApiAttenFlag == AttenFlag.Value);
                    if (curRepair.IsNotNull())
                    {
                        if (curRepair.IsPardon)
                        {
                            oper.Message = "已补卡";
                        }
                        else
                        {
                            if (curRepair.VerifyType == (int)VerifyFlag.Verifing)
                            {
                                oper.Message = "正在审核中";
                            }
                            else if (curRepair.VerifyType == (int)VerifyFlag.Waitting)
                            {
                                oper.Message = "等待用户确认中";
                            }
                            else
                            {
                                oper.Message = "已补卡";
                            }
                        }
                    }
                    else
                    {
                        AttendanceRepairDto attenReapair = new AttendanceRepairDto()
                        {
                            AdminId = adminId,
                            AttendanceId = atten.Id,
                            ApiAttenFlag = AttenFlag.Value,
                            VerifyType = (int)VerifyFlag.Verifing,
                            IsPardon = false,
                            Reasons = Reasons,
                            IsDoubleScore = _attendanceContract.IsDeductionDoubleScore(atten.Id, AttenFlag.Value)
                        };
                        oper = this.Insert(attenReapair);
                    }
                }
                oper.Data = null;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后";
            }
            return oper;
        }

        /// <summary>
        /// 系统自动申请补卡（扣双倍积分）
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="id"></param>
        /// <param name="AttenFlag"></param>
        /// <param name="Reasons"></param>
        /// <returns></returns>
        public OperationResult ApplyRepairBySystem(int adminId, string project = "website")
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                var admin = _adminRepository.GetByKey(adminId);
                if (admin == null)
                {
                    oper.Message = "不存在该用户";
                    return oper;
                }

                //获取用户需要扣除双倍积分的数据
                var doubleScoreList = _attendanceContract.GetDoubleScoreReminderList(adminId);

                var arlist = this.AttendanceRepairs.Where(x => x.AdminId == adminId && x.IsDeleted == false && x.IsEnabled == true && x.VerifyType == (int)VerifyFlag.Waitting && x.IsDoubleScore).ToList();

                if ((doubleScoreList == null || doubleScoreList.Count() == 0) && (arlist == null || arlist.Count() == 0))
                {
                    oper.Message = "无需扣除双倍积分的数据";
                    return oper;
                }
                double paidScore = 0;
                //if (project == "website")
                //{
                //double.TryParse(XmlStaticHelper.GetXmlNodeByXpath("AttendanceRepair", "AttendanceRepairConfig", "PaidScore", "0"), out paidScore);
                //}
                //else
                //{
                //double.TryParse(XmlStaticHelper.GetXmlNodeByXpath_Url("Offices/Configure/GetXmlNodeByXpath", "AttendanceRepair", "AttendanceRepairConfig", "PaidScore", "0"), out paidScore);
                //}
                double.TryParse(_configureContract.GetConfigureValue("AttendanceRepair", "AttendanceRepairConfig", "PaidScore", "0"), out paidScore);

                UnitOfWork.TransactionEnabled = true;

                IDictionary<string, double> dic = new Dictionary<string, double>();
                dic.Add("LateScore", 0);
                dic.Add("LateCount", 0);
                dic.Add("LeaveEarlyScore", 0);
                dic.Add("LeaveEarlyCount", 0);
                dic.Add("NoSignOutScore", 0);
                dic.Add("NoSignOutCount", 0);
                dic.Add("PaidScore", 0);
                dic.Add("adminScore", 0);


                dic["adminScore"] = admin.Member == null ? 0 : Convert.ToDouble(admin.Member.Score);
                if (doubleScoreList != null)
                {
                    foreach (var item in doubleScoreList)
                    {
                        AttendanceRepairDto attenReapair = new AttendanceRepairDto()
                        {
                            AdminId = adminId,
                            AttendanceId = item.Id,
                            ApiAttenFlag = item.AttenFlag,
                            VerifyType = (int)VerifyFlag.Waitting,
                            IsPardon = false,
                            Reasons = "系统自动扣除双倍积分",
                            IsDoubleScore = true,
                            PaidScore = paidScore
                        };
                        this.Insert(attenReapair);

                        if (item.AttenFlag == (int)ApiAttenFlag.Late)
                        {//如果类型为迟到，则迟到数量+1，迟到积分相应增加
                            dic["LateCount"] += 1;
                            dic["LateScore"] += attenReapair.PaidScore;
                        }
                        else if (item.AttenFlag == (int)ApiAttenFlag.LeaveEarly)
                        {//如果类型为早退，则迟到数量+1，迟到积分相应增加
                            dic["LeaveEarlyCount"] += 1;
                            dic["LeaveEarlyScore"] += attenReapair.PaidScore;
                        }
                        if (item.AttenFlag == (int)ApiAttenFlag.NoSignOut)
                        {//如果类型为未签退，则迟到数量+1，迟到积分相应增加
                            dic["NoSignOutCount"] += 1;
                            dic["NoSignOutScore"] += attenReapair.PaidScore;
                        }

                        dic["PaidScore"] += attenReapair.PaidScore;
                    }
                }

                if (arlist != null)
                {
                    foreach (var item in arlist)
                    {
                        item.PaidScore = paidScore;
                        this.Update(item);

                        if (item.ApiAttenFlag == (int)ApiAttenFlag.Late)
                        {//如果类型为迟到，则迟到数量+1，迟到积分相应增加
                            dic["LateCount"] += 1;
                            dic["LateScore"] += item.PaidScore;
                        }
                        else if (item.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly)
                        {//如果类型为早退，则迟到数量+1，迟到积分相应增加
                            dic["LeaveEarlyCount"] += 1;
                            dic["LeaveEarlyScore"] += item.PaidScore;
                        }
                        if (item.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut)
                        {//如果类型为未签退，则迟到数量+1，迟到积分相应增加
                            dic["NoSignOutCount"] += 1;
                            dic["NoSignOutScore"] += item.PaidScore;
                        }

                        dic["PaidScore"] += item.PaidScore;
                    }
                }

                int count = UnitOfWork.SaveChanges();

                oper.ResultType = OperationResultType.Success;
                oper.Message = "补卡记录已自动生成，请及时确认";
                oper.Data = dic;
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后";
            }
            return oper;
        }
        #endregion
        #region 确认补卡
        public OperationResult ConfirmRepair(int adminId, int id, int? AttenFlag, string project = "website")
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                UnitOfWork.TransactionEnabled = true;
                Administrator admin;
                Attendance atten = _attendanceRepository.Entities
    .Where(x => x.AdminId == adminId && x.Id == id)
    .FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true);
                var goper = this.GetWorkTime(adminId, atten.AttendanceTime, out admin);

                if (goper.ResultType == OperationResultType.Success)
                {
                    if (atten == null || !AttenFlag.HasValue)
                    {
                        oper.Message = "数据不存在";
                    }
                    else
                    {
                        WorkTime workTime = goper.Data as WorkTime;
                        AttendanceRepair attendanceRepair = atten.AttendanceRepairs.Where(x => x.IsDeleted == false && x.IsEnabled == true)
                                 .FirstOrDefault(x => x.VerifyType == (int)VerifyFlag.Waitting && x.ApiAttenFlag == AttenFlag.Value);
                        if (attendanceRepair.IsNotNull())
                        {
                            if (attendanceRepair.IsPardon)
                            {
                                oper.Message = "已补卡";
                            }
                            else
                            {
                                double paidScore = 0;

                                if (attendanceRepair.IsDoubleScore)
                                {
                                    //if (project == "website")
                                    //{
                                    //    double.TryParse(XmlStaticHelper.GetXmlNodeByXpath("AttendanceRepair", "AttendanceRepairConfig", "PaidScore", attendanceRepair.PaidScore.ToString()), out paidScore);
                                    //}
                                    //else
                                    //{
                                    //    double.TryParse(XmlStaticHelper.GetXmlNodeByXpath_Url("Offices/Configure/GetXmlNodeByXpath", "AttendanceRepair", "AttendanceRepairConfig", "PaidScore", attendanceRepair.PaidScore.ToString()), out paidScore);
                                    //}
                                    double.TryParse(_configureContract.GetConfigureValue("AttendanceRepair", "AttendanceRepairConfig", "PaidScore", attendanceRepair.PaidScore.ToString()), out paidScore);
                                }
                                else
                                {
                                    paidScore = attendanceRepair.PaidScore;
                                }

                                double adminScore = ((admin?.Member?.Score) ?? 0).CastTo<double>();
                                double resultScore = adminScore - Math.Abs(paidScore);
                                if (resultScore < 0)
                                {
                                    oper.Message = "本次需扣除积分" + paidScore + ",当前积分不足";
                                }
                                else
                                {
                                    //更新考勤统计
                                    DateTime current = DateTime.Now;
                                    DateTime attenTime = atten.AttendanceTime;
                                    //更新考勤
                                    if (AttenFlag == (int)ApiAttenFlag.Absence)
                                    {
                                        atten.IsAbsence = 1;
                                        atten.AmStartTime = workTime.AmStartTime;
                                    }
                                    else if (AttenFlag == (int)ApiAttenFlag.Late)
                                    {
                                        atten.IsLate = 1;
                                        atten.LateMinutes = 0;
                                    }
                                    else if (AttenFlag == (int)ApiAttenFlag.LeaveEarly)
                                    {
                                        atten.IsLeaveEarly = 1;
                                        atten.LeaveEarlyMinutes = 0;
                                        atten.PmEndTime = workTime.PmEndTime;
                                    }
                                    else if (AttenFlag == (int)ApiAttenFlag.NoSignOut)
                                    {
                                        atten.IsNoSignOut = 1;
                                    }
                                    //else
                                    //{
                                    //    atten.IsAbsence = 0;
                                    //    atten.AmStartTime = workTime.AmStartTime;
                                    //    atten.IsLate = 0;
                                    //    atten.LateMinutes = 0;
                                    //    atten.IsLeaveEarly = 0;
                                    //    atten.LeaveEarlyMinutes = 0;
                                    //    atten.PmEndTime = workTime.PmEndTime;
                                    //}
                                    atten.IsPardon = true;
                                    atten.UpdatedTime = current;
                                    atten.OperatorId = adminId;
                                    //更新员工积分
                                    //admin.Score = resultScore;
                                    admin.Member.Score = resultScore.CastTo<decimal>();
                                    admin.UpdatedTime = current;
                                    admin.OperatorId = adminId;
                                    //更新补卡数据
                                    attendanceRepair.IsPaid = true;
                                    attendanceRepair.IsPardon = true;
                                    attendanceRepair.VerifyType = (int)VerifyFlag.Pass;
                                    attendanceRepair.UpdatedTime = current;
                                    attendanceRepair.OperatorId = adminId;
                                    _attendanceRepairRepository.Update(attendanceRepair);
                                    _attendanceRepository.Update(atten);
                                    _memberConsumeContract.LogScoreWhenAttendanceRepair((int)admin.MemberId, Convert.ToInt32(paidScore));
                                    _adminRepository.Update(admin);
                                }
                            }
                        }
                        else
                        {
                            oper.Message = "数据不存在";
                        }
                    }
                }
                int count = UnitOfWork.SaveChanges();
                if (count > 0)
                {
                    oper.Message = "补卡成功";
                    oper.ResultType = OperationResultType.Success;
                }
                else
                {
                    if (oper.Message.IsNullOrEmpty())
                    {
                        oper.Message = "补卡失败";
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后";
            }
            return oper;
        }

        /// <summary>
        /// 系统自动扣除双倍积分
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public OperationResult ConfirmRepairBySystem(int adminId, string project = "website")
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            try
            {
                //UnitOfWork.TransactionEnabled = true;
                Administrator admin = _adminRepository.GetByKey(adminId);

                var arlist = this.AttendanceRepairs.Where(x => x.AdminId == adminId && x.IsDeleted == false && x.IsEnabled == true && x.VerifyType == (int)VerifyFlag.Waitting && x.IsDoubleScore).ToList();

                if (arlist == null || arlist.Count() == 0)
                {
                    oper.Message = "无需要扣除双倍积分的补卡信息";
                    return oper;
                }
                //要扣除的总积分
                double scoretotal = 0;

                DateTime current = DateTime.Now;
                foreach (var item in arlist)
                {
                    Attendance atten = _attendanceRepository.Entities
        .Where(x => x.AdminId == adminId && x.Id == item.AttendanceId)
        .FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true);

                    //var goper = this.GetWorkTime(adminId, atten.AttendanceTime, out admin);

                    //if (goper.ResultType != OperationResultType.Success)
                    //{
                    //    continue;
                    //}


                    if (atten == null)
                    {
                        continue;
                    }
                    //WorkTime workTime = goper.Data as WorkTime;


                    double paidScore = 0;

                    //if (project == "website")
                    //{
                    //    double.TryParse(XmlStaticHelper.GetXmlNodeByXpath("AttendanceRepair", "AttendanceRepairConfig", "PaidScore", item.PaidScore.ToString()), out paidScore);
                    //}
                    //else
                    //{
                    //    double.TryParse(XmlStaticHelper.GetXmlNodeByXpath_Url("Offices/Configure/GetXmlNodeByXpath", "AttendanceRepair", "AttendanceRepairConfig", "PaidScore", item.PaidScore.ToString()), out paidScore);
                    //}
                    double.TryParse(_configureContract.GetConfigureValue("AttendanceRepair", "AttendanceRepairConfig", "PaidScore", item.PaidScore.ToString()), out paidScore);

                    scoretotal += paidScore;

                    //更新考勤统计
                    DateTime attenTime = atten.AttendanceTime;
                    //更新考勤
                    if (item.ApiAttenFlag == (int)ApiAttenFlag.Late)
                    {
                        atten.IsLate = 1;
                        atten.LateMinutes = 0;
                    }
                    else if (item.ApiAttenFlag == (int)ApiAttenFlag.LeaveEarly)
                    {
                        atten.IsLeaveEarly = 1;
                        atten.LeaveEarlyMinutes = 0;
                        //atten.PmEndTime = workTime.PmEndTime;
                    }
                    else if (item.ApiAttenFlag == (int)ApiAttenFlag.NoSignOut)
                    {
                        atten.IsNoSignOut = 1;
                    }

                    atten.IsPardon = true;
                    atten.UpdatedTime = current;
                    atten.OperatorId = adminId;
                    //更新补卡数据
                    item.IsPaid = true;
                    item.IsPardon = true;
                    item.VerifyType = (int)VerifyFlag.Pass;
                    item.UpdatedTime = current;
                    item.OperatorId = adminId;
                    _attendanceRepairRepository.Update(item);
                    _attendanceRepository.Update(atten);
                }

                double adminScore = ((admin?.Member?.Score) ?? 0).CastTo<double>();
                double resultScore = adminScore - Math.Abs(scoretotal);

                if (resultScore < 0)
                {
                    oper.Message = "本次需扣除积分" + scoretotal + ",当前积分不足";
                    return oper;
                }

                //更新员工积分
                admin.Member.Score = resultScore.CastTo<decimal>();
                admin.UpdatedTime = current;
                admin.OperatorId = adminId;

                _adminRepository.Update(admin);

                _memberConsumeContract.LogScoreWhenAttendanceRepair((int)admin.MemberId, Convert.ToInt32(scoretotal));

                //int count = UnitOfWork.SaveChanges();
                //if (count > 0)
                //{
                oper.Message = "补卡成功";
                oper.ResultType = OperationResultType.Success;
                //}
                //else
                //{
                //    if (oper.Message.IsNullOrEmpty())
                //    {
                //        oper.Message = "补卡失败";
                //    }
                //}
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                oper.Message = "服务器忙，请稍后";
            }
            return oper;
        }
        #endregion

        /// <summary>
        /// 获取用过的工作时间
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private OperationResult GetWorkTime(int adminId, DateTime dt, out Administrator admin)
        {

            OperationResult oper = new OperationResult(OperationResultType.Error);
            admin = _adminRepository.GetByKey(adminId);
            if (admin == null)
            {
                oper.ResultType = OperationResultType.Error;
                oper.Message = "会员不存在";
            }
            else
            {
                WorkTime workTime = new WorkTime();
                if (admin.IsPersonalTime)
                {
                    int _workId = admin.WorkTimeId == null ? 0 : admin.WorkTimeId.Value;
                    var _wd = _workTimeDetaile.Entities.FirstOrDefault(x => x.WorkDay == dt.Day && x.Month == dt.Month && x.Year == dt.Year && x.WorkTimeId == _workId);
                    if (_wd != null)
                    {
                        workTime.AmStartTime = _wd.AmStartTime;
                        workTime.PmEndTime = _wd.PmEndTime;
                    }
                    else
                    {
                        oper.ResultType = OperationResultType.Error;
                        oper.Message = "排班表不存在";
                    }
                }
                else
                {
                    oper.ResultType = OperationResultType.Success;
                    workTime = admin.JobPosition.WorkTime;
                }

                oper.Data = workTime;
            }
            return oper;
        }
    }
}
