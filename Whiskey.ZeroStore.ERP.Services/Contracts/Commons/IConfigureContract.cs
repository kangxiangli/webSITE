
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IConfigureContract : IBaseContract<Configure, ConfigureDto>
    {
        ///<summary>
        /// 设置节点
        ///</summary>
        ///<param name="DirName">配置文件名</param>
        /// <param name="xmlFileName">配置类名</param>
        /// <param name="QueryElementID">配置节点名称</param>
        /// <param name="innerText">配置的值</param>
        /// <returns></returns>
        bool SetConfigure(string DirName, string xmlFileName, string QueryElementID, string innerText);

        ///<summary>
        /// 获取配置值
        ///</summary>
        ///<param name="DirName">配置文件名</param>
        /// <param name="xmlFileName">配置类名</param>
        /// <param name="QueryElementID">配置节点名称</param>
        /// <param name="defValue">默认的值</param>
        /// <returns></returns>
        string GetConfigureValue(string DirName, string xmlFileName, string QueryElementID, string defValue = "0");
    }
}

