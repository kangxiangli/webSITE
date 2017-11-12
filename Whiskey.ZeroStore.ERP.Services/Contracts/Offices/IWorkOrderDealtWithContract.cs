
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IWorkOrderDealtWithContract : IBaseContract<WorkOrderDealtWith, WorkOrderDealtWithDto>
    {
        /// <summary>
        /// 处理工单
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="status">处理状态（-1：已拒绝 1：已接受；2：已完成）</param>
        /// <returns></returns>
        OperationResult DealtWith(int Id, int status, string Notes);
    }
}

