using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Whiskey.Web.Helper;
using Whiskey.Utility.Data;
using Whiskey.jpush.api;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Hubs
{
    public class APIConfirmLoginConnection : PersistentConnection
    {
       
        protected override bool AuthorizeRequest(IRequest request)
        {
            var adminId = AuthorityHelper.OperatorId;

            return adminId > 0;
        }
        private class dto
        {
            public int Context { get; set; }
            public string LoginName { get; set; }
        }
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return base.OnReceived(request, connectionId, data);
        }


    }
}