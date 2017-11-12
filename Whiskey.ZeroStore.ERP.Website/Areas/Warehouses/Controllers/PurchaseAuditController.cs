using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    public class PurchaseAuditController : Controller
    {
        //
        // GET: /Warehouses/PurchaseAudit/

        protected readonly IPurchaseAuditContract _purchaseAuditContract;
        public PurchaseAuditController(IPurchaseAuditContract purchaseAuditContract) {
            _purchaseAuditContract = purchaseAuditContract;
        }
	}
}