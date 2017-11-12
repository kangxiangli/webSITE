
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IAppVerManageContract : IBaseContract<AppVerManage, AppVerManageDto>
    {
        /// <summary>
        /// App�汾���¼��
        /// </summary>
        /// <param name="version">��ǰ�汾��,��:1.0.1</param>
        /// <param name="AppFlag">App����</param>
        /// <returns></returns>
        OperationResult CheckUpdate(string version, AppTypeFlag? AppFlag);
    }
}

