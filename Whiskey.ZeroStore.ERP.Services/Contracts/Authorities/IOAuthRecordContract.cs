using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IOAuthRecordContract : IBaseContract<OAuthRecord>
    {
        /// <summary>
        /// 不存时自动添加,存在直接返回成功
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        OperationResult InsertIfNotExist(OAuthRecord entity);
    }
}
