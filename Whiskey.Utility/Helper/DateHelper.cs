using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Utility.Helper
{
    public static class DateHelper
    {
        /// <summary>
        /// 返回为星期
        /// </summary>
        /// <param name="time">要转换的时间</param>
        /// <returns></returns>
        public static string GetWeekOfXQ(DateTime time)
        {
            return "星期" + "日一二三四五六".Substring((int)time.DayOfWeek, 1);
            //return time.ToString("dddd", new System.Globalization.CultureInfo("zh-CN"));       //该方法暂未进行测试，暂不使用
        }

        /// <summary>
        /// 返回为周
        /// </summary>
        /// <param name="time">要转换的时间</param>
        /// <returns></returns>
        public static string GetWeekOfZ(DateTime time)
        {
            return "周" + "日一二三四五六".Substring((int)time.DayOfWeek, 1);
        }

        /// <summary>
        /// 返回为英文的星期
        /// </summary>
        /// <param name="time">要转换的时间</param>
        /// <returns></returns>
        public static string GetWeekOfEnglish(DateTime time)
        {
            return time.DayOfWeek.ToString();
        }
        private static Random rand = new Random();
        /// <summary>
        /// 根据指定的日期时间段,按天数生成随机时间
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns></returns>
        public static List<DateTime> GetRandomDateTime(DateTime startDate,DateTime endDate,int count)
        {
            if (startDate.Date > endDate.Date)
            {
                throw new Exception("起始日期不能超过结束日期");
            }
            


            List<DateTime> list = new List<DateTime>();
            if (startDate.Date == endDate.Date) //相同日期的情况
            {
                list.Add(startDate.Date);
            }
            else
            {
                for (int i = 0; i <= (endDate - startDate).Days; i++)
                {

                    var date = startDate.AddDays(i);
                    
                    list.Add(date);
                }
            }

            if (list.Count <= 0)
            {
                throw new Exception("日期数据不能为空");
            }
            
            List<DateTime> randomDates = new List<DateTime>();
            while (randomDates.Count<count)
            {
                var date =list[rand.Next(0,list.Count)]
                    .AddHours(rand.Next(9, 19))
                    .AddMinutes(rand.Next(0, 60))
                    .AddSeconds(rand.Next(0, 60));
                randomDates.Add(date);
            }

            return randomDates.ToList();
        }
    }
}
