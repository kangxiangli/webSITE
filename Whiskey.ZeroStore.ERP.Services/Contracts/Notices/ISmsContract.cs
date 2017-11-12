using System.Collections.Generic;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ISmsContract : IBaseContract<Sms, SmsDto>
    {
        /// <summary>
        /// 获取云短信平台剩余短信数量
        /// </summary>
        /// <returns></returns>
        int? GetRemainSmsCount();
        /// <summary>
        /// 获取云短信平台已发送短信数量
        /// </summary>
        /// <returns></returns>
        int? GetSendSmsCount();

        bool SendSms(string phone, string content);

        bool SendSms(string phone, TemplateNotificationType flag, Dictionary<string, object> dic);

        OperationResult ConfirmSend(int Id);
    }
}
