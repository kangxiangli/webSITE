using Whiskey.RongCloudServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Contracts;


namespace Whiskey.ZeroStore.ERP.Services
{
    public class MemberIMService : ServiceBase, IMemberIMProfileContract
    {
        private const string APP_KEY = "pkfcgjstffq98";
        private const string APP_SECRET = "xmwo5qe2jvT3";

        private IRepository<MemberIMProfile, int> _repo;
        private IMemberContract _memContract;
        public MemberIMService(IRepository<MemberIMProfile, int> repo, IMemberContract memContract) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _memContract = memContract;
        }
        public IQueryable<MemberIMProfile> Entities
        {
            get
            {
                return _repo.Entities;
            }
        }

        public string GetToken(string memberId, string name, string portraitUri)
        {
            var mId = int.Parse(memberId);
            var entity = _repo.Entities.FirstOrDefault(m => m.MemberId == mId && !m.IsDeleted && m.IsEnabled);
            if (entity == null)
            {
                //check member exist
                var memEntity = _memContract.Members.FirstOrDefault(m => m.Id == mId && !m.IsDeleted && m.IsEnabled);
                if (memEntity == null)
                {
                    return string.Empty;
                }
                //generate token for member
                var res = RongCloudServerHelper.GetToken(memberId, name, portraitUri);
                if (res.code != 200 || res.userId != memberId)
                {
                    return string.Empty;
                }

                //persist token
                var count = _repo.Insert(new MemberIMProfile() { MemberId = mId, Token = res.token });
                if (count <= 0)
                {
                    return string.Empty;
                }

                return res.token;

            }
            return entity.Token;
        }
    }
}
