using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using System.Collections.Generic;
using System.Collections;
using Whiskey.RongCloudServer;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class MemberIMFriendService : ServiceBase, IMemberIMFriendContract
    {
        private IRepository<MemberIMFriend, int> _repo;
        private IMemberContract _memberContract;
        protected readonly IMemberCollocationContract _memberCollocationContract;

        public MemberIMFriendService(IRepository<MemberIMFriend, int> repo, IMemberContract memContract, IMemberCollocationContract memberCollocationContract) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _memberContract = memContract;
            _memberCollocationContract = memberCollocationContract;
        }

        public IQueryable<MemberIMFriend> Entities
        {
            get
            {
                return _repo.Entities;
            }
        }

        public OperationResult AcceptFriend(int memberId1, int memberId2)
        {
            // user exist check
            var sourceMember = _memberContract.Members.Where(m => m.Id == memberId1 && !m.IsDeleted && m.IsEnabled).FirstOrDefault();
            if (sourceMember == null)
            {
                return new OperationResult(OperationResultType.ValidError, "用户1不存在");
            }
            var targetMember = _memberContract.Members.Where(m => m.Id == memberId2 && !m.IsDeleted && m.IsEnabled).FirstOrDefault();
            if (targetMember == null)
            {
                return new OperationResult(OperationResultType.ValidError, "用户2不存在");
            }

            // assemble entity
            MemberIMFriend friendship = new MemberIMFriend()
            {
                MemberId1 = memberId1,
                MemberId2 = targetMember.Id,
                Status = 1
            };

            // check exist before
            var exist = _repo.Entities.FirstOrDefault(f => (f.MemberId1 == memberId1 && f.MemberId2 == memberId2) ||
                                                           (f.MemberId1 == memberId2 && f.MemberId2 == memberId1));
            if (exist != null)
            {
                if (exist.Status == 1 && !exist.IsDeleted && exist.IsEnabled)
                {
                    return new OperationResult(OperationResultType.Success);
                }
                //update opt
                exist.Status = 1;
                exist.IsDeleted = false;
                exist.IsEnabled = true;
                return _repo.Update(exist) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            var count = _repo.Insert(friendship);
            if (count <= 0)
            {
                return new OperationResult(OperationResultType.Error);
            }
            //向请求者发送一条消息
            var msg = new PlainMsg()
            {
                content = "我已同意了你的好友请求，开始聊天吧~",
                user = new UserInfo()
                {
                    id = memberId1.ToString(),
                    name = sourceMember.MemberName,
                    icon = string.IsNullOrEmpty(sourceMember.UserPhoto) ? string.Empty : WebUrl + sourceMember.UserPhoto,
                }
            };

            var res = RongCloudServerHelper.publishPrivate(memberId1.ToString(), new string[] { memberId2.ToString() }, JsonHelper.ToJson(msg), msg.GetMsgType(), msg.content, string.Empty, "1", 1, 1, 1);

            if (res.code != 200)
            {
                return new OperationResult(OperationResultType.Error);
            }

            // intert opt
            return new OperationResult(OperationResultType.Success);
        }

        public OperationResult GetFriendDetail(int userId, int friendId, string host = "https://api.0-fashion.com/")
        {
            var sourceMember = _memberContract.Members.Where(m => m.Id == friendId && !m.IsDeleted && m.IsEnabled).Select(m => new
            {
                m.Id,
                m.MemberName,
                m.UserPhoto,
                m.MemberType.MemberTypeName,
                m.CreatedTime
            }).FirstOrDefault();

            if (sourceMember == null)
            {
                return new OperationResult(OperationResultType.ValidError, "用户不存在");
            }

            string strWebUrl = host;

            //查询会员搭配
            var collList = _memberCollocationContract.GetList(friendId).Where(m => !string.IsNullOrEmpty(m.ColloImagePath)).ToList();
            var res = RongCloudServer.RongCloudServerHelper.QueryBlack(userId.ToString());
            var isBlack = res.users.Contains(friendId.ToString());
            //组装数据
            var resData = new
            {
                Id = sourceMember.Id,
                sourceMember.MemberName,
                UserPhoto = string.IsNullOrEmpty(sourceMember.UserPhoto) ? string.Empty : WebUrl + sourceMember.UserPhoto,
                sourceMember.MemberTypeName,
                CollocationFeed = collList.Select(c => new { c.ColloId, ColloImagePath = string.IsNullOrEmpty(c.ColloImagePath) ? string.Empty : (strWebUrl + c.ColloImagePath) }).ToList(),
                CreatedTime = sourceMember.CreatedTime.ToUnixTime(),
                IsBlack = isBlack
            };

            return new OperationResult(OperationResultType.Success, string.Empty, resData);
        }

        public OperationResult GetMyFriend(int memberId)
        {
            //从好友关系中检索与memberid有关联的信息
            var friends = _repo.Entities.Where(f => !f.IsDeleted && f.IsEnabled && (f.MemberId1 == memberId || f.MemberId2 == memberId)).ToList();

            // 过滤出每条记录关联的friendid
            List<int> friendIds = new List<int>();
            friendIds.AddRange(friends.Where(f => f.MemberId1 == memberId).Select(f => f.MemberId2).ToList());
            friendIds.AddRange(friends.Where(f => f.MemberId2 == memberId).Select(f => f.MemberId1).ToList());
            // 去重
            friendIds = friendIds.Distinct().ToList();
            if (friendIds.Count <= 0)
            {
                return new OperationResult(OperationResultType.Success, string.Empty, new string[] { });
            }

            var friendInfo = _memberContract.Members.Where(m => !m.IsDeleted && m.IsEnabled && friendIds.Contains(m.Id)).Select(m => new
            {
                m.Id,
                m.MemberName,
                m.UserPhoto,
            }).ToList();

            var resData = friendInfo.Select(m => new
            {
                m.Id,
                m.MemberName,
                UserPhoto = string.IsNullOrEmpty(m.UserPhoto) ? string.Empty : WebUrl + m.UserPhoto
            }).ToList();
            return new OperationResult(OperationResultType.Success, string.Empty, resData);

        }

        public OperationResult Insert(MemberIMFriend entity)
        {
            return _repo.Insert(entity) > 0 ? new OperationResult(OperationResultType.Success, string.Empty) : new OperationResult(OperationResultType.Error, "添加失败");
        }

        public OperationResult Update(MemberIMFriend entity)
        {
            return _repo.Update(entity) > 0 ? new OperationResult(OperationResultType.Success, string.Empty) : new OperationResult(OperationResultType.Error, "没有变化");
        }

        public IQueryable<Member> GetMyFriends(int memberId)
        {
            var memberids = (from s in Entities.Where(w => w.IsEnabled && !w.IsDeleted && (w.MemberId1 == memberId || w.MemberId2 == memberId) && w.Status == 1)//Status==1表示已同意成为好友
                             let id1 = s.MemberId1
                             let id2 = s.MemberId2
                             select id1 == memberId ? id2 : id1).Distinct();

            var list = from s in _memberContract.Members.Where(w => w.IsEnabled && !w.IsDeleted)
                       where memberids.Contains(s.Id)
                       select s;
            return list;

        }
    }
}