
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IConfigureContract : IBaseContract<Configure, ConfigureDto>
    {
        ///<summary>
        /// ���ýڵ�
        ///</summary>
        ///<param name="DirName">�����ļ���</param>
        /// <param name="xmlFileName">��������</param>
        /// <param name="QueryElementID">���ýڵ�����</param>
        /// <param name="innerText">���õ�ֵ</param>
        /// <returns></returns>
        bool SetConfigure(string DirName, string xmlFileName, string QueryElementID, string innerText);

        ///<summary>
        /// ��ȡ����ֵ
        ///</summary>
        ///<param name="DirName">�����ļ���</param>
        /// <param name="xmlFileName">��������</param>
        /// <param name="QueryElementID">���ýڵ�����</param>
        /// <param name="defValue">Ĭ�ϵ�ֵ</param>
        /// <returns></returns>
        string GetConfigureValue(string DirName, string xmlFileName, string QueryElementID, string defValue = "0");
    }
}

