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

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class OvertimeService : ServiceBase, IOvertimeContract
    {
        #region 声明数据层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(OvertimeService));

        protected readonly IRepository<Overtime, int> _overtimeRepository;

        private readonly IRepository<Administrator, int> _adminRepository;

        private readonly IRepository<Attendance, int> _attendanceRepository;

        private readonly IRepository<Member, int> _memberRepository;

        private readonly IRepository<MemberDeposit, int> _memberDepositRepository;

        private readonly IMemberDepositContract _memberDepositContract;

        private readonly IRepository<WorkTimeDetaile, int> _workTimeDetaileRepository;

        private readonly IRepository<OvertimeRestItem, int> _overtimeRestItemRepository;
        public OvertimeService(IRepository<Overtime, int> overtimeRepository,
            IRepository<Administrator, int> adminRepository,
            IRepository<Attendance, int> attendanceRepository,
            IRepository<OvertimeRestItem, int> overtimeRestItemRepository,
            IRepository<Member, int> memberRepository,
            IMemberDepositContract memberDepositContract,
            IRepository<MemberDeposit, int> memberDepositRepository,
            IRepository<WorkTimeDetaile, int> workTimeDetaileRepository)
            : base(overtimeRepository.UnitOfWork)
        {
            _overtimeRepository = overtimeRepository;
            _adminRepository = adminRepository;
            _attendanceRepository = attendanceRepository;
            _overtimeRestItemRepository = overtimeRestItemRepository;
            _memberRepository = memberRepository;
            _memberDepositContract = memberDepositContract;
            _memberDepositRepository = memberDepositRepository;
            _workTimeDetaileRepository = workTimeDetaileRepository;
        }
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Overtime View(int Id)
        {
            var entity = _overtimeRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象

        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public OvertimeDto Edit(int Id)
        {
            var entity = _overtimeRepository.GetByKey(Id);
            Mapper.CreateMap<Overtime, OvertimeDto>();
            var dto = Mapper.Map<Overtime, OvertimeDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集

        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Overtime> Overtimes { get { return _overtimeRepository.Entities; } }

        public IQueryable<OvertimeRestItem> OvertimeRestItems { get { return _overtimeRestItemRepository.Entities; } }
        #endregion

        #region 添加数据

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params OvertimeDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                IQueryable<Overtime> listOvertime = this.Overtimes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType != (int)VerifyFlag.NoPass);
                foreach (OvertimeDto dto in dtos)
                {
                    IQueryable<Overtime> listAtt = listOvertime.Where(x => x.AdminId == dto.AdminId);
                    foreach (Overtime att in listAtt)
                    {
                        if ((att.StartTime < dto.StartTime && att.EndTime > dto.StartTime) || (att.StartTime < dto.EndTime && att.EndTime > dto.EndTime))
                        {
                            return new OperationResult(OperationResultType.Error, "加班时间段有重叠，请重新选择时间段！");
                        }
                    }
                }
                OperationResult result = _overtimeRepository.Insert(dtos,
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
        public OperationResult Update(params OvertimeDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Overtime> list = this.Overtimes;
                IQueryable<Overtime> listOvertime = this.Overtimes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.VerifyType != (int)VerifyFlag.NoPass);
                foreach (var dto in dtos)
                {
                    Overtime overtime = list.Where(x => x.Id == dto.Id).FirstOrDefault();
                    if (overtime == null)
                    {
                        return new OperationResult(OperationResultType.Error, "修改失败，数据不存在");
                    }
                    else
                    {
                        if (overtime.VerifyType == (int)VerifyFlag.Pass)
                        {
                            return new OperationResult(OperationResultType.Error, "审核通过，不能修改数据");
                        }
                        else if (overtime.VerifyType == (int)VerifyFlag.Verifing)
                        {
                            IQueryable<Overtime> listAtt = listOvertime.Where(x => x.AdminId == dto.AdminId && x.Id != dto.Id);
                            foreach (Overtime att in listAtt)
                            {
                                if ((att.StartTime <= dto.StartTime && att.EndTime >= dto.StartTime) || (att.StartTime <= dto.EndTime && att.EndTime >= dto.EndTime))
                                {
                                    return new OperationResult(OperationResultType.Error, "加班时间段有重叠，请重新选择时间段！");
                                }
                            }
                        }
                    }
                }

                OperationResult result = _overtimeRepository.Update(dtos,
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
                var entities = _overtimeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _overtimeRepository.Update(entity);
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
                var entities = _overtimeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _overtimeRepository.Update(entity);
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
                var entities = _overtimeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _overtimeRepository.Update(entity);
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
                var entities = _overtimeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _overtimeRepository.Update(entity);
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
        public OperationResult Verify(OvertimeDto dto)
        {
            try
            {
                int adminId = dto.AdminId;
                OperationResult oper = new OperationResult(OperationResultType.Error, "审核数据不存在");
                Overtime overtime = _overtimeRepository.Entities.Where(x => x.AdminId == adminId && x.Id == dto.Id).FirstOrDefault();
                if (overtime == null)
                {
                    return oper;
                }
                else
                {
                    if (overtime.VerifyType == (int)VerifyFlag.Pass)
                    {
                        if (dto.VerifyType == (int)VerifyFlag.NoPass)
                        {
                            oper = VerifyNoPass(overtime);
                            return oper;
                        }
                    }
                    else
                    {
                        if (dto.VerifyType == (int)VerifyFlag.Pass)
                        {
                            oper = VerifyPass(overtime, adminId);
                            return oper;
                        }
                        else if (dto.VerifyType == (int)VerifyFlag.NoPass)
                        {
                            oper = VerifyNoPass(overtime);
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
        /// <param name="overtime"></param>
        /// <returns></returns>
        private OperationResult VerifyNoPass(Overtime overtime)
        {
            overtime.VerifyType = (int)VerifyFlag.NoPass;
            overtime.UpdatedTime = DateTime.Now;
            overtime.OperatorId = AuthorityHelper.OperatorId;
            int num = _overtimeRepository.Update(overtime);
            return num > 0 ? new OperationResult(OperationResultType.Success, "审核成功") : new OperationResult(OperationResultType.Error, "审核失败");
        }
        #endregion

        #region 审核通过
        /// <summary>
        /// 审核通过
        /// </summary>
        /// <param name="overtime"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        private OperationResult VerifyPass(Overtime overtime, int adminId)
        {
            Administrator admin = _adminRepository.Entities.Where(x => x.Id == adminId).FirstOrDefault();
            if (admin == null) return new OperationResult(OperationResultType.Error, "审核的员工不存在");
            if (admin.JobPosition == null) return new OperationResult(OperationResultType.Error, "该员工没有工作职位，添加数据失败");
            UnitOfWork.TransactionEnabled = true;
            double OvertimeDays = overtime.OvertimeDays;
            overtime.VerifyType = (int)VerifyFlag.Pass;
            overtime.UpdatedTime = DateTime.Now;
            overtime.OperatorId = AuthorityHelper.OperatorId;
            _overtimeRepository.Update(overtime);
            //生成积分奖励记录
            _memberDepositContract.LogGetScoreWhenOvertime(admin.Member, Convert.ToDecimal(overtime.GetPoints));
            admin.Member.Score = admin.Member.Score + Convert.ToDecimal(overtime.GetPoints);
            _memberRepository.Update(admin.Member);

            if (admin.IsPersonalTime)
            {
                WorkTimeDetaile wtd = _workTimeDetaileRepository.Entities.FirstOrDefault(w => w.WorkTimeId == (admin.WorkTimeId ?? 0) && w.Year == overtime.StartTime.Year && w.Month == overtime.StartTime.Month && w.WorkDay == overtime.StartTime.Day);
                if (wtd != null)
                {
                    wtd.WorkTimeType = 1;
                    if (Convert.ToDateTime(overtime.StartTime.ToShortDateString() + " " + wtd.AmStartTime) > overtime.StartTime)
                    {
                        wtd.AmStartTime = overtime.StartTime.Hour + ":" + overtime.StartTime.Minute;
                    }
                    if (Convert.ToDateTime(overtime.EndTime.ToShortDateString() + " " + wtd.PmEndTime) < overtime.EndTime)
                    {
                        wtd.PmEndTime = overtime.EndTime.Hour + ":" + overtime.EndTime.Minute;
                    }
                    wtd.WorkHour = Convert.ToInt32(Math.Ceiling((Convert.ToDateTime(overtime.EndTime.ToShortDateString() + " " + wtd.PmEndTime) - Convert.ToDateTime(overtime.StartTime.ToShortDateString() + " " + wtd.AmStartTime)).TotalHours));

                    _workTimeDetaileRepository.Update(wtd);
                }
            }
            return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "审核成功") : new OperationResult(OperationResultType.Error, "审核失败");
        }
        #endregion
        #region 加班兑换
        public OperationResult ExchangeOvertime(int Id)
        {
            try
            {
                OperationResult oper = new OperationResult(OperationResultType.Error);
                Overtime overtime = _overtimeRepository.GetByKey(Id);
                if (overtime.VerifyType == (int)VerifyFlag.Pass)
                {
                    int count = _overtimeRestItemRepository.Entities.Where(x => x.OvertimeId == overtime.Id).Count();
                    if (count > 0)
                    {
                        oper.Message = "已经兑换过了，不能再次兑换";
                    }
                    else
                    {
                        OvertimeRestItem overtimeRestItem = new OvertimeRestItem()
                        {
                            OvertimeId = overtime.Id,
                            OperatorId = AuthorityHelper.OperatorId
                        };
                        count = _overtimeRestItemRepository.Update(overtimeRestItem);
                        if (count > 0)
                        {
                            oper.ResultType = OperationResultType.Success;
                            oper.Message = "兑换成功";
                        }
                        else
                        {
                            oper.Message = "兑换失败";
                        }
                    }
                }
                else
                {
                    oper.Message = "审核还没有通过，请稍后尝试";
                }
                return oper;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
