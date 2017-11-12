
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IAppVerManageContract : IBaseContract<AppVerManage, AppVerManageDto>
    {
        /// <summary>
        /// App版本更新检测
        /// </summary>
        /// <param name="version">当前版本号,例:1.0.1</param>
        /// <param name="AppFlag">App类型</param>
        /// <returns></returns>
        OperationResult CheckUpdate(string version, AppTypeFlag? AppFlag);
    }
}

