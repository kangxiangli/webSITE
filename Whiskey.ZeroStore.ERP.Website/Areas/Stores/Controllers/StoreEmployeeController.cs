using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;


namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    public class StoreEmployeeController : BaseController
    {
        //
        // GET: /Stores/StoreEmployee/
        protected readonly IStoreContract _storeContract;
        public StoreEmployeeController(IStoreContract storeContract)
        {
            _storeContract = storeContract;
        }

        public ActionResult Index() 
        {
           
            return View();
        }

        public ActionResult Create()
        {
            return PartialView();
        }

        public ActionResult List()
        {
            return null;
        }
	}
}