using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Whiskey.Web.Helper
{
    public static class StaticHelper
    {
        public static List<SelectListItem> DiscountList(string title = "请选择")
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = "不打折", Value = "1.0", Selected = true });
            for (int d = 99; d >= 1; d--)
            {
                if ((d / 100f) >= 0.1)
                {
                    var discount = (d / 100f).ToString();
                    list.Add(new SelectListItem { Text = ConvertHelper.NumberToChinese(d.ToString(), true) + "" + "（" + discount + "）", Value = discount });
                }
            }
            return list;
        }
        public static List<SelectListItem> CommList(string title = "请选择")
        {
            var list = new List<SelectListItem>();
            list.Add(new SelectListItem { Text = "无提成", Value = "1.0", Selected = true });
            for (int d = 99; d >= 1; d--)
            {
                if ((d / 100f) >= 0.1)
                {
                    var discount = (d / 100f).ToString();
                    list.Add(new SelectListItem { Text = ConvertHelper.NumberToChinese(d.ToString(), true) + "" + "（" + discount + "）", Value = discount });
                }
            }
            return list;
        }
    }
}
