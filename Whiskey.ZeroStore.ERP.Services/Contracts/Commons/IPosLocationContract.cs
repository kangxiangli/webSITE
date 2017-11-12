
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IPosLocationContract : IBaseContract<PosLocation, PosLocationDto>
    {
        /// <summary>
        /// IMEI不存在自动创建,存在则刷新
        /// </summary>
        /// <param name="IEMI">设备标识</param>
        /// <param name="Longitude">经度</param>
        /// <param name="Latitude">纬度</param>
        /// <returns></returns>
        OperationResult UpdateLocation(string IMEI, double Longitude, double Latitude);
    }
}

