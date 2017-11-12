using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Secutiry;


namespace Whiskey.Web.Net.WeChatPay
{
    public static class WeChatPayRequest
    {
        #region 参数配置
        private const string RequestUrl = "https://api.mch.weixin.qq.com/pay/unifiedorder";
        private static readonly string InputCharset;
        #endregion

        #region 初始化参数
        static WeChatPayRequest()
        {
            InputCharset = WeChatPayConfig.InputCharset;
        }
        #endregion

        #region 请求数据
        /// <summary>
        /// 建立请求，以模拟远程HTTP的POST请求方式构造并获取微信的处理结果
        /// </summary>
        /// <param name="sParaTemp">请求参数数组</param>
        /// <returns>微信处理结果</returns>
        public static string BuildRequest(string strRequestData)
        {
            Encoding code = Encoding.GetEncoding(InputCharset);
            
            //待请求参数数组字符串
            //string strRequestData = BuildRequestParaToString(notify, code);

            //把数组转换成流中所需字节数组类型
            byte[] bytesRequestData = code.GetBytes(strRequestData);

            //构造请求地址
            string strUrl = RequestUrl + "_input_charset=" + InputCharset;

            //请求远程HTTP
            string strResult = string.Empty;
            try
            {
                //设置HttpWebRequest基本信息
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(strUrl);
                myReq.Method = "post";
                myReq.ContentType = "application/x-www-form-urlencoded";

                //填充POST数据
                myReq.ContentLength = bytesRequestData.Length;
                Stream requestStream = myReq.GetRequestStream();
                requestStream.Write(bytesRequestData, 0, bytesRequestData.Length);
                requestStream.Close();

                //发送POST数据请求服务器
                HttpWebResponse httpWResp = (HttpWebResponse)myReq.GetResponse();
                Stream myStream = httpWResp.GetResponseStream();

                //获取服务器返回信息
                if (myStream != null)
                {
                    StreamReader reader = new StreamReader(myStream, code);
                    StringBuilder responseData = new StringBuilder();
                    String line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        responseData.Append(line);
                    }

                    //释放
                    myStream.Close();

                    strResult = responseData.ToString();
                }
            }
            catch (Exception exp)
            {
                strResult = "报错：" + exp.Message;
            }

            return strResult;
        }

       
        #endregion

        #region 注释- 生成要请求给微信的参数数组

        /// <summary>
        /// 生成要请求给微信的参数数组
        /// </summary>
        /// <param name="sParaTemp">请求前的参数数组</param>
        /// <param name="code">字符编码</param>
        /// <returns>要请求的参数数组字符串</returns>
        //private static string BuildRequestParaToString(WeChatNotify notify, Encoding code)
        //{
        //    //待签名请求参数字符串
        //    WeChatNotifyFomatter infoFo = new WeChatNotifyFomatter(code);
        //    string strSign = infoFo.Format("nosign", notify, null);
        //    notify.Sign = HashHelper.GetMd5(strSign, code).ToUpper();

        //    //把参数组中所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串，并对参数值做urlencode
        //    string strRequestData = infoFo.Format("all", notify, null);

        //    return strRequestData;
        //}
        
        #endregion
     }
}
