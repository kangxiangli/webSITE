using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.InterfaceContracts;

namespace Whiskey.ZeroStore.ERP.Services.SuperContracts
{
    /// <summary>
    /// 抽象请假
    /// </summary>
    public abstract class WorkDaysSuper : IDays
    {
            
        public abstract OperationResult ComputeDay(WorkTime workTime, DateTime startDate, DateTime endDate, Dictionary<string, int> dic);
        
    }
}
