
using System;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IAppointmentGenContract : IBaseContract<AppointmentGen, AppointmentGenDto>
    {
        OperationResult Insert(Action<bool, string, int[]> process, params AppointmentGenDto[] dtos);

        /// <summary>
        /// 批量导入预约数据
        /// </summary>
        /// <param name="process"></param>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult BatchImpot(Action<bool, string, int[]> process, params AppointmentGenBatchDto[] dtos);
    }
}

