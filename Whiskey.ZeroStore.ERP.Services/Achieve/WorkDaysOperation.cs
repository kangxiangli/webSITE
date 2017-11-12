using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.SuperContracts;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;

namespace Whiskey.ZeroStore.ERP.Services.Achieve
{
    public class WorkDaysOperation : WorkDaysSuper
    {
        #region 计算工作天数
        /// <summary>
        /// 计算工作天数
        /// </summary>
        /// <param name="workTime"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        public override OperationResult ComputeDay(WorkTime workTime, DateTime startDate, DateTime endDate, Dictionary<string, int> dic)
        {
            OperationResult oper = null;
            if (workTime.WorkTimeType == (int)WorkTimeFlag.AllDay)
            {
                oper = this.ComputeAllWorkDay(workTime, startDate, endDate, dic);
            }
            else
            {
                oper = this.ComputeHalfWorkDay(workTime, startDate, endDate, dic);
            }
            return oper;
        }
        #endregion

        #region 全天班--计算工作天数
        /// <summary>
        /// 全天班--计算工作天数
        /// </summary>
        /// <param name="workTime"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        private OperationResult ComputeAllWorkDay(WorkTime workTime, DateTime startDate, DateTime endDate, Dictionary<string, int> dic)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "请选择正确的工作时间");
            if (dic.Any() == false)
            {
                oper.Message = "请先添加公休假";
            }
            else
            {
                workTime = OfficeHelper.CheckworkTime(workTime);
                TimeSpan timeSpan = endDate - startDate;
                int days = timeSpan.Days;
                //if (days > 0) {
                //    //请假天数大于1
                //    string endTimeStr = endDate.ToString("t");
                //    if (DateTime.Compare(DateTime.Parse(endTimeStr), DateTime.Parse("09:00")) > 0)
                //    {
                //        if (DateTime.Compare(DateTime.Parse(workTime.PmEndTime), DateTime.Parse(endTimeStr)) > 0)
                //        {
                //            //请假时间小于上午下班时间
                //        }
                //    }
                //}
                DateTime amStartTime = DateTime.Parse(workTime.AmStartTime);
                DateTime amEndTime = DateTime.Parse(workTime.AmEndTime);
                DateTime pmStartTime = DateTime.Parse(workTime.PmStartTime);
                DateTime pmEndTime = DateTime.Parse(workTime.PmEndTime);
                string[] weeks = workTime.WorkWeek.Split(',');
                int restDay = OfficeHelper.GetRestDay(weeks, startDate, endDate, workTime.IsVacations, dic, workTime);
                if (amStartTime.Hour == startDate.Hour && amStartTime.Minute == startDate.Minute)
                {
                    if (amEndTime.Hour == endDate.Hour && amEndTime.Minute == endDate.Minute)
                    {
                        oper.ResultType = OperationResultType.Success;
                        string data = (days + 0.5 - restDay).ToString();
                        oper.Data = data;
                    }
                    else if (pmEndTime.Hour == endDate.Hour && pmEndTime.Minute == endDate.Minute)
                    {
                        oper.ResultType = OperationResultType.Success;
                        string data = (days + 1 - restDay).ToString();
                        oper.Data = data;
                    }
                }
                else if (pmStartTime.Hour == startDate.Hour && pmStartTime.Minute == startDate.Minute)
                {
                    if (amEndTime.Hour == endDate.Hour && amEndTime.Minute == endDate.Minute)
                    {
                        oper.ResultType = OperationResultType.Success;
                        string data = (days + 1 - restDay).ToString();
                        oper.Data = data;
                    }
                    else if (pmEndTime.Hour == endDate.Hour && pmEndTime.Minute == endDate.Minute)
                    {
                        oper.ResultType = OperationResultType.Success;
                        string data = (days + 0.5 - restDay).ToString();
                        oper.Data = data;
                    }
                }
            }
            return oper;
        }
        #endregion

        #region 小时工--计算工作天数

        /// <summary>
        /// 小时工--计算工作天数
        /// </summary>
        /// <param name="workTime"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="dic"></param>
        /// <returns></returns>
        private OperationResult ComputeHalfWorkDay(WorkTime workTime, DateTime startDate, DateTime endDate, Dictionary<string, int> dic)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "请选择正确的工作时间");
            if (dic.Any() == false)
            {
                oper.Message = "请先添加公休假";
            }
            else
            {
                TimeSpan timeSpan = endDate - startDate;
                int days = timeSpan.Days;
                workTime = OfficeHelper.CheckworkTime(workTime);
                DateTime amStartTime = DateTime.Parse(workTime.AmStartTime);
                DateTime amEndTime = DateTime.Parse(workTime.PmEndTime);
                string[] weeks = workTime.WorkWeek.Split(',');
                int restDay = OfficeHelper.GetRestDay(weeks, startDate, endDate, workTime.IsVacations, dic, workTime);
                if (workTime.WorkTimeType == (int)WorkTimeFlag.HalfDay && workTime.IsFlexibleWork == true)
                {
                    oper.Message = "小时工暂时不支持弹性工作时间";
                }
                else
                {
                    if (amStartTime.Hour == startDate.Hour && amStartTime.Minute == startDate.Minute)
                    {
                        if (amEndTime.Hour == endDate.Hour && amEndTime.Minute == endDate.Minute)
                        {
                            oper.ResultType = OperationResultType.Success;
                            string data = (days + 1).ToString() + "," + (days + 1 - restDay).ToString();
                            oper.Data = data;
                        }
                    }
                }
            }
            return oper;
        }
        #endregion

    }
}
