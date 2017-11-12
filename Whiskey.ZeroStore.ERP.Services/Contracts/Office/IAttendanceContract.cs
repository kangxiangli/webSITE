using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 声明接口
    /// </summary>
    public interface IAttendanceContract : IDependency
    {
        #region IAttendanceContract

        Attendance View(int Id);

        AttendanceDto Edit(int Id);

        IQueryable<Attendance> Attendances { get; }

        OperationResult Insert(params AttendanceDto[] dtos);

        OperationResult Update(params AttendanceDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        /// <summary>
        /// 签到
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="department">跳过department参数</param>
        /// <returns></returns>
        OperationResult LoginIn(int adminId, Department department, bool jmpDep = false);

        /// <summary>
        /// 签退
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="department">跳过department参数</param>
        /// <returns></returns>
        OperationResult LoginOut(int adminId, Department department, bool jmpDep = false);

        OperationResult Pardon(DateTime currentDate);

        OperationResult GetList(int adminId, int days);
        OperationResult GetNoLoginCount(int adminId);
        OperationResult CreateOrUpdate(AttendanceDto dto, string strAdminId);
        OperationResult Create(AttendanceDto dto, List<int> adminIds);
        #endregion

        void Add(List<Attendance> listEntity, List<AttendanceStatistics> listStatisticsAdd, List<AttendanceStatistics> listStatisticsUpdate);


        OperationResult Repair(DateTime Date, int AdminId, int Id);

        /// <summary>
        /// 是否需要扣除双倍积分
        /// </summary>
        /// <param name="attenId">要判断的考勤Id</param>
        /// <param name="attenFlag">要判断的考勤标识</param>
        /// <returns></returns>
        bool IsDeductionDoubleScore(int attenId, int attenFlag);

        /// <summary>
        /// 双倍积分签到提醒
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        IDictionary<string, bool> DoubleScoreReminderBySign(int adminId);

        /// <summary>
        /// 扣除双倍积分的数据
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        List<DoubleScore> GetDoubleScoreReminderList(int adminId);
    }
}
