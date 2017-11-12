using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IRechargeOrderContract : IDependency
    {
        IQueryable<RechargeOrder> RechargeOrders { get; }
        OperationResult Insert(params RechargeOrder[] rules);
        OperationResult Update(params RechargeOrderDto[] rules);
        OperationResult Update(params RechargeOrder[] entities);
        OperationResult Remove(params int[] ids);
        OperationResult PaymentSuccess(string transaction_id, int status, params string[] orderNumber);
        DbContextTransaction GetTransaction();
    }
}
