using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IDepartmentContract:IDependency
    {
         #region Administrator

		Department View(int Id);

		Department Edit(int Id);
       
		OperationResult Insert(params DepartmentDto[] dtos);
        OperationResult Insert(params Department[] deps);
        OperationResult Update(params DepartmentDto[] dtos);
        OperationResult Update(params Department[] des);

		OperationResult Remove(params int[] ids);

		OperationResult Recovery(params int[] ids);

        OperationResult Delete(params int[] ids);

		OperationResult Enable(params int[] ids);

		OperationResult Disable(params int[] ids);

        bool CheckNameExist(string AdminName);

        OperationResult CheckLogin(DepartmentDto dto);

        OperationResult FindPassword(DepartmentDto dto);

        IQueryable<Department> Departments { get; }
        bool CheckExists(Expression<Func<Department, bool>> predicate, int id = 0);

        //yxk 2015-9-17
        IEnumerable<Values<string,string>>SelectDepartment(string title, Expression<Func<Department, bool>> predicate);

        List<SelectListItem> SelectList(string title);

        List<SelectListItem> SelectList(string p, int departId);
        #endregion





        
    }
}
