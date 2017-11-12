
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IPosLocationContract : IBaseContract<PosLocation, PosLocationDto>
    {
        /// <summary>
        /// IMEI�������Զ�����,������ˢ��
        /// </summary>
        /// <param name="IEMI">�豸��ʶ</param>
        /// <param name="Longitude">����</param>
        /// <param name="Latitude">γ��</param>
        /// <returns></returns>
        OperationResult UpdateLocation(string IMEI, double Longitude, double Latitude);
    }
}

