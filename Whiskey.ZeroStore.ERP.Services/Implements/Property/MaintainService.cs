using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models.Entities.Properties;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using XKMath36;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Property
{
    public class MaintainService : ServiceBase, IMaintainContract
    {
        static readonly object lockobj = new object();
        private readonly IRepository<MaintainExtend, int> _maintainRepository;
        public MaintainService(IRepository<MaintainExtend, int> maintainRepository)
            : base(maintainRepository.UnitOfWork)
        {
            _maintainRepository = maintainRepository;
        }
        public IQueryable<MaintainExtend> Maintains
        {
            get { return _maintainRepository.Entities; }
        }

        public Utility.Data.OperationResult Insert(params MaintainExtend[] maintains)
        {
            lock (lockobj)
            {
                var maxnum = 0;
                if (_maintainRepository.Entities.Any())
                {

                    maxnum = _maintainRepository.Entities.Max(c => c.OnlyNum);
                }
                foreach (var maint in maintains)
                {

                    Math36 math36 = new Math36();
                    maint.ExtendNumber = math36.To36(maxnum + 1).PadLeft(4, '0');
                    maint.OnlyNum = maxnum + 1;
                    maxnum += 1;

                }


            }
            return _maintainRepository.Insert((IEnumerable<MaintainExtend>)maintains) > 0
                ? new OperationResult(OperationResultType.Success)
                : new OperationResult(OperationResultType.Error);
        }

        public Utility.Data.OperationResult Update(params MaintainExtend[] maintain)
        {
            lock (lockobj)
            {
                for (int i = 0; i < maintain.Count(); i++)
                {
                    var ma = maintain[i];
                    var t = _maintainRepository.Entities.FirstOrDefault(c => c.Id == ma.Id);
                    t.ExtendName = ma.ExtendName;
                    t.Descript = ma.Descript;
                    t.IsEnabled = ma.IsEnabled;
                    t.ImgPath = ma.ImgPath;
                    maintain[i] = t;
                }
            }
            return _maintainRepository.Update((ICollection<MaintainExtend>)maintain);
        }

        public OperationResult Remove(params int[] ids)
        {
            int?[] _ids = ids.Select(c => (int?) c).ToArray();
            var te = _maintainRepository.Entities.Where(c =>_ids.Contains(c.Id)||_ids.Contains(c.ParentId));
            te.Each(c => {
                c.IsDeleted = true;
                c.UpdatedTime = DateTime.Now;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.Descript = "删除数据";
            });
            return _maintainRepository.Update(te.ToArray());
        }


        public OperationResult Recovery(params int[] ids)
        {
            int?[] _ids = ids.Select(c => (int?)c).ToArray();
            var te = _maintainRepository.Entities.Where(c => _ids.Contains(c.Id) || _ids.Contains(c.ParentId)).ToList();
            var parIds= te.Select(c => c.ParentId);
            te.AddRange(_maintainRepository.Entities.Where(c => parIds.Contains(c.Id)));
            te.Each(c => {
                c.IsDeleted = false;
                c.UpdatedTime = DateTime.Now;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.Descript = "数据恢复";
                
            });
            return _maintainRepository.Update(te.ToArray());
        }
    }
}
