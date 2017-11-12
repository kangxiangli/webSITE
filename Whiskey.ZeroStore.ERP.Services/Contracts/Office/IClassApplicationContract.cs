using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services
{
    public interface IClassApplicationContract : IDependency
    {

        IQueryable<ClassApplication> ClassApplications { get; }

        OperationResult Insert(params ClassApplication[] dtos);

        OperationResult Update(params ClassApplicationDto[] dtos);

        OperationResult Remove(bool isdelate, params int[] ids);
        OperationResult ToExamine(int ToExamineStatues, params int[] ids);
    }
}
