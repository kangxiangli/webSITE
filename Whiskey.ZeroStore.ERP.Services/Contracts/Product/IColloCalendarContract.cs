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
    public interface IColloCalendarContract : IDependency
    {

        #region IColloCalendarContract

        /// <summary>
        /// 获取数据集
        /// </summary>
        IQueryable<ColloCalendar> ColloCalendars { get; }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">一个或多个数据对象</param>
        /// <returns></returns>
        OperationResult Insert(params ColloCalendarDto[] dtos);

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dtos">一个或多个数据对象</param>
        /// <returns></returns>
        OperationResult Update(params ColloCalendarDto[] dtos);


         


        #endregion


    }
}
