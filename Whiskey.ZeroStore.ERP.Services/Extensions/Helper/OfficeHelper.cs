using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Extensions.Helper
{
    public static class OfficeHelper
    {
        public const string _holidayKey = "dicDate";
       
        #region 校验工作时间
        /// <summary>
        /// 校验是否为工作时间
        /// </summary>
        /// <param name="attenTime"></param>
        /// <param name="workTime"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static bool IsWorkDay(DateTime attenTime, WorkTime workTime, Dictionary<string, int> dic)
        {
            string strCurrentWeek = attenTime.DayOfWeek.ToString("d");
            string strCurrentDate = attenTime.ToString("yyyyMMdd");
            string[] arrWeek = workTime.WorkWeek.Split(',');
            int count = 0;
            if (workTime.IsPersonalTime)
            {
                strCurrentWeek = attenTime.Day.ToString().PadLeft(2, '0');
            }
            if (arrWeek.Contains(strCurrentWeek))
            {
                if (workTime.IsVacations == true)
                {
                    if (dic != null)
                    {
                        count = dic.Where(x => x.Key == strCurrentDate && x.Value == (int)WorkDateFlag.Holidays).Count();
                    }
                    if (count > 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (workTime.IsVacations == true)
                {
                    if (dic != null)
                    {
                        count = dic.Where(x => x.Key == strCurrentDate && x.Value == (int)WorkDateFlag.WorkDay).Count();
                    }
                    if (count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
        }

        #endregion



        #region 当为弹性工作时间时初始化对象
        /// <summary>
        /// 当为弹性工作时间时初始化对象
        /// </summary>
        /// <returns></returns>
        public static WorkTime CheckworkTime(WorkTime workTime)
        {
            if (workTime.IsFlexibleWork == true)
            {
                workTime.AmStartTime = "9:00";
                workTime.AmEndTime = "12:00";
                workTime.PmStartTime = "13:00";
                workTime.PmEndTime = "18:00";
                workTime.WorkHour = 8;
            }
            return workTime;
        }
        #endregion

        #region 获取休假时间
        /// <summary>
        /// 获取休假时间
        /// </summary>
        /// <param name="weeks"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static int GetRestDay(string[] weeks, DateTime startDate, DateTime endDate, bool IsVacations, Dictionary<string, int> dic, WorkTime workTime)
        {
            int restDay = 0;
            int index = 0;
            while (true)
            {
                DateTime tempDate = startDate.AddDays(index);
                if (tempDate.Year == endDate.Year && tempDate.Month == endDate.Month && tempDate.Day == endDate.Day)
                {
                    restDay = restDay + CalcRestDay(tempDate, weeks, dic, IsVacations, workTime);
                    break;
                }
                else
                {
                    restDay = restDay + CalcRestDay(tempDate, weeks, dic, IsVacations,workTime);
                }
                index += 1;
            }
            return restDay;
        }

        private static int CalcRestDay(DateTime tempDate, string[] weeks, Dictionary<string, int> dic, bool IsVacations, WorkTime workTime)
        {
            int restDay = 0;
            string strCurrentWeek = tempDate.DayOfWeek.ToString("d");
            if (workTime.IsPersonalTime)
            {
                strCurrentWeek = tempDate.Day.ToString().PadLeft(2,'0');
            }
            string strCurrentDate = tempDate.ToString("yyyyMMdd");
            if (weeks.Contains(strCurrentWeek))
            {
                if (IsVacations == true)
                {
                    if (dic.ContainsKey(strCurrentDate))
                    {
                        if (dic[strCurrentDate] == (int)WorkDateFlag.Holidays)
                        {
                            restDay += 1;
                        }
                    }
                }

            }
            else
            {
                if (IsVacations == true)
                {
                    if (dic.ContainsKey(strCurrentDate))
                    {
                        if (dic[strCurrentDate] != (int)WorkDateFlag.WorkDay)
                        {
                            restDay += 1;
                        }
                    }
                    else
                    {
                        restDay += 1;
                    }
                }
                else
                {
                    restDay += 1;
                }
            }
            return restDay;
        }
        #endregion

        #region 将假期放入到缓存中
        /// <summary>
        /// 将假期放入到缓存中
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Dictionary<string, int> InsertHolidayCache(List<Holiday> list)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            object obj = CacheHelper.GetCache(_holidayKey);
            if (obj == null)
            {
                foreach (Holiday holiday in list)
                {
                    DateTime startTime = holiday.StartTime;
                    DateTime endTime = holiday.EndTime;
                    int holidayDays = holiday.HolidayDays;
                    while (true)
                    {
                        if (holidayDays == 0)
                        {
                            break;
                        }
                        else
                        {
                            dic.Add(startTime.ToString("yyyyMMdd"), (int)WorkDateFlag.Holidays);
                        }
                        startTime = startTime.AddDays(1);
                        holidayDays -= 1;
                    }
                    string workDates = holiday.WorkDates;
                    if (!string.IsNullOrEmpty(workDates))
                    {
                        string[] arrDates = workDates.Split(',');
                        foreach (string date in arrDates)
                        {
                            if (!string.IsNullOrEmpty(date))
                            {
                                DateTime currentDate = DateTime.Parse(date);
                                dic.Add(currentDate.ToString("yyyyMMdd"), (int)WorkDateFlag.WorkDay);
                            }
                        }
                    }
                }
                CacheHelper.SetCache(_holidayKey, dic);
            }
            else
            {
                dic = obj as Dictionary<string, int>;
            }
            return dic;

        }
        #endregion

    }
}
