using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IProductBarcodeDetailContract : IDependency
    {
        IQueryable<ProductBarcodeDetail> productBarcodeDetails { get; }
        OperationResult Insert(params ProductBarcodeDetail[] details);
        OperationResult Update(params ProductBarcodeDetail[] details);

        OperationResult BulkInsert(IEnumerable<ProductBarcodeDetail> details);
        OperationResult BulkUpdate(IEnumerable<ProductBarcodeDetail> details);

    }
}
