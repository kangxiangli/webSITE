using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.VideoConfig
{
    public class VideoData
    {
        /// <summary>
        /// 零时尚接口数据类，所有的API接口通信都依赖这个数据结构，
        /// 在调用接口之前先填充各个字段的值，然后进行接口通信，
        /// 这样设计的好处是可扩展性强，用户可随意对协议进行更改而不用重新设计数据结构，
        /// 还可以随意组合出不同的协议数据包，不用为每个协议设计一个数据包结构
        public VideoData()
        {

        }

        public string signKey = "";//PaymentConfigHelper.signKey;

        //采用排序的Dictionary的好处是方便对数据包进行签名，不用再签名之前再做一次排序
        private SortedDictionary<string, object> m_values = new SortedDictionary<string, object>();

        /**
        * 设置某个字段的值
        * @param key 字段名
         * @param value 字段值
        */
        public void SetValue(string key, object value)
        {
            m_values[key] = value;
        }

        /**
        * 根据字段名获取某个字段的值
        * @param key 字段名
         * @return key对应的字段值
        */
        public object GetValue(string key)
        {
            object o = null;
            m_values.TryGetValue(key, out o);
            return o;
        }

        /**
         * 判断某个字段是否已设置
         * @param key 字段名
         * @return 若字段key已被设置，则返回true，否则返回false
         */
        public bool IsSet(string key)
        {
            object o = null;
            m_values.TryGetValue(key, out o);
            if (null != o)
                return true;
            else
                return false;
        }


        /**
        * @Dictionary格式转化成url参数格式
        * @ return url格式串
        */
        public string ToUrl()
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (pair.Value == null)
                {
                    throw new Exception("FashionData内部含有值为null的字段!");
                }
                buff += pair.Key + ":" + pair.Value + ",";
            }
            buff = buff.Trim(',');
            return buff;
        }

        /**
        * @values格式化成能在Web页面上显示的结果（因为web页面上不能直接输出xml格式的字符串）
        */


        /**
        * @生成签名，详见签名生成算法
        * @return 签名, sign字段不参加签名
        */
        public string MakeSign(string time, string nonce, string appSecret)
        {
            //转url格式
            string str = ToUrl();
            str += ",time:" + time + ",";
            str += "nonce:" + nonce + ",";
            str += "appSecret:" + appSecret + "";
            //MD5加密
            var md5 = MD5.Create();
            var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2"));
            }
            //所有字符转为大写
            return sb.ToString().ToLower();
        }

        /**
        * @获取Dictionary
        */
        public SortedDictionary<string, object> GetValues()
        {
            return m_values;
        }

        public bool CheckTimspan(string timestamp)
        {
            //判断timespan是否有效
            double ts1 = 0;
            double ts2 = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
            bool timespanvalidate = double.TryParse(timestamp, out ts1);
            double ts = ts2 - ts1;
            bool falg = ts > 5 * 60;
            if (falg || (!timespanvalidate))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string GetNonce()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
