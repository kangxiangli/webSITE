using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
 
namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface ICircleContract : IDependency
    {
        #region Circle

        Circle View(int Id);

        CircleDto Edit(int Id);

        IQueryable<Circle> Circles { get; }

        OperationResult Insert(params CircleDto[] dtos);

        OperationResult Update(params CircleDto[] dtos);
         
        OperationResult Remove(params int[] ids);

        OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

        OperationResult Enable(params int[] ids);

        OperationResult Disable(params int[] ids);

        bool CheckExists(Expression<Func<Circle, bool>> predicate, int id = 0);

        OperationResult AddCircle(int memberId, int circleId);
        #endregion


       
    }
}
