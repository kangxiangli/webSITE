using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Utility.Helper
{
    public class EnumHelper
    {
        /// <summary>
        /// 获取枚举内容值或描述
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerationValue"></param>
        /// <returns></returns>
        public static T GetValue<T>(Enum enumerationValue)
        {
            Type type = enumerationValue.GetType();
            MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                var result = ((DescriptionAttribute)attrs[0]).Description;
                return (T)Convert.ChangeType(result, typeof(T));
            }
            else
            {
                return (T)Convert.ChangeType(enumerationValue, typeof(T));
            }
        }

    }
}
