using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class PunishScoreRecordService : IPunishScoreRecordContract
    {
        private IRepository<PunishScoreRecord, int> _repo;
        public PunishScoreRecordService(IRepository<PunishScoreRecord, int> repo)
        {
            _repo = repo;
        }
        public IQueryable<PunishScoreRecord> Entities => _repo.Entities.Where(r => !r.IsDeleted && r.IsEnabled);

        public OperationResult Insert(params PunishScoreRecord[] entity)
        {
            if (entity==null || entity.Length<=0)
            {
                throw new Exception("参数错误");
            }
            var count = _repo.Insert(entity.AsEnumerable());
            if (count <= 0)
            {
                return OperationResult.Error("保存失败");
            }
            return OperationResult.OK();

        }
    }
}
