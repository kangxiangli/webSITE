using Whiskey.Core.Data;
using Whiskey.Utility.Helper;

namespace Whiskey.Core
{
    /// <summary>
    /// 业务实现基类
    /// </summary>
    public abstract class ServiceBase
    {
        /// <summary>
        /// WebUrl地址，来自web.config
        /// </summary>
        protected readonly string WebUrl = ConfigurationHelper.WebUrl;
        /// <summary>
        /// ApiUrl地址，来自web.config
        /// </summary>
        protected readonly string ApiUrl = ConfigurationHelper.ApiUrl;

        protected ServiceBase(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        /// <summary>
        /// 获取或设置 单元操作对象
        /// </summary>
        protected IUnitOfWork UnitOfWork { get; private set; }
    }
}