using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IAttendanceRepairContract : IDependency
    {
        #region IAttendanceContract

        AttendanceRepair View(int Id);

        AttendanceRepairDto Edit(int Id);

        IQueryable<AttendanceRepair> AttendanceRepairs { get; }

        OperationResult Insert(params AttendanceRepairDto[] dtos);

        OperationResult Update(params AttendanceRepairDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);
        OperationResult Verify(AttendanceRepairDto dto);

        /// <summary>
        /// 申请补卡
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        OperationResult ApplyRepair(int adminId, int id, int? AttenFlag, string Reasons = "");
        OperationResult ApplyRepairBySystem(int adminId, string project = "website");

        /// <summary>
        /// 确认补卡
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        OperationResult ConfirmRepair(int adminId, int id, int? AttenFlag, string project = "website");
        OperationResult ConfirmRepairBySystem(int adminId, string project = "website");
        OperationResult ApplyNoLoginRepair(int adminId, string nowTime, int? AttenFlag);
        #endregion

    }
}
