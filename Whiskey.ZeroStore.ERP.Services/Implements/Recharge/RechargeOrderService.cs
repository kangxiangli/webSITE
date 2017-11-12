using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Recharge
{
  public   class RechargeOrderService : ServiceBase, IRechargeOrderContract
    {
        protected readonly IRepository<RechargeOrder, int> _orderRepository;
        public RechargeOrderService(IRepository<RechargeOrder, int> orderRepository):base(orderRepository.UnitOfWork)
        {
            _orderRepository = orderRepository;
        }

        public IQueryable<RechargeOrder> RechargeOrders
        {
            get { return _orderRepository.Entities; }
        }



        public OperationResult Insert( params RechargeOrder[] rules)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in rules)
            {
                rule.OperatorId = AuthorityHelper.OperatorId;
                resul = _orderRepository.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }

        public Utility.Data.OperationResult Update(params RechargeOrderDto[] rules)
        {
            var resul = _orderRepository.Update(rules, null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<RechargeOrder>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsEnabled = false;
                return ent;
            });
            return resul;
        }

        public Utility.Data.OperationResult Update(params RechargeOrder[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;
                OperationResult result = _orderRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                return OperationHelper.ReturnOperationResult(count > 0, opera);
            }, Operation.Update);
        }

        public OperationResult Remove(params int[] ids)
        {
            var entys = _orderRepository.Entities.Where(c => ids.Contains(c.Id));
            entys.Each(c =>
            {
                c.IsDeleted = true;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _orderRepository.Update(entys.ToList());
        }

        public OperationResult PaymentSuccess(string transaction_id,int status, params string[] orderNumber)
        {
            var entys = _orderRepository.Entities.Where(c => orderNumber.Contains(c.order_Uid));
            entys.Each(c =>
            {
                c.Prepay_Id = transaction_id;
                c.pay_status = status;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _orderRepository.Update(entys.ToList());
        }

        public DbContextTransaction GetTransaction()
        {
            return _orderRepository.GetTransaction();
        }
    }
}
