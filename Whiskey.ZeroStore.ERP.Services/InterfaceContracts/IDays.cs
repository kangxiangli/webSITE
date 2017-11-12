using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.InterfaceContracts
{
    /// <summary>
    /// 定义请假接口
    /// </summary>
    public interface IDays
    {

        OperationResult ComputeDay(WorkTime workTime, DateTime startDate, DateTime endDate, Dictionary<string, int> dic);
        
    }
}
