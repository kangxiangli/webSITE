
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IWorkOrderDealtWithContract : IBaseContract<WorkOrderDealtWith, WorkOrderDealtWithDto>
    {
        /// <summary>
        /// ������
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="status">����״̬��-1���Ѿܾ� 1���ѽ��ܣ�2������ɣ�</param>
        /// <returns></returns>
        OperationResult DealtWith(int Id, int status, string Notes);
    }
}

