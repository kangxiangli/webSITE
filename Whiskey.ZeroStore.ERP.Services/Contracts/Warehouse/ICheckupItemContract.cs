using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;


namespace Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse
{
    public interface ICheckupItemContract : IDependency
    {
        IQueryable<CheckupItem> CheckupItems { get; }

        OperationResult Insert(params CheckupItem[] items);        

        OperationResult ChangeAllChecker(int Id, int Flag);

        OperationResult RemoveLack(params int[] Ids);

        OperationResult AddInventory(params int[] Ids);
    }
}
