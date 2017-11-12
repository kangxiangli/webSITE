using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Web.Helper
{
    public static class ConvertHelper
    {
        public static string NumberToChinese(string num, bool hideZero = false)
        {

            StringBuilder result = new StringBuilder();
            if (num == "100")
            {
                result.Append("原价");
                return result.ToString();
            }
            foreach (char s in num)
            {
                switch (s.ToString())
                {
                    case "0":
                        if (!hideZero) result.Append("零");
                        break;
                    case "1":
                        result.Append("一");
                        break;
                    case "2":
                        result.Append("二");
                        break;
                    case "3":
                        result.Append("三");
                        break;
                    case "4":
                        result.Append("四");
                        break;
                    case "5":
                        result.Append("五");
                        break;
                    case "6":
                        result.Append("六");
                        break;
                    case "7":
                        result.Append("七");
                        break;
                    case "8":
                        result.Append("八");
                        break;
                    case "9":
                        result.Append("九");
                        break;
                }
            }
            return result.ToString() + "折";
        }


        public static void ObjectClone<T>(T sourceObject, T targetObject)
        {
            try
            {
                foreach (var item in typeof(T).GetProperties())
                {
                    item.SetValue(targetObject, item.GetValue(sourceObject, new object[] { }), null);
                }
            }
            catch (NullReferenceException NullEx)
            {
                throw NullEx;
            }
            catch (Exception Ex)
            {
                throw Ex;
            }
        }

    }
}
