using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Controllers
{
    public class PurchaseItemController : BaseController
    {
        //
        // GET: /Warehouses/PurchaseItem/

        protected readonly IPurchaseItemContract _purchaseItemContract;
        public ActionResult Index()
        {
            return View();
        }
        public PurchaseItemController(IPurchaseItemContract purchaseitemContract)
        {
            _purchaseItemContract = purchaseitemContract;
        }
        [HttpPost]
        public JsonResult Edit()
        {
            OperationResult res = new OperationResult(OperationResultType.Error, "修改失败");
            var quantity = Request["Quantity"];
            var id = Request["Id"];
            int itemId;
            PurchaseItem pur;
            if (id != null && id != "" && quantity != null && quantity != "")
            {
                itemId = int.Parse(id);
                pur = _purchaseItemContract.PurchaseItems.Where(c => c.Id == itemId).FirstOrDefault();
                if (pur != null)
                {
                    pur.Quantity = Convert.ToInt32(quantity);
                    pur.UpdatedTime = DateTime.Now;
                    PurchaseItemDto purdto = AutoMapper.Mapper.Map<PurchaseItemDto>(pur);
                    res = _purchaseItemContract.Update(purdto);
                }
            }
            return Json(res);
        }
    }
}