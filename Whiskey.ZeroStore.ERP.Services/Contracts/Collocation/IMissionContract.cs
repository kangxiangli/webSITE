using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 优惠卷业务接口
    /// </summary>
    public interface IMissionContract : IDependency
    {
        #region Mission

        Mission View(int Id);

        MissionDto Edit(int Id);

        IQueryable<Mission> Missions { get; }

        //IQueryable<MissionItem> MissionItems { get; }

        OperationResult Insert(params MissionDto[] dtos);

        OperationResult Update(params MissionDto[] dtos);

        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        OperationResult GetList(int memberId, int collocationId, int PageIndex, int PageSize);
        #endregion



        
    }
}
