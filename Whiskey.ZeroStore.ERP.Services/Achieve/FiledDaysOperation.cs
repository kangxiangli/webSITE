using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.ZeroStore.ERP.Services.SuperContracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;

namespace Whiskey.ZeroStore.ERP.Services.Achieve
{
    public class FiledDaysOperation : FiledDaysSuper
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
            OperationResult oper = new OperationResult(OperationResultType.Success);
            if (endDate <= startDate)
            {
                oper = new OperationResult(OperationResultType.Error, "外勤开始时间必须小于外勤结束时间！");
            }
            oper.Data = Math.Round((endDate - startDate).TotalHours, 0);
            //if (workTime.WorkTimeType == (int)WorkTimeFlag.AllDay)
            //{
            //    oper = this.ComputeAllFieldDay(workTime, startDate, endDate, dic);
            //}
            //else
            //{
            //    oper = this.ComputeHalfFieldDay(workTime, startDate, endDate, dic);
            //}
            return oper;
        }
        #endregion

        #region 全天班--计算外勤天数
        /// <summary>
        /// 全天班--计算外勤天数
        /// </summary>
        /// <param name="workTime"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        private OperationResult ComputeAllFieldDay(WorkTime workTime, DateTime startDate, DateTime endDate, Dictionary<string, int> dic)
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
                        double WorkDays = days + 0.5 - restDay;
                        double FieldDays = days + 0.5;
                        oper.Data = new
                        {
                            WorkDays,
                            FieldDays
                        };
                    }
                    else if (pmEndTime.Hour == endDate.Hour && pmEndTime.Minute == endDate.Minute)
                    {
                        oper.ResultType = OperationResultType.Success;
                        double WorkDays = days + 1 - restDay;
                        double FieldDays = days + 1;
                        oper.Data = new
                        {
                            WorkDays,
                            FieldDays
                        };
                    }
                }
                else if (pmStartTime.Hour == startDate.Hour && pmStartTime.Minute == startDate.Minute)
                {
                    if (amEndTime.Hour == endDate.Hour && amEndTime.Minute == endDate.Minute)
                    {
                        oper.ResultType = OperationResultType.Success;
                        double WorkDays = days + 1 - restDay;
                        double FieldDays = days + 1;
                        oper.Data = new
                        {
                            WorkDays,
                            FieldDays
                        };
                    }
                    else if (pmEndTime.Hour == endDate.Hour && pmEndTime.Minute == endDate.Minute)
                    {
                        oper.ResultType = OperationResultType.Success;
                        double WorkDays = days + 0.5 - restDay;
                        double FieldDays = days + 0.5;
                        oper.Data = new
                        {
                            WorkDays,
                            FieldDays
                        };
                    }
                }
            }
            return oper;

        }
        #endregion

        #region 小时工--计算外勤天使
        /// <summary>
        /// 小时工--计算外勤天使
        /// </summary>
        /// <returns></returns>
        private OperationResult ComputeHalfFieldDay(WorkTime workTime, DateTime startDate, DateTime endDate, Dictionary<string, int> dic)
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
                            double WorkDays = days + 1 - restDay;
                            double FieldDays = days + 1;
                            oper.Data = new
                            {
                                WorkDays,
                                FieldDays
                            };
                        }
                    }
                }
            }
            return oper;
        }

        #endregion
    }
}
