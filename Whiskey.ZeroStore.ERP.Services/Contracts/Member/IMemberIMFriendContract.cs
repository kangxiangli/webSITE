using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Services.Contracts
{
    public interface IMemberIMFriendContract : IDependency
    {
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns></returns>
        OperationResult Insert(MemberIMFriend entity);

        IQueryable<MemberIMFriend> Entities { get; }

        OperationResult AcceptFriend(int member1, int member2);

        OperationResult GetMyFriend(int memberId);

        OperationResult GetFriendDetail(int userId,int friendId,string host= "https://api.0-fashion.com/");

        /// <summary>
        /// 获取我的好友（对应会员）
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        IQueryable<Member> GetMyFriends(int memberId);

    }
}
