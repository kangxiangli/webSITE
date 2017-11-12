using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts.StoreCollocation;
using Whiskey.ZeroStore.ERP.Transfers.Entities.StoreCollocation;

namespace Whiskey.ZeroStore.ERP.Services.Implements.StoreCollocation
{
   public class StoreProductCollocationService : ServiceBase, IStoreProductCollocationContract
    {
        protected readonly IRepository<StoreProductCollocation, int> _storeProductCollocation;
        public StoreProductCollocationService(IRepository<StoreProductCollocation, int> storeProductCollocation):base(storeProductCollocation.UnitOfWork)
        {
            _storeProductCollocation = storeProductCollocation;
        }

        public IQueryable<StoreProductCollocation> StoreProductCollocations
        {
            get { return _storeProductCollocation.Entities; }
        }



        public OperationResult Insert(params StoreProductCollocation[] rules)
        {

            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in rules)
            {

                resul = _storeProductCollocation.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }

        public Utility.Data.OperationResult Update(params StoreCollocationDto[] rules)
        {
            var resul = _storeProductCollocation.Update(rules, null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<StoreProductCollocation>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsEnabled = true;
                return ent;
            });
            return resul;
        }
        
        public OperationResult Remove(bool statues, params int[] ids)
        {
            var entys = _storeProductCollocation.Entities.Where(c => ids.Contains(c.Id));
            entys.Each(c =>
            {
                c.IsDeleted = statues;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _storeProductCollocation.Update(entys.ToList());
        }

        public OperationResult Disable(bool statues, params int[] ids)
        {
            var enty = _storeProductCollocation.Entities.Where(c => ids.Contains(c.Id));
            enty.Each(c =>
            {
                c.IsEnabled = statues;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _storeProductCollocation.Update(enty.ToList());
        }

        public OperationResult TrueRemove(params int[] ids)
        {
            var entys = _storeProductCollocation.Entities.Where(c => ids.Contains(c.Id));
            int result = _storeProductCollocation.Delete(entys);
            if (result > 0)
            {
                return new OperationResult(OperationResultType.Success, "删除成功！", "");
            }
            else
            {
                return new OperationResult(OperationResultType.Error, "删除失败！", "");
            }
        }
    }
}
