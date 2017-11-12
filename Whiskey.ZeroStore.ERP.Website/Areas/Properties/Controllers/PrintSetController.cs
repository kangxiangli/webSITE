using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{
    public class PrintSetController : BaseController
    {
        private readonly IBarCodeConfigContract _barCodeConfigContract;
        public PrintSetController(
            IBarCodeConfigContract _barCodeConfigContract
            )
        {
            this._barCodeConfigContract = _barCodeConfigContract;
        }

        [Layout]
        public ActionResult Index()
        {
            BarCodeConfig modbcc = _barCodeConfigContract.BarCodeConfigs.FirstOrDefault(f => !f.IsDeleted && f.IsEnabled);
            if (modbcc.IsNull())
            {
                modbcc = new BarCodeConfig();
            }
            return View(modbcc);
        }

        public ActionResult SavePrintSet(BarCodeConfigDto dto)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            try
            {
                if (dto.Id == 0)//新增
                {
                    resul = _barCodeConfigContract.Insert(dto);
                }
                else
                {
                    resul = _barCodeConfigContract.Update(dto);
                }
            }
            catch (Exception ex)
            {
                resul.Message = ex.Message;
            }
            return Json(resul);
        }
    }
}