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
    public interface IResignationContract : IDependency
    {
        #region Resignation

        Resignation View(int Id);

        ResignationDto Edit(int Id);

        IQueryable<Resignation> Resignations { get; }

        OperationResult Insert(params ResignationDto[] dtos);

        OperationResult Update(params ResignationDto[] dtos);

        OperationResult ToExamine(int ToExamineStatues, params int[] ids);


        #endregion
    }
}
