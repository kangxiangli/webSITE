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
using Whiskey.ZeroStore.ERP.Models.Entities.StoreCollocation;
using Whiskey.ZeroStore.ERP.Services.Contracts.StoreCollocation;
using Whiskey.ZeroStore.ERP.Transfers.Entities.StoreCollocation;

namespace Whiskey.ZeroStore.ERP.Services.Implements.StoreCollocation
{
    public class StoreCollocationInfoService : ServiceBase, IStoreCollocationInfoContract
    {
        protected readonly IRepository<StoreCollocationInfo, int> _storeCollocationInfo;
        public StoreCollocationInfoService(IRepository<StoreCollocationInfo, int> storeCollocationInfo) : base(storeCollocationInfo.UnitOfWork)
        {
            _storeCollocationInfo = storeCollocationInfo;
        }

        public IQueryable<StoreCollocationInfo> StoreCollocationInfos
        {
            get { return _storeCollocationInfo.Entities; }
        }

        public OperationResult Insert(params StoreCollocationInfo[] rules)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in rules)
            {
                rule.OperatorId = AuthorityHelper.OperatorId;
                resul = _storeCollocationInfo.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }

        public Utility.Data.OperationResult Update(params StoreCollocationInfoDto[] rules)
        {
            var resul = _storeCollocationInfo.Update(rules, null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<StoreCollocationInfo>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsEnabled = false;
                return ent;
            });
            return resul;
        }

        public OperationResult Remove(bool statues, params int[] ids)
        {
            var entys = _storeCollocationInfo.Entities.Where(c => ids.Contains(c.Id));
            entys.Each(c =>
            {
                c.IsDeleted = statues;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _storeCollocationInfo.Update(entys.ToList());
        }
        public OperationResult TrueRemove(params int[] ids)
        {
            var entys = _storeCollocationInfo.Entities.Where(c => ids.Contains(c.Id));
            int result = _storeCollocationInfo.Delete(entys);
            if (result > 0)
            {
                return new OperationResult(OperationResultType.Success, "删除成功！", "");
            }
            else
            {
                return new OperationResult(OperationResultType.Error, "删除失败！", "");
            }

        }
        public OperationResult DeleteByCollocationId(params int[] ids)
        {
            var entys = _storeCollocationInfo.Entities.Where(c => ids.Contains(c.StoreCollocationId.Value));

            int result = _storeCollocationInfo.Delete(entys.ToList());
            if (result > 0)
            {
                return new OperationResult(OperationResultType.Success, "删除成功！", "");
            }
            else
            {
                return new OperationResult(OperationResultType.Error, "删除失败！", "");
            }
        }
        public OperationResult Disable(bool statues, params int[] ids)
        {
            var enty = _storeCollocationInfo.Entities.Where(c => ids.Contains(c.Id));
            enty.Each(c =>
            {
                c.IsEnabled = statues;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _storeCollocationInfo.Update(enty.ToList());
        }
    }
}
