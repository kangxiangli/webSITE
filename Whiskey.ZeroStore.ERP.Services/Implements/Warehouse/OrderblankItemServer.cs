using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services
{
    public class OrderblankItemServer : ServiceBase, IOrderblankItemContract
    {
        protected readonly IRepository<OrderblankItem, int> _orderblankItemrepository;
        public OrderblankItemServer(IRepository<OrderblankItem, int> orderblankItemrepository)
            : base(orderblankItemrepository.UnitOfWork)
        {
            _orderblankItemrepository = orderblankItemrepository;
        }
        public IQueryable<Models.OrderblankItem> OrderblankItems
        {
            get
            {
                return _orderblankItemrepository.Entities;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public Utility.Data.OperationResult Insert(OrderblankItem[] items)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            if (items != null)
            {
                resul = _orderblankItemrepository.Insert((IEnumerable<OrderblankItem>)items) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            else
                resul = new OperationResult(OperationResultType.Error);
            return resul;


        }
        public OperationResult Delete(int[] delids)
        {
            return _orderblankItemrepository.Delete(c => delids.Contains(c.Id)) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public OperationResult Update(OrderblankItem[] orderblankItem)
        {
           return _orderblankItemrepository.Update(orderblankItem);
        }
    }
}
