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
using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    /// <summary>
    /// 服务合约
    /// </summary>
    public interface IColorContract : IDependency
    {
        #region Color

		Color View(int Id);

		ColorDto Edit(int Id);

        IQueryable<Color> Colors { get; }

		OperationResult Insert(params ColorDto[] dtos);

		OperationResult Update(params ColorDto[] dtos);

		OperationResult Remove(params int[] ids);

		OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

		OperationResult Enable(params int[] ids);

		OperationResult Disable(params int[] ids);

        IEnumerable<KeyValue<string, string>> SelectList(string title);

        /// <summary>
        /// 获取颜色键值对列表
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValue<string, string>> SelectList();

        
        /// <summary>
        /// 获取一级颜色分类
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        IEnumerable<SelectListItem> ParentSelectList(string title);

        string FullName(int id, int level);

        string FullCode(int id, int level);

        bool CheckExists(Expression<Func<Color, bool>> predicate, int id = 0);

        /// <summary>
        /// 生成编码
        /// </summary>
        /// <param name="ParentId"></param>
        /// <returns></returns>
        //string GetCode(int ParentId);

        #endregion

    }
}