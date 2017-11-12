using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Extensions.Helper;
using Whiskey.ZeroStore.ERP.Services.SuperContracts;

namespace Whiskey.ZeroStore.ERP.Services.Achieve
{
    public class OvertimeDaysOperation : OvertimeDaysSuper
    {
        public override OperationResult ComputeDay(WorkTime workTime, DateTime startDate, DateTime endDate, Dictionary<string, int> dic)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "请选择正确的加班时间");
            if (dic.Any()==false)
            {
                oper.Message = "请先添加公休假";                
            }
            else
            {
                workTime = OfficeHelper.CheckworkTime(workTime);
                DateTime amStartTime = DateTime.Parse(workTime.AmStartTime);
                DateTime amEndTime = DateTime.Parse(workTime.AmEndTime);
                DateTime pmStartTime = DateTime.Parse(workTime.PmStartTime);
                DateTime pmEndTime = DateTime.Parse(workTime.PmEndTime);
                string[] weeks = workTime.WorkWeek.Split(',');
                int restDay = OfficeHelper.GetRestDay(weeks, startDate, endDate, workTime.IsVacations, dic,workTime);
                oper.ResultType = OperationResultType.Success;
                oper.Data = restDay;
            }
            return oper;
        }
    }
}
