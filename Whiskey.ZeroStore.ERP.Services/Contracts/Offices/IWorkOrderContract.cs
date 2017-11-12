
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IWorkOrderContract : IBaseContract<WorkOrder, WorkOrderDto>
    {
        /// <summary>
        /// ������ָ���������
        /// </summary>
        /// <param name="cancel"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        OperationResult CancelOrRecoveryCancel(bool cancel, params int[] ids);

        /// <summary>
        /// ָ�ɹ���
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="adminId"></param>
        /// <returns></returns>
        OperationResult Assign(int Id, int adminId);
    }
}

