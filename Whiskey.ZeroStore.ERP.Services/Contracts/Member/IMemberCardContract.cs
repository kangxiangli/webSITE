﻿
//   This file was generated by T4 code generator Contract_Creater.tt.



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
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 服务合约
    /// </summary>
    public interface IMemberCardContract : IDependency
    {
        #region MemberCard

		MemberCard View(int Id);

		MemberCardDto Edit(int Id);

        IQueryable<MemberCard> MemberCards { get; }

		OperationResult Insert(params MemberCardDto[] dtos);

		OperationResult Update(params MemberCardDto[] dtos);

		OperationResult Remove(params int[] ids);

		OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

		OperationResult Enable(params int[] ids);

		OperationResult Disable(params int[] ids);

        bool CheckExists(Expression<Func<MemberCard, bool>> predicate, int id = 0);

        #endregion
    }
}