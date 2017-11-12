using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.WebApi.Controllers
{
    public class CartController : Controller
    {
        #region 声明业务层操作对象
                
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CartController));
        //声明业务层操作对象
        protected readonly ICartContract _cartContract;

        public CartController(ICartContract cartContract)
        {
            _cartContract = cartContract;
        }
        #endregion

        #region 添加购物车
        /// <summary>
        /// 添加购物车
        /// </summary>
        /// <returns></returns>
        public ActionResult Add()
        {
            return null;
        }
        #endregion
    }
}
