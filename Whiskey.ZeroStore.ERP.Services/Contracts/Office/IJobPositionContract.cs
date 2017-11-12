using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 职位接口
    /// </summary>
    public interface IJobPositionContract:IDependency
    {
        #region IJobPositionContract

        JobPosition View(int Id);

        JobPositionDto Edit(int Id);

        IQueryable<JobPosition> JobPositions { get; }

        OperationResult Insert(params JobPositionDto[] dtos);

        OperationResult Update(params JobPositionDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        List<SelectListItem> SelectList(string title,int departmentId);

        #endregion

        /// <summary>
        /// 获取用户所属职位可以管理到的所有设计师
        /// </summary>
        /// <returns></returns>
        List<Designer> GetManagedDesigners(int AdminId);

    }
}
