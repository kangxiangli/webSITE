using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Warehouses;
using Whiskey.ZeroStore.ERP.Services.Contracts.Warehouse;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Warehouse
{
    //public class InventoryDetailService : ServiceBase, IInventoryDetailContract
    //{
    //    private IRepository<InventoryDetail, int> _inventoryDetailRepository;

    //    public InventoryDetailService(IRepository<InventoryDetail, int> inventoryDetailRepository)
    //        : base(inventoryDetailRepository.UnitOfWork)
    //    {
    //        _inventoryDetailRepository = inventoryDetailRepository;
    //    }
    //    public IQueryable<InventoryDetail> InventoryDetails
    //    {
    //        get { return _inventoryDetailRepository.Entities; }
    //    }

    //    public OperationResult Insert(params InventoryDetail[] inventoryDetails)
    //    {
    //        return _inventoryDetailRepository.Insert((IEnumerable<InventoryDetail>)inventoryDetails) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error
    //            );
    //    }

    //    public Utility.Data.OperationResult Update(Models.Entities.Warehouses.InventoryDetail[] dtos, bool isTrans = false)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Utility.Data.OperationResult Remove(params int[] ids)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
