using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.WeChat
{
    /// <summary>
    /// 微信支付错误码
    /// </summary>
    public enum ErrCodeFlag
    {
        /// <summary>
        /// 商户无此接口权限	
        /// </summary>
        NOAUTH,

        /// <summary>
        /// 余额不足	
        /// </summary>
        NOTENOUGH,

        /// <summary>
        /// 商户订单已支付
        /// </summary>
        ORDERPAID,

        /// <summary>
        /// 订单已关闭
        /// </summary>
        ORDERCLOSED,

        /// <summary>
        /// 系统错误	
        /// </summary>
        SYSTEMERROR,

        /// <summary>
        /// APPID不存在	
        /// </summary>
        APPID_NOT_EXIST,

        /// <summary>
        /// MCHID不存在	
        /// </summary>
        MCHID_NOT_EXIST,

        /// <summary>
        /// appid和mch_id不匹配	
        /// </summary>
        APPID_MCHID_NOT_MATCH,

        /// <summary>
        /// 缺少参数	
        /// </summary>
        LACK_PARAMS,

        /// <summary>
        /// 商户订单号重复
        /// </summary>
        OUT_TRADE_NO_USED,

        /// <summary>
        /// 签名错误	
        /// </summary>
        SIGNERROR,

        /// <summary>
        /// XML格式错误	
        /// </summary>
        XML_FORMAT_ERROR,

        /// <summary>
        /// 请使用post方法	
        /// </summary>
        REQUIRE_POST_METHOD,

        /// <summary>
        /// post数据为空	
        /// </summary>
        POST_DATA_EMPTY	,

        /// <summary>
        /// 编码格式错误	
        /// </summary>
        NOT_UTF8,
    }
}
