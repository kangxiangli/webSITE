using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    //yxk 2015-11-26
    public class GroupService : ServiceBase, IGroupContract
    {
        protected readonly IRepository<Group, int> _groupRepository;
        public GroupService(IRepository<Group, int> groupRepository)
            : base(groupRepository.UnitOfWork)
        {
            _groupRepository = groupRepository;
        }
        public IQueryable<Models.Group> Groups
        {
            get { return _groupRepository.Entities; }
        }

        public Utility.Data.OperationResult Insert(params Models.Group[] groups)
        {
            return _groupRepository.Insert((IEnumerable<Group>)groups) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public Utility.Data.OperationResult Update(params Models.Group[] groups)
        {
            var result = _groupRepository.Update(groups);
            if (result.ResultType == OperationResultType.Success)
            {
                CacheAccess.ClearPermissionCache();
            }
            return result;
        }

        public Utility.Data.OperationResult Remove(params int[] ids)
        {
            throw new NotImplementedException();
        }
    }
}
