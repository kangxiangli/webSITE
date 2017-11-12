using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IHolidayContract : IDependency
    {
        #region IHolidayContract

        Holiday View(int Id);

        HolidayDto Edit(int Id);

        IQueryable<Holiday> Holidays { get; }

        OperationResult Insert(params HolidayDto[] dtos);

        OperationResult Update(params HolidayDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        Dictionary<string, int> GetHoliday();
         

        #endregion




        
    }
}
