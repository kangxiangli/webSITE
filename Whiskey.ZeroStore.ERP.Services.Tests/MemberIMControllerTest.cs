using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Autofac;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.RongCloudServer;
namespace UnitTest
{
    public class MemberIMControllerTest : IClassFixture<AutofacFixture>
    {
        private AutofacFixture _fixture;
        public MemberIMControllerTest(AutofacFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void TestGetToken()
        {
            var contract = _fixture.Container.Resolve<IMemberIMProfileContract>();
            var memberId = "26";
            var name = "李成";
            var avatar = "https://coding.net/static/fruit_avatar/Fruit-14.png";
            var res = contract.GetToken(memberId, name, avatar);
            
            Assert.NotNull(res);
        }

        [Fact]
        public void TestAcceptFriend()
        {
            var userId1 = 26;
            var userId2 = 5275;
            var biz = _fixture.Container.Resolve<IMemberIMFriendContract>();
            var res = biz.Insert(new Whiskey.ZeroStore.ERP.Models.MemberIMFriend() { MemberId1 = userId1, MemberId2 = userId2 });
            Assert.True(res.ResultType == Whiskey.Utility.Data.OperationResultType.Success);
        }


       
        [Fact]
        public void TestGetMyFriend()
        {
            var memberId = 26;
            var service = _fixture.Container.Resolve<IMemberIMFriendContract>();
            var res = service.GetMyFriend(memberId);
            Assert.True(res.ResultType == Whiskey.Utility.Data.OperationResultType.Success);

        }

        [Fact]
        public void TestGetFriendDetail()
        {
            var userId = 26;
            var memberId = 5275;
            var service = _fixture.Container.Resolve<IMemberIMFriendContract>();
            var res = service.GetFriendDetail(userId,memberId);
            Assert.True(res.ResultType == Whiskey.Utility.Data.OperationResultType.Success);

        }


        [Fact]
        public void TestBlackList()
        {
            var userId = "26";
            var blackUserId = new string[] { "5275" };
            var res = RongCloudServerHelper.Black(userId, blackUserId);
            Assert.Equal(200, res.code);

            var res2 = RongCloudServerHelper.QueryBlack(userId);
            Assert.Equal(200, res2.code);
            var blackUsers = res2.users;
            Assert.True(blackUsers.Count() == 1);


            var res3 = RongCloudServerHelper.UnBlack(userId, blackUserId.First());
            Assert.Equal(200, res3.code);
            blackUsers = RongCloudServerHelper.QueryBlack(userId).users;
            Assert.True(blackUsers.Count() == 0);

        }
    }
}
