using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.Helper
{
    public static class StringHelper
    {
        public static string GetPrefix(int level)
        {
            var result = string.Empty;
            for (var i = 0; i < level; i++)
            {
                result += "　　";
            }
            if (level != 0)
            {
                //for (var i = 0; i <= level; i++)
                //{
                //    result += "│";
                //}
                result += "|–";  // 代码 └├│┊┆┄┈┬┴└┌//|–
            }
            return result;
        }

        /// <summary>
        /// 获取字符串实际长度
        /// </summary>
        /// <param name="str"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string FixedLength(string text, int length)
        {
            var result = string.Empty;
            if (text != null && text.Length > 0)
            {
                var len = Encoding.GetEncoding("gb2312").GetBytes(text).Length;
                if (len<length)
                {
                    var s = "";
                    for (var i = 0; i < (length-len); i++)
                    {
                        s += " ";
                    }
                    result = text+s;
                }else {
                    var counter = 0;
                    var sb = new StringBuilder();
                    foreach (char c in text)
                    {
                        int t = Encoding.GetEncoding("gb2312").GetByteCount(new char[] { c });
                        if (counter + t > length)
                        {
                            break;
                        }
                        sb.Append(c);
                        counter += t;
                    }
                    result = sb.ToString();
                }
            }
            return result;
        }

        #region Base64转换成string
                
        /// <summary>
        /// Base64转换成string
        /// </summary>
        /// <param name="param">Base64</param>
        /// <returns></returns>
        public static string ConvertToString(string param)
        {
            byte[] buffer = Convert.FromBase64String(param);
            string strParam = Encoding.Default.GetString(buffer);
            return strParam;
        }
        #endregion
    }
}
