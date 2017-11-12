using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Whiskey.Utility.Data;

namespace Whiskey.ZeroStore.MobileApi.Controllers
{
    public class ErrorController : ApiController
    {
        public OperationResult GetReturnError()
        {
            return new OperationResult(OperationResultType.Error, "访问失败！");
        }
    }
}
