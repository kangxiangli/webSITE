using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Warehouse;

namespace Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse
{
    public interface IInventoryRecordContract:IDependency
    {
        #region Inventory

        InventoryRecord View(int Id);

        

        IQueryable<InventoryRecord> InventoryRecords { get; }


        OperationResult Insert(params InventoryRecord[] inventories);


        OperationResult Remove(params int[] ids);
        OperationResult Disable(int[] id);
        OperationResult Enable(int[] id);


        #endregion

    }
}
