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
    public interface IAttendanceStatisticsContract : IDependency
    {

        #region IAttendanceStatisticsContract

        AttendanceStatistics View(int Id);

        AttendanceStatisticsDto Edit(int Id);

        IQueryable<AttendanceStatistics> AttendanceStatisticses { get; }

        OperationResult Insert(params AttendanceStatisticsDto[] dtos);

        OperationResult Update(params AttendanceStatisticsDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);


        
        #endregion


        
    }
}
